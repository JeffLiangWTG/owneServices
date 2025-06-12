using System.Linq;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin;
using Microsoft.BizTalk.Adapter.Framework;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Sftp
{
	public class SftpWinScpAdapterManagementTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SftpWinScpAdapterManagement_ValidateReceiveHandler_Default()
		{
			var target = new SftpWinScpAdapterManagement();
			var configXml = TestHelpers.GetDefaultXml(target.GetConfigSchema(ConfigType.ReceiveHandler));

			var result = target.ValidateConfiguration(ConfigType.ReceiveHandler, configXml.ToString());

			var actual = XElement.Parse(result).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("InterfacesLogDir", "C:\\Logs\\BizTalk\\Interfaces"),
				("StructuredLogDir", "C:\\Logs\\Structured\\Interfaces"),
				("TerminateWaitLimit", "60000"),
				("LogLevel", "Debug"),
				("LogDir", "C:\\Logs\\BizTalk\\Handlers"),
				("LogMaxSize", "2"),
				("LogMaxCount", "10")
			}));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SftpWinScpAdapterManagement_ValidateReceiveHandler_Values()
		{
			var target = new SftpWinScpAdapterManagement();
			var configXml = TestHelpers.GetDefaultXml(target.GetConfigSchema(ConfigType.ReceiveHandler));
			configXml.Element("InterfacesLogDir").Value = "C:\\BizTalk\\Logs\\Interfaces";
			configXml.Element("StructuredLogDir").Value = "C:\\Structured\\Logs\\Interfaces";
			configXml.Element("TerminateWaitLimit").Value = "60000";
			configXml.Element("LogLevel").Value = "Trace";
			configXml.Element("LogDir").Value = "C:\\BizTalk\\Logs\\Handlers";
			configXml.Element("LogMaxSize").Value = "1";
			configXml.Element("LogMaxCount").Value = "20";

			var result = target.ValidateConfiguration(ConfigType.ReceiveHandler, configXml.ToString());

			var actual = XElement.Parse(result).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("InterfacesLogDir", "C:\\BizTalk\\Logs\\Interfaces"),
				("StructuredLogDir", "C:\\Structured\\Logs\\Interfaces"),
				("TerminateWaitLimit", "60000"),
				("LogLevel", "Trace"),
				("LogDir", "C:\\BizTalk\\Logs\\Handlers"),
				("LogMaxSize", "1"),
				("LogMaxCount", "20")
			}));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SftpWinScpAdapterManagement_ValidateTransmitHandler_Default()
		{
			var target = new SftpWinScpAdapterManagement();
			var configXml = TestHelpers.GetDefaultXml(target.GetConfigSchema(ConfigType.TransmitHandler));

			var result = target.ValidateConfiguration(ConfigType.TransmitHandler, configXml.ToString());

			var actual = XElement.Parse(result).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("InterfacesLogDir", "C:\\Logs\\BizTalk\\Interfaces"),
				("StructuredLogDir", "C:\\Logs\\Structured\\Interfaces"),
				("TerminateWaitLimit", "60000"),
				("LogLevel", "Debug"),
				("LogDir", "C:\\Logs\\BizTalk\\Handlers"),
				("LogMaxSize", "2"),
				("LogMaxCount", "10")
			}));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SftpWinScpAdapterManagement_ValidateTransmitHandler_Values()
		{
			var target = new SftpWinScpAdapterManagement();
			var configXml = TestHelpers.GetDefaultXml(target.GetConfigSchema(ConfigType.TransmitHandler));
			configXml.Element("InterfacesLogDir").Value = "C:\\BizTalk\\Logs\\Interfaces";
			configXml.Element("StructuredLogDir").Value = "C:\\Structured\\Logs\\Interfaces";
			configXml.Element("TerminateWaitLimit").Value = "60000";
			configXml.Element("LogLevel").Value = "Trace";
			configXml.Element("LogDir").Value = "C:\\BizTalk\\Logs\\Handlers";
			configXml.Element("LogMaxSize").Value = "1";
			configXml.Element("LogMaxCount").Value = "20";

			var result = target.ValidateConfiguration(ConfigType.TransmitHandler, configXml.ToString());

			var actual = XElement.Parse(result).Elements().Select(x => (x.Name, x.Value));
			Assert.That(actual, Is.EquivalentTo(new (XName Name, string Value)[]
			{
				("InterfacesLogDir", "C:\\BizTalk\\Logs\\Interfaces"),
				("StructuredLogDir", "C:\\Structured\\Logs\\Interfaces"),
				("TerminateWaitLimit", "60000"),
				("LogLevel", "Trace"),
				("LogDir", "C:\\BizTalk\\Logs\\Handlers"),
				("LogMaxSize", "1"),
				("LogMaxCount", "20")
			}));
		}
	}
}
