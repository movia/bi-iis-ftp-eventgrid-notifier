# IIS FTP Event Grid Notifier

A small implementation of IIS Custom Log Provider that streams events to Azure Event Grid.

## Use Case

You have some legacy on-prem FTP / FTPS server running on IIS 7.5 or newer (e.g. Windows Server 2008 R2 or newer). 3rd parties are dumping file on it, and you want Azure Data Factory, an Azure Function or other event-driven components to fire, once a file has been uploaded to the server.

## Build

Build the `IisFtpEventGridNotifier.sln` with Visual Studio. If you are biulding this on a machine which have not installed the IIS FTP Service, then copy the `Microsoft.Web.FtpServer.dll` from your FTP server and put it into the `Reference Assemblies` folder.

## Installation and Configuration

1. Prerequisites: You have already an Event Grid Topic created in Azure, and have the corresponding url of the *Event Grid End Point* and *Event Grid Key*.

2. Copy the resulting `IisFtpEventGridNotifier.dll` to the server with IIS and install it into the [Global Assembly Cache](https://docs.microsoft.com/en-us/dotnet/framework/app-domains/gac), e.g. using the [GAC Util](https://docs.microsoft.com/en-us/dotnet/framework/tools/gacutil-exe-gac-tool) that ships with Visual Studio:

``.\gacutil.exe /if .\Release\IisFtpEventGridNotifier.dll``

:point_right: Rembember to execute this from an evaluated prompt (e.g. Run as Administrator)

3. Create a Json-file named `secrets.json` and fill it with your configuration:

```json
{
  "logFilePath": "C:\\logs\\IisFtpEventGridNotifier.log", // Set to empty to disable logging
  "eventGridEndPoint": "<your event grid topic>",
  "eventGridKey": "<your event grid key>",
  "eventGridSubjectPrefix": "<prefix to event subject>" // E.g. if you have multiple servers, you can use this to distinguish them
}
```

4. Run `register-iis.ps1` to install and activate the Custom Log Provider for the site named `Default FTP Site` (ajust the script id your FTP site is named differently).

:point_right: Rembember to execute this from an evaluated prompt (e.g. Run as Administrator)

5. Enjoy!

## Example

The below example shows the structure of the events 

```json
{
  "data": {
    "path": "/dir/data.csv",
    "size": 563459,
    "statusCode": 226,
    "username": "SomeUser"
  },
  "eventType": "FileAvailable",
  "id": "971e8b7d-2a6c-418f-9d7d-308794dd2678",
  "subject": "prefix/dir/data.csv",
  "dataVersion": "",
  "metadataVersion": "1",
  "eventTime": "2022-01-11T10:39:23",
  "topic": "/subscriptions/xxx/resourceGroups/eventgrid-rg/providers/Microsoft.EventGrid/topics/event-grid-topic"
}
```

Credits: The overall approach was based on the guide from Microsoft: [How to Use Managed Code (C#) to Create a Simple FTP Logging Provider](https://docs.microsoft.com/en-us/iis/develop/developing-for-ftp/how-to-use-managed-code-c-to-create-a-simple-ftp-logging-provider).
