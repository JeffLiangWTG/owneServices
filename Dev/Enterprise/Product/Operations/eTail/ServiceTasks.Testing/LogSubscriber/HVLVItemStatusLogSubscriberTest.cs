using System;
using System.Linq;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.ServiceTasks.Testing
{
	[TestedType(typeof(HVLVItemStatusLogSubscriber))]
	class HVLVItemStatusLogSubscriberTest : LogSubscriberTest<HVLVItemStatusLogSubscriber>
	{
		public void TestItemStatusUpdated()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT123";
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var departureLeg = shipment.TransportsIncludingRelated.AddNew();
			departureLeg.JW_ETD = new ZDateTime(2017, 6, 1);
			departureLeg.JW_ETA = new ZDateTime(2017, 6, 3);
			var arrivalLeg = shipment.TransportsIncludingRelated.AddNew();
			arrivalLeg.JW_ETD = new ZDateTime(2017, 6, 3);
			arrivalLeg.JW_ETA = new ZDateTime(2017, 6, 5);

			AssertEquals(departureLeg, shipment.TransportsIncludingRelated.DepartureTransport);
			AssertEquals(arrivalLeg, shipment.TransportsIncludingRelated.ArrivalTransport);

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item1, HVLVItemStatus.Codes.ShipmentAllocated, HVLVItemStatus.Codes.ShipmentDeparted, true))
			using (HVLVTestHelper.AssertStatusUpdatedEvent(item2, HVLVItemStatus.Codes.ShipmentAllocated, HVLVItemStatus.Codes.ShipmentDeparted, true))
			{
				AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item1.HVI_Status);
				AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item2.HVI_Status);

				RunLogWalkerCycleForTest();
				item1.Reload();
				item2.Reload();
				AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item1.HVI_Status);
				AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item2.HVI_Status);

				departureLeg.JW_ATD = new ZDateTime(2017, 6, 1);
				Factory.Save();

				RunLogWalkerCycleForTest();

				item1.Reload();
				item2.Reload();
				AssertEquals(HVLVItemStatus.Codes.ShipmentDeparted, item1.HVI_Status);
				AssertEquals(HVLVItemStatus.Codes.ShipmentDeparted, item2.HVI_Status);
				AssertCollectionContains("[HVLVItemStatusLogSubscriber] DEP Event processed for Shipment(s): SHIPMENT123", NotifiedEventList);
			}

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item1, HVLVItemStatus.Codes.ShipmentDeparted, HVLVItemStatus.Codes.ShipmentArrived, true))
			using (HVLVTestHelper.AssertStatusUpdatedEvent(item2, HVLVItemStatus.Codes.ShipmentDeparted, HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, true))
			{
				arrivalLeg.JW_ATA = new ZDateTime(2017, 6, 5);
				item2.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
				Factory.Save();

				RunLogWalkerCycleForTest();
				item1.Reload();
				item2.Reload();
				AssertEquals(HVLVItemStatus.Codes.ShipmentArrived, item1.HVI_Status);
				AssertEquals(HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, item2.HVI_Status);
				AssertCollectionContains("[HVLVItemStatusLogSubscriber] ARV Event processed for Shipment(s): SHIPMENT123", NotifiedEventList);
			}
		}

		public void TestItemStatusLogs_IncludeReferenceAndType()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				#region Setup

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = "SHIPMENT123";
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				var item1 = Factory.NewWithPrimaryKey<HVLVItem>(Guid.NewGuid());
				item1.HVI_JS_LoadedOnShipment = shipment.PK;
				item1.HVI_HVC_Consignment = consignment.PK;
				item1.HVI_CurrentBarcode = "111";

				var item2 = Factory.NewWithPrimaryKey<HVLVItem>(Guid.NewGuid());
				item2.HVI_JS_LoadedOnShipment = shipment.PK;
				item2.HVI_HVC_Consignment = consignment.PK;
				item2.HVI_ShipperReference = "333";

				var departureLeg = shipment.TransportsIncludingRelated.AddNew();
				departureLeg.JW_ETD = new ZDateTime(2017, 6, 1);
				departureLeg.JW_ETA = new ZDateTime(2017, 6, 3);
				var arrivalLeg = shipment.TransportsIncludingRelated.AddNew();
				arrivalLeg.JW_ETD = new ZDateTime(2017, 6, 3);
				arrivalLeg.JW_ETA = new ZDateTime(2017, 6, 5);

				Factory.Save();

				#endregion

				departureLeg.JW_ATD = new ZDateTime(2017, 6, 1);
				Factory.Save();

				RunLogWalkerCycleForTest();
				item1.Reload();
				item2.Reload();

				var item1Logs = Factory.Load<StmALog>(
					new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode)
					.AddToFilter(StmALogSchema.SL_Parent, item1.PK)
					.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "NEW=DEP"));

				var item2Logs = Factory.Load<StmALog>(
					new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode)
					.AddToFilter(StmALogSchema.SL_Parent, item2.PK)
					.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "NEW=DEP"));

				CombineAssertions("Pre-conditions", delegate
				{
					AssertEquals(HVLVItemStatus.Codes.ShipmentDeparted, item1.HVI_Status);
					AssertEquals(HVLVItemStatus.Codes.ShipmentDeparted, item2.HVI_Status);
					AssertEquals(1, item1Logs.Length);
					AssertEquals(1, item2Logs.Length);

					var item1Log = item1Logs[0];
					var item2Log = item2Logs[0];

					AssertEquals("Should be an estimate", true, item1Log.SL_IsEstimate);
					AssertEquals("Should be an estimate", true, item2Log.SL_IsEstimate);
					AssertContains("|OLD=SHP|NEW=DEP|RFN=111|TYP=ITEM BARCODE", item1Log.SL_Reference);
					AssertContains("|OLD=SHP|NEW=DEP|RFN=333|TYP=ITEM BARCODE", item2Log.SL_Reference);
					AssertEquals("Should defer firing workflow", true, item1Log.SL_FireWorkflow);
					AssertEquals("Should defer firing workflow", true, item2Log.SL_FireWorkflow);
				});

				item1.HVI_CurrentBarcode = string.Empty;

				item2.HVI_ShipperReference = string.Empty;
				item2.HVI_ItemId = "444";
				arrivalLeg.JW_ATA = new ZDateTime(2017, 6, 5);
				Factory.Save();

				RunLogWalkerCycleForTest();
				item1.Reload();
				item2.Reload();

				item1Logs = Factory.Load<StmALog>(
					new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode)
					.AddToFilter(StmALogSchema.SL_Parent, item1.PK)
					.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "NEW=ARV"));

				item2Logs = Factory.Load<StmALog>(
					new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode)
					.AddToFilter(StmALogSchema.SL_Parent, item2.PK)
					.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "NEW=ARV"));

				CombineAssertions(delegate
				{
					AssertEquals(HVLVItemStatus.Codes.ShipmentArrived, item1.HVI_Status);
					AssertEquals(HVLVItemStatus.Codes.ShipmentArrived, item2.HVI_Status);

					AssertEquals(1, item1Logs.Length);
					AssertEquals(1, item2Logs.Length);

					var item1Log = item1Logs[0];
					var item2Log = item2Logs[0];

					AssertEquals("Should be an estimate", true, item1Log.SL_IsEstimate);
					AssertEquals("Should be an estimate", true, item2Log.SL_IsEstimate);
					AssertContains("|OLD=DEP|NEW=ARV|RFN=111|TYP=ITEM BARCODE", item1Log.SL_Reference);
					AssertContains("|OLD=DEP|NEW=ARV|RFN=333|TYP=ITEM BARCODE", item2Log.SL_Reference);
					AssertEquals("Should defer firing workflow", true, item1Log.SL_FireWorkflow);
					AssertEquals("Should defer firing workflow", true, item2Log.SL_FireWorkflow);
				});
			}
		}

		public void TestItemStatusUpdated_ReferenceNumber_MaxLength()
		{
			var referenceColumnsMaxLengths = new[]
			{
				HVLVItemSchema.HVI_CurrentBarcode.MaxLength,
				HVLVItemSchema.HVI_ShipperReference.MaxLength,
				HVLVItemSchema.HVI_ItemId.MaxLength
			};

			AssertEquals("Please adjust @ItemsToUpdate.Reference column length in LogSubscriber query",
				50,
				referenceColumnsMaxLengths.Max());
		}

		public void TestConsignmentStatusUpdated()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT123";
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var departureLeg = shipment.TransportsIncludingRelated.AddNew();
			departureLeg.JW_ETD = new ZDateTime(2017, 6, 1);
			departureLeg.JW_ETA = new ZDateTime(2017, 6, 3);
			var arrivalLeg = shipment.TransportsIncludingRelated.AddNew();
			arrivalLeg.JW_ETD = new ZDateTime(2017, 6, 3);
			arrivalLeg.JW_ETA = new ZDateTime(2017, 6, 5);

			AssertEquals(departureLeg, shipment.TransportsIncludingRelated.DepartureTransport);
			AssertEquals(arrivalLeg, shipment.TransportsIncludingRelated.ArrivalTransport);

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item1 = consignment1.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item1, HVLVItemStatus.Codes.ShipmentAllocated, HVLVItemStatus.Codes.ShipmentDeparted, true))
			using (HVLVTestHelper.AssertStatusUpdatedEvent(item2, HVLVItemStatus.Codes.ShipmentAllocated, HVLVItemStatus.Codes.ShipmentDeparted, true))
			{
				AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item1.HVI_Status);
				AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item2.HVI_Status);

				RunLogWalkerCycleForTest();
				item1.Reload();
				item2.Reload();
				AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item1.HVI_Status);
				AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item2.HVI_Status);
				AssertEquals("BKD", consignment1.HVC_Status);
				AssertEquals("BKD", consignment2.HVC_Status);

				departureLeg.JW_ATD = new ZDateTime(2017, 6, 1);
				Factory.Save();

				RunLogWalkerCycleForTest();

				item1.Reload();
				item2.Reload();
				consignment1.Reload();
				consignment2.Reload();
				AssertEquals(HVLVItemStatus.Codes.ShipmentDeparted, item1.HVI_Status);
				AssertEquals(HVLVItemStatus.Codes.ShipmentDeparted, item2.HVI_Status);
				AssertEquals(HVLVConsignmentStatus.Codes.DepartedFromOriginDepot, consignment1.HVC_Status);
				AssertEquals(HVLVConsignmentStatus.Codes.DepartedFromOriginDepot, consignment2.HVC_Status);

				AssertCollectionContains("[HVLVItemStatusLogSubscriber] DEP Event processed for Shipment(s): SHIPMENT123", NotifiedEventList);
			}

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item1, HVLVItemStatus.Codes.ShipmentDeparted, HVLVItemStatus.Codes.ShipmentArrived, true))
			using (HVLVTestHelper.AssertStatusUpdatedEvent(item2, HVLVItemStatus.Codes.ShipmentDeparted, HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, true))
			{
				arrivalLeg.JW_ATA = new ZDateTime(2017, 6, 5);
				item2.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
				Factory.Save();

				RunLogWalkerCycleForTest();
				item1.Reload();
				item2.Reload();
				consignment1.Reload();
				consignment2.Reload();
				AssertEquals(HVLVItemStatus.Codes.ShipmentArrived, item1.HVI_Status);
				AssertEquals(HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, item2.HVI_Status);
				AssertEquals(HVLVConsignmentStatus.Codes.ArrivedAtDestination, consignment1.HVC_Status);
				AssertEquals(HVLVConsignmentStatus.Codes.ArrivedAtDestination, consignment2.HVC_Status);
				AssertCollectionContains("[HVLVItemStatusLogSubscriber] ARV Event processed for Shipment(s): SHIPMENT123", NotifiedEventList);
			}
		}

		public void TestConsignmentStatusLogs_IncludeReferenceAndType()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				#region Setup

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = "SHIPMENT123";
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_WaybillNumber = "123456789";
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				var departureLeg = shipment.TransportsIncludingRelated.AddNew();
				departureLeg.JW_ETD = new ZDateTime(2017, 6, 1);
				departureLeg.JW_ETA = new ZDateTime(2017, 6, 3);
				var arrivalLeg = shipment.TransportsIncludingRelated.AddNew();
				arrivalLeg.JW_ETD = new ZDateTime(2017, 6, 3);
				arrivalLeg.JW_ETA = new ZDateTime(2017, 6, 5);

				Factory.Save();

				#endregion

				using (HVLVTestHelper.AssertStatusUpdatedEvent(item, HVLVItemStatus.Codes.ShipmentAllocated, HVLVItemStatus.Codes.ShipmentDeparted, true))
				{
					AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item.HVI_Status);

					RunLogWalkerCycleForTest();
					item.Reload();
					AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item.HVI_Status);
					AssertEquals("BKD", consignment.HVC_Status);

					departureLeg.JW_ATD = new ZDateTime(2017, 6, 1);
					Factory.Save();

					RunLogWalkerCycleForTest();

					item.Reload();
					consignment.Reload();

					var consignmentLog = Factory.Load<StmALog>(
						new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode)
						.AddToFilter(StmALogSchema.SL_Parent, consignment.PK))
						.FirstOrDefault();

					CombineAssertions(() =>
					{
						AssertEquals(HVLVConsignmentStatus.Codes.DepartedFromOriginDepot, consignment.HVC_Status);
						AssertEquals("Should be an estimate", true, consignmentLog.SL_IsEstimate);
						AssertContains("|OLD=BKD|NEW=DEP|RFN=123456789|TYP=CONSIGNMENT WAYBILL NUMBER", consignmentLog.SL_Reference);
						AssertEquals("Should defer firing workflow", true, consignmentLog.SL_FireWorkflow);
					});
				}

				using (HVLVTestHelper.AssertStatusUpdatedEvent(item, HVLVItemStatus.Codes.ShipmentDeparted, HVLVItemStatus.Codes.ShipmentArrived, true))
				{
					arrivalLeg.JW_ATA = new ZDateTime(2017, 6, 5);
					Factory.Save();

					RunLogWalkerCycleForTest();
					item.Reload();
					consignment.Reload();

					var consignmentLog = Factory.Load<StmALog>(
						new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode)
						.AddToFilter(StmALogSchema.SL_Parent, consignment.PK)
						.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "NEW=ARV"))
						.FirstOrDefault();

					CombineAssertions(() =>
					{
						AssertEquals(HVLVConsignmentStatus.Codes.ArrivedAtDestination, consignment.HVC_Status);
						AssertNotNull("Expected a log to contain NEW=ARV on consignment", consignmentLog);
						AssertEquals("Should be an estimate", true, consignmentLog.SL_IsEstimate);
						AssertContains("|OLD=DEP|NEW=ARV|RFN=123456789|TYP=CONSIGNMENT WAYBILL NUMBER", consignmentLog.SL_Reference);
						AssertEquals("Expected to defer firing workflow", true, consignmentLog.SL_FireWorkflow);
					});
				}
			}
		}
	}
}
