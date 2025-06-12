using System.Xml;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Common
{
	public class WinScpTransmitterConfigurationTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		[TestCase("%MessageId%", "winscp://[ContextUser]@[ContextServer]:2221/[ContextFolder]/%MessageId%")]
		[TestCase("%MessageID%.msg", "winscp://[ContextUser]@[ContextServer]:2221/[ContextFolder]/%MessageID%.msg")]
		[TestCase("%OverrideFilename%.%datetime_bts2000%.txt", "winscp://[ContextUser]@[ContextServer]:2221/[ContextFolder]/%OverrideFilename%.%datetime_bts2000%.txt")]
		[TestCase("%DestinationPartyQualifier%.%datetime_bts2000%", "winscp://[ContextUser]@[ContextServer]:2221/[ContextFolder]/%DestinationPartyQualifier%.%datetime_bts2000%")]
		public void TestConstructor_UseContextConfiguration(string fileName, string expectedUri)
		{
			var configXml = new XmlDocument();
			configXml.LoadXml(@$"
<Config>
	<Server>server</Server>
	<Folder>in</Folder>
	<Port>2221</Port>
	<UseContextConfiguration>true</UseContextConfiguration>
	<TargetFileName>{fileName}</TargetFileName>
</Config>");
			var config = new WinScpTransmitConfiguration(configXml);

			Assert.That(config.Uri, Is.EqualTo(expectedUri));
		}
	}
}
