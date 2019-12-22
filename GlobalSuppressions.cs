// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.


[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:WebServerTransporter.TransporterServer.Transport(Microsoft.AspNetCore.Http.HttpContext,System.Func{System.Threading.Tasks.Task})~System.Threading.Tasks.Task")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Serialization fails if it is read only", Scope = "member", Target = "~P:PeakSWC.WebServerTransporter.HttpRequestPacket.Headers")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>", Scope = "member", Target = "~P:PeakSWC.WebServerTransporter.HttpResponsePacket.Cookies")]