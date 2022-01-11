$systemRoot = [environment]::GetEnvironmentVariable("systemroot")
$Env:Path += ";$systemRoot\System32\inetsrv\"

$config = gc "secrets.json" | Out-String | ConvertFrom-Json 
$config

$fullName=[System.Reflection.Assembly]::LoadWithPartialName("IisFtpEventGridNotifier").FullName
$fullName

appcmd.exe set config -section:system.ftpServer/providerDefinitions /-"[name='IisFtpEventGridNotifier']" /commit:apphost
appcmd.exe set config -section:system.ftpServer/providerDefinitions /-"activation.[name='IisFtpEventGridNotifier']" /commit:apphost

appcmd.exe set config -section:system.ftpServer/providerDefinitions /+"[name='IisFtpEventGridNotifier',type='IisFtpEventGridNotifier.IisFtpEventGridNotifier, $fullName']" /commit:apphost
appcmd.exe set config -section:system.ftpServer/providerDefinitions /+"activation.[name='IisFtpEventGridNotifier']" /commit:apphost
appcmd.exe set config -section:system.ftpServer/providerDefinitions /+"activation.[name='IisFtpEventGridNotifier'].[key='logFilePath',value='$($config.logFilePath)']" /commit:apphost
appcmd.exe set config -section:system.ftpServer/providerDefinitions /+"activation.[name='IisFtpEventGridNotifier'].[key='eventGridEndPoint',value='$($config.eventGridEndPoint)']" /commit:apphost
appcmd.exe set config -section:system.ftpServer/providerDefinitions /+"activation.[name='IisFtpEventGridNotifier'].[key='eventGridKey',value='$($config.eventGridKey)']" /commit:apphost
appcmd.exe set config -section:system.ftpServer/providerDefinitions /+"activation.[name='IisFtpEventGridNotifier'].[key='eventGridSubjectPrefix',value='$($config.eventGridSubjectPrefix)']" /commit:apphost

appcmd.exe set site "Default FTP Site" /+"ftpServer.customFeatures.providers.[name='IisFtpEventGridNotifier',enabled='true']" /commit:apphost
