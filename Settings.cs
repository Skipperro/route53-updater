using System;
using System.IO;
using Amazon;
using Amazon.Route53;
using Amazon.Runtime;
using Newtonsoft.Json;

namespace route53_updater
{
    public class Settings
    {
        public string AwsKey { get; set; }
        public string AwsSecret { get; set; }
        public int RefreshInterval { get; set; }
        public string HostedZone { get; set; }
        public string RecordName { get; set; }
        public bool UseIPv6 { get; set; }

        public Settings(string awsKey, string awsSecret, int refreshInterval, string hostedZone, string recordName,
            bool useIPv6)
        {
            AwsKey = awsKey;
            AwsSecret = awsSecret;
            RefreshInterval = refreshInterval;
            HostedZone = hostedZone;
            RecordName = recordName;
            UseIPv6 = useIPv6;
        }

        public Settings()
        {
            // For JSON
        }

        public static Settings Load()
        {
            string settingsPath = GetConfigFilePath();

            if (!File.Exists(settingsPath))
            {
                var nsettings = new Settings("INPUT YOUR AWS IAM KEY HERE", "INPUT YOUR AWS IAM SECRET HERE", 60000,
                    "INPUT YOUR HOSTED ZONE DOMAIN HERE", "INPUT YOUR FULL RECORD NAME HERE", true);
                var njson = JsonConvert.SerializeObject(nsettings, Formatting.Indented);
                File.WriteAllText(settingsPath, njson);
            }

            var json = File.ReadAllText(settingsPath);
            return JsonConvert.DeserializeObject<Settings>(json);
        }

        public static string GetConfigFilePath()
        {
            string exeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string workPath = System.IO.Path.GetDirectoryName(exeFilePath);
            string settingsPath = Path.Combine(workPath, "config.json");
            return settingsPath;
        }
    }
}