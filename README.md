# WebServerTransporter 

WebServerTransporter is a nuget package that allows a developer to easily modify a dot net core Kestrel web server that resides on a private IP network (the "PrivateServer") and expose the server
on the public network via a proxy called the "Transporter" server.

This functionality is useful when the Private Server cannot be moved to the public cloud because either it interacts with local hardware or depends on legacy systems that would be difficult or expensive to deploy in the cloud.

# How it Works 

The Private Server connects to the Transporter via a WebSocket connection on port 80. Next, user web requests to the Transporter are routed via the WebSocket to the 
Private Server. Finally, the Private Server sends the web reponses back to the Transporter which then forwards the responses back to the user's web browser.

# Quick Start
Create a razor sample web site

```
dotnet new razor
```

Add the reference to the nuget package to the razor.csproj file 

```
 <ItemGroup>
    <PackageReference Include="PeakSWC.WebServerTransporter" Version="0.0.5-pre" />
  </ItemGroup>
```

Modify the Startup.cs as follows:
```
using PeakSWC.WebServerTransporter;
```

Insert "AddWebServerTransporter" to ConfigureServices as follows:

```
 public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddWebServerTransporter(new Uri("http://localhost:53343"));
        }
```

Finally, insert "app.UseWebServerTransporter();" into the Configure method as follows:
```
 if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseWebServerTransporter();
```
Download the Transporter.exe

Start the Transporter server
```
./Transporter.exe http://localhost:53343
```
Start the razor sample server

```
dotnet run
```

Now open a web browser using the URL of the transporter (http://localhost:53343)

# Limitations
Most of the basic functions of the Kestrel web server are supported. However, there a limitations such as Cookie Authentication that is not yet supported