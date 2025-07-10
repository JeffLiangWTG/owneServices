using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ContainerEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestImportUniversalEventWithContainerAsTarget()
		{
			var consol = Factory.New<CommonConsol>();
			consol.FillWithValidTestData();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA";
			container.JC_ContainerJobID = "D000015";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(importXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Container 'AAAA'.".Trim(), serviceTaskLog.ToString());
				AssertMultilineASCIIEquals("Message Log Note", @"Linked Event to Container 'AAAA'.".Trim(), message.GetLogNoteText());

				container.Reload();
				var logs = container.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.GateInCode));
				AssertEquals(1, logs.Length);

				var log = logs[0];
				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join(System.Environment.NewLine, contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
Container Number - BBBB
MBOL Origin UNLOCO - AUSYD
MBOL Destination UNLOCO - SGSIN
".Trim(), actualContextItems);
			});
		}

		const string importXml = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingContainer</Type>
          <Key>D000015</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2016-05-01T07:00:00</EventTime>
    <EventType>GIN</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>BBBB</Value>
      </Context>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>AUSYD</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>SGSIN</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
	}
}
