using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVItemDataContextManager))]
	public class HVLVItemDataContextManagerTest : DataContextManagerTestCase<HVLVItemDataContextManager, HVLVItem>
	{
		public void TestMatchesEventByItemId()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var item = header.Consignments.AddNew().Items.AddNew();
				item.HVI_CurrentBarcode = "ITEM1";

				Factory.SaveForTesting();

				AssertEquals("Precondition: Item ID defaulted", "ITEM1", item.HVI_ItemId);

				var universalEvent = new UniversalEvent();
				universalEvent.DataContext = DataContextFactory.New();
				universalEvent.DataContext.AddDataTarget(DataContextType.HVLVItem, "ITEM2");
				universalEvent.EventType = AutoEvents.BookingConfirmedCode;
				universalEvent.EventTime = new ZDateTimeOffset(2017, 6, 1);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalEventMessage(universalEvent);
				manager.Process(message);

				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				universalEvent.DataContext.DataTargetCollection.Single().Key = "ITEM1";
				serviceTaskLog = new ServiceTaskLogForTesting();
				manager = new UniversalMessageProcessingManager(serviceTaskLog);
				message = GetQueuedUniversalEventMessage(universalEvent);
				manager.Process(message);
				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertEquals("Service Task Log", "Linked Event to HVLV Item ITEM1.", serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertEquals("Message Log", "Linked Event to HVLV Item ITEM1.", logNoteText);

					var itemInNewFactory = new BusinessObjectFactory().Load<HVLVItem>(item.PK);
					var importedEvent = itemInNewFactory.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingConfirmedCode).Single();
					AssertEquals(new ZDateTime(2017, 6, 1), importedEvent.SL_EventTime);
				});
			}
		}

		public void TestEventContextValues_Default()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item = header.Consignments.AddNew().Items.AddNew();
			item.HVI_ShipperReference = "SHIPREF123";

			var expectedContextValues = new[]
			{
				"ShippersReference - SHIPREF123"
			};
			var actualContextValues = ((IEventDataContextManager)item.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("WaybillNumber exported as HAWBNumber", expectedContextValues, actualContextValues);
		}

		public void TestEventContextValues_WhenLoadedToNonAIRShipment()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item = header.Consignments.AddNew().Items.AddNew();
			item.HVI_ShipperReference = "SHIPREF123";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var expectedContextValues = new[]
			{
				"ShippersReference - SHIPREF123"
			};
			var actualContextValues = ((IEventDataContextManager)item.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("WaybillNumber exported as HBOLNumber", expectedContextValues, actualContextValues);
		}

		public void TestAttachmentsInXUEForItemGetStoredInConsignmentEDocs()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (Factory.BOFactory.AddDisposableService())
				using (var memoryStream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes("test")))
				{
					var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
					var consignment = header.Consignments.AddNew();
					var item = consignment.Items.AddNew();
					item.HVI_CurrentBarcode = "ITEM1";

					Factory.SaveForTesting();

					AssertEquals("pre-condition", 0, consignment.DocManagerInfo().AllEDocs.Count);

					var universalEvent = new UniversalEvent();
					universalEvent.DataContext = DataContextFactory.New();
					universalEvent.DataContext.AddDataTarget(DataContextType.HVLVItem, "ITEM1");
					universalEvent.EventType = AutoEvents.BookingConfirmedCode;
					universalEvent.EventTime = new ZDateTimeOffset(2017, 6, 1);
					universalEvent.AttachedDocumentCollection = new List<AttachedDocument>();

					var attachedDocument = new AttachedDocument()
					{
						Type = new DocumentType() { Code = "MSC", Description = "Miscellaneous Document" },
						FileName = "TestAttachedDocument.xxx",
						IsPublished = true,
						ImageData = memoryStream
					};
					universalEvent.AttachedDocumentCollection.Add(attachedDocument);

					var serviceTaskLog = new ServiceTaskLogForTesting();
					var manager = new UniversalMessageProcessingManager(serviceTaskLog);
					var message = GetQueuedUniversalEventMessage(universalEvent);
					manager.Process(message);

					Factory.SaveForTesting();

					AssertEquals("Attached document should be added to parent consignment eDocs", 1, consignment.DocManagerInfo().AllEDocs.Count);
					AssertEquals("File name should be correct", "TestAttachedDocument.xxx", consignment.DocManagerInfo().AllEDocs[0].FileName);
				}
			}
		}

		#region VolCam

		public void TestProcessScanEvent_CreatesScanAndStatusUpdateLogs()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var item = consignment.Items.AddNew();
				item.HVI_ShipperReference = "SHIPREF123";

				Factory.SaveForTesting();

				CombineAssertions("Precondition: ", () =>
				{
					AssertEquals("Item ID defaulted", "SHIPREF123", item.HVI_ItemId);
					AssertEquals("Item has no log", 0, item.Logs.DatabaseCount);
					AssertEquals("Item initial status", HVLVItemStatus.Codes.ManifestedByETailer, item.HVI_Status);
				});

				var eventDeserializer = new XmlEventDeserializer();
				var scanEventReference = "FAC=TWS|LOC=AUSYD|WGT=15.500KG|LEN=2.5M|WID=2M|HGT=3.75M|VOL=18.750M3";
				var universalEvent = eventDeserializer.Parse(VolCamEventBuilder(scanEventReference, "SHIPREF123")) as UniversalEvent;
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalEventMessage(universalEvent);
				manager.Process(message);

				var itemLogs = item.Logs.GetAllLogs().Cast<StmALog>();

				AssertContainsExactElementsInAnyOrder(
					"We should have registered a single scan event, which in turn triggers a status update",
					new[] { AutoEvents.ScannedCode, AutoEvents.StatusUpdatedCode },
					itemLogs.Select(log => log.SL_SE_NKEvent));
				Assert("Scan event reference matches XUE", itemLogs.Single(log => log.SL_SE_NKEvent == AutoEvents.ScannedCode).CheckReferenceEquals(scanEventReference));
				AssertEquals("Item status updated", HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, item.HVI_Status);
			}
		}

		public void TestVolcamScanEvent_PopulatesItemFromEvent()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Kilograms;
			item.HVI_UnitOfDimension = Length.Metres;

			var dataContextManager = (IEventDataContextManager)item.GetUniversalDataContextManager();
			var xmlEvent = eventDeserializer.Parse(VolCamEventBuilder("FAC=TWS|LOC=AUSYD|WGT=15.500KG|LEN=2.5M|WID=2M|HGT=3.75M|VOL=18.750M3"));
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			CombineAssertions("Item dimmensions should be stored", () =>
			{
				AssertEquals($"Weight: ", 15.5m, item.HVI_ActualWeight);
				AssertEquals($"Length:", 2.5m, item.HVI_Length);
				AssertEquals($"Width:", 2m, item.HVI_Width);
				AssertEquals($"Height:", 3.75m, item.HVI_Height);
			});
		}

		public void TestVolcamScanEvent_ShouldNotUpdateIfEventIsInvalid()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Kilograms;
			item.HVI_UnitOfDimension = Length.Metres;

			item.HVI_ActualWeight = 1;
			item.HVI_Width = 2;
			item.HVI_Height = 3;
			item.HVI_Length = 4;

			var dataContextManager = (IEventDataContextManager)item.GetUniversalDataContextManager();
			var xmlEvent = eventDeserializer.Parse(VolCamEventBuilder("FAC=TWS|LOC=AUSYD|WGT=1.200|LEN=10|WID=20|HGT=30|VOL=6000"));
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("Precondition: Event type should be Scan", AutoEvents.ScannedCode, xmlEvent.EventType);

			CombineAssertions("Item dimmensions should be unchanged:", () =>
			{
				AssertEquals("Weight:", 1m, item.HVI_ActualWeight);
				AssertEquals("Width:", 2m, item.HVI_Width);
				AssertEquals("Height:", 3m, item.HVI_Height);
				AssertEquals("Length:", 4m, item.HVI_Length);
			});
		}

		public void TestVolcamScanEvent_ShouldConvertUnits()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Kilograms;
			item.HVI_UnitOfDimension = Length.Metres;

			var dataContextManager = (IEventDataContextManager)item.GetUniversalDataContextManager();
			var xmlEvent = eventDeserializer.Parse(VolCamEventBuilder("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10CM|WID=20CM|HGT=30CM|VOL=0.006M3"));
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("Precondition: Event type should be Scan", AutoEvents.ScannedCode, xmlEvent.EventType);

			var expectedLength = ConvertLength(10m, Length.Centimetres, Length.Metres);
			var expectedWidth = ConvertLength(20m, Length.Centimetres, Length.Metres);
			var expectedHeight = ConvertLength(30m, Length.Centimetres, Length.Metres);

			CombineAssertions("Item dimmensions should be converted to correct units", () =>
			{
				AssertEquals($"Weight: ", 1.2m, item.HVI_ActualWeight);
				AssertEquals($"Length ({Length.Centimetres} -> {Length.Metres}):", expectedLength, item.HVI_Length);
				AssertEquals($"Width ({Length.Centimetres} -> {Length.Metres}):", expectedWidth, item.HVI_Width);
				AssertEquals($"Height ({Length.Centimetres} -> {Length.Metres}):", expectedHeight, item.HVI_Height);
			});
		}

		public void TestVolcamScanEvent_ShouldConvertMixedUnits()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Pounds;
			item.HVI_UnitOfDimension = Length.Yards;

			var dataContextManager = (IEventDataContextManager)item.GetUniversalDataContextManager();
			var xmlEvent = eventDeserializer.Parse(VolCamEventBuilder("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10IN|WID=20CM|HGT=30FT|VOL=16.438M3"));
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("Precondition: Event type should be Scan", AutoEvents.ScannedCode, xmlEvent.EventType);

			var expectedWeight = ConvertWeight(1.2m, Weight.Kilograms, Weight.Pounds);
			var expectedLength = ConvertLength(10m, Length.Inches, Length.Yards);
			var expectedWidth = ConvertLength(20m, Length.Centimetres, Length.Yards);
			var expectedHeight = ConvertLength(30m, Length.Feet, Length.Yards);

			CombineAssertions("Item dimmensions should be converted to correct units", () =>
			{
				AssertEquals($"Weight ({Weight.Kilograms} -> {Weight.Pounds}): ", expectedWeight, item.HVI_ActualWeight);
				AssertEquals($"Length ({Length.Inches} -> {Length.Yards}):", expectedLength, item.HVI_Length);
				AssertEquals($"Width ({Length.Centimetres} -> {Length.Yards}):", expectedWidth, item.HVI_Width);
				AssertEquals($"Height ({Length.Feet} -> {Length.Yards}):", expectedHeight, item.HVI_Height);
			});
		}

		public void TestVolcamScanEvent_ShouldUseDefaultUnits()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = string.Empty;
			item.HVI_UnitOfDimension = string.Empty;

			var dataContextManager = (IEventDataContextManager)item.GetUniversalDataContextManager();
			var xmlEvent = eventDeserializer.Parse(VolCamEventBuilder("FAC=TWS|LOC=AUSYD|WGT=1.200LB|LEN=0.5IN|WID=2IN|HGT=1.5IN|VOL=1.5CI"));
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("Precondition: Event type should be Scan", AutoEvents.ScannedCode, xmlEvent.EventType);

			var expectedWeightUQ = (string)HVLVConsignmentSchema.HVC_WeightUQ.SqlDbDefault;
			var expectedLengthUQ = (ZString)Env.Registry.OuterPacklinesMeasurementDefaultUnit;

			var expectedWeight = ConvertWeight(1.2m, Weight.Pounds, expectedWeightUQ);
			var expectedLength = ConvertLength(0.5m, Length.Inches, expectedLengthUQ);
			var expectedWidth = ConvertLength(2m, Length.Inches, expectedLengthUQ);
			var expectedHeight = ConvertLength(1.5m, Length.Inches, expectedLengthUQ);

			CombineAssertions("Item dimmensions should be converted to default units", () =>
			{
				AssertEquals($"Weight ({Weight.Pounds} -> {expectedWeightUQ}): ", expectedWeight, item.HVI_ActualWeight);
				AssertEquals($"Length ({Length.Inches} -> {expectedLengthUQ}):", expectedLength, item.HVI_Length);
				AssertEquals($"Width ({Length.Inches} -> {expectedLengthUQ}):", expectedWidth, item.HVI_Width);
				AssertEquals($"Height ({Length.Inches} -> {expectedLengthUQ}):", expectedHeight, item.HVI_Height);
			});

			CombineAssertions("Units should have been set to defaults", () =>
			{
				AssertEquals(expectedWeightUQ, item.Consignment.HVC_WeightUQ);
				AssertEquals(expectedLengthUQ, item.HVI_UnitOfDimension);
			});
		}

		public void TestGivenNewUniversalEventAdded_WhenStatusCodeIsSTU_ThenUpdateItemStatus()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			var dataContextManager = (IEventDataContextManager)item.GetUniversalDataContextManager();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(GetXUEForStatusTest("STU", "|NEW=" + HVLVItemStatus.Codes.Delivered));

			var referenceParameters = StmALog.GetParametersFromReference(xmlEvent.EventReference);
			referenceParameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, out var newStatus);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: NEW in EventReference expected to be DLV", HVLVItemStatus.Codes.Delivered, newStatus);
				AssertEquals("Precondition: original HVI_Status of newly created item is MAN", HVLVItemStatus.Codes.ManifestedByETailer, item.HVI_Status);
			});

			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);
			AssertEquals("Expected HVI_Status to be DLV", HVLVItemStatus.Codes.Delivered, item.HVI_Status);
		}

		#endregion

		#region Implementation

		static string VolCamEventBuilder(string eventReference) => VolCamEventBuilder(eventReference, string.Empty);
		static string VolCamEventBuilder(string eventReference, string itemId) => $@"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>SSC</EventType>
		<EventReference>{eventReference}</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>HVLVItem</Type>
					<Key>{itemId}</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
	</Event>
</UniversalEvent>";

		ZDecimal ConvertWeight(ZDecimal amount, ZString originalUnit, ZString convertedUnit) => new ZWeight(amount, originalUnit).ConvertTo(convertedUnit).Round(3);
		ZDecimal ConvertLength(ZDecimal amount, ZString originalUnit, ZString convertedUnit) => ((ZDecimal)Length.Convert(amount, originalUnit, convertedUnit)).Round(3);

		string GetXUEForStatusTest(string eventType, string eventReference) => $@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>HVLVConsignment</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>DAU</Code>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
			</Company>
		</DataContext>
		<EventTime>2017-12-02T08:40:00</EventTime>
		<EventType>{eventType}</EventType>
		<EventReference>{eventReference}</EventReference>
	</Event>
</UniversalEvent>";

		#endregion
	}
}
