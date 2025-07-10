using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ShipmentEventContextReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestAdditionalFieldsToUpdateCollectionUpdatesProperlyOnFieldsAndRelatedObjects()
		{
			var shipment = new BusinessObjectFactory().NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001016";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "JACK014N73RN";

			shipment.Factory.Save();

			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);

			var message = GetQueuedUniversalEventMessage(resourceRetriever.GetString(ForwardingShipmentDataContextManagerTest.GetResourcePathFor("UniversalEventWithAdditionalFieldsToUpdate.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Shipment S00001016 (House Bill='JACK014N73RN').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Shipment S00001016 (House Bill='JACK014N73RN').
Field [JobShipment.docsandcartage.JP_DeliveryCartageCompleted] has been updated to value [2011-08-12] on Shipment S00001016 (House Bill='JACK014N73RN').
Field [ForwardingShipment.JS_InterimReceipt] has been updated to value [JARRA12345] on Shipment S00001016 (House Bill='JACK014N73RN').
".Trim(), message.GetLogNoteText());

				shipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
				var logs = shipment.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOutCode));
				AssertEquals("[GOU] - Gate Out", 1, logs.Length);
				var log = (StmALog)logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"Crazy Horse - Eat at Joes", actualContextItems);

				AssertEquals("shipment.DocsAndCartage.JP_DeliveryCartageCompleted", new ZDateTime(2011, 8, 12), shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
				AssertEquals("shipment.JS_InterimReceipt", "JARRA12345", shipment.JS_InterimReceipt);
			});
		}
	}
}
