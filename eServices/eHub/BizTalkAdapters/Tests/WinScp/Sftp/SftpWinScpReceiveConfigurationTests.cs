using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Transferrer.UI;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin;
using Microsoft.BizTalk.Adapter.Framework;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Sftp
{
	public class SftpWinScpReceiveConfigurationTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SftpWinScpReceiveConfiguration_SchemaDefaults()
		{
			var configXml = TestHelpers.GetDefaultXml(new SftpWinScpAdapterManagement().GetConfigSchema(ConfigType.ReceiveLocation));
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			var target = new SftpWinScpReceiveConfiguration(configDom);

			var actual = XElement.Parse(target.ToString()).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("uri", "sftpwinscp://:22"),
				("Server", string.Empty),
				("Port", "22"),
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
				("EmptyFileOption", "Ignore"),
				("UseRealPath", "true")
			}));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SftpWinScpReceiveConfiguration_SingleLocation()
		{
			var configXml = TestHelpers.GetDefaultXml(new SftpWinScpAdapterManagement().GetConfigSchema(ConfigType.ReceiveLocation));
			configXml.Element("Server").Value = "server";
			configXml.Element("Port").Value = "2222";
			configXml.Element("UserName").Value = "user";
			configXml.Element("Password").Value = "password";
			configXml.Element("Folder").Value = "folder";
			configXml.Element("FileMask").Value = ".*";
			configXml.Element("EmptyFileOption").Value = "Discard";
			configXml.Element("UseRealPath").Value = "false";
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			var target = new SftpWinScpReceiveConfiguration(configDom);

			Assert.That(target.Locations.FirstOrDefault(), Is.InstanceOf<WinScpLocation>()
				.With.Property(nameof(WinScpLocation.Server)).EqualTo("server")
				.And.Property(nameof(WinScpLocation.Port)).EqualTo(2222)
				.And.Property(nameof(WinScpLocation.UserName)).EqualTo("user")
				.And.Property(nameof(WinScpLocation.Password)).EqualTo("password")
				.And.Property(nameof(WinScpLocation.Folder)).EqualTo("folder")
				.And.Property(nameof(WinScpLocation.FileName)).EqualTo(".*")
			);

			var actual = XElement.Parse(target.ToString()).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("uri", "sftpwinscp://user@server:2222/folder/.*"),
				("Server", "server"),
				("Port", "2222"),
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
				("EmptyFileOption", "Discard"),
				("UseRealPath", "false")
			}));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SftpWinScpReceiveConfiguration_MultipleLocations()
		{
			var configXml = TestHelpers.GetDefaultXml(new SftpWinScpAdapterManagement().GetConfigSchema(ConfigType.ReceiveLocation));
			configXml.Element("MultipleLocations").Value =
				"winscp://user1@server:2222/path1/*.xml\r\n" +
				"winscp://user1@server:2222/path2/*.xml\r\n" +
				"winscp://user2@server:2222\r\n";
			configXml.Element("MultipleLocationsCredentials").Value =
				"user1:password1@server:2222\r\n" +
				"user2:password2@server:2222\r\n";
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			var target = new SftpWinScpReceiveConfiguration(configDom);

			Assert.That(target.Locations.Select(l => (l.GetUri(), l.Password)),
				Is.EquivalentTo(new[]
				{
					(Uri: "winscp://user1@server:2222/path1/*.xml", Password: "password1"),
					(Uri: "winscp://user1@server:2222/path2/*.xml", Password: "password1"),
					(Uri: "winscp://user2@server:2222", Password: "password2"),
				}));

			var actual = XElement.Parse(target.ToString()).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("uri", "sftpwinscp://[MULTIPLE]@server:2222/[MULTIPLE]/[MULTIPLE]"),
				("Server", string.Empty),
				("Port", "22"),
				("UserName", string.Empty),
				("Password", string.Empty),
				("Timeout", "90000"),
				("LogLevel", "Debug"),
				("LogMaxSize", "2"),
				("LogMaxCount", "10"),
				("LogFormat", "Flat"),
				("MultipleLocations",
					"winscp://user1@server:2222/path1/*.xml\n" +
					"winscp://user1@server:2222/path2/*.xml\n" +
					"winscp://user2@server:2222"),
				("MultipleLocationsCredentials",
					"user1:password1@server:2222\n" +
					"user2:password2@server:2222"),
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
				("EmptyFileOption", "Ignore"),
				("UseRealPath", "true")
			}));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SftpWinScpReceiveConfiguration_MultipleLocationsWithPassword_NoUIPrompt()
		{
			var configXml = TestHelpers.GetDefaultXml(new SftpWinScpAdapterManagement().GetConfigSchema(ConfigType.ReceiveLocation));
			configXml.Element("MultipleLocations").Value =
				"winscp://user1:password1@server:2222/path1/*.xml\r\n" +
				"winscp://user1:password1@server:2222/path2/*.xml\r\n" +
				"winscp://user2:password2@server:2222\r\n";
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			var managementUIMock = new Mock<IAdapterManagementUI>();
			var target = new SftpWinScpReceiveConfiguration(configDom, managementUIMock.Object);

			managementUIMock.Verify(x => x.GetPasswordPrompt(It.IsAny<string>()), Times.Never);

			Assert.That(target.Locations.Select(l => (l.GetUri(), l.Password)),
				Is.EquivalentTo(new[]
				{
					(Uri: "winscp://user1@server:2222/path1/*.xml", Password: "password1"),
					(Uri: "winscp://user1@server:2222/path2/*.xml", Password: "password1"),
					(Uri: "winscp://user2@server:2222", Password: "password2"),
				}));

			var actual = XElement.Parse(target.ToString()).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("uri", "sftpwinscp://[MULTIPLE]@server:2222/[MULTIPLE]/[MULTIPLE]"),
				("Server", string.Empty),
				("Port", "22"),
				("UserName", string.Empty),
				("Password", string.Empty),
				("Timeout", "90000"),
				("LogLevel", "Debug"),
				("LogMaxSize", "2"),
				("LogMaxCount", "10"),
				("LogFormat", "Flat"),
				("MultipleLocations",
					"winscp://user1@server:2222/path1/*.xml\n" +
					"winscp://user1@server:2222/path2/*.xml\n" +
					"winscp://user2@server:2222"),
				("MultipleLocationsCredentials",
					"user1:password1@server:2222\n" +
					"user2:password2@server:2222"),
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
				("EmptyFileOption", "Ignore"),
				("UseRealPath", "true")
			}));
		}
	}
}
