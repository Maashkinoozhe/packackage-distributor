# packackage-distributor
Simple .NET application for distributing file packages between clients.

## Commands
List of commands that are used for communication between the nodes.

- GetNodes
  - Returns a list of known Nodes

- GetPackages
  - Returns a list of packages and revisions
  - Contains Package and revision information
  - Contains some revision metadata like expected size, number of blobs

- GetPackage
  - Returns all package/revision blobs and BlobOccurrences

- PushBlob
  - Instructs a node to send back a given blob

- InitiateConnection
  - Instructs a node 1 (most likely master) with connection to another node 2 to negotiate hole punching with me (node 0)
  - node 0 instructs node 1, node 1 will instruct node 2 to connect with node 2

- StartConnecting
  - Node 1 instructs node 2 to connect with node 0

- RegisterNode
  - Lets a node announce it self with master, or any other node and transmit basic node information.
