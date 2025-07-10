using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.eManifest.Business;
using Enterprise.eManifest.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.eManifest.DataTransfer.Testing
{
	[TestedType(typeof(eManifestLineDataContextManager))]
	[SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification hides desired base class")]
	class eManifestLineDataContextManagerTest : ShipmentDataContextManagerTestCase<eManifestLineDataContextManager, SupplierBookingLine>
	{
		public void TestEventComingIn()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "BIG BAD GIANT";
			consignor.OH_RL_NKClosestPort = "USBUM"; // Butler,Missouri.

			var shipment = Factory.BOFactory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009999";

			var header = Factory.New<SupplierBookingHeader>();
			header.DH_SupplierReference = "FEEFIFOFUM";
			header.DH_OA_Consignor = consignor.MainAddress.PK;

			var line = Factory.New<SupplierBookingLine>();
			line.DL_DH_BookingHeader = header.PK;
			line.DL_ConsigneeReference = "WALLET";
			line.DL_JS_ApprovedShipment = ((BusinessObject)shipment).PK;
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(UniversalEventText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Supplier Booking Line WALLET(Consignee='').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Supplier Booking Line WALLET(Consignee='').
".Trim(), message.GetLogNoteText());

				line.Reload();
				var logs = line.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorised event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
Short Eating Capacity - Stupendous
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
".Trim(), actualContextItems);
			});
		}

		#region UniversalEventText

		const string UniversalEventText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent Version=""1.0"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>eManifestLine</Type>
          <Key>S00009999~WALLET</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventTime>2011-04-18T14:41:28.79</EventTime>
    <EventType>ATH</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>ShortEatingCapacity</Type>
        <Value>Stupendous</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

		#endregion

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return System.Array.Empty<RecipientRoleType>(); }
		}

		#region Overrides to stop Universal Shipment Import Testing

		// eManifest does not support importing of Universal Shipments at this point.

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get { return @""; }
		}

		protected override bool ManagerChecksDataTargetToImport
		{
			get { return false; }
		}

		#endregion

		public void TestEventContextValues()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "BIG BAD GIANT";
			consignor.OH_RL_NKClosestPort = "USBUM"; // Butler,Missouri.

			var shipment = Factory.BOFactory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009999";

			var header = Factory.New<SupplierBookingHeader>();
			header.DH_SupplierReference = "FEEFIFOFUM";
			header.DH_OA_Consignor = consignor.MainAddress.PK;

			var line = Factory.New<SupplierBookingLine>();
			line.DL_DH_BookingHeader = header.PK;
			line.DL_ConsigneeReference = "WALLET";
			line.DL_JS_ApprovedShipment = ((BusinessObject)shipment).PK;
			line.DL_OrderTrackingNumber = "OTN0001";
			Factory.SaveForTesting();

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
OrderTrackingNumber - OTN0001
			".Trim(), (line.GetUniversalDataContextManager() as IEventDataContextManager).EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}
	}
}
