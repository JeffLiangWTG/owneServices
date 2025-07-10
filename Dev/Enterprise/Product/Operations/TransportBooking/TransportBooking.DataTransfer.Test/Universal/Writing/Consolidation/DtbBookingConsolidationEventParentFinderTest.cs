using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	class DtbBookingConsolidationEventParentFinderTest : DtbBookingTestCaseWithFactory
	{
		public void TestEventsComingInWithJustDataContextIdentificationGetLinked()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			consolidation.KB_JobDirection = "PIC";

			// Set Transport Booking Parent
			var transportBookingParent = Factory.New<DummyWithDtbBooking>();
			consolidation.KB_ParentID = transportBookingParent.PK;
			consolidation.KB_ParentTableCode = transportBookingParent.TablePrefix;

			// Set transport Booking PackageJob
			var packageJob = consolidation.PackageJob;
			packageJob.KJ_JobID = "PJ00000001";
			packageJob.KJ_ParentID = consolidation.PK;
			packageJob.KJ_ParentTableCode = consolidation.TablePrefix;

			// Set transport Bookings
			consolidation.Bookings.AddNew();
			consolidation.Bookings.AddNew();

			Factory.Save();

			var message = GetQueuedUniversalEventMessage(string.Format(UniversalEventXML, consolidation.KB_JobID));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals(
					"Service Task Log", string.Format(@"
Linked Event to Transport Booking Consolidation {0}.
", consolidation.KB_JobID).Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", string.Format(@"
Linked Event to Transport Booking Consolidation {0}.
Field [DtbBookingConsolidation.KB_Status] has been updated to value [XOX] on Transport Booking Consolidation CM00000001.
", consolidation.KB_JobID).Trim(), message.GetLogNoteText());

				consolidation.Reload();
				var logs = consolidation.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CargoCheckinDiscrepancyCode));
				AssertEquals("[CCD] - Cargo Checkin Discrepancy event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertContains("Context Items on Event", "My Head Size - HUGE", actualContextItems);
			});
		}

		const string UniversalEventXML = @"
<UniversalEvent>
	<Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001157</Key>
        </DataSource>
      </DataSourceCollection>
      <DataTargetCollection>
        <DataTarget>
          <Type>TransportBookingConsolidation</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation</Name>
      </Company>
      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <EnterpriseID>EDI</EnterpriseID>
      <EventType>
        <Code>DSN</Code>
        <Description>Document Sent</Description>
      </EventType>
      <ServerID>DAT</ServerID>
      <TriggerDescription>MAWB printed</TriggerDescription>
      <TriggerReference>MAWB Printed</TriggerReference>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<ContextCollection>
			<Context>
				<Type>MyHeadSize</Type>
				<Value>HUGE</Value>
			</Context>
		</ContextCollection>

		<AdditionalFieldsToUpdateCollection>
			<AdditionalFieldsToUpdate>
				<Type>DtbBookingConsolidation.KB_Status</Type>
				<Value>XOX</Value>
			</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>";

		protected IEDIMessage GetQueuedUniversalEventMessage(string messageText)
		{
			var message = GetQueuedUniversalDataMessage();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_MessageText = messageText.Trim();

			return message;
		}

		IEDIMessage GetQueuedUniversalDataMessage()
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			return message;
		}
	}
}
