# CargoWise.eHub.BizTalkAdapters

In house developed BizTalk adapters.

**CargoWise.eHub.BizTalkAdapters.Common**, **CargoWise.eHub.BizTalkAdapters.Common.UI**  
Base classes and helpers for FTPEx, SFTPEx and WSHTTPEx adapters.  
See [CargoWise.eHub.BizTalkAdapters.Common](Common\readme.md) for details.

**CargoWise.eHub.BizTalkAdapters.Transferrer.Core**, **CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter**  
New framework for creating adapters with handlers that do not have any BizTalk dependencies.  
See [CargoWise.eHub.BizTalkAdapters.Transferrer.Core](Transferrer.Core\readme.md) for details.

**CargoWise.eHub.BizTalkAdapters.FtpEx**, **CargoWise.eHub.BizTalkAdapters.FtpEx.Admin**  
Send and receive with external FTP servers. Providers more options than built-in FTP adapter moving and renaming on files on the server and better logging.

**CargoWise.eHub.BizTalkAdapters.FtpExPolling**, **CargoWise.eHub.BizTalkAdapters.FtpExPolling.Admin**  
Must be configured as a two-way send/receive port to use credentials contained in a message received from BizTalk to poll external FTPEx sites.  
Required to allow credentials to be stored in database instead of preconfigured in BizTalk.

**CargoWise.eHub.BizTalkAdapters.HttpEx**, **CargoWise.eHub.BizTalkAdapters.HttpEx.Admin**  
Send to external HTTP servers. Allows interface specific categorization of HTTP response codes as success, temporary error, and failure.

**CargoWise.eHub.BizTalkAdapters.IntegrationTests**  
Manually tests adapters against live servers. Servers require manual configuration.

**CargoWise.eHub.BizTalkAdapters.Loopback**, **CargoWise.eHub.BizTalkAdapters.Loopback.Admin**  
It's a loopback. That's what it does.

**CargoWise.eHub.BizTalkAdapters.Null**, **CargoWise.eHub.BizTalkAdapters.Null**  
For discarding BizTalk messages. By default will log message headers and contents to rolling log files.

**CargoWise.eHub.BizTalkAdapters.SftpEx**, **CargoWise.eHub.BizTalkAdapters.SftpEx.Admin**  
Send and receive with external SFTP servers. Was needed in BizTalk 2008 because there was no built-in one but is still more configurable and has better logging than the current built-in one.

**CargoWise.eHub.BizTalkAdapters.Transferrer.Ftp**  
Refactored FTP handlers.

**CargoWise.eHub.BizTalkAdapters.WSHttpEx**, **CargoWise.eHub.BizTalkAdapters.WSHttpEx.Admin**
For sending to external SOAP interfaces. Allows client certificates to be provided in BizTalk message context headers.

**FluentFTP**  
Open-source FTP client library. Modified to allow multi-threaded logging.

**Microsoft.Samples.BizTalk.Adapter.Common**  
Adapter framework from BizTalk SDK.

**Renci.SshNet**  
Open-source SFTP client library. Modified to allow multi-threaded logging.
## Installation

Installation is done with an MSI installer created in solution [$eServices/BizTalkAdapters/CargoWise.eHub.BizTalkAdapters.Setup.sln](CargoWise.eHub.BizTalkAdapters.Setup.sln) (requires VS2013)  
See CargoWise.eHub.BizTalkAdapters.Setup's [readme.md](Setup\readme.md) for details.

