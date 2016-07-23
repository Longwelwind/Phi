# Phi
Phi is a Rimworld mod that enables online multiplayer interactions between players.

## Projects
### PhiClient
This is the mod for Rimworld. Rim-world specific files (like Defs) are located in PhiClient/Phi

Since Rimworld doesn't handle multiple assemblies very well, the 4 assemblies (PhiClient, PhiData, SocketLibrary and websocket-sharp) must be bundled in a single .dll using a software like ILMerge.

### PhiServer
This is the server program.

### PhiData
Contains the shared code between the server and the client. It is mainly the data structures that are synced between the clients and the server.
Used by PhiClient and PhiServer.

### SocketLibrary
A wrapper library around websocket-sharp.
Used by PhiClient and PhiServer.