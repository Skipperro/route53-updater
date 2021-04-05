#!/bin/bash

if [ "$(id -u)" != "0" ]; then
   echo "This script must be run as root" 1>&2
   exit 1
fi

echo Stopping service...
sudo service route53-updater stop
echo Copying files...
sudo cp -rf ./ /usr/share/route53-updater/
sudo cp -rf ./route53-updater.service /etc/systemd/system/route53-updater.service
echo Starting service...
sudo systemctl daemon-reload
sudo systemctl enable route53-updater.service
sudo service route53-updater start