# packackage-distributor
Simple .NET application for distributing file packages between clients.

# Services

## Router
Service that will listen on a udp port and receive packages.
It will insprect the nature of the package and will foreword it to the matching receiver.

## CommandListener
Service will received messages from the router.
It will receive commands and will process them.