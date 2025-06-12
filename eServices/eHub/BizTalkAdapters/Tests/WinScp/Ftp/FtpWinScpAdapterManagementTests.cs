using System.Linq;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin;
using Microsoft.BizTalk.Adapter.Framework;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Ftp
{
	public class FtpWinScpAdapterManagementTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		[TestCase(ConfigType.ReceiveHandler)]
		[TestCase(ConfigType.TransmitHandler)]
		public void FtpWinScpAdapterManagement_ValidateReceiveHandler_Default(ConfigType type)
		{
			var target = new FtpWinScpAdapterManagement();
			var configXml = TestHelpers.GetDefaultXml(target.GetConfigSchema(type));

			var result = target.ValidateConfiguration(type, configXml.ToString());

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
		[TestCase(ConfigType.ReceiveHandler)]
		[TestCase(ConfigType.TransmitHandler)]
		public void FtpWinScpAdapterManagement_ValidateReceiveHandler_Values(ConfigType type)
		{
			var target = new FtpWinScpAdapterManagement();
			var configXml = TestHelpers.GetDefaultXml(target.GetConfigSchema(type));
			configXml.SetElementValue("InterfacesLogDir", "C:\\BizTalk\\Logs\\Interfaces");
			configXml.SetElementValue("StructuredLogDir", "C:\\Structured\\Logs\\Interfaces");
			configXml.SetElementValue("TerminateWaitLimit", "60000");
			configXml.SetElementValue("LogLevel", "Trace");
			configXml.SetElementValue("LogDir", "C:\\BizTalk\\Logs\\Handlers");
			configXml.SetElementValue("LogMaxSize", "1");
			configXml.SetElementValue("LogMaxCount", "20");

			var result = target.ValidateConfiguration(type, configXml.ToString());

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
		[TestCase(ConfigType.ReceiveLocation)]
		[TestCase(ConfigType.TransmitLocation)]
		public void FtpWinScpAdapterManagement_LocationType_CallGetResolvedSchemaFromResource(ConfigType type)
		{
			var target = new Mock<FtpWinScpAdapterManagement> { CallBase = true };
			target.Protected()
				.Setup<string>("GetResolvedSchemaFromResource", ItExpr.IsAny<string>())
				.Returns(string.Empty)
				.Verifiable();

			_ = target.Object.GetConfigSchema(type);

			target.Verify();
		}
	}
}
