using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using Microsoft.Samples.BizTalk.Adapter.Common;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Common
{
	public class WinScpReceiveConfigurationTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestConstructor_LocationFromClientRegistration()
		{
			var conString = "eHubTransactionsContext";
			var regType = "GLSHK";

			var configXml = new XmlDocument();
			configXml.LoadXml($@"
<Config>
	<Server>server</Server>
	<pollingInterval>1</pollingInterval>
	<pollingUnitOfMeasure>Seconds</pollingUnitOfMeasure>
	<RegistrationConnectionStringName>{conString}</RegistrationConnectionStringName>
	<RegistrationType>{regType}</RegistrationType>
</Config>");

			var config = new WinScpReceiveConfiguration(configXml);
			Assert.That(config.Uri, Is.EqualTo("winscp://[GLSHK]@clientregistration/[eHubTransactionsContext]"));
		}

		private const string MoveRenameNotSupported = "MoveBeforeDownload and RenameBeforeDownload are not supported when MaximumConcurrentDownloads > 1.";
		private static IEnumerable<TestCaseData> WinScpReceiveConfiguration_AdapterException_TestCaseSource = new TestCaseData[] {
			new(new[] { ("MaximumConcurrentDownloads", "10"), ("RenameBeforeDownload", "/") }, MoveRenameNotSupported),
			new(new[] { ("MaximumConcurrentDownloads", "10"), ("MoveBeforeDownload", "/") }, MoveRenameNotSupported),
			new(new[] { ("MaximumConcurrentDownloads", "10") }, null),
		};

		[TestCaseSource(nameof(WinScpReceiveConfiguration_AdapterException_TestCaseSource))]
		[Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void WinScpReceiveConfiguration_AdapterException((string Key, string Value)[] config, string message)
		{
			var configXml = new XElement("Config",
				new XElement("Server", string.Empty),
				new XElement("Port", "22"),
				new XElement("UserName", string.Empty),
				new XElement("Password", string.Empty),
				new XElement("Timeout", "90000"),
				new XElement("LogLevel", "Debug"),
				new XElement("LogMaxSize", "2"),
				new XElement("LogMaxCount", "10"),
				new XElement("Folder", string.Empty),
				new XElement("FileMask", string.Empty),
				new XElement("FlagFile", string.Empty),
				new XElement("RenameBeforeDownload", string.Empty),
				new XElement("MoveBeforeDownload", string.Empty),
				new XElement("RenameAfterDownload", string.Empty),
				new XElement("MoveAfterDownload", string.Empty),
				new XElement("SortOrder", "None"),
				new XElement("pollingInterval", "1"),
				new XElement("pollingUnitOfMeasure", "Minutes"),
				new XElement("TransferErrorsDisablePort", "true"),
				new XElement("TransferErrorsRetryCount", "10"),
				new XElement("TransferErrorsRetryInterval", "5"),
				new XElement("MaximumConcurrentDownloads", "1"),
				new XElement("ReceiveProcessingTimeWarning", "300"),
				new XElement("DownloadExcludedFilesLimit", "100"),
				new XElement("EmptyFileOption", "Ignore"),
				new XElement("UseRealPath", "true")
			);

			foreach (var item in config)
			{
				configXml.Element(item.Key).Value = item.Value;
			}
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			if (message is null)
			{
				Assert.DoesNotThrow(() => new WinScpReceiveConfiguration(configDom));
			}
			else
			{
				Assert.Throws(Is.TypeOf<AdapterException>().With.Message.EqualTo(message), () => new WinScpReceiveConfiguration(configDom));
			}
		}
	}
}
