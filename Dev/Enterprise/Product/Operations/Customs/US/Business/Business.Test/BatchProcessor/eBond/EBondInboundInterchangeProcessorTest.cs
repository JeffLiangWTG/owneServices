using System.Linq;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EBondInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var document = new XmlDocument();
			document.Load(GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.US.Business.Testing.BatchProcessor.Testing.EBondInterchangeWithMultipleEventsBody.xml"));

			var interchange = EBondMssageProcessorFactoryTest.CreateInterchangeForEBondMessage(Factory);
			interchange.EI_BodyText = document.InnerXml;

			Factory.Save();

			var logInformation = new LoggingInformation();
			var processor = new EBondInboundInterchangeProcessor(logInformation);

			processor.ExecuteBatch();

			CombineAssertions(() =>
			{
				var expectedLog = "Interchange 'EBond190319054146' has been processed successfully.";
				var actualLog = string.Join(System.Environment.NewLine, logInformation.UserLogStrings.Cast<string>().Select(c => c.Trim()));
				AssertEquals("Log", expectedLog, actualLog);

				var messages = interchange.ContainedMessages.OfType<EBondEDIMessage>().ToArray();
				AssertEquals("Should create two messages.", 2, messages.Length);
				Assert("EM_ReceiveTransmit", messages.All(c => c.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive));
				Assert("EM_Status", messages.All(c => c.EM_Status == EDIInterchangeStatusList.Codes.Queued));

				var expectedMessageContents = document.GetElementsByTagName("UniversalEvent").Cast<XmlNode>().Select(c => c.OuterXml);
				var actualMessageContents = messages.Select(c => c.EM_MessageText);
				AssertContainsExactElementsInAnyOrder("EM_MessageText", expectedMessageContents, actualMessageContents);
			});
		}
	}
}
