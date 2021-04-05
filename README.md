# Route53 Updater

## What is is for?

Imagine you have a network like this:

![NAT](https://github.com/Skipperro/route53-updater/blob/master/img/NAT.png "NAT architecture")

There are some PCs behind NAT and the router is getting dynamic IP from ISP, so each time you reconnect with the
Internet, you get new IP address. This is a very common setup in private and residential networks.

If you want to access multiple computers behind the router with something like SSH, RDP you would have to:

- Setup the services on each PC using unique port for each.
- Setup port forwarding rules on your router.
- Use something like NoIP or DynDNS to keep your dynamic IP up-to-date with some domain.

This is pretty complicated and could be impossible in some cases.

Route53 Updater can solve this issue by creating IPv6 DNS records on AWS Route 53 service, that points directly to
specific PC and keep them updated it your IP changes.

## How it's working

![Route53](https://github.com/Skipperro/route53-updater/blob/master/img/Route53.png "Route 53 DNS")

While the service is running the DNS record configured in config file is kept up-to-date with **current IPv6 address of
the PC**, so it's always possible to access the machine by using subdomain like "workstation.mydomain.net"
or "laptop.mydomain.net" and all the ports are available to use on all machines, so all of them can for example use
default port 22 for SSH or can have web servers on port 80.

So if you are away and want to access on your NAS at home, you just connect with "nas.mydomain.net" and access it like
it would be publicly available in the Internet.

## Setup

### Amazon Route 53

In order to setup this dynamic DNS you must first configure Amazon Route 53 service.

For start, you will need to have your own domain parked on AWS. You could register domain starting from 9$ per year.

Simply go to this URL and register or transfer domain of your choice:
https://console.aws.amazon.com/route53/v2/home#GetStarted

When this is done, you have to create public "Hosted Zone" via AWS Console. This should match the registered domain.

Doing this would give you an option, to edit DNS records manually.

### Amazon IAM

You will also need to create a user with programatic access to your AWS via API Key in order to update DNS records
automatically.

Go to AWS IAM, create new user, give him programatic access and permissions to Route53 service. At the end of this
process, you will get API Key and Secret. **Please save them for later**.

### Configure Route53-Updater

You could build it on your own, but easiest way would be to simply grab latest binaries from here:
https://github.com/Skipperro/route53-updater/releases

Download the package, extract it and edit `config.json` file.

This is how it should look like by default:

```json
{
"AwsKey": "INPUT YOUR AWS IAM KEY HERE",
"AwsSecret": "INPUT YOUR AWS IAM SECRET HERE",
"RefreshInterval": 60000,
"HostedZone": "INPUT YOUR HOSTED ZONE DOMAIN HERE",
"RecordName": "INPUT YOUR FULL RECORD NAME HERE",
"UseIPv6": true
}
```

Description:

- AwsKey - API Key of IAM user created to access Route 53 service.
- AwsSecret - Second, secret part of your IAM API Key.
- RefreshInterval - How often should the service check, if IP have changed (in milliseconds)
- HostedZone - Name of your Hosted Zone on AWS Route 53. Typically it should be just your domain name, like "
  mydomain.net".
- RecordName - Subdomain for this specific PC, like "workstation.mydomain.net".
- UseIPv6 - Switch to false if you want to just update IPv4 address instead of IPv6.

Fill out the missing values and save the file.

### Install and run (Linux)

For Linux I've prepared easy installation script, that will copy files into `/usr/share/`, define it as a service and
run automatically.

To start the installation script just run:

```
sudo sh ./install.sh
```

After that you can check if everything is working fine by typing:

```
sudo service route53-updater status
```

And that's it. Your DNS will be kept up-to-date!

### Install and run (Windows)

This is still not done, but you could just start
`route53-updater.exe` to run it manually.

You could also add it to autostart, preferably via Task Scheduler.

### Troubleshooting

If you can't access your PC using defined domain, please check your Router settings. Some of them, like FritzBox have
IPv6 forwarding disabled by default. You need do activate it in order to have access to your PCs.