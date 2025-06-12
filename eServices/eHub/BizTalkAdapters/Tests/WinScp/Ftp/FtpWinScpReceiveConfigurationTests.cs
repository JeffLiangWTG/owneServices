using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin;
using Microsoft.BizTalk.Adapter.Framework;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Ftp
{
	public class FtpWinScpReceiveConfigurationTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FtpWinScpReceiveConfiguration_SchemaDefaults()
		{
			var configXml = TestHelpers.GetDefaultXml(new FtpWinScpAdapterManagement().GetConfigSchema(ConfigType.ReceiveLocation));
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			var target = new FtpWinScpReceiveConfiguration(configDom);

			var actual = XElement.Parse(target.ToString()).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("uri", "ftpwinscpv2://:21"),
				("Server", string.Empty),
				("Port", "21"),
				("UserName", string.Empty),
				("Password", string.Empty),
				("Timeout", "90000"),
				("LogLevel", "Debug"),
				("LogMaxSize", "2"),
				("LogMaxCount", "10"),
				("LogFormat", "Flat"),
				("Folder", string.Empty),
				("FileMask", string.Empty),
				("FlagFile", string.Empty),
				("RenameBeforeDownload", string.Empty),
				("MoveBeforeDownload", string.Empty),
				("RenameAfterDownload", string.Empty),
				("MoveAfterDownload", string.Empty),
				("SortOrder", "None"),
				("pollingInterval", "1"),
				("pollingUnitOfMeasure", "Minutes"),
				("TransferErrorsDisablePort", "true"),
				("TransferErrorsRetryCount", "10"),
				("TransferErrorsRetryInterval", "5"),
				("MaximumConcurrentDownloads", "1"),
				("ReceiveProcessingTimeWarning", "300"),
				("DownloadExcludedFilesLimit", "100"),
				("Mode", "Passive"),
				("FtpsMode", "None"),
				("ValidateServerCert", "true"),
				("ClientCertPath", string.Empty),
				("EmptyFileOption", "Ignore"),
				("HostCertificateThumbprint", string.Empty)
			}));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FtpWinScpReceiveConfiguration_SingleLocation()
		{
			var configXml = TestHelpers.GetDefaultXml(new FtpWinScpAdapterManagement().GetConfigSchema(ConfigType.ReceiveLocation));
			configXml.SetElementValue("Server", "server");
			configXml.SetElementValue("Port", "2221");
			configXml.SetElementValue("UserName", "user");
			configXml.SetElementValue("Password", "password");
			configXml.SetElementValue("Folder", "folder");
			configXml.SetElementValue("FileMask", ".*");
			configXml.SetElementValue("EmptyFileOption", "Discard");
			configXml.SetElementValue("HostCertificateThumbprint", "3F:12:A9:8D:4B:6C:2F:7E:55:89:1A:BC:D4:EF:33:21:AA:77:88:99:00:66:55:44:11:22:33:44:55:66:77:88");
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			var target = new FtpWinScpReceiveConfiguration(configDom);

			var actual = XElement.Parse(target.ToString()).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("uri", "ftpwinscpv2://user@server:2221/folder/.*"),
				("Server", "server"),
				("Port", "2221"),
				("UserName", "user"),
				("Password", "password"),
				("Timeout", "90000"),
				("LogLevel", "Debug"),
				("LogMaxSize", "2"),
				("LogMaxCount", "10"),
				("LogFormat", "Flat"),
				("Folder", "folder"),
				("FileMask", ".*"),
				("FlagFile", string.Empty),
				("RenameBeforeDownload", string.Empty),
				("MoveBeforeDownload", string.Empty),
				("RenameAfterDownload", string.Empty),
				("MoveAfterDownload", string.Empty),
				("SortOrder", "None"),
				("pollingInterval", "1"),
				("pollingUnitOfMeasure", "Minutes"),
				("TransferErrorsDisablePort", "true"),
				("TransferErrorsRetryCount", "10"),
				("TransferErrorsRetryInterval", "5"),
				("MaximumConcurrentDownloads", "1"),
				("ReceiveProcessingTimeWarning", "300"),
				("DownloadExcludedFilesLimit", "100"),
				("Mode", "Passive"),
				("FtpsMode", "None"),
				("ValidateServerCert", "true"),
				("ClientCertPath", string.Empty),
				("EmptyFileOption", "Discard"),
				("HostCertificateThumbprint", "3F:12:A9:8D:4B:6C:2F:7E:55:89:1A:BC:D4:EF:33:21:AA:77:88:99:00:66:55:44:11:22:33:44:55:66:77:88")
			}));
		}
	}
}
