using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WarehouseVASOrderEventParentFinderTest : WhsUniversalTestCase
	{
		#region TestEventsComingInWithJustDataContextIdentificationGetLinked

		public void TestEventsComingInWithJustDataContextIdentificationGetLinked()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var vasOrderBO = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(string.Format(UniversalEventXML, vasOrderBO.WVO_JobID));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals(
					"Service Task Log", string.Format(@"
Linked Event to Warehouse VAS Order {0}.
", vasOrderBO.WVO_JobID).Trim(), serviceTaskLog.ToString());

				vasOrderBO.Reload();
				var logs = vasOrderBO.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CargoCheckinDiscrepancyCode));
				AssertEquals("[CCD] - Cargo Checkin Discrepancy event count", 1, logs.Length);
				var log = logs[0];
			});
		}

		#endregion

		#region UniversalEventXML

		const string UniversalEventXML = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>WarehouseVASOrder</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventType>CCD</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>

	<ContextCollection>
	  <Context>
	    <Type>NeedContext</Type>
	    <Value>OrAWarningIsAdded</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>";

		#endregion

		#region Implementation

		protected IEDIMessage GetQueuedUniversalEventMessage(string messageText)
		{
			var message = GetQueuedUniversalDataMessage();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_MessageText = messageText.Trim();

			return message;
		}

		IEDIMessage GetQueuedUniversalDataMessage()
		{
			var message = Factory.BOFactory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			return message;
		}
		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);

		#endregion
	}
}