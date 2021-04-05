using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Route53;
using Amazon.Route53.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace route53_updater
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private string _lastIpv4 = "";
        private string _lastIpv6 = "";
        private int _interval = 60000;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    UpdateIp();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    _logger.LogError(e.StackTrace);
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async void UpdateIp()
        {
            try
            {
                var settings = Settings.Load();
                _interval = settings.RefreshInterval;
                if (settings.AwsKey.Contains(" ") || settings.AwsSecret.Contains(" ") ||
                    !settings.HostedZone.Contains(".") || !settings.RecordName.Contains("."))
                {
                    _logger.LogError($"Config file is invalid. Please edit {Settings.GetConfigFilePath()} file!");
                    return;
                }

                var httpClient = new HttpClient();
                var ip = "";

                if (settings.UseIPv6)
                {
                    var apiUrl = "http://api64.ipify.org";
                    ip = await httpClient.GetStringAsync(apiUrl);

                    if (_lastIpv6 == ip) return;
                    _lastIpv6 = ip;
                }
                else
                {
                    var apiUrl = "http://api.ipify.org";
                    ip = await httpClient.GetStringAsync(apiUrl);

                    if (_lastIpv4 == ip) return;
                    _lastIpv4 = ip;
                }

                var key = settings.AwsKey;
                var secret = settings.AwsSecret;

                AWSCredentials creds = new BasicAWSCredentials(key, secret);
                var client = new AmazonRoute53Client(creds, RegionEndpoint.EUCentral1);
                var zones = await client.ListHostedZonesAsync();

                foreach (var hzone in zones.HostedZones)
                {
                    if (!hzone.Name.ToLower().StartsWith(settings.HostedZone.ToLower())) continue;
                    var hostedZoneId = hzone.Id;
                    var rtype = RRType.A;
                    if (settings.UseIPv6) rtype = RRType.AAAA;
                    var recordSet = new ResourceRecordSet()
                    {
                        Name = settings.RecordName.ToLower(),
                        TTL = 60,
                        Type = rtype,
                        ResourceRecords = new List<ResourceRecord>
                        {
                            new ResourceRecord {Value = ip}
                        }
                    };
                    var change = new Change()
                    {
                        ResourceRecordSet = recordSet,
                        Action = ChangeAction.UPSERT
                    };
                    var changeBatch = new ChangeBatch()
                    {
                        Changes = new List<Change> {change}
                    };
                    var recordsetRequest = new ChangeResourceRecordSetsRequest()
                    {
                        HostedZoneId = hostedZoneId,
                        ChangeBatch = changeBatch
                    };
                    var recordsetResponse = client.ChangeResourceRecordSetsAsync(recordsetRequest).Result;
                    _logger.LogInformation(
                        $"Updated AWS Route 53 record of [{settings.RecordName.ToLower()}] to [{ip}]");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
            }
        }
    }
}