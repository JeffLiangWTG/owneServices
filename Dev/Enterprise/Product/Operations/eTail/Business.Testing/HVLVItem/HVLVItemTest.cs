using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVItem))]
	public class HVLVItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHVLVItemTriggerNotFiredWhenBulkCopyingHVLVItems()
		{
			var billToParty = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WeightUQ = Weight.Ounces;
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;

			for (var i = 0; i < Factory.DefaultBulkCopyThreshold(); i++)
			{
				consignment.Items.AddNew();
			}

			using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
			{
				AssertNoExceptionThrown(Factory.Save);
				Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVItemSchema.Constants.TableName, ["CheckConstraints"]));
			}
		}

		public void TestBulkSaveHVLVItems()
		{
			for (var i = 0; i < Factory.DefaultBulkCopyThreshold() + 1; i++)
			{
				Factory.NewWithValidTestData<HVLVItem>();
			}

			using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
			{
				AssertNoExceptionThrown(Factory.Save);
				Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVItemSchema.Constants.TableName, ["CheckConstraints"]));
				Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVConsignmentSchema.Constants.TableName, ["FireTriggers"]));
			}
		}

		public void TestBookingServiceLevel()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var item = bookingHeader.Consignments.AddNew().Items.AddNew();

			CombineAssertions("item Booking Service Level should be empty, same as its BookingHeader's", () =>
			{
				AssertNullOrEmpty(bookingHeader.HVH_RS_NKBookingServiceLevel);
				AssertNullOrEmpty(item.BookingServiceLevel);
			});

			bookingHeader.HVH_RS_NKBookingServiceLevel = "EXP";
			AssertEquals("item Booking Service Level should be same as its BookingHeader's", "EXP", item.BookingServiceLevel);
		}

		public void TestConsgineeCountry()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			var newCountry = Factory.New<RefCountry>();
			newCountry.RN_Code = "%&";
			newCountry.RN_Desc = "Planet Mars";

			CombineAssertions("item Consignee Country should be empty like its consignment", () =>
			{
				AssertNullOrEmpty(consignment.HVC_RN_NKConsigneeCountryCode);
				AssertNullOrEmpty(item.ConsigneeCountry);
			});

			consignment.HVC_RN_NKConsigneeCountryCode = "%&";
			AssertEquals("item Consignee Country should be same as its consignment", "Planet Mars", item.ConsigneeCountry);
		}

		public void TestLoadedOnShipmentReadOnly_WhenConsignmentHeaderIsEmpty()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			Assert("Read only when cosignment header is empty", item.HVI_JS_LoadedOnShipmentInfo.ReadOnly);

			var cosnignmentHeader = Factory.New<HVLVConsignmentHeader>();
			consignment.HVC_HCH_Header = cosnignmentHeader.PK;
			Assert("Not read only when cosignment header is not empty", !item.HVI_JS_LoadedOnShipmentInfo.ReadOnly);
		}

		public void TestItemIdStaysIfNotNewWhenSaveFails()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			Factory.Save();

			AssertNotNullOrEmpty("Consignment Id should not be empty", item.HVI_ItemId);
			item.HVI_Status = "AB";

			AssertExceptionThrown<ZSaveException>("Save should fail", Factory.Save);
			AssertNotNullOrEmpty("Item Id should not be cleared", item.HVI_ItemId);
		}

		public void TestSetIsValidatedForUniquenessToTrue_WhenAutoGenerateID()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();

			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Factory.Save();
				Assert("Validated for uniqueness is true", item.HVI_IsValidatedForUniqueness);
			}
		}

		public void TestUpdateHVI_IsScannedAtDestinationWhenSetHVI_Status()
		{
			AssertUpdateIsScannedAtDestination(HVLVItemStatus.Codes.ReadyForLastMileDelivery);
			AssertUpdateIsScannedAtDestination(HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot);
			AssertUpdateIsScannedAtDestination(HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException);
			AssertUpdateIsScannedAtDestination(HVLVItemStatus.Codes.SurplusAtDestinationDepot);
			AssertUpdateIsScannedAtDestination(HVLVItemStatus.Codes.DispatchedToLastMileCarrier);

			void AssertUpdateIsScannedAtDestination(string status)
			{
				var item = Factory.NewWithValidTestData<HVLVItem>();
				Assert("precondition : IsScannedAtDestination is false", !item.HVI_IsScannedAtDestination);

				item.HVI_Status = status;
				Assert(item.HVI_IsScannedAtDestination);
			}
		}

		public void TestUpdateHVI_IsScannedAtDestinationWhenSetHVI_Status_OnlyUpdatesToTrue()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			var allStatusCodes = typeof(HVLVItemStatus.Codes).GetAllPublicConstantValues();

			CombineAssertions(() =>
			{
				foreach (var code in allStatusCodes)
				{
					item.HVI_IsScannedAtDestination = true;
					item.HVI_Status = code;
					Assert($"changing status to {code} should not update IsScannedAtDestination to false", item.HVI_IsScannedAtDestination);
				}
			});
		}

		public void TestHVI_IsScannedAtDestination_ReadOnly()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			Assert(item.HVI_IsScannedAtDestinationInfo.ReadOnly);
		}

		public void TestIsScannedCleared()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();

			item.HVI_Status = HVLVItemStatus.Codes.DirectedToHeightenedSecurity;
			Assert(!item.IsScannedCleared);

			item.HVI_IsScannedAtDestination = true;
			item.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
			Assert(item.IsScannedCleared);
		}

		public void TestIsScannedHeld()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();

			item.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			Assert(!item.IsScannedHeld);

			item.HVI_IsScannedAtDestination = true;
			item.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;
			Assert(item.IsScannedHeld);
		}

		public void TestIsScannedSurplus()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();

			item.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			Assert(!item.IsScannedSurplus);

			item.HVI_Status = HVLVItemStatus.Codes.SurplusAtDestinationDepot;
			Assert(item.IsScannedSurplus);
		}

		public void TestIsScannedNotReported()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();

			item.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			Assert(!item.IsScannedNotReported);

			item.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
			Assert(item.IsScannedNotReported);
		}

		[TestDate(2021, 5, 3)]
		public void TestShipperUsageTimeIsSet_OnSaving_WhenNoShipmentOrLoadList()
		{
			using (HVLVTestHelper.SuspendTrigger(HVLVItem.Schema.Triggers.PopulateUsageTimes_InsertUpdate, HVLVItemSchema.Constants.TableName))
			{
				var item = Factory.NewWithValidTestData<HVLVItem>();

				Assert("precondition", item.HVI_HVL_LoadList.IsEmpty);
				Assert("precondition", item.HVI_JS_LoadedOnShipment.IsEmpty);
				Assert("precondition", item.HVI_ShipperFirstUsageTimeUtc.IsEmpty);

				Factory.Save();

				AssertEquals(new ZDateTime(2021, 5, 3), item.HVI_ShipperFirstUsageTimeUtc);
			}
		}

		[TestDate(2021, 5, 3)]
		public void TestOriginUsageTimeIsSet_OnSaving_WhenHasLoadList()
		{
			using (HVLVTestHelper.SuspendTrigger(HVLVItem.Schema.Triggers.PopulateUsageTimes_InsertUpdate, HVLVItemSchema.Constants.TableName))
			{
				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_HVL_LoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>().PK;

				Assert("precondition", !item.HVI_HVL_LoadList.IsEmpty);
				Assert("precondition", item.HVI_OriginFirstUsageTimeUtc.IsEmpty);

				Factory.Save();

				AssertEquals(new ZDateTime(2021, 5, 3), item.HVI_OriginFirstUsageTimeUtc);
			}
		}

		[TestDate(2021, 5, 3)]
		public void TestDestinationUsageTimeIsSet_OnSaving_WhenHasShipment()
		{
			using (HVLVTestHelper.SuspendTrigger(HVLVItem.Schema.Triggers.PopulateUsageTimes_InsertUpdate, HVLVItemSchema.Constants.TableName))
			{
				var item = Factory.NewWithValidTestData<HVLVItem>();

				item.HVI_JS_LoadedOnShipment = Factory.NewWithValidTestData<ForwardingShipment>().PK;

				Assert("precondition", !item.HVI_JS_LoadedOnShipment.IsEmpty);
				Assert("precondition", item.HVI_DestinationFirstUsageTimeUtc.IsEmpty);

				Factory.Save();

				AssertEquals(new ZDateTime(2021, 5, 3), item.HVI_DestinationFirstUsageTimeUtc);
			}
		}

		public void TestDGCodes()
		{
			var substance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance1.DG_Code = "AA";

			var substance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance2.DG_Code = "BB";

			var item = Factory.NewWithValidTestData<HVLVItem>();

			var dgItem1 = item.UNDGs.AddNew();
			dgItem1.DI_DG = substance1.PK;

			var dgItem2 = item.UNDGs.AddNew();
			dgItem2.DI_DG = substance2.PK;

			var itemDGCodes = item.DGCodes;

			CombineAssertions("", () =>
			{
				AssertEquals(2, itemDGCodes.Count);
				Assert(itemDGCodes.Contains("AA"));
				Assert(itemDGCodes.Contains("BB"));
			});
		}

		HVLVItem CreateTestItemWithDangerousGoods()
		{
			var substance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance1.DG_Code = "013";

			var substance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance2.DG_Code = "9601";

			var substance3 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance3.DG_Code = "9601X";

			var substance4 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance4.DG_Code = "9603Y";

			var item = Factory.NewWithValidTestData<HVLVItem>();

			item.UNDGs.AddNew().DI_DG = substance1.PK;
			item.UNDGs.AddNew().DI_DG = substance2.PK;
			item.UNDGs.AddNew().DI_DG = substance3.PK;
			item.UNDGs.AddNew().DI_DG = substance4.PK;

			return item;
		}

		public void TestDGUNNOValues()
		{
			var item = CreateTestItemWithDangerousGoods();

			var unnoCodes = item.DGUNNOValues().ToArray();
			Array.Sort(unnoCodes);

			ZString[] expected = { "013", "9601", "9603" };
			AssertSequencesEqual("UNNO values match", expected, unnoCodes);
		}

		public void TestDGCodesWithVariants()
		{
			var item = CreateTestItemWithDangerousGoods();

			var substances = item.DGCodes.ToArray();

			Array.Sort(substances);

			ZString[] expected = { "013", "9601", "9601X", "9603Y" };
			AssertSequencesEqual("DGCodes values match", expected, substances);
		}

		public void TestEffectiveWeight()
		{
			var item = Factory.New<HVLVItem>();
			item.HVI_ManifestedWeight = 123;
			AssertEquals(123m, item.EffectiveWeight);

			item.HVI_ActualWeight = 456;
			AssertEquals(456m, item.EffectiveWeight);

			item.HVI_ActualWeight = ZDecimal.Zero;
			AssertEquals(123m, item.EffectiveWeight);
		}

		public void TestEffectiveVolume()
		{
			var item = Factory.New<HVLVItem>();
			item.HVI_ManifestedVolume = 123;

			AssertEquals(123m, item.EffectiveVolume);

			item.HVI_ActualVolume = 456;
			AssertEquals(456m, item.EffectiveVolume);

			item.HVI_ActualVolume = ZDecimal.Zero;
			AssertEquals(123m, item.EffectiveVolume);
		}

		public void TestHVLVItemCodePropertyAttribute()
		{
			var attributes = TypeDescriptor.GetAttributes(typeof(HVLVItem));
			var codePropertyAttributeType = typeof(CodePropertyAttribute);

			CombineAssertions(() =>
			{
				AssertNotNull("CodePropertyAttribute exists for HVLVItem", attributes[codePropertyAttributeType]);
				AssertEquals($"PropertyName is supposed to be {AutoHVLVItem.Schema.HVI_ItemId}",
					AutoHVLVItem.Schema.HVI_ItemId,
					(attributes[codePropertyAttributeType] as CodePropertyAttribute).PropertyName);
			});
		}

		public void TestLinkedLastMileTransportBooking()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			item.HVI_KM_LastMileTransportBooking = transportBooking.PK;

			AssertEquals(transportBooking, item.LastMileTransportBooking);
		}

		public void TestIsLastMileCarrierBooked()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_ItemId = "001";

			AssertEquals("Precondition:", false, item.IsLastMileCarrierBooked);

			item.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);
			Factory.Save();

			AssertEquals(true, item.IsLastMileCarrierBooked);
		}

		public void TestCalculateManifestedWeightFromItemLines()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = "G";
			var item = consignment.Items.AddNew();

			Assert("precondition", !item.HasItemLines);

			var itemLine1 = item.Lines.AddNew();
			var itemLine2 = item.Lines.AddNew();

			Assert(item.HasItemLines);
			AssertEquals("precondition: Item's manifested weight hasn't been calculated", 0M, item.HVI_ManifestedWeight);

			itemLine1.HVS_GrossWeight = 123.45;
			itemLine2.HVS_GrossWeight = 543.21;
			AssertEquals("Item's manifested weight hasn't been calculated when itemline's weight unit is empty", 0M, item.HVI_ManifestedWeight);

			itemLine1.HVS_WeightUnit = "XX";
			itemLine2.HVS_WeightUnit = "XX";
			Assert("precondition: itemline1's weight unit is invalid", itemLine1.HVS_WeightUnitInfo.HasErrors());
			Assert("precondition: itemline2's weight unit is invalid", itemLine2.HVS_WeightUnitInfo.HasErrors());
			AssertEquals("Item's manifested weight hasn't been calculated when itemline's weight unit is invaild", 0M, item.HVI_ManifestedWeight);

			itemLine1.HVS_WeightUnit = "KG";
			Assert("precondition: Itemline1's weight unit is valid", !itemLine1.HVS_WeightUnitInfo.HasErrors());
			Assert("precondition: Itemline2's weight unit is invalid", itemLine2.HVS_WeightUnitInfo.HasErrors());
			AssertEquals("Only calculate itemline which has vaild weight unit", 123450M, item.HVI_ManifestedWeight);

			itemLine2.HVS_WeightUnit = "LB";
			Assert("precondition: Itemline1's weight unit is valid", !itemLine1.HVS_WeightUnitInfo.HasErrors());
			Assert("precondition: Itemline2's weight unit is valid", !itemLine2.HVS_WeightUnitInfo.HasErrors());
			AssertEquals("Item's manifested weight should be calculated from lines", 369845.911M, item.HVI_ManifestedWeight);

			consignment.HVC_WeightUQ = ZString.Empty;
			item.CalculateManifestedWeight();
			AssertEquals("Item's manifested weight hasn't been calculated when item's weight unit is empty", 0M, item.HVI_ManifestedWeight);

			consignment.HVC_WeightUQ = "XX";
			item.CalculateManifestedWeight();
			Assert("precondition:item's/consignment weight unit is invalid", consignment.HVC_WeightUQInfo.HasErrors());
			AssertEquals("Item's manifested weight hasn't been calculated when item's/consignment weight unit is invaild", 0M, item.HVI_ManifestedWeight);
		}

		public void TestUnitOfMeasureProperties_WhenSetValueLowerCase_GetIsUpperCase()
		{
			var item = Factory.New<HVLVItem>();
			item.HVI_UnitOfDimension = "m";

			AssertEquals("M", item.HVI_UnitOfDimension);
		}

		public void TestItemLinesAreDeletedOnItemDeleting()
		{
			var item = Factory.New<HVLVItem>();
			var itemLine = item.Lines.AddNew();

			Assert("pre-condition", !itemLine.IsDeleted);
			Assert("pre-condition", item.Lines.Contains(itemLine));

			item.Delete();
			Assert("Item Line should be removed from item lines collection", !item.Lines.Contains(itemLine));
			Assert("Item Line should be deleted", itemLine.IsDeleted);
		}

		public void TestUsageTimesAreAllReadonly()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			Assert(item.HVI_ShipperFirstUsageTimeUtcInfo.ReadOnly);
			Assert(item.HVI_OriginFirstUsageTimeUtcInfo.ReadOnly);
			Assert(item.HVI_DestinationFirstUsageTimeUtcInfo.ReadOnly);
		}

		public void TestShipperFirstUsageTimeIsPopulated()
		{
			var anotherFactory = new BusinessObjectFactory();
			var item = Factory.NewWithValidTestData<HVLVItem>();
			AssertNull(item.LoadList);
			AssertNull(item.Shipment);
			Assert(item.HVI_ShipperFirstUsageTimeUtc.IsEmpty);
			Factory.Save();

			item = anotherFactory.Load<HVLVItem>(item.PK);
			Assert(!item.HVI_ShipperFirstUsageTimeUtc.IsEmpty);

			item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>().PK;
			AssertNotNull(item.LoadList);
			AssertNull(item.Shipment);
			Assert(item.HVI_ShipperFirstUsageTimeUtc.IsEmpty);
			Factory.Save();

			item = anotherFactory.Load<HVLVItem>(item.PK);
			Assert(item.HVI_ShipperFirstUsageTimeUtc.IsEmpty);

			item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_JS_LoadedOnShipment = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			AssertNull(item.LoadList);
			AssertNotNull(item.Shipment);
			Assert(item.HVI_ShipperFirstUsageTimeUtc.IsEmpty);
			Factory.Save();

			item = anotherFactory.Load<HVLVItem>(item.PK);
			Assert(item.HVI_ShipperFirstUsageTimeUtc.IsEmpty);
		}

		public void TestOriginFirstUsageTimeIsPopulated()
		{
			var anotherFactory = new BusinessObjectFactory();
			var item = Factory.NewWithValidTestData<HVLVItem>();
			AssertNull(item.LoadList);
			Assert(item.HVI_OriginFirstUsageTimeUtc.IsEmpty);
			Factory.Save();

			item = anotherFactory.Load<HVLVItem>(item.PK);
			Assert(item.HVI_OriginFirstUsageTimeUtc.IsEmpty);

			item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>().PK;
			AssertNotNull(item.LoadList);
			Assert(item.HVI_OriginFirstUsageTimeUtc.IsEmpty);
			Factory.Save();

			item = anotherFactory.Load<HVLVItem>(item.PK);
			Assert(!item.HVI_OriginFirstUsageTimeUtc.IsEmpty);
		}

		public void TestDestinationFirstUsageTimeIsPopulated()
		{
			var anotherFactory = new BusinessObjectFactory();
			var item = Factory.NewWithValidTestData<HVLVItem>();
			AssertNull(item.Shipment);
			Assert(item.HVI_DestinationFirstUsageTimeUtc.IsEmpty);
			Factory.Save();

			item = anotherFactory.Load<HVLVItem>(item.PK);
			Assert(item.HVI_DestinationFirstUsageTimeUtc.IsEmpty);

			item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_JS_LoadedOnShipment = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			AssertNotNull(item.Shipment);
			Assert(item.HVI_DestinationFirstUsageTimeUtc.IsEmpty);
			Factory.Save();

			item = anotherFactory.Load<HVLVItem>(item.PK);
			Assert(!item.HVI_DestinationFirstUsageTimeUtc.IsEmpty);
		}

		public void TestDefaultValues()
		{
			var item = Factory.New<HVLVItem>();

			CombineAssertions(delegate
			{
				AssertEquals("All measurements should be 0 by default", 0m, item.HVI_Length);
				AssertEquals("All measurements should be 0 by default", 0m, item.HVI_Width);
				AssertEquals("All measurements should be 0 by default", 0m, item.HVI_Height);
				AssertEquals("Unit of dimension should come from the registry", Env.Registry.OuterPacklinesMeasurementDefaultUnit, new ZString(item.HVI_UnitOfDimension));
			});
		}

		public void TestDefaultingShipmentForItem()
		{
			var consignment = Factory.New<HVLVConsignment>();
			AssertNull("pre-condition", consignment.ManagingShipment);

			var item = consignment.Items.AddNew();
			AssertNull(item.Shipment);

			var shipment = Factory.New<ForwardingShipment>();
			consignment.ManagingShipment = shipment;

			item = consignment.Items.AddNew();
			AssertNotNull(item.Shipment);
			AssertEquals(item.Shipment, shipment);
		}

		public void TestContainerNumberReadonly()
		{
			var item = Factory.New<HVLVItem>();
			AssertNull("pre-condition", item.Shipment);
			Assert("HVI_ContainerNumber should be readonly", item.HVI_ContainerNumberInfo.ReadOnly);

			var shipment = Factory.New<ForwardingShipment>();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			AssertNotNull("pre-condition", item.Shipment);
			Assert("HVI_ContainerNumber should not be readonly", !item.HVI_ContainerNumberInfo.ReadOnly);
		}

		public void TestIsPackageIdValidSSCCBarCode()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			Assert(!item.IsPackageIdValidSSCCBarCode);

			item.HVI_CurrentBarcode = "00667676545577876556";
			Assert(item.IsPackageIdValidSSCCBarCode);
		}

		public void TestHVI_Status()
		{
			var item = Factory.New<HVLVItem>();
			AssertEquals("Default value", HVLVItemStatus.Codes.ManifestedByETailer, item.HVI_Status);
			AssertEquals("Default value", HVLVItemStatus.Descriptions.ManifestedByETailer, item.HVI_Status_Description);

			item.HVI_Status = HVLVItemStatus.Codes.LoadListLodged;
			AssertEquals(HVLVItemStatus.Descriptions.LoadListLodged, item.HVI_Status_Description);

			item.HVI_Status = HVLVItemStatus.Codes.ShortShippedAtDestinationDepot;
			AssertEquals(HVLVItemStatus.Descriptions.ShortShippedAtDestinationDepot, item.HVI_Status_Description);
		}

		public void TestHVI_StatusIsChangedToARVorDEP_LogIsMarkedWithIsEstimate()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = Factory.New<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			Factory.Save();

			foreach (var itemStatusCode in AllItemStatusCodes)
			{
				item.HVI_Status = itemStatusCode;
			}

			var logsWithIsEstimate = item.Logs.Find(x => x.SL_IsEstimate);
			AssertEquals("There are 2 logs that are estimates", 2, logsWithIsEstimate.Count());

			var arvLog = item.Logs.Find(x => x.SL_Reference.Contains("|NEW=ARV")).Single();
			var depLog = item.Logs.Find(x => x.SL_Reference.Contains("|NEW=DEP")).Single();

			Assert("ARV log is an estimate", arvLog.SL_IsEstimate);
			Assert("DEP log is an estimate", depLog.SL_IsEstimate);
		}

		public void TestHVI_StatusIsSet_ThenSTULogIsAddedOnItem()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			using (item.SuspendAddStatusUpdatedEvent())
			{
				item.HVI_Status = "ARV";
			}

			var logCount = item.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).Count();
			AssertEquals("No STU event should be logged as status was updated while STU event logging was suspended", 0, logCount);

			item.HVI_Status = "DEP";
			logCount = item.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).Count();
			AssertEquals("No STU event should be logged as STU event logging was resumed, but HVLVItem is not in the database", 0, logCount);

			Factory.Save();
			item.HVI_Status = "ARV";
			logCount = item.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).Count();
			AssertEquals("One STU event should be logged as STU event logging was resumed, and HVLVItem is in the database", 1, logCount);
		}

		public void TestHVI_Status_IsSet_OnlyLogNEWStatusWhenCreatingAnItem()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_CurrentBarcode = "BC001";
			item.HVI_HVC_Consignment = consignment.PK;

			AssertEquals("No STU event should be logged when creating new item without saving it into the database", 0,
				item.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).Count());

			Factory.Save();
			item.HVI_Status = HVLVItemStatus.Codes.ShipmentDeparted;
			AssertEquals("|NEW=DEP|OLD=SHP|RFN=BC001|TYP=ITEM BARCODE", item.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).OrderByDescending(l => l.SL_EventTimeUtc).FirstOrDefault().SL_Reference);
		}

		public void TestHVI_CarrierBookingStatus()
		{
			var item = Factory.New<HVLVItem>();
			AssertEquals("Default value", "NON", item.HVI_CarrierBookingStatus);

			item.HVI_CarrierBookingStatus = "BKQ";
			AssertEquals("BKQ", item.HVI_CarrierBookingStatus);

			item.HVI_CarrierBookingStatus = "BKJ";
			AssertEquals("BKJ", item.HVI_CarrierBookingStatus);

			item.HVI_CarrierBookingStatus = "BKL";
			AssertEquals("BKL", item.HVI_CarrierBookingStatus);

			item.HVI_CarrierBookingStatus = "BKC";
			AssertEquals("BKC", item.HVI_CarrierBookingStatus);
		}

		public void TestHVI_HVL_LoadList_ReadOnly()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = Factory.New<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;

			foreach (var itemStatusCode in AllItemStatusCodes)
			{
				item.HVI_Status = itemStatusCode;
				if (itemStatusCode == HVLVItemStatus.Codes.ManifestedByETailer ||
					itemStatusCode == HVLVItemStatus.Codes.LoadListAllocated)
				{
					AssertEquals(false, item.HVI_HVL_LoadListInfo.ReadOnly);
				}
				else
				{
					AssertEquals(true, item.HVI_HVL_LoadListInfo.ReadOnly);
				}
			}
		}

		public void TestHVI_JS_LoadedOnShipment_ReadOnly()
		{
			var item = Factory.New<HVLVItem>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_HCH_Header = Factory.New<HVLVConsignmentHeader>().PK;
			var shipment = Factory.New<ForwardingShipment>();

			item.HVI_HVC_Consignment = consignment.PK;
			Assert(!item.HVI_JS_LoadedOnShipmentInfo.ReadOnly);

			consignment.ManagingShipment = shipment;
			Assert(item.HVI_JS_LoadedOnShipmentInfo.ReadOnly);
		}

		public void TestStatusUpdatedOnLoadListLinking()
		{
			var item = GetNewBusinessObject() as HVLVItem;
			var originLoadList = Factory.New<HVLVOriginLoadList>();
			item.HVI_HVL_LoadList = ZGuid.Empty;
			AssertEquals(HVLVItemStatus.Codes.ManifestedByETailer, item.HVI_Status);
			Factory.Save();

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item, HVLVItemStatus.Codes.ManifestedByETailer, HVLVItemStatus.Codes.LoadListAllocated))
			{
				item.HVI_HVL_LoadList = originLoadList.PK;
				AssertEquals(HVLVItemStatus.Codes.LoadListAllocated, item.HVI_Status);
			}
		}

		public void TestStatusNotUpdatedOnLoadListLinking()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = Factory.New<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			var originLoadList = Factory.New<HVLVOriginLoadList>();

			foreach (var itemStatusCode in AllItemStatusCodes.Except(new string[] { HVLVItemStatus.Codes.ManifestedByETailer }))
			{
				item.HVI_HVL_LoadList = ZGuid.Empty;
				item.HVI_Status = itemStatusCode;
				item.HVI_HVL_LoadList = originLoadList.PK;
				AssertEquals(itemStatusCode, item.HVI_Status);
			}
		}

		public void TestStatusUpdatedOnLoadListDetach()
		{
			var item = GetNewBusinessObject() as HVLVItem;
			var originLoadList = Factory.New<HVLVOriginLoadList>();
			item.HVI_HVL_LoadList = originLoadList.PK;
			AssertEquals(HVLVItemStatus.Codes.LoadListAllocated, item.HVI_Status);
			Factory.Save();

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item, HVLVItemStatus.Codes.LoadListAllocated, HVLVItemStatus.Codes.ManifestedByETailer))
			{
				item.HVI_HVL_LoadList = ZGuid.Empty;
				AssertEquals(HVLVItemStatus.Codes.ManifestedByETailer, item.HVI_Status);
			}
		}

		public void TestStatusNotUpdatedOnLoadListDetach()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = Factory.New<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			var originLoadList = Factory.New<HVLVOriginLoadList>();

			foreach (var itemStatusCode in AllItemStatusCodes.Except(new string[] { HVLVItemStatus.Codes.LoadListAllocated }))
			{
				item.HVI_HVL_LoadList = originLoadList.PK;
				item.HVI_Status = itemStatusCode;
				item.HVI_HVL_LoadList = ZGuid.Empty;
				AssertEquals(itemStatusCode, item.HVI_Status);
			}
		}

		public void TestStatusUpdatedOnPlannedShipmentLinking()
		{
			var plannedShipment = CreateShipmentWithArrivalAndDepartureLegs();
			var item = GetNewBusinessObject() as HVLVItem;
			Factory.Save();

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item, HVLVItemStatus.Codes.ManifestedByETailer, HVLVItemStatus.Codes.ShipmentAllocated))
			{
				item.HVI_JS_LoadedOnShipment = plannedShipment.PK;
				AssertEquals(HVLVItemStatus.Codes.ShipmentAllocated, item.HVI_Status);
			}
		}

		public void TestStatusUpdatedOnDepartedShipmentLinking()
		{
			var departedShipment = CreateShipmentWithArrivalAndDepartureLegs();
			departedShipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = new ZDateTime(2017, 6, 1);

			var item = GetNewBusinessObject() as HVLVItem;
			item.HVI_Status = HVLVItemStatus.Codes.ShipmentAllocated;
			Factory.Save();

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item, HVLVItemStatus.Codes.ShipmentAllocated, HVLVItemStatus.Codes.ShipmentDeparted))
			{
				item.HVI_JS_LoadedOnShipment = departedShipment.PK;
				AssertEquals(HVLVItemStatus.Codes.ShipmentDeparted, item.HVI_Status);
			}
		}

		public void TestStatusUpdatedOnArrivedShipmentLinking()
		{
			var arrivedShipment = CreateShipmentWithArrivalAndDepartureLegs();
			arrivedShipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = new ZDateTime(2017, 6, 1);
			arrivedShipment.TransportsIncludingRelated.ArrivalTransport.JW_ATA = new ZDateTime(2017, 6, 5);

			var item = GetNewBusinessObject() as HVLVItem;
			item.HVI_Status = HVLVItemStatus.Codes.ShipmentDeparted;
			Factory.Save();

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item, HVLVItemStatus.Codes.ShipmentDeparted, HVLVItemStatus.Codes.ShipmentArrived))
			{
				item.HVI_JS_LoadedOnShipment = arrivedShipment.PK;
				AssertEquals(HVLVItemStatus.Codes.ShipmentArrived, item.HVI_Status);
			}
		}

		public void TestStatusNotUpdatedOnShipmentLinkingWhenCurrentStatusIsAfterShipmentArrived()
		{
			var arrivedShipment = CreateShipmentWithArrivalAndDepartureLegs();
			arrivedShipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = new ZDateTime(2017, 6, 1);
			arrivedShipment.TransportsIncludingRelated.ArrivalTransport.JW_ATA = new ZDateTime(2017, 6, 5);

			var item = Factory.New<HVLVItem>();
			item.HVI_Status = HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException;

			item.HVI_JS_LoadedOnShipment = arrivedShipment.PK;
			AssertEquals(HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException, item.HVI_Status);
		}

		public void TestStatusUpdateOnShipmentLinking_DoesNotQueryStmALogTable()
		{
			var departedShipment = CreateShipmentWithArrivalAndDepartureLegs();
			departedShipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = new ZDateTime(2017, 6, 1);
			Factory.Save();

			var newFactory = NewFactory();
			var item = newFactory.New<HVLVItem>();
			item.HVI_Status = HVLVItemStatus.Codes.ShipmentAllocated;

			var expectedStmALogHits = new Dictionary<string, int> { { "StmALog", 0 } };

			using (AssertDbHitsWithUsefulQueryInformation(expectedStmALogHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				item.HVI_JS_LoadedOnShipment = departedShipment.PK;
				AssertEquals(HVLVItemStatus.Codes.ShipmentDeparted, item.HVI_Status);
			}
		}

		public void TestStatusUpdatedOnShipmentDetachWithLoadList()
		{
			var arrivedShipment = CreateShipmentWithArrivalAndDepartureLegs();
			arrivedShipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = new ZDateTime(2017, 6, 1);
			arrivedShipment.TransportsIncludingRelated.ArrivalTransport.JW_ATA = new ZDateTime(2017, 6, 5);

			var originLoadList = Factory.New<HVLVOriginLoadList>();
			var item = GetNewBusinessObject() as HVLVItem;
			item.HVI_HVL_LoadList = originLoadList.PK;
			item.HVI_JS_LoadedOnShipment = arrivedShipment.PK;
			AssertEquals(HVLVItemStatus.Codes.ShipmentArrived, item.HVI_Status);
			Factory.Save();

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item, HVLVItemStatus.Codes.ShipmentArrived, HVLVItemStatus.Codes.LoadListAllocated))
			{
				item.HVI_JS_LoadedOnShipment = ZGuid.Empty;
				AssertEquals(HVLVItemStatus.Codes.LoadListAllocated, item.HVI_Status);
			}
		}

		public void TestStatusUpdatedOnShipmentDetachWithoutLoadList()
		{
			var arrivedShipment = CreateShipmentWithArrivalAndDepartureLegs();
			arrivedShipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = new ZDateTime(2017, 6, 1);
			arrivedShipment.TransportsIncludingRelated.ArrivalTransport.JW_ATA = new ZDateTime(2017, 6, 5);

			var item = GetNewBusinessObject() as HVLVItem;
			item.HVI_JS_LoadedOnShipment = arrivedShipment.PK;
			AssertEquals(HVLVItemStatus.Codes.ShipmentArrived, item.HVI_Status);
			Factory.Save();

			using (HVLVTestHelper.AssertStatusUpdatedEvent(item, HVLVItemStatus.Codes.ShipmentArrived, HVLVItemStatus.Codes.ManifestedByETailer))
			{
				item.HVI_JS_LoadedOnShipment = ZGuid.Empty;
				AssertEquals(HVLVItemStatus.Codes.ManifestedByETailer, item.HVI_Status);
			}
		}

		public void TestClusterKeyIsCascadedFromConsignment()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ClusterKey = 7890;

			var item = Factory.New<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;

			AssertEquals(consignment.HVC_ClusterKey, item.HVI_ClusterKey);
		}

		public void TestDeletionWarningMessage()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var shipment = Factory.New<ForwardingShipment>();

			Assert(item1.CanDelete);
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			Assert(item1.CanDelete);
			AssertEquals("Item1 should give a warning", "Item(s) is attached to a Shipment.", item1.GetWarningBeforeBeingDeleted());
			AssertEquals("Item2 should give no warning", "", item2.GetWarningBeforeBeingDeleted());
		}

		public void TestDeletionWarningMessageFromShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			Assert(item1.CanDelete);
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			Assert(item1.CanDelete);
			AssertEquals("No warning message", "", item1.GetWarningBeforeBeingDeleted());
			AssertEquals("No warning message", "", item2.GetWarningBeforeBeingDeleted());
		}

		public void TestCanDelete()
		{
			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			Factory.Save();
			var item2 = Factory.NewWithValidTestData<HVLVItem>();

			CombineAssertions(() =>
			{
				AssertEquals("Cannot delete Item 1", false, item1.CanDelete);
				AssertEquals("Can delete Item 2", true, item2.CanDelete);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			Factory.Save();
			var item2 = Factory.NewWithValidTestData<HVLVItem>();

			CombineAssertions(() =>
			{
				AssertEquals("Cannot delete Item 1", "Existing items cannot be deleted. Mark them as inactive instead.", item1.ReasonForNotAbleToDelete);
				AssertEquals("No reason for not able to delete Item 2", string.Empty, item2.ReasonForNotAbleToDelete);
			});
		}

		public void TestSavedInactiveItemHasReadOnlyProperties()
		{
			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_IsActive = false;
			Factory.Save();

			var propertyInfos = item1.GetType().GetProperties().Where(p => p.PropertyType == typeof(ZPropertyInfo)).Select(info => (ZPropertyInfo)info.GetValue(item1));

			bool ExpectReadOnly(ZPropertyInfo propertyInfo) => propertyInfo.Name != nameof(HVLVItem.HVI_IsActive);

			CombineAssertions(() =>
			{
				propertyInfos.ForEach(info => AssertEquals($"{info.Name} ReadOnly:", ExpectReadOnly(info), info.ReadOnly));
			});
		}

		public void TestHVLVItemActiveStatusIsNotLogged_WhenConsignmentIsDeactivatedAndHasMultipleItems()
		{
			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			var consignment = item1.Consignment;
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();
			consignment.HVC_IsActive = true;
			Factory.Save();

			consignment.HVC_IsActive = false;
			Factory.Save();

			AssertEquals("precondition - Consignment should have three items", 3, consignment.Items.Count);

			item1.HVI_IsActive = true;
			item2.HVI_IsActive = true;
			item3.HVI_IsActive = true;

			CombineAssertions("Only INA Event from precondition should be logged", () =>
			{
				AssertEquals(AutoEvents.SetToInactive, item1.Logs.MostRecentLog.Event);
				AssertEquals(AutoEvents.SetToInactive, item2.Logs.MostRecentLog.Event);
				AssertEquals(AutoEvents.SetToInactive, item3.Logs.MostRecentLog.Event);
			});
		}

		public void TestHVLVItemActiveStatusIsLogged()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			Factory.Save();

			item.HVI_IsActive = false;
			Factory.Save();

			AssertEquals(AutoEvents.SetToInactive, item.Logs.MostRecentLog.Event);

			item.HVI_IsActive = true;
			Factory.Save();

			AssertEquals(AutoEvents.SetToActive, item.Logs.MostRecentLog.Event);
		}

		public void TestHVLVItemActiveStatusIsReadOnlyWhenNewRecordNotSaved()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			AssertEquals("pre condition", false, item.IsInDatabase);
			AssertEquals("When new item is not saved, HVI_IsActive should be readonly", true, item.HVI_IsActiveInfo.ReadOnly);

			Factory.Save();
			AssertEquals(true, item.IsInDatabase);
			AssertEquals("When new item is saved, HVI_IsActive should no longer be readonly", false, item.HVI_IsActiveInfo.ReadOnly);
		}

		public void TestLoadList()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();
			ReleaseFactory();

			item = Factory.Load<HVLVItem>(item.PK);
			Assert("LoadList reloaded", !ReferenceEquals(loadList, item.LoadList));
			AssertEquals(loadList.PK, item.LoadList.PK);
		}

		public void TestHumanReadableName()
		{
			var item = Factory.New<HVLVItem>();
			AssertEquals("HVLV Item", item.HumanReadableName);

			item.HVI_ItemId = "123456";
			AssertEquals("HVLV Item 123456", item.HumanReadableName);
		}

		public void TestHasArrivedAtDestinationDepot()
		{
			var item = Factory.New<HVLVItem>();

			item.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			AssertEquals(false, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.LoadListAllocated;
			AssertEquals(false, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.LoadListLodged;
			AssertEquals(false, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.ShipmentAllocated;
			AssertEquals(false, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.ShipmentDeparted;
			AssertEquals(false, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.ShipmentArrived;
			AssertEquals(false, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.SurplusAtDestinationDepot;
			AssertEquals(true, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
			AssertEquals(true, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.DiscardedAtDestinationDepot;
			AssertEquals(true, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException;
			AssertEquals(true, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.SeizedByCustoms;
			AssertEquals(true, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.DirectedToHeightenedSecurity;
			AssertEquals(true, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.ReadyForLastMileDelivery;
			AssertEquals(true, item.HasArrivedAtDestinationDepot);

			item.HVI_Status = HVLVItemStatus.Codes.ShortShippedAtDestinationDepot;
			AssertEquals(false, item.HasArrivedAtDestinationDepot);
		}

		public void TestDefersWorkflow()
		{
			AssertEquals(true, (Factory.New<HVLVItem>() as IStmALogParent).DeferFiringWorkflow);
		}

		public void TestChangingHVIStatus_UsesFallback_ToDetermineUniqueIdentifier()
		{
			#region Setup

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			var item1 = Factory.New<HVLVItem>();
			item1.HVI_HVC_Consignment = consignment.PK;
			item1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item1.HVI_CurrentBarcode = "111";
			item1.HVI_ShipperReference = "333";
			item1.HVI_ItemId = "444";
			Factory.Save();

			#endregion

			item1.HVI_Status = HVLVItemStatus.Codes.ShipmentAllocated;

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode)
				.AddToFilter(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "NEW=SHP"));
			var logs = item1.Logs.Find(query);

			CombineAssertions("Pre-Condition - The first choice for identifier should be barcode", delegate
			{
				AssertContains("TYP=ITEM BARCODE", logs[0].SL_Reference);
				AssertContains("RFN=111", logs[0].SL_Reference);
				AssertEquals(true, logs[0].SL_FireWorkflow);
			});

			item1.HVI_CurrentBarcode = string.Empty;
			item1.HVI_Status = HVLVItemStatus.Codes.ShipmentArrived;

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode)
				.AddToFilter(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "NEW=ARV"));
			logs = item1.Logs.Find(query);

			CombineAssertions("The 2nd choice for identifier should be shipper reference", delegate
			{
				AssertContains("TYP=ITEM SHIPPER REFERENCE", logs[0].SL_Reference);
				AssertContains("RFN=333", logs[0].SL_Reference);
				AssertEquals(true, logs[0].SL_FireWorkflow);
			});

			item1.HVI_ShipperReference = string.Empty;
			item1.HVI_Status = HVLVItemStatus.Codes.ReadyForLastMileDelivery;

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode)
				.AddToFilter(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "NEW=RLM"));
			logs = item1.Logs.Find(query);

			CombineAssertions("The last choice for identifier should be item id", delegate
			{
				AssertContains("TYP=ITEM ID", logs[0].SL_Reference);
				AssertContains("RFN=444", logs[0].SL_Reference);
				AssertEquals(true, logs[0].SL_FireWorkflow);
			});
		}

		public void TestCanAdd_WorkflowTriggersToIndividualHVLVItems()
		{
			#region Setup

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			var item1 = Factory.New<HVLVItem>();
			item1.HVI_HVC_Consignment = consignment.PK;
			item1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item1.HVI_CurrentBarcode = "111";

			var item2 = Factory.New<HVLVItem>();
			item2.HVI_HVC_Consignment = consignment.PK;
			item2.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item2.HVI_CurrentBarcode = "123";

			var trigger = consignment.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger on STU";
			trigger.P9_Type = Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.StatusUpdatedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

			var statusUpdatedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode)
				.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "NEW=RLM")
				.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "TYP=ITEM BARCODE");

			var triggerQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode)
				.AddToFilter(new ZQuery(StmALogSchema.SL_Parent, trigger.PK));

			Factory.Save();

			#endregion

			#region Pre-Conditions

			item1.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
			item2.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;

			Factory.Save();
			var logger = new LoggerForTest();
			LogWalkerRunner.Master().Process(logger, CancellationToken.None);
			LogWalkerRunner.Default().Process(logger, CancellationToken.None);
			LogWalkerRunner.Purge().Process(logger, CancellationToken.None);
			item1.Reload();
			item2.Reload();

			var triggerLogs = Factory.Load<StmALog>(triggerQuery);

			CombineAssertions("Pre-Conditions", delegate
			{
				AssertEquals("There should be two STU triggers, one for default status and one for SHP", 2, triggerLogs.Length);

				AssertEquals("There should be no RLM logs on item 1", 0, item1.Logs.Find(statusUpdatedQuery).Length);
				AssertEquals("There should be no RLM logs on item 2", 0, item2.Logs.Find(statusUpdatedQuery).Length);

				AssertEquals(HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, item1.HVI_Status);
				AssertEquals(HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, item2.HVI_Status);
			});

			#endregion

			item1.HVI_Status = HVLVItemStatus.Codes.ReadyForLastMileDelivery;
			item2.HVI_Status = HVLVItemStatus.Codes.SeizedByCustoms;

			Factory.Save();
			LogWalkerRunner.Master().Process(logger, CancellationToken.None);
			LogWalkerRunner.Default().Process(logger, CancellationToken.None);
			LogWalkerRunner.Purge().Process(logger, CancellationToken.None);
			item1.Reload();
			item2.Reload();

			triggerLogs = Factory.Load<StmALog>(triggerQuery);

			var item1Logs = item1.Logs.Find(statusUpdatedQuery);
			var item1StatusUpdatedLog = item1Logs[0];

			CombineAssertions(delegate
			{
				AssertEquals(4, triggerLogs.Length);

				AssertEquals("There should be 1 RLM logs on item 1", 1, item1Logs.Length);
				AssertEquals("There should be no RLM logs on item 2", 0, item2.Logs.Find(statusUpdatedQuery).Length);

				AssertEquals(AutoEvents.StatusUpdatedCode, item1StatusUpdatedLog.SL_SE_NKEvent);
				AssertContains("The event should have a reference to the item barcode", $"|RFN={item1.HVI_CurrentBarcode}", item1StatusUpdatedLog.SL_Reference);

				AssertEquals(HVLVItemStatus.Codes.ReadyForLastMileDelivery, item1.HVI_Status);
				AssertEquals(HVLVItemStatus.Codes.SeizedByCustoms, item2.HVI_Status);
			});
		}

		public void TestManifestedVolume_CalculatesIfAllMeasurementsAboveZero()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			var item = consignment.Items.AddNew();

			CombineAssertions("Pre-conditions", delegate
			{
				AssertEquals(0m, item.HVI_Length);
				AssertEquals(0m, item.HVI_Height);
				AssertEquals(0m, item.HVI_Width);
				AssertEquals("Manifested volume shouldn't be calculated", 0m, item.HVI_ManifestedVolume);
			});

			item.HVI_Length = 1m;

			AssertEquals("Manifested volume shouldn't be calculated", 0m, item.HVI_ManifestedVolume);

			item.HVI_Width = 1m;

			AssertEquals("Manifested volume shouldn't be calculated", 0m, item.HVI_ManifestedVolume);

			item.HVI_Height = 1m;

			AssertEquals("Now that all values are > 0, it should have calculated", 1m, item.HVI_ManifestedVolume);
		}

		public void TestActualVolume_UnitOfDimensions()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			var item = consignment.Items.AddNew();

			item.HVI_Length = 10m;
			item.HVI_Width = 10m;
			item.HVI_Height = 10m;

			item.HVI_UnitOfDimension = Length.Metres;

			AssertEquals("Expected HVLVItem's actual volume calculation based on consignment's VolumeUQ and HVI_Length, HVI_Width and HVI_Height", 1000m, item.HVI_ActualVolume);

			item.HVI_UnitOfDimension = Length.Centimetres;

			AssertEquals("Expected the actual volume to use to centimetres squared according to HVI_UnitOfDimension", 0.001m, item.HVI_ActualVolume);

			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			item.HVI_UnitOfDimension = Length.Centimetres;

			Factory.Save();

			AssertEquals("Expected the actual volume to use cubic centimetres according to HVC_VolumeUQ", 1000m, item.HVI_ActualVolume);
		}

		public void TestGivenManifestedVolume_WhenUnitOfDimensionsChanges_ThenRecalculateManifestedVolume()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			var item = consignment.Items.AddNew();

			item.HVI_Length = 100m;
			item.HVI_Width = 100m;
			item.HVI_Height = 100m;
			item.HVI_UnitOfDimension = Length.Metres;
			item.HVI_ManifestedVolume = 0.000;

			AssertEquals("Precondition", 0.000m, item.HVI_ManifestedVolume);

			item.HVI_UnitOfDimension = Length.Centimetres;

			AssertEquals("Expected HVI_ManifestedVolume to recalculate since the unit of dimension changed", 1m, item.HVI_ManifestedVolume);

			item.HVI_UnitOfDimension = Length.Metres;

			AssertEquals("Expected HVI_ManifestedVolume to recalculate since the unit of dimension changed, regardless if manifested volume has a value of zero", 1000000m, item.HVI_ManifestedVolume);
		}

		public void TestGivenManifestedVolumeSetToZero_WhenLWHChanges_ThenRecalculatManifestedVolume()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			var item = consignment.Items.AddNew();
			item.HVI_ManifestedVolume = 0.000;

			AssertEquals("Precondition", 0.000m, item.HVI_ManifestedVolume);

			item.HVI_Length = 30m;
			item.HVI_Width = 30m;
			item.HVI_Height = 30m;

			AssertEquals("Expected HVI_ManifestedVolume to recalculate after setting LWH", 27000m, item.HVI_ManifestedVolume);
		}

		public void TestGivenManifestedVolumeSetNotToZero_WhenLWHChanges_ThenDoNotRecalculateManifestedVolume()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			var item = consignment.Items.AddNew();
			item.HVI_Length = 20m;
			item.HVI_Width = 20m;
			item.HVI_Height = 20m;
			item.HVI_UnitOfDimension = Length.Metres;

			AssertEquals("Precondition", 8000m, item.HVI_ManifestedVolume);

			item.HVI_Length = 30m;
			item.HVI_Width = 30m;
			item.HVI_Height = 30m;

			AssertEquals("Expected HVI_ManifestedVolume to recalculate after setting LWH", 8000m, item.HVI_ManifestedVolume);
		}

		public void TestWhenLWHChanges_ThenAlwaysRecalculateActualVolume()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			var item = consignment.Items.AddNew();
			item.HVI_ActualVolume = 0.000;

			AssertEquals("Precondition", 0.000m, item.HVI_ActualVolume);

			item.HVI_Length = 30m;
			item.HVI_Width = 30m;
			item.HVI_Height = 30m;

			AssertEquals("Expected HVI_ActualVolume to recalculate after setting LWH", 27000m, item.HVI_ActualVolume);

			item.HVI_Length = 40m;
			item.HVI_Width = 40m;
			item.HVI_Height = 40m;

			AssertEquals("Expected HVI_ActualVolume to recalculate after setting LWH", 64000m, item.HVI_ActualVolume);
		}

		public void TestSetItemOuterPackage_SetsOuterPackageLoadedOnConsol()
		{
			var item = Factory.New<HVLVItem>();
			item.HVI_JS_LoadedOnShipment = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			item.Shipment.Consols.AddNew();

			var outerPackage = Factory.New<HVLVOuterPackage>();

			item.HVI_HVO_OuterPackage = outerPackage.PK;

			AssertEquals("Outer package Loaded On Consol value should match shipment arrival consol PK", item.Shipment.ArrivalConsol.PK, item.OuterPackage.HVO_JK_LoadedOnConsol);
		}

		public void TestSetItemOuterPackage_WhenShipmentIsNull_DoesNotSetOuterPackageLoadedOnConsol()
		{
			var item = Factory.New<HVLVItem>();

			var outerPackage = Factory.New<HVLVOuterPackage>();

			item.HVI_HVO_OuterPackage = outerPackage.PK;

			AssertEquals("Check that the LoadedOnConsol values are the same when item.Shipment is null", outerPackage.HVO_JK_LoadedOnConsol, item.OuterPackage.HVO_JK_LoadedOnConsol);
		}

		public void TestSetItemOuterPackage_WhenShipmentArrivalConsolIsNull_DoesNotSetOuterPackageLoadedOnConsol()
		{
			var item = Factory.New<HVLVItem>();
			item.HVI_JS_LoadedOnShipment = Factory.NewWithValidTestData<ForwardingShipment>().PK;

			var outerPackage = Factory.New<HVLVOuterPackage>();

			item.HVI_HVO_OuterPackage = outerPackage.PK;

			AssertEquals("Check that the LoadedOnConsol values are the same when item.Shipment.ArrivalConsol is null", outerPackage.HVO_JK_LoadedOnConsol, item.OuterPackage.HVO_JK_LoadedOnConsol);
		}

		public void TestSetItemOuterPackage_WhenGivenANonExistentGUID_WillNotThrowNullReferenceException()
		{
			var item = Factory.New<HVLVItem>();
			item.HVI_JS_LoadedOnShipment = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			item.Shipment.Consols.AddNew();

			AssertNoExceptionThrown("Should not have tried to set LoadedOnConsol for OuterPackage that does not exist", () => item.HVI_HVO_OuterPackage = ZGuid.NewZGuid());
		}

		public void TestDefaultContainerNumber_DefaultContainerNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.ManagingShipment = shipment;

			var consol = shipment.Consols.AddNew();
			consol.Containers.AddNew().JC_ContainerNum = "CONT1234565";

			var item = consignment.Items.AddNew();

			AssertEquals("HVLVItem has a default container number", "CONT1234565", item.HVI_ContainerNumber);
		}

		public void TestDefaultContainerNumber_NoDefaultForMultiContainer()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.ManagingShipment = shipment;

			var consol1 = shipment.Consols.AddNew();
			consol1.Containers.AddNew().JC_ContainerNum = "CONT1234565";
			var consol2 = shipment.Consols.AddNew();
			consol2.Containers.AddNew().JC_ContainerNum = "HLCU7654320";

			var item = consignment.Items.AddNew();

			AssertEquals("HVLVItem has no default container number if there are multiple containers", string.Empty, item.HVI_ContainerNumber);
		}

		public void TestDefaultContainerNumber_NoDefaultForNoContainer()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.ManagingShipment = shipment;

			shipment.Consols.AddNew();

			var item = consignment.Items.AddNew();

			AssertEquals("HVLVItem has no default container number if there is no container", string.Empty, item.HVI_ContainerNumber);
		}

		public void TestDefaultContainerNumber_NoDefaultForNonSea()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.ManagingShipment = shipment;

			var consol = shipment.Consols.AddNew();
			consol.Containers.AddNew().JC_ContainerNum = "CONT1234565";

			var item = consignment.Items.AddNew();

			AssertEquals("HVLVItem has no default container number if transport mode is not sea", string.Empty, item.HVI_ContainerNumber);
		}

		public void TestHVI_UsageType_SetHVI_UsageTypeAsPForShipmentFromPlusCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.ManagingShipment = shipment;

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_HVC_Consignment = consignment.PK;

				AssertEquals("HVI_Usage is set to P for US", HVLVItemUsageTypes.Codes.Plus, item.HVI_UsageType);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "ZAABT";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.ManagingShipment = shipment;

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_HVC_Consignment = consignment.PK;

				AssertEquals("HVI_Usage is set to P for ZA", HVLVItemUsageTypes.Codes.Plus, item.HVI_UsageType);
			}
		}

		public void TestHVI_UsageType_WhenRegistryDisabledFilingForNonUSBranch_SetHVI_UsageTypeAsSForUSShipmentFromNonUSBranch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.ManagingShipment = shipment;

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_HVC_Consignment = consignment.PK;

				AssertEquals("HVI_Usage is set to S", HVLVItemUsageTypes.Codes.Standard, item.HVI_UsageType);
			}
		}

		public void TestHVI_UsageType_WhenRegistryEnabledFilingForNonUSBranch_SetHVI_UsageTypeAsPForUSShipmentFromNonUSBranch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.ManagingShipment = shipment;

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_HVC_Consignment = consignment.PK;

				AssertEquals("HVI_Usage is set to P", HVLVItemUsageTypes.Codes.Plus, item.HVI_UsageType);
			}
		}

		public void TestHVI_UsageType_SetHVI_UsageTypeAsSForStandardCountry()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.ManagingShipment = shipment;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;

			AssertEquals("HVI_Usage is set to S", HVLVItemUsageTypes.Codes.Standard, item.HVI_UsageType);
		}

		public void TestHVI_UsageType_SetHVI_UsageTypeWhenNullShipment()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = Factory.NewWithValidTestData<HVLVItem>();

			item.HVI_HVC_Consignment = consignment.PK;

			AssertEquals("HVI_Usage is set to S", HVLVItemUsageTypes.Codes.Standard, item.HVI_UsageType);
		}

		public void TestSuspendAndResumeAddStatusUpdateEvent_Once()
		{
			var item = Factory.New<HVLVItem>();
			using (item.SuspendAddStatusUpdatedEvent())
			{
				AssertEquals("Add status update event suspended should be true after calling suspend add status update event", true, item.AddStatusUpdatedEventSuspended);
			}

			AssertEquals("Add status update event suspended should be false after calling resume add status update event", false, item.AddStatusUpdatedEventSuspended);
		}

		public void TestSuspendAndResumeAddStatusUpdateEvent_MultipleTimes()
		{
			var item = Factory.New<HVLVItem>();
			using (item.SuspendAddStatusUpdatedEvent())
			{
				using (item.SuspendAddStatusUpdatedEvent())
				{
					AssertEquals("Add status update event suspended should be true after calling suspend twice", true, item.AddStatusUpdatedEventSuspended);
				}

				AssertEquals("Add status update event suspended should still be true after resuming once with suspend twice", true, item.AddStatusUpdatedEventSuspended);
			}

			AssertEquals("Add status update event suspended should be false after resuming twice with suspend twice", false, item.AddStatusUpdatedEventSuspended);
		}

		public void TestGetItemEventReferenceNumberAndType()
		{
			var item = Factory.New<HVLVItem>();
			AssertEquals("Item reference type should be ItemId", HVLVConstants.ItemReferenceTypes.ItemId, item.GetItemEventReferenceType());

			item.HVI_ShipperReference = "S0001";
			AssertEquals("Item reference type should be ShipperReference after adding ShipperReference", HVLVConstants.ItemReferenceTypes.ShipperReference, item.GetItemEventReferenceType());

			item.HVI_CurrentBarcode = "123456789";
			AssertEquals("Item reference type should be Barcode after adding CurrentBarcode", HVLVConstants.ItemReferenceTypes.Barcode, item.GetItemEventReferenceType());
		}

		public void TestHVLVItemLogEventDisplayReferenceDisplaysStatusDescription()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item.HVI_CurrentBarcode = "RTX4080";

			Factory.Save();

			var codeList = HVLVItemLookups.GetAllHVLVItemStatus();

			foreach (var newStatusCode in AllItemStatusCodes.Except(new[] { HVLVItemStatus.Codes.ManifestedByETailer }))
			{
				var oldStatusCode = item.HVI_Status;
				item.HVI_Status = newStatusCode;

				Factory.Save();

				var latestLog = item.Logs.MostRecentLogByPostedDate;
				var oldStatusDescription = codeList.GetDescriptionFromCode(oldStatusCode);
				var newStatusDescription = codeList.GetDescriptionFromCode(newStatusCode);
				AssertEquals($"Status Updated: ITEM BARCODE, Reference No. RTX4080, from {oldStatusDescription} to {newStatusDescription}", latestLog.DisplayEventReference.ToString());
			}
		}

		public void TestConcurrencyPolicyIsSetToIgnoreForDataTable()
		{
			var bizo = Factory.New<HVLVItem>();
			var consignmentDataTable = ((INeedTable)bizo).Table;
			AssertEquals(ConcurrencyPolicy.Ignore, consignmentDataTable.ExtendedProperties[typeof(ConcurrencyPolicy)]);
		}

		public void TestHVLVItemShipperReferenceUniqueIndexFailureHandlerWhenDuplicates()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

				var consignment = header.Consignments.AddNew();

				var item1 = consignment.Items.AddNew();
				item1.HVI_ShipperReference = "DUPLICATE";

				var item2 = consignment.Items.AddNew();
				item2.HVI_ShipperReference = "DUPLICATE";

				try
				{
					Factory.Save();
					Fail("Should cause UniqueIndexFailure");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				const string expectedError = "Error saving record. Item Shipper Reference must be unique on the Shipment or Booking Header. The duplicate value is (DUPLICATE).";

				AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Header should not be saved and in database", !header.IsInDatabase);
				AssertIDs(nameof(item1), item1, "", "DUPLICATE");
				AssertIDs(nameof(item2), item2, "", "DUPLICATE");
			}
		}

		public void TestValidationType_WhenItemIsMarkedValidatingForSeaCargoReport()
		{
			var item = Factory.New<HVLVItem>();
			AssertType<HVLVItemValidation>(item.Validation);

			using (item.MarkValidatingForSeaCargoReport())
			{
				AssertType<HVLVItemValidationForSeaCargoReport>(item.Validation);
			}
		}

		public void TestNoAuditLog()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_ManifestedVolume = 1;
			Factory.Save();

			item.HVI_ManifestedVolume = 2;
			Factory.Save();

			item.Delete();
			Factory.Save();

			CombineAssertions("No audit log for HVLVItemLine", () =>
			{
				Assert("No ADD log when created", !item.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystemCode));
				Assert("No EDT log when edited", !item.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode));
				Assert("No DEL log when deleted", !item.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.DeletedARecordInTheSystemCode));
			});
		}

		public void TestHVI_LastUsageCodeIsPopulatedWithBranchCodeWhenItemIsCreated()
		{
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			testCompany.GC_Code = "TST";
			var testBranch1 = testCompany.Branches.AddNew();
			testBranch1.GB_Code = "BR1";
			var testBranch2 = testCompany.Branches.AddNew();
			testBranch2.GB_Code = "BR2";
			Factory.Save();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			Factory.Save();

			AssertEquals(@"
HVI_LastUsageCode should be populated with branch code when item is created.

This info will be used by HVLVItemCreationSubscriber(watching Insert only) to get creation branch code.
It is awared that this field was designed to store values from HVLVConstants.UsageCodes and now it will represent two different info.
The value of HVLVConstants.UsageCodes will be used by HVLVItemLastUsageCodeSubscriber(watching Update only),
and the two subscribers won't intefere each other since they are watching different operations.
This is a bit hacky but the cheapest way to collect item createion usage data when HVLVItem audit log is removed.", "BNE", item.HVI_LastUsageCode);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.China))
			{
				AssertNotEquals("Branch code is different", "BNE", GlbBranch.CurrentBranch.GB_Code);

				item = Factory.Load<HVLVItem>(item.PK);
				item.HVI_CurrentBarcode = "123456";
				Factory.Save();

				AssertEquals("HVI_LastUsageCode should not be updated when item is updated", "BNE", item.HVI_LastUsageCode);
			}
		}

		public void TestUsageLoggedWhenItemUsedByOtherCompany()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "IEC";
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "IE";
			branch.GB_GC = company.PK;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TUS";
			staff.GS_LoginName = "test user";
			staff.GS_GB_LastLogonBranch = branch.PK;

			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_SystemCreateUser = staff.GS_Code;
			Factory.Save();

			AssertEquals("TUS", item.HVI_SystemCreateUser);

			var currentUser = Factory.New<GlbStaff>();
			currentUser.GS_Code = "XX";
			currentUser.GS_LoginName = "xx test";
			Factory.Save();

			using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(new UserContext("xx test", Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				item.HVI_GoodsDescription = "goods description";
				Factory.Save();

				AssertEquals("XX", item.HVI_SystemLastEditUser);
				AssertEquals("A new record has been created since editing company is different from creating company", 1, HVLVTestHelper.GetRecordCountFromHXUTable());
				CombineAssertions(() =>
				{
					AssertEquals("The user in HVLVUsage table should record the current login user", currentUser.GS_Code, HVLVTestHelper.GetUserFromHXUTable());
					AssertEquals("The usageCode in HVLVUsage table should be CWE", HVLVConstants.UsageCodes.CargoWiseUsageByOtherCompany, HVLVTestHelper.GetUsageCodeFromHXUTable());
					Assert("The HXU_HVI_ParentItem should be item's pk",HVLVTestHelper.ExistsUsageCodeWithItemPK(item.PK.ToString()));
				});
			}
		}

		public void TestNoUsageLoggedWhenItemUsedByTheSameCompany()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "IEC";
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "IE";
			branch.GB_GC = company.PK;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TUS";
			staff.GS_LoginName = "test user";
			staff.GS_GB_LastLogonBranch = branch.PK;

			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_SystemCreateUser = staff.GS_Code;
			Factory.Save();

			AssertEquals("TUS", item.HVI_SystemCreateUser);

			using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(new UserContext("test user", branch.PK.ToGuid(), Env.CurrentDepartmentPK)))
			{
				item.HVI_GoodsDescription = "goods description";
				Factory.Save();

				AssertEquals("TUS", item.HVI_SystemLastEditUser);
				AssertEquals("No new records created as both staff are from same company", 0, HVLVTestHelper.GetRecordCountFromHXUTable());
			}
		}

		public void TestNoUsageLoggedWhenCreateUserIsInvalidUser()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			var editUser = Factory.New<GlbStaff>();
			editUser.GS_Code = "XX";
			editUser.GS_LoginName = "xx user";

			AssertUsageLogWithInvalidCreateUser(User.ServiceUserCode);
			AssertUsageLogWithInvalidCreateUser(User.SupportUserCode);
			AssertUsageLogWithInvalidCreateUser(User.UnKnownUserCode);
			AssertUsageLogWithInvalidCreateUser(User.InterchangeUserCode);
			AssertUsageLogWithInvalidCreateUser(User.WebUserCode);

			void AssertUsageLogWithInvalidCreateUser(string userCode)
			{
				item.HVI_SystemCreateUser = userCode;
				Factory.Save();

				using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (Env.SetTemporaryUserContext(new UserContext("xx user", Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					item.HVI_GoodsDescription = "goods description";
					Factory.Save();

					AssertEquals("No new records created as the created user is a invalid user", 0, HVLVTestHelper.GetRecordCountFromHXUTable());
				}
			}
		}

		public void TestNoUsageLoggedWhenEditUserIsInvalidUser()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "IEC";
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "IE";
			branch.GB_GC = company.PK;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TUS";
			staff.GS_LoginName = "test user";
			staff.GS_GB_LastLogonBranch = branch.PK;

			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_SystemCreateUser = staff.GS_Code;
			Factory.Save();

			AssertUsageLogWithInvalidEditUser(User.ServiceUserCode);
			AssertUsageLogWithInvalidEditUser(User.SupportUserCode);
			AssertUsageLogWithInvalidEditUser(User.UnKnownUserCode);
			AssertUsageLogWithInvalidEditUser(User.InterchangeUserCode);
			AssertUsageLogWithInvalidEditUser(User.WebUserCode);

			void AssertUsageLogWithInvalidEditUser(string userCode)
			{
				var editUserPK = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, userCode)).PK;
				using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (Env.SetTemporaryUserContext(new UserContext(editUserPK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					item.HVI_GoodsDescription = "goods description";
					Factory.Save();

					AssertEquals("No new records created as the edit user is a invalid user", 0, HVLVTestHelper.GetRecordCountFromHXUTable());
				}
			}
		}

		#region TestPopulateFromEvent

		public void TestUpdateFromScan_ShouldPopulateItemFromEvent()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			item.HVI_UnitOfDimension = Length.Metres;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=15.500KG|LEN=2.5M|WID=2M|HGT=3.75M|VOL=18.750M3");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			CombineAssertions("Item dimmensions should be stored", () =>
			{
				AssertEquals($"Weight: ", 15.5m, item.HVI_ActualWeight);
				AssertEquals($"Length:", 2.5m, item.HVI_Length);
				AssertEquals($"Width:", 2m, item.HVI_Width);
				AssertEquals($"Height:", 3.75m, item.HVI_Height);
				AssertEquals($"Volume:", 18.75m, item.HVI_ActualVolume);
			});
		}

		public void TestUpdateFromScan_ShouldNotUpdateIfEventIsInvalid()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			item.HVI_UnitOfDimension = Length.Metres;

			item.HVI_ActualWeight = 1;
			item.HVI_Width = 2;
			item.HVI_Height = 3;
			item.HVI_Length = 4;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|XXX=1.200KG|LEN=10CM|WID=20CM|HGT=30CM|VOL=6000CC");
			//Event wasn't actually invalid before
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			CombineAssertions("Item dimmensions should be unchanged:", () =>
			{
				AssertEquals("Weight:", 1m, item.HVI_ActualWeight);
				AssertEquals("Width:", 2m, item.HVI_Width);
				AssertEquals("Height:", 3m, item.HVI_Height);
				AssertEquals("Length:", 4m, item.HVI_Length);
			});
			ErrorReporter.Clear();
		}

		public void TestUpdateFromScan_ShouldNotUpdateIfWeightUnitIsInvalid()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			item.HVI_UnitOfDimension = Length.Metres;

			item.HVI_ActualWeight = 1;
			item.HVI_Width = 2;
			item.HVI_Height = 3;
			item.HVI_Length = 4;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=1.200XX|LEN=10CM|WID=20CM|HGT=30CM|VOL=6000CC");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			CombineAssertions("Item dimmensions should be unchanged:", () =>
			{
				AssertEquals("Weight:", 1m, item.HVI_ActualWeight);
				AssertEquals("Width:", 2m, item.HVI_Width);
				AssertEquals("Height:", 3m, item.HVI_Height);
				AssertEquals("Length:", 4m, item.HVI_Length);
			});
		}

		public void TestUpdateFromScanShouldNotUpdateIfLengthUnitIsInvalid()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			item.HVI_UnitOfDimension = Length.Metres;

			item.HVI_ActualWeight = 1;
			item.HVI_Width = 2;
			item.HVI_Height = 3;
			item.HVI_Length = 4;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10XX|WID=20CM|HGT=30CM|VOL=6000CC");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			CombineAssertions("Item dimmensions should be unchanged:", () =>
			{
				AssertEquals("Weight:", 1m, item.HVI_ActualWeight);
				AssertEquals("Width:", 2m, item.HVI_Width);
				AssertEquals("Height:", 3m, item.HVI_Height);
				AssertEquals("Length:", 4m, item.HVI_Length);
			});
		}

		public void TestUpdateFromScan_ShouldNotUpdateIfWidthUnitIsInvalid()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			item.HVI_UnitOfDimension = Length.Metres;

			item.HVI_ActualWeight = 1;
			item.HVI_Width = 2;
			item.HVI_Height = 3;
			item.HVI_Length = 4;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10CM|WID=20XX|HGT=30CM|VOL=6000CC");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			CombineAssertions("Item dimmensions should be unchanged:", () =>
			{
				AssertEquals("Weight:", 1m, item.HVI_ActualWeight);
				AssertEquals("Width:", 2m, item.HVI_Width);
				AssertEquals("Height:", 3m, item.HVI_Height);
				AssertEquals("Length:", 4m, item.HVI_Length);
			});
		}

		public void TestUpdateFromScan_ShouldNotUpdateIfHeightUnitIsInvalid()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			item.HVI_UnitOfDimension = Length.Metres;

			item.HVI_ActualWeight = 1;
			item.HVI_Width = 2;
			item.HVI_Height = 3;
			item.HVI_Length = 4;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10CM|WID=20CM|HGT=30XX|VOL=6000CC");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			CombineAssertions("Item dimmensions should be unchanged:", () =>
			{
				AssertEquals("Weight:", 1m, item.HVI_ActualWeight);
				AssertEquals("Width:", 2m, item.HVI_Width);
				AssertEquals("Height:", 3m, item.HVI_Height);
				AssertEquals("Length:", 4m, item.HVI_Length);
			});
		}

		public void TestUpdateFromScan_ShouldConvertUnits()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Kilograms;
			item.HVI_UnitOfDimension = Length.Metres;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10CM|WID=20CM|HGT=30CM|VOL=0.006M3");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

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

		public void TestUpdateFromScan_ShouldConvertMixedUnits()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = Weight.Pounds;
			item.HVI_UnitOfDimension = Length.Yards;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10IN|WID=20CM|HGT=30FT|VOL=16.438M3");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

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

		public void TestUpdateFromScan_ShouldUseDefaultUnits()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			consignment.HVC_WeightUQ = string.Empty;
			item.HVI_UnitOfDimension = string.Empty;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=1.200LB|LEN=0.5IN|WID=2IN|HGT=1.5IN|VOL=1.5CI");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

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

		public void TestUpdateFromScan_WhenItemHasNoSPRorSPCEvent_AndImportReleaseStatusCLR_ItemStatusIsRLM()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.None;
			var item = consignment.Items.AddNew();
			var xmlEvent = CreateUniversalEvent(AutoEvents.ScannedCode);

			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item Status should change to RLM", HVLVItemStatus.Codes.ReadyForLastMileDelivery, item.HVI_Status);
		}

		public void TestUpdateFromScan_WhenItemHasNoSPRorSPCEvent_AndImportReleaseStatusHLD_ItemStatusIsPCD()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Cleared;
			var item = consignment.Items.AddNew();
			var xmlEvent = CreateUniversalEvent(AutoEvents.ScannedCode);

			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item Status should change to PCD", HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, item.HVI_Status);
		}

		public void TestUpdateFromScan_WhenItemHasNoSPRorSPCEvent_AndImportReleaseStatusNON_ItemStatusIsPCD()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Cleared;
			var item = consignment.Items.AddNew();
			var xmlEvent = CreateUniversalEvent(AutoEvents.ScannedCode);

			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item Status should change to PCD", HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, item.HVI_Status);
		}

		public void TestUpdateFromScan_WhenItemHasSPREventOnly_AndImportReleaseStatusCLR_ItemStatusIsRDX()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;
			var item = consignment.Items.AddNew();
			item.Logs.AddNew(AutoEvents.SpecialHandlingRequested);

			var xmlEvent = CreateUniversalEvent(AutoEvents.ScannedCode);
			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item Status should change to RDX", HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException, item.HVI_Status);
		}

		public void TestUpdateFromScan_WhenItemHasSPREventOnly_AndImportReleaseStatusHLD_ItemStatusIsRDX()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;
			var item = consignment.Items.AddNew();
			item.Logs.AddNew(AutoEvents.SpecialHandlingRequested);

			var xmlEvent = CreateUniversalEvent(AutoEvents.ScannedCode);
			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item Status should change to RDX", HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException, item.HVI_Status);
		}

		public void TestUpdateFromScan_WhenItemHasSPREventOnly_AndImportReleaseStatusNON_ItemStatusIsRDX()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
			var item = consignment.Items.AddNew();
			item.Logs.AddNew(AutoEvents.SpecialHandlingRequested);

			var xmlEvent = CreateUniversalEvent(AutoEvents.ScannedCode);
			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item Status should change to RDX", HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException, item.HVI_Status);
		}

		public void TestUpdateFromScan_WhenItemHasSPRAndSPCEvent_AndImportReleaseStatusCLR_ItemStatusIsRLM()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;
			var item = consignment.Items.AddNew();
			item.Logs.AddNew(AutoEvents.SpecialHandlingRequested);
			item.Logs.AddNew(AutoEvents.SpecialHandlingCompleted);

			var xmlEvent = CreateUniversalEvent(AutoEvents.ScannedCode);
			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item Status should change to RLM", HVLVItemStatus.Codes.ReadyForLastMileDelivery, item.HVI_Status);
		}

		public void TestUpdateFromScan_WhenItemHasSPRAndSPCEvent_AndImportReleaseStatusHLD_ItemStatusIsPCD()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;
			var item = consignment.Items.AddNew();
			item.Logs.AddNew(AutoEvents.SpecialHandlingRequested);
			item.Logs.AddNew(AutoEvents.SpecialHandlingCompleted);

			var xmlEvent = CreateUniversalEvent(AutoEvents.ScannedCode);
			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item Status should change to PCD", HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, item.HVI_Status);
		}

		public void TestUpdateFromScan_WhenItemHasSPRAndSPCEvent_AndImportReleaseStatusNON_ItemStatusIsPCD()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
			var item = consignment.Items.AddNew();
			item.Logs.AddNew(AutoEvents.SpecialHandlingRequested);
			item.Logs.AddNew(AutoEvents.SpecialHandlingCompleted);

			var xmlEvent = CreateUniversalEvent(AutoEvents.ScannedCode);
			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item Status should change to PCD", HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, item.HVI_Status);
		}

		public void TestUpdateFromScan_UpdateIsScannedAtDestination_WhenXUEEventCodeIsSSC_AndXUEEventReferenceLocationEqualsShipmentDestination()
		{
			CombineAssertions(() =>
			{
				AssertXUEUpdatedItemIsScannedAtDestination(HVLVReleaseStatus.None, "S001");
				AssertXUEUpdatedItemIsScannedAtDestination(HVLVReleaseStatus.Cleared, "S002");
				AssertXUEUpdatedItemIsScannedAtDestination(HVLVReleaseStatus.Held, "S003");
			});
		}

		void AssertXUEUpdatedItemIsScannedAtDestination(ZString releaseStatus, ZString consignmentReference)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = consignmentReference;
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ImportReleaseStatus = releaseStatus;
			var item = consignment.Items.AddNew();

			Factory.Save();

			Assert("Precondition: HVI_IsScannedAtDestination = false", !item.HVI_IsScannedAtDestination);

			var eventDeserializer = new XmlEventDeserializer();
			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "LOC=USLAX");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			Assert("Item should have been updated to be: HVI_IsScannedAtDestination = true.", item.HVI_IsScannedAtDestination);
		}

		public void TestUpdateFromScan_ShouldUpdateActualVolume()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_ActualVolume = 0;
			item.HVI_Width = 3;
			item.HVI_Height = 4;
			item.HVI_Length = 5;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|LEN=5M|WID=3M|HGT=4M|VOL=0KG");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item HVI_ActualVolume changes to 60", (ZDecimal)60, item.HVI_ActualVolume);
		}

		public void TestUpdateFromScan_WhenUnitOfDimensionChanges_ThenCalculateActualVolume()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_ActualVolume = 17;
			item.HVI_Width = 3;
			item.HVI_Height = 4;
			item.HVI_Length = 5;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=1.5KG|LEN=5M|WID=3M|HGT=4M|VOL=1.5KG");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			item.HVI_UnitOfDimension = Length.Centimetres;

			Factory.Save();

			AssertEquals("Item HVI_ActualVolume to be updated to 60", (ZDecimal)60, item.HVI_ActualVolume);
		}

		public void TestUpdateFromScan_WhenManifestedVolumeIsZero_ThenRecalculateManifestedVolume()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_ManifestedVolume = 0;
			item.HVI_Width = 3;
			item.HVI_Height = 4;
			item.HVI_Length = 2;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|LEN=2M|WID=3M|HGT=4M|VOL=0KG");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item HVI_ManifestedVolume changes to 24", (ZDecimal)24, item.HVI_ManifestedVolume);
		}

		public void TestUpdateFromScan_GivenManifestedVolumeIsNotZero_WhenUnitOfDimensionChanges_ThenManifestedVolumeIsRecalculated()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_ManifestedVolume = 10;
			item.HVI_Width = 3;
			item.HVI_Height = 4;
			item.HVI_Length = 5;

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|LEN=5M|WID=3M|HGT=4M|VOL=1.5KG");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			item.UpdateFromScan(xmlEvent);

			AssertEquals("Item HVI_ManifestedVolume to remain 10", (ZDecimal)10, item.HVI_ManifestedVolume);

			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			item.HVI_UnitOfDimension = Length.Centimetres;

			Factory.Save();

			AssertEquals("Item HVI_ManifestedVolume to update to 60", (ZDecimal)60, item.HVI_ManifestedVolume);
		}

		[TestDate]
		public void TestUpdateConsignmentHeaderScanStartTime_WhenScanStartTimeIsEmpty()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var header = Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=1.200XX|LEN=10CM|WID=20CM|HGT=30CM|VOL=6000CC");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			xmlEvent.EventTime = ZDateTimeOffset.Now;

			Assert("Precondition: no scan start time on header", header.HCH_ScanStartTime.IsEmpty);

			item.UpdateFromScan(xmlEvent);
			AssertEquals("Header scan start time is now updated to current time", ZDateTimeOffset.Now, header.HCH_ScanStartTime);
		}

		[TestDate]
		public void TestUpdateConsignmentHeaderScanStartTime_WhenScanStartTimeIsNotEmpty()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var header = Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			header.HCH_ScanStartTime = ZDateTimeOffset.Now;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			var eventText = ScanEventBuilder(AutoEvents.ScannedCode, "FAC=TWS|LOC=AUSYD|WGT=1.200XX|LEN=10CM|WID=20CM|HGT=30CM|VOL=6000CC");
			var xmlEvent = (UniversalEvent)eventDeserializer.Parse(eventText);
			xmlEvent.EventTime = ZDateTimeOffset.Now.AddHours(-5);

			Assert("Precondition: there is a scan start time on header", !header.HCH_ScanStartTime.IsEmpty);

			item.UpdateFromScan(xmlEvent);
			AssertEquals("Header scan start time is updated to an earlier time", ZDateTimeOffset.Now.AddHours(-5), header.HCH_ScanStartTime);
		}

		#region Implementation

		static string ScanEventBuilder(string eventType, string eventReference) => $@"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>{eventType}</EventType>
		<EventReference>{eventReference}</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>HVLVItem</Type>
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

		UniversalEvent CreateUniversalEvent(string eventType, string eventReference = "")
		{
			var eventDeserializer = new XmlEventDeserializer();
			var eventText = ScanEventBuilder(eventType, eventReference);
			return (UniversalEvent)eventDeserializer.Parse(eventText);
		}

		ZDecimal ConvertWeight(ZDecimal amount, ZString originalUnit, ZString convertedUnit) => new ZWeight(amount, originalUnit).ConvertTo(convertedUnit).Round(3);
		ZDecimal ConvertLength(ZDecimal amount, ZString originalUnit, ZString convertedUnit) => ((ZDecimal)Length.Convert(amount, originalUnit, convertedUnit)).Round(3);

		#endregion

		#endregion

		#region IWorkflowTriggerEventSource

		public void TestIWorkflowTriggerEventSource_ParentWorkflowProviders()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = Factory.New<HVLVItem>();

			AssertEquals(0, ((IWorkflowTriggerEventSource)item).ParentWorkflowProviders.Count);

			item.HVI_HVC_Consignment = consignment.PK;
			Factory.Save();

			AssertEquals(1, ((IWorkflowTriggerEventSource)consignment.Items.First()).ParentWorkflowProviders.Count);
		}

		public void TestIWorkflowTriggerEventSource_JobHeaderCompany()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			AssertEquals(GlbCompany.CurrentCompany.PK, ((IWorkflowTriggerEventSource)item).JobHeaderCompany.PK);
		}

		#endregion

		#region Chargeable

		public void TestVolumeWeight()
		{
			var shipmentBySea = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentBySea.JS_TransportMode = TransportModes.Sea;
			Assert("Precondition: Shipment is chargeable by volume when transport mode is sea", !shipmentBySea.IsShipmentChargeableByWeight);

			var consignmentBySea = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentBySea.HVC_JS_ManifestedOnShipment = shipmentBySea.PK;

			var itemBySea = consignmentBySea.Items.AddNew();

			itemBySea.HVI_ManifestedWeight = 5000m;
			AssertEquals("Weight Volume: convert actual weight to volume", 5M, itemBySea.VolumeWeight);
			AssertEquals("Weight Volume Unit: convert actual weight to volume", "M3", consignmentBySea.ChargeableUQ);
			AssertEquals("Weight Volume for display", "5 M3", itemBySea.VolumeWeightForDisplay);

			var shipmentByAir = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentByAir.JS_TransportMode = TransportModes.Air;

			var consignmentByAir = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentByAir.HVC_JS_ManifestedOnShipment = shipmentByAir.PK;
			Assert("Precondition: Shipment is chargeable by weight when transport mode is air", shipmentByAir.IsShipmentChargeableByWeight);

			var itemByAir = consignmentByAir.Items.AddNew();

			itemByAir.HVI_ActualVolume = 10M;
			AssertEquals("Volume Weight: convert actual volume to weight", 1666.667M, Math.Round(itemByAir.VolumeWeight, 3));
			AssertEquals("Volume Weight Unit: convert actual volume to weight", "KG", consignmentByAir.ChargeableUQ);
			AssertEquals("Volume Weight for display", "1666.667 KG", itemByAir.VolumeWeightForDisplay);
		}

		public void TestChargeable()
		{
			var shipmentBySea = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentBySea.JS_TransportMode = TransportModes.Sea;
			Assert("Precondition: Shipment is chargeable by volume when transport mode is sea", !shipmentBySea.IsShipmentChargeableByWeight);

			var consignmentBySea = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentBySea.HVC_JS_ManifestedOnShipment = shipmentBySea.PK;
			var itemBySea = consignmentBySea.Items.AddNew();

			itemBySea.HVI_ActualWeight = 10000M;
			AssertEquals(10M, itemBySea.Chargeable);
			AssertEquals("M3", consignmentBySea.ChargeableUQ);
			AssertEquals("10 M3", itemBySea.ChargeableForDisplay);

			itemBySea.HVI_ActualVolume = 50M;
			CombineAssertions("Chargable is max value of weight volume and actual volume when transport mode is sea", () =>
			{
				AssertEquals(10M, itemBySea.VolumeWeight);
				AssertEquals(50M, itemBySea.Chargeable);
				AssertEquals("M3", consignmentBySea.ChargeableUQ);
				AssertEquals("50 M3", itemBySea.ChargeableForDisplay);
			});

			var shipmentByAir = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentByAir.JS_TransportMode = TransportModes.Air;
			Assert("Precondition: Shipment is chargeable by weight when transport mode is air", shipmentByAir.IsShipmentChargeableByWeight);

			var consignmentByAir = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentByAir.HVC_JS_ManifestedOnShipment = shipmentByAir.PK;
			var itemByAir = consignmentByAir.Items.AddNew();

			itemByAir.HVI_ActualWeight = 10M;
			AssertEquals(10M, itemByAir.Chargeable);
			AssertEquals("KG", consignmentByAir.ChargeableUQ);
			AssertEquals("10 KG", itemByAir.ChargeableForDisplay);

			itemByAir.HVI_ActualVolume = 50M;
			CombineAssertions("Chargable is max value of volume weight and actual weight when transport mode is air", () =>
			{
				AssertEquals(8333.333M, Math.Round(itemByAir.VolumeWeight, 3));
				AssertEquals(8333.333M, Math.Round(itemByAir.Chargeable, 3));
				AssertEquals("KG", consignmentByAir.ChargeableUQ);
				AssertEquals("8333.333 KG", itemByAir.ChargeableForDisplay);
			});
		}

		public void TestChargeable_RecalculateAfterChangeWeightOrVolume()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item = consignment.Items.AddNew();
			item.HVI_ManifestedVolume = 1;
			item.HVI_ManifestedWeight = 1;

			AssertEquals("166.667 KG", item.VolumeWeightForDisplay);
			AssertEquals("166.667 KG", item.ChargeableForDisplay);

			item.HVI_ManifestedVolume = 2;
			CombineAssertions("after change manifested volume", () =>
			{
				AssertEquals("333.333 KG", item.VolumeWeightForDisplay);
				AssertEquals("333.333 KG", item.ChargeableForDisplay);
			});

			item.HVI_ManifestedWeight = 400;
			CombineAssertions("after change manifested weight", () =>
			{
				AssertEquals("333.333 KG", item.VolumeWeightForDisplay);
				AssertEquals("400 KG", item.ChargeableForDisplay);
			});

			item.HVI_ActualVolume = 1;
			CombineAssertions("after change manifested weight", () =>
			{
				AssertEquals("166.667 KG", item.VolumeWeightForDisplay);
				AssertEquals("400 KG", item.ChargeableForDisplay);
			});

			item.HVI_ActualWeight = 1;
			CombineAssertions("after change manifested weight", () =>
			{
				AssertEquals("166.667 KG", item.VolumeWeightForDisplay);
				AssertEquals("166.667 KG", item.ChargeableForDisplay);
			});
		}

		#endregion

		#region Density Factor

		public void TestDensityFactor()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			consignment.HVC_WeightUQ = Weight.Kilograms;

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			item.HVI_ActualWeight = 1m;
			item.HVI_ActualVolume = 6000m;

			AssertEquals("Density factor should be calculated", 1m, item.DensityFactor);

			item.HVI_ActualVolume = 3000m;
			AssertEquals("Density factor should change when volume changed", 0.5m, item.DensityFactor);

			item.HVI_ActualWeight = 0.5m;
			AssertEquals("Density factor should change when weight changed", 1m, item.DensityFactor);

			item.HVI_ActualVolume = 0m;
			AssertEquals("Density factor should be 0 when volume is 0", 0m, item.DensityFactor);

			item.HVI_ActualVolume = 3000m;
			item.HVI_ActualWeight = 0m;
			AssertEquals("Density factor should be 0 when weight is 0", 0m, item.DensityFactor);

			item.HVI_ActualWeight = 1m;
			item.HVI_ActualVolume = 6000m;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedItem = newFactory.Load<HVLVItem>(item.PK);
			AssertEquals("Density factor should be calculated right after data loaded", 1m, loadedItem.DensityFactor);
		}

		#endregion

		#region IUNDGDataProvider Tests

		public void TestGivenForwardingUNDGDataItem_WhenUNDGDataItemHasHVLVItemParent_ThenUNDGDataItemShipmentEqualtoHVLVItemShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKDestination = "AU";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var forwardingUNDGDataItem = item.UNDGs.AddNew() as ForwardingUNDGDataItem;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull("Expected itemLine to have a ForwardingUNDGDataItem in its UNDGs", forwardingUNDGDataItem);
				AssertEquals("Expected forwardingUNDGDataItem to have HVI as its parent table code", HVLVItemSchema.Constants.Prefix, forwardingUNDGDataItem.DI_ParentTableCode);
				AssertEquals("Expected forwardingUNDGDataItem to have a parent ID", item.PK, forwardingUNDGDataItem.DI_ParentID);
				AssertEquals("Expected forwardingUNDGDataItem should have the itemLine as its parent", item.PK, forwardingUNDGDataItem.ParentHVLVItem.PK);
			});
		}

		public void TestGivenForwardingUNDGDataItem_WhenConsolDoesNotAcceptDangerousGoods_ThenReturnForwardingUNDGDataItemValidationErrorMessage()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.MostInterestingTransportForBinding[0].JW_IsCargoOnly = false;
				consol.MostInterestingTransportForBinding[0].JW_TransportMode = TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKDestination = "AU";

				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				var item = consignment.Items.AddNew();
				item.HVI_HVC_Consignment = consignment.PK;
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				var forwardingUNDGDataItem = item.UNDGs.AddNew() as ForwardingUNDGDataItem;

				var substance = Factory.New<UNDGSubstance>();
				substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				substance.DG_UNNO = "1234";
				substance.DG_Variant = "a";
				substance.DG_LQMaxAmt = 2;
				substance.DG_LQMaxAmtType = "NLM";
				substance.DG_LQMaxAmtUQ = "KG";
				substance.DG_CargoPackAmtType = "NLM";
				substance.DG_CargoMaxAmt = 50;
				substance.DG_CargoMaxAmtUQ = "KG";
				substance.DG_LQ2OrPaxMaxAmtType = "NLM";
				substance.DG_LQ2OrPaxMaxAmt = 5;
				substance.DG_LQ2OrPaxMaxAmtUQ = "KG";

				forwardingUNDGDataItem.DI_DG = substance.PK;
				forwardingUNDGDataItem.DI_DGWeight = 1;
				forwardingUNDGDataItem.DI_UnitOfWeight = "KG";
				forwardingUNDGDataItem.DI_IsLimitedQuantity = true;
				forwardingUNDGDataItem.DI_PackageCount = 1;

				Factory.Save();
				forwardingUNDGDataItem.RunPreSaveValidation();

				AssertNoError(forwardingUNDGDataItem.DI_DGWeightInfo, "This quantity exceeds the maximum allowed for a Passenger Aircraft.");

				forwardingUNDGDataItem.DI_DGWeight = 3;
				forwardingUNDGDataItem.DI_IsLimitedQuantity = true;
				forwardingUNDGDataItem.RunPreSaveValidation();

				AssertHasError(forwardingUNDGDataItem.DI_DGWeightInfo, "This quantity exceeds the maximum allowed for a Passenger Aircraft.");
			}
		}

		public void TestCheckSavingHVLVShipmentWithoutFetchingUNDGHints()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var item = consignmentHeader.Consignments.AddNew().Items.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "AA";

			var undgDataItem = item.UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;

			Factory.Save();

			var factory = new BusinessObjectFactory();
			item = factory.Load<HVLVItem>(item.PK);
			item.RunPreSaveValidationWithFetchHints();

			AssertCollectionNotContains(UNDGDataItemSchema.Constants.TableName, factory.GetAllFetchHintedTableNames());
		}

		#endregion

		#region IHVLVISFItemInfoProvider

		public void TestShouldImplementIHVLVISFItemInfoProvider()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			var itemLine1 = item.Lines.AddNew();
			var itemLine2 = item.Lines.AddNew();

			var isfItem = item as IHVLVISFItemInfoProvider;

			CombineAssertions(() =>
			{
				AssertEquals("Should implement Consignment", consignment, isfItem.Consignment);
				AssertEquals("Should implement Lines", 2, isfItem.Lines.Count);
			});
		}

		#endregion

		#region Implementation

		ForwardingShipment CreateShipmentWithArrivalAndDepartureLegs()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var departureLeg = shipment.TransportsIncludingRelated.AddNew();
			departureLeg.JW_ETD = new ZDateTime(2017, 6, 1);
			departureLeg.JW_ETA = new ZDateTime(2017, 6, 3);
			var arrivalLeg = shipment.TransportsIncludingRelated.AddNew();
			arrivalLeg.JW_ETD = new ZDateTime(2017, 6, 3);
			arrivalLeg.JW_ETA = new ZDateTime(2017, 6, 5);

			AssertEquals(departureLeg, shipment.TransportsIncludingRelated.DepartureTransport);
			AssertEquals(arrivalLeg, shipment.TransportsIncludingRelated.ArrivalTransport);

			return shipment;
		}

		IEnumerable<string> AllItemStatusCodes => HVLVItemLookups.GetAllHVLVItemStatus().ToArray().Select(codeDescriptionPair => codeDescriptionPair.Code);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consignment = factory.NewWithValidTestData<HVLVConsignment>();
			var item = factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;

			return item;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_RN_NKOriginCountryCode = "AU";
			itemLine.HVS_OriginTariff = "123456";

			return item;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			return item;
		}

		void AssertIDs(string itemName, HVLVItem item, ZString itemId, ZString shipperRef)
		{
			AssertEquals($"{itemName} ItemID", itemId, item.HVI_ItemId);
			AssertEquals($"{itemName} ShipperReference", shipperRef, item.HVI_ShipperReference);
		}

		#endregion Implementation
	}

	public class HVLVItemIdUniqueIndexFailureHandlerTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		HVLVConsignment Consignment => consignment ?? (consignment = GetHVLVConsignmentForTest());
		HVLVConsignment consignment;

		HVLVConsignment GetHVLVConsignmentForTest()
		{
			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			consignmentHeader.HCH_ClusterKey = 1;

			return consignmentHeader.Consignments.AddNew();
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				if (fInsertValues == null)
				{
					fInsertValues = base.AdditionalInsertValues;

					fInsertValues.Add(HVLVItemSchema.HVI_IsValidatedForUniqueness.Name, "1");
					fInsertValues.Add(HVLVItemSchema.HVI_HVC_Consignment.Name, string.Format("'{0}'", Consignment.PK.ToString()));
					fInsertValues.Add(HVLVItemSchema.HVI_ClusterKey.Name, string.Format("'{0}'", Consignment.HVC_ClusterKey.ToString()));
				}

				return fInsertValues;
			}
		}

		NameValueCollection fInsertValues;

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			((HVLVItem)testBizO).HVI_IsValidatedForUniqueness = true;
			((HVLVItem)testBizO).HVI_HVC_Consignment = Consignment.PK;
			((HVLVItem)testBizO).HVI_ClusterKey = Consignment.HVC_ClusterKey;
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain => HVLVItemSchema.HVI_ItemId;

		protected override Type BizOTypeToTest => typeof(HVLVItem);

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.HVLVItemId;

		public override void TestNumberFountainFix()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				base.TestNumberFountainFix();
			}
		}
	}
}
