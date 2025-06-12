using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin;
using Microsoft.BizTalk.Adapter.Framework;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Sftp
{
	public class SftpWinScpTransmitConfigurationTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SftpWinScpTransmitConfiguration_SchemaDefaults()
		{
			var configXml = TestHelpers.GetDefaultXml(new SftpWinScpAdapterManagement().GetConfigSchema(ConfigType.TransmitLocation));
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			var target = new SftpWinScpTransmitConfiguration(configDom);

			var actual = XElement.Parse(target.ToString()).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("uri", "sftpwinscp://:22/%MessageID%.xml"),
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
				("TargetFileName", "%MessageID%.xml"),
				("FlagFile", string.Empty),
				("TemporaryFolder", string.Empty),
				("TemporaryFileName", string.Empty),
				("ConnectionLimit", "1"),
				("UseContextConfiguration", "false"),
				("UseRealPath", "true")
			}));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SftpWinScpTransmitConfiguration_SingleLocation()
		{
			var configXml = TestHelpers.GetDefaultXml(new SftpWinScpAdapterManagement().GetConfigSchema(ConfigType.TransmitLocation));
			configXml.Element("Server").Value = "server";
			configXml.Element("Port").Value = "2222";
			configXml.Element("UserName").Value = "user";
			configXml.Element("Password").Value = "password";
			configXml.Element("Folder").Value = "folder";
			configXml.Element("TargetFileName").Value = "%OverrideFilename%";
			configXml.Element("UseRealPath").Value = "false";
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			var target = new SftpWinScpTransmitConfiguration(configDom);

			Assert.That(target.Location, Is.InstanceOf<WinScpLocation>()
				.With.Property(nameof(WinScpLocation.Server)).EqualTo("server")
				.And.Property(nameof(WinScpLocation.Port)).EqualTo(2222)
				.And.Property(nameof(WinScpLocation.UserName)).EqualTo("user")
				.And.Property(nameof(WinScpLocation.Password)).EqualTo("password")
				.And.Property(nameof(WinScpLocation.Folder)).EqualTo("folder")
				.And.Property(nameof(WinScpLocation.FileName)).EqualTo("%OverrideFilename%")
			);

			var actual = XElement.Parse(target.ToString()).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("uri", "sftpwinscp://user@server:2222/folder/%OverrideFilename%"),
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
				("TargetFileName", "%OverrideFilename%"),
				("FlagFile", string.Empty),
				("TemporaryFolder", string.Empty),
				("TemporaryFileName", string.Empty),
				("ConnectionLimit", "1"),
				("UseContextConfiguration", "false"),
				("UseRealPath", "false")
			}));
		}
	}
}
