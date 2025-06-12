using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin;
using Microsoft.BizTalk.Adapter.Framework;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Ftp
{
	public class FtpWinScpTransmitConfigurationTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FtpWinScpTransmitConfiguration_SchemaDefaults()
		{
			var configXml = TestHelpers.GetDefaultXml(new FtpWinScpAdapterManagement().GetConfigSchema(ConfigType.TransmitLocation));
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			var target = new FtpWinScpTransmitConfiguration(configDom);

			var actual = XElement.Parse(target.ToString()).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("uri", "ftpwinscpv2://:21/%MessageID%.xml"),
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
				("TargetFileName", "%MessageID%.xml"),
				("FlagFile", string.Empty),
				("TemporaryFolder", string.Empty),
				("TemporaryFileName", string.Empty),
				("ConnectionLimit", "1"),
				("Mode", "Passive"),
				("FtpsMode", "None"),
				("ValidateServerCert", "true"),
				("ClientCertPath", string.Empty),
				("UseContextConfiguration", "false"),
				("HostCertificateThumbprint", string.Empty)
			}));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FtpWinScpTransmitConfiguration_SingleLocation()
		{
			var configXml = TestHelpers.GetDefaultXml(new FtpWinScpAdapterManagement().GetConfigSchema(ConfigType.TransmitLocation));
			configXml.SetElementValue("Server", "server");
			configXml.SetElementValue("Port", "2221");
			configXml.SetElementValue("UserName", "user");
			configXml.SetElementValue("Password", "password");
			configXml.SetElementValue("Folder", "folder");
			configXml.SetElementValue("TargetFileName", "%OverrideFilename%");
			configXml.SetElementValue("HostCertificateThumbprint", "3F:12:A9:8D:4B:6C:2F:7E:55:89:1A:BC:D4:EF:33:21:AA:77:88:99:00:66:55:44:11:22:33:44:55:66:77:88");
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			var target = new FtpWinScpTransmitConfiguration(configDom);

			var actual = XElement.Parse(target.ToString()).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("uri", "ftpwinscpv2://user@server:2221/folder/%OverrideFilename%"),
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
				("TargetFileName", "%OverrideFilename%"),
				("FlagFile", string.Empty),
				("TemporaryFolder", string.Empty),
				("TemporaryFileName", string.Empty),
				("ConnectionLimit", "1"),
				("Mode", "Passive"),
				("FtpsMode", "None"),
				("ValidateServerCert", "true"),
				("ClientCertPath", string.Empty),
				("UseContextConfiguration", "false"),
				("HostCertificateThumbprint", "3F:12:A9:8D:4B:6C:2F:7E:55:89:1A:BC:D4:EF:33:21:AA:77:88:99:00:66:55:44:11:22:33:44:55:66:77:88")
			}));
		}
	}
}
