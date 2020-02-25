# WebServerTransporter WebSocket Sample

This sample shows how to modify your web server in order for it to transport WebSocket calls.  The UseWebServerTransporter has an optional parameter that allows you to implement the middleware which will interface with a WebSocket.
An instance of HttpTransporterContext is passed to the middleware instead of an instance of HttpContext. The main difference between the two classes is that the WebSockets method of HttpTransporterContext returns and instance of TransportableWebSocketManager which handles the WebSocket routing between the web server and the Transporter server.

The Startup.cs file contains the code which opens the WebSocket and writes battery status to the client.

Download the TransporterServer.exe from https://github.com/budcribar/WebServerTransporter/releases

Start the Transporter server and specify a port as follows
```
./Transporter.exe http://localhost:53343
```
Set the BatteryMonitor server as the startup project in Visual Studio and run.

Now open a web browser using the URL of the transporter (http://localhost:53343) and try it out.
At this point you can host the TransporterServer.exe in the public cloud and have access to your Battery status from anywhere!

# Problems
Please open a bug or feature request here: https://github.com/budcribar/WebServerTransporter/issues/new/choose

