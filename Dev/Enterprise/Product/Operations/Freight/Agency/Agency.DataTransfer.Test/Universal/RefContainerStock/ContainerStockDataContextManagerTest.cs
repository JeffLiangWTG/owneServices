using System.IO;
using Enterprise.Freight.Agency.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ContainerStockDataContextManager))]
	sealed class ContainerStockDataContextManagerTest : DataContextManagerTestCase<ContainerStockDataContextManager, RefContainerStock>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageLinksToCorrectContainer()
		{
			var container = Factory.NewWithValidTestData<RefContainerStock>();
			container.R6_ContainerNum = "TEST1111117";
			Factory.SaveForTesting();
			var message = GetQueuedUniversalEventMessage(File.ReadAllText(UniversalEventFilePath));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Container TEST1111117.", serviceTaskLog.ToString());
			AssertContains("Message Log Note", @"Linked Event to Container TEST1111117.", message.GetLogNoteText());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageDoesntLinkToUnmatchedContainer()
		{
			var container = Factory.NewWithValidTestData<RefContainerStock>();
			container.R6_ContainerNum = "TEST2222227";
			Factory.SaveForTesting();
			var message = GetQueuedUniversalEventMessage(File.ReadAllText(UniversalEventFilePath));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertContains("Message Log Note", @"Message Discarded", message.GetLogNoteText());
		}

		string UniversalEventFilePath => BaseSourcePath + @"Enterprise\Product\Operations\Freight\Agency\Agency.DataTransfer.Test\Universal\RefContainerStock\TestFiles\UniversalEvent.xml";
	}
}
