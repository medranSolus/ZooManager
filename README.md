# Zoo Manager
Project for simple zoo managment. Current components consist of:
- Client app (ZooManager)
- Communication DLL (ZooCom)
- Server (ZooService)

**1. Client app**

  WPF application connecting through DLL module with server to obtain data about Zoo.
  
**2. Communication DLL**

  DLL module responsible for transfering data between client app and server. Protocols of how client app will receive data from database are described here.
  It is possible to define another method of communication other than TCP connection by replacing ZooCom.dll in client app directory.
  Also data types are defined here used both by server and client app to send and receive database data.
  
**3. Server**

  TCP server responsible for communicating with client app and local database. Database is based on SQL Server (schema and initial data in ZooSchema.sql).
  To access database Entity Framework is used and standard SqlConnection.
  
Current versions of used programs:
  - IDE: Visual Studio 2017
  - Database: SQL Server 2017 Express
  - ORM: Entity Framework 6
