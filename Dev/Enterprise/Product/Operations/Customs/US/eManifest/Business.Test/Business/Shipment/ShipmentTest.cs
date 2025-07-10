using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(Shipment))]
	sealed class ShipmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLicenceLoggedOnShipmentOnlyOnceAfterCleared()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			Factory.Save();

			var logQuery = new ZQuery(StmActivityLogSchema.S7_FormCaption, "MAN");
			var count = Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery);
			var eventQuery = new ZQuery(StmALogSchema.SL_Parent, shipment.PK);
			eventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.CustomsEntryStatusCode);
			eventQuery.AddToFilter(StmALogSchema.SL_Reference, new[] { shipment.Lookups.ReleaseStatusList.GetCodeDescription(ShipmentEntryStatusList.Codes.Clear),
				shipment.Lookups.ReleaseStatusList.GetCodeDescription(ShipmentEntryStatusList.Codes.Linked) });

			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Clear;
			shipment.B0_ReleaseStatusDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("licence logged", count + 1, Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery));
			var licenceLogs = Factory.Load<StmALog>(eventQuery);
			AssertEquals(1, licenceLogs.Length);

			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Error;
			shipment.B0_ReleaseStatusDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("no new licence", count + 1, Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery));
			licenceLogs = Factory.Load<StmALog>(eventQuery);
			AssertEquals(1, licenceLogs.Length);

			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Clear;
			shipment.B0_ReleaseStatusDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("no new licence", count + 1, Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery));
			licenceLogs = Factory.Load<StmALog>(eventQuery);
			AssertEquals("new event logged, but no new licence", 2, licenceLogs.Length);
		}

		public void TestB0_RX_NKGoodsValueCurrencyIsUSD()
		{
			var shipment = Factory.NewWithValidTestData<Shipment>();
			AssertEquals(Constants.CurrencyCodes.UnitedStates, shipment.B0_RX_NKGoodsValueCurrency);
		}

		public void TestSettingDefaultDoesNotMarkAsNeedingValidationIncludingChildren()
		{
			var shipment = Factory.NewWithValidTestData<Shipment>();
			AssertEquals("precondition", ShipmentTypes.Codes.PAPS, shipment.B0_ShipmentType);
			var business = (IBusiness)shipment;
			AssertEquals("Setting default value for shipment bizo should not create any children", 0, business.Children.Length);
		}

		public void TestICusInBondBilIsCorrectlySetup()
		{
			AssertEquals(typeof(Shipment), ObjectFactory.GetType<Integration.Customs.US.eManifest.ICusInBondBill>());
		}

		public void TestPopulateCommodityValueIfRequired()
		{
			var shipment = Factory.NewWithValidTestData<Shipment>();
			var commodity = shipment.Commodities.AddNew();
			commodity.BY_ManifestUnitCode = "PCS";
			shipment.B0_ManifestQty = 12;
			shipment.B0_ManifestUQ = "B0_UQ";
			shipment.B0_Weight = 234.15;
			shipment.B0_WeightUQ = "TN";
			shipment.B0_DescriptionOfCargo = "B0_DescriptionOfCargo1";
			shipment.B0_RN_NKCountryOfExport = "SG";
			AssertEquals(1, shipment.Commodities.Count);
			AssertEquals(shipment.B0_ManifestQty, commodity.BY_PieceCount);
			AssertEquals(shipment.B0_ManifestUQ.Substring(0, commodity.BY_ManifestUnitCodeInfo.MaxLength), commodity.BY_ManifestUnitCode);
			AssertEquals(shipment.B0_Weight, commodity.BY_GrossWeight);
			AssertEquals(shipment.B0_WeightUQ, commodity.BY_GrossWeightUnit);
			AssertEquals(shipment.B0_DescriptionOfCargo, commodity.BY_Description);
			AssertEquals(shipment.B0_RN_NKCountryOfExport, commodity.BY_RN_NKCountryOfOrigin);
		}

		public void TestSelectionDescription()
		{
			var shipment = Factory.NewWithValidTestData<Shipment>();
			var commodity = shipment.Commodities.AddNew();
			commodity.BY_ManifestUnitCode = "PCS";
			shipment.B0_ManifestQty = 12;
			shipment.B0_ManifestUQ = "B0_UQ";
			shipment.B0_Weight = 234.15;
			shipment.B0_WeightUQ = "TN";
			shipment.B0_DescriptionOfCargo = "B0_DescriptionOfCargo1";
			shipment.B0_RN_NKCountryOfExport = "SG";
			shipment.B0_ReferenceID = "ShipDescTest";
			AssertEquals("ShipDescTest true", "ShipDescTest", shipment.SelectionDescription);
		}

		public void TestSetDefaultValues()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			AssertEquals("Default shipment type", ShipmentTypes.Codes.PAPS, shipment.B0_ShipmentType);
			AssertEquals("Pre-condition: B0_VolumeUQ", string.Empty, shipment.B0_VolumeUQ);
			AssertEquals("Pre-condition: B0_WeightUQ", string.Empty, shipment.B0_WeightUQ);
			shipment.B0_Volume = 1;
			shipment.B0_Weight = 1;
			AssertEquals("Volume UQ should be defaulted", Constants.Volume.CubicMetres, shipment.B0_VolumeUQ);
			AssertEquals("Weight UQ should be defaulted", Constants.Weight.Kilograms, shipment.B0_WeightUQ);
			shipment.B0_RL_NKPortOfLading = "AUSYD";
			AssertEquals("60267", shipment.B0_PortOfLadingKCode);
		}

		public void TestConsigneeAndShipperCreatedAndLoaded()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			var consignee = shipment.Consignee;
			consignee.E2_AddressOverride = true;
			var shipper = shipment.Shipper;
			shipper.E2_AddressOverride = true;
			AssertEquals("Consignee type is correct", PartyTypes.Codes.Consignee, consignee.E2_AddressType);
			AssertEquals("Shipper type is correct", PartyTypes.Codes.Shipper, shipper.E2_AddressType);
			Factory.Save();
			var shipmentInNewFactory = new BusinessObjectFactory().Load<Shipment>(shipment.PK);
			AssertEquals("Consignee loaded", consignee.PK, shipmentInNewFactory.Consignee.PK);
			AssertEquals("Shipper loaded", shipper.PK, shipmentInNewFactory.Shipper.PK);
		}

		public void TestInBond()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			var inBond = shipment.InBond;
			inBond.InBondNumber = "123456";
			Factory.Save();
			AssertEquals("Proper InBond should be loaded", "123456", new BusinessObjectFactory().Load<Shipment>(shipment.PK).InBond.InBondNumber);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
			Factory.Save();
			AssertEquals("InBond should be deleted if shipment type is not Inbond", true, inBond.IsDeleted);
		}

		public void TestDelete()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.Commodities.DeleteAll();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			var inbond = shipment.InBond;
			var commodity = shipment.Commodities.AddNew();
			var party = shipment.Parties.AddNew();
			shipment.Delete();
			Assert("InBond should be deleted", inbond.IsDeleted);
			Assert("Commodities should be deleted", commodity.IsDeleted);
			Assert("Parties should be deleted", party.IsDeleted);
		}

		public void TestStatusChangedLog()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Error;
			AssertNull(shipment.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, shipment.B0_ReleaseStatusCodeDescription));
			shipment.B0_ReleaseStatusDate = ZDateTime.Now;
			var log = shipment.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, shipment.B0_ReleaseStatusCodeDescription);
			AssertNotNull("Error status has been set and release date has been updated", log);
			var count = shipment.Logs.GetAllLogs().Count;
			shipment.B0_ReleaseStatus = ZString.Empty;
			shipment.B0_ReleaseStatusDate = ZDateTime.Now.AddDays(1);
			AssertEquals("No logs if status cleared", count, shipment.Logs.GetAllLogs().Count);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			AssertNull(shipment.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, shipment.B0_ReleaseStatusCodeDescription));
			shipment.B0_ReleaseStatusDate = ZDateTime.Now.AddDays(2);
			log = shipment.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, shipment.B0_ReleaseStatusCodeDescription);
			AssertNotNull("Clear status has been set and release date has been updated", log);
		}

		public void TestIsLinked()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			AssertEquals("IsLinked", false, shipment.IsLinked);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			AssertEquals("IsLinked", false, shipment.IsLinked);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.LodgedWithOtherTrip;
			AssertEquals("IsLinked", false, shipment.IsLinked);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Linked;
			AssertEquals("IsLinked", true, shipment.IsLinked);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Released;
			AssertEquals("IsLinked", true, shipment.IsLinked);
		}

		public void TestIsLodged()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Error;
			Factory.Save();
			AssertIsLodged(shipment, isLodged: false, isReadOnly: false);
			shipment.B0_IsLodged = true;
			AssertIsLodged(shipment, isLodged: true, isReadOnly: false);
			AssertEquals("B0_ReleaseStatus", ShipmentEntryStatusList.Codes.LodgedWithOtherTrip, shipment.B0_ReleaseStatus);
			shipment.B0_IsLodged = false;
			AssertIsLodged(shipment, isLodged: false, isReadOnly: false);
			AssertEquals("B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Error, shipment.B0_ReleaseStatus);
			shipment.B0_IsLodged = true;
			Factory.Save();
			shipment.B0_IsLodged = false;
			AssertEquals("B0_ReleaseStatus", ZString.Empty, shipment.B0_ReleaseStatus);
			shipment.B0_ReleaseStatus = EntryStatusList.Codes.Cancelled;
			AssertIsLodged(shipment, isLodged: false, isReadOnly: false);
			shipment.B0_ReleaseStatus = EntryStatusList.Codes.Error;
			AssertIsLodged(shipment, isLodged: false, isReadOnly: false);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			AssertIsLodged(shipment, isLodged: true, isReadOnly: true);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Released;
			AssertIsLodged(shipment, isLodged: true, isReadOnly: true);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Linked;
			AssertIsLodged(shipment, isLodged: true, isReadOnly: true);
		}

		static void AssertIsLodged(Shipment shipment, bool isLodged, bool isReadOnly)
		{
			const string reasonForNotAbleToDelete = "This Shipment is lodged to Customs, you have to send cancellation message for this shipment before deleting it.";
			AssertEquals("B0_IsLodged", isLodged, shipment.B0_IsLodged);
			AssertEquals("B0_IsLodged.ReadOnly", isReadOnly, shipment.B0_IsLodgedInfo.ReadOnly);
			AssertEquals("CanDelete", !isLodged, shipment.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, shipment.ReasonForNotAbleToDelete);
			AssertEquals("B0_MasterBillNumber.ReadOnly", isLodged, shipment.B0_MasterBillNumberInfo.ReadOnly);
		}

		public void TestFirstCommodity_ReadOnly()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();

			AssertEquals(true, shipment.FirstCommodityCountryOfOriginInfo.ReadOnly);
			AssertEquals(false, shipment.FirstCommodityHazardousGoodsIdentifierInfo.ReadOnly);
			AssertEquals(false, shipment.FirstCommodityHazardousGoodsContactInfo.ReadOnly);
			AssertEquals(true, shipment.FirstCommodityC4CodesInfo.ReadOnly);

			shipment.B0_ShipmentType = ShipmentTypes.Codes.LowValue;
			shipment.FirstCommodity.Shipment = shipment;
			AssertEquals(false, shipment.FirstCommodityCountryOfOriginInfo.ReadOnly);

			_ = shipment.Commodities.AddNew();
			_ = shipment.FirstCommodity.UNDGs.AddNew();
			_ = shipment.FirstCommodity.UNDGs.AddNew();
			AssertEquals(true, shipment.FirstCommodityHazardousGoodsIdentifierInfo.ReadOnly);
			AssertEquals(true, shipment.FirstCommodityHazardousGoodsContactInfo.ReadOnly);

			shipment.B0_ShipmentType = ShipmentTypes.Codes.BRASS;
			AssertEquals(false, shipment.FirstCommodityC4CodesInfo.ReadOnly);
		}

		public void TestFirstCommodity_ShouldRecalculate_WhenItemAdded()
		{
			var shipment = Factory.New<Shipment>();
			shipment.Commodities.DeleteAll();
			AssertNull(shipment.FirstCommodity);
			var item1 = shipment.Commodities.AddNew();
			item1.BY_PieceCount = 10;
			AssertEquals(item1.PK, shipment.FirstCommodity.PK);
			AssertEquals(10, shipment.FirstCommodityPieceCount);
			shipment.Commodities.AddNew();
			AssertNull(shipment.FirstCommodity);
			AssertEquals(0, shipment.FirstCommodityPieceCount);
		}

		public void TestFirstCommodity_ShouldRecalculate_WhenItemDeleted()
		{
			var shipment = Factory.New<Shipment>();
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			var item2 = shipment.Commodities.AddNew();
			AssertNull(shipment.FirstCommodity);
			shipment.Commodities.Delete(item1);
			AssertEquals(item2.PK, shipment.FirstCommodity.PK);
		}

		public void TestHasMultipleItemLine()
		{
			var shipment = Factory.New<Shipment>();
			shipment.Commodities.DeleteAll();
			AssertEquals(false, shipment.HasMultipleItemLine);
			shipment.Commodities.AddNew();
			AssertEquals(false, shipment.HasMultipleItemLine);
			shipment.Commodities.AddNew();
			AssertEquals(true, shipment.HasMultipleItemLine);
		}

		public void TestUpdateShipmentTypeDependentsAsNeeded_RemovingC4Codes()
		{
			var shipment = Factory.New<Shipment>();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.BRASS;
			shipment.Commodities.DeleteAll();

			var commodity1 = shipment.Commodities.AddNew();
			commodity1.C4Codes.AddNew();

			var commodity2 = shipment.Commodities.AddNew();
			commodity2.C4Codes.AddNew();
			commodity2.C4Codes.AddNew();

			AssertEquals("Precondition: There are 3 C4 codes in total across 2 commodities on the Shipment", 3, shipment.Commodities.Sum(x => x.C4Codes.Count));

			shipment.B0_ShipmentType = ShipmentTypes.Codes.LowValue;
			AssertEquals("C4 Codes are removed when Shipment Type is changed to a type that is not BRASS", 0, shipment.Commodities.Sum(x => x.C4Codes.Count));

			shipment.B0_ShipmentType = ShipmentTypes.Codes.BRASS;
			AssertEquals("There are no C4 Codes after changing Shipment Type back to BRASS", 0, shipment.Commodities.Sum(x => x.C4Codes.Count));
		}

		public void TestFirstCommodityPieceCount_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_PieceCount = 2;
			var firstValueChange = 3;
			var secondValueChange = 4;
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityPieceCountInfo, delegate
			{
				shipment.FirstCommodityPieceCount = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityPieceCount = secondValueChange;
			}, firstValueChange.ToString(), secondValueChange.ToString());
		}

		public void TestFirstCommodityManifestUnitCode_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_ManifestUnitCode = Constants.PkgUnit.Box;
			var firstValueChange = Constants.PkgUnit.Bag;
			var secondValueChange = Constants.PkgUnit.BaleCompressed;
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityManifestUnitCodeInfo, delegate
			{
				shipment.FirstCommodityManifestUnitCode = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityManifestUnitCode = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCommodityWeight_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_GrossWeight = 1m;
			var firstValueChange = 2m;
			var secondValueChange = 3m;
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityWeightInfo, delegate
			{
				shipment.FirstCommodityWeight = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityWeight = secondValueChange;
			}, firstValueChange.ToString(), secondValueChange.ToString());
		}

		public void TestFirstCommodityWeightUnit_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_GrossWeightUnit = Constants.Weight.Grams;
			var firstValueChange = Constants.Weight.Hectograms;
			var secondValueChange = Constants.Weight.Kilograms;
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityWeightUnitInfo, delegate
			{
				shipment.FirstCommodityWeightUnit = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityWeightUnit = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCommodityDescription_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_Description = "111";
			var firstValueChange = "2222";
			var secondValueChange = "33333";
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityDescriptionInfo, delegate
			{
				shipment.FirstCommodityDescription = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityDescription = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCommodityBJ_Equipment_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_BJ_Equipment = ZGuid.NewZGuid();
			var firstValueChange = ZGuid.NewZGuid();
			var secondValueChange = ZGuid.NewZGuid();
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityEquipmentInfo, delegate
			{
				shipment.FirstCommodityEquipment = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityEquipment = secondValueChange;
			}, firstValueChange.ToString(), secondValueChange.ToString());
		}

		public void TestFirstCommodityMarksAndNumbers_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_MarksAndNumbers = "111";
			var firstValueChange = "2222";
			var secondValueChange = "33333";
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityMarksAndNumbersInfo, delegate
			{
				shipment.FirstCommodityMarksAndNumbers = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityMarksAndNumbers = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCommodityMonetaryValue_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_MonetaryValue = 1m;
			var firstValueChange = 2m;
			var secondValueChange = 3m;
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityMonetaryValueInfo, delegate
			{
				shipment.FirstCommodityMonetaryValue = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityMonetaryValue = secondValueChange;
			}, firstValueChange.ToString(), secondValueChange.ToString());
		}

		public void TestFirstCommodityRN_NKCountryOfOrigin_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			var firstValueChange = Core.Constants.CountryCodes.UnitedStates;
			var secondValueChange = Core.Constants.CountryCodes.Canada;
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityCountryOfOriginInfo, delegate
			{
				shipment.FirstCommodityCountryOfOrigin = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityCountryOfOrigin = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCommodityHazardousGoodsIdentifier_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_HazardousGoodsIdentifier = ZGuid.NewZGuid();
			var firstValueChange = ZGuid.NewZGuid();
			var secondValueChange = ZGuid.NewZGuid();
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityHazardousGoodsIdentifierInfo, delegate
			{
				shipment.FirstCommodityHazardousGoodsIdentifier = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityHazardousGoodsIdentifier = secondValueChange;
			}, firstValueChange.ToString(), secondValueChange.ToString());
		}

		public void TestFirstCommodityVehicleIdentificationNumbers_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_VehicleIdentificationNumbers = "111";
			var firstValueChange = "2222";
			var secondValueChange = "33333";
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityVehicleIdentificationNumbersInfo, delegate
			{
				shipment.FirstCommodityVehicleIdentificationNumbers = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityVehicleIdentificationNumbers = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCommodityC4Codes_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_C4Codes = "111";
			var firstValueChange = "2222";
			var secondValueChange = "33333";
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityC4CodesInfo, delegate
			{
				shipment.FirstCommodityC4Codes = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityC4Codes = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCommodityHazardousGoodsContact_ValueChanged()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_HazardousGoodsContact = ZGuid.NewZGuid();
			var firstValueChange = ZGuid.NewZGuid();
			var secondValueChange = ZGuid.NewZGuid();
			AssertFirstCommodityPropertyValueChangedWorkCorrectly(shipment.FirstCommodityHazardousGoodsContactInfo, delegate
			{
				shipment.FirstCommodityHazardousGoodsContact = firstValueChange;
				shipment.Commodities.AddNew();
				_ = shipment.FirstCommodity;
				shipment.Commodities.Delete(item1);
				shipment.FirstCommodityHazardousGoodsContact = secondValueChange;
			}, firstValueChange.ToString(), secondValueChange.ToString());
		}

		public void TestFirstCommodityHazardousGoodsContactPhone()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Big Boss";
			contact.OC_Phone = "+3 (126) 4846516";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Big Boss 2";
			contact2.OC_Phone = "+3 (126) 3335545";
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.Commodities.DeleteAll();
			var item1 = shipment.Commodities.AddNew();
			item1.BY_HazardousGoodsContact = contact.PK;
			AssertEquals(contact.OC_Phone, item1.BY_HazardousGoodsContactPhone);
			AssertEquals(contact.PK, shipment.FirstCommodityHazardousGoodsContact);
			AssertEquals(contact.OC_Phone, shipment.FirstCommodityHazardousGoodsContactPhone);
			shipment.FirstCommodityHazardousGoodsContact = contact2.PK;
			AssertEquals(contact2.OC_Phone, shipment.FirstCommodityHazardousGoodsContactPhone);
			AssertEquals(contact2.PK, item1.BY_HazardousGoodsContact);
			AssertEquals(contact2.OC_Phone, item1.BY_HazardousGoodsContactPhone);
		}

		public void TestFirstCommodityWillBeCreatedWhenAssignValueOnShipment()
		{
			var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
			shipment.FirstCommodityDescription = "DESC";
			Factory.Save();
			var commodityPK = shipment.FirstCommodity.PK;
			var factory = new BusinessObjectFactory();
			var commodity = factory.Load<Commodity>(commodityPK);
			AssertNotNull(commodity);
		}

		public void TestCommodityIsAddedOnSetting()
		{
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				var shipment = GetNewBusinessObjectForDeleteTest(Factory) as Shipment;
				shipment.Commodities.DeleteAll();
				var firstCommodity = shipment.FirstCommodity;
				AssertEquals("firstCommodity.IsNull", true, firstCommodity.IsNull);
				AssertEquals("shipment.Commodities.Count", 0, shipment.Commodities.Count);
				shipment.B0_ManifestQty = 99;
				shipment.FirstCommodityDescription = "DESC";
				firstCommodity = shipment.FirstCommodity;
				AssertEquals("firstCommodity.IsNull", false, firstCommodity.IsNull);
				AssertEquals("shipment.Commodities.Count", 1, shipment.Commodities.Count);
				AssertSame("shipment.Commodities[0]", firstCommodity, shipment.Commodities[0]);
				AssertEquals("shipment.Commodities[0] Packages", 99, shipment.Commodities[0].BY_PieceCount);
				AssertEquals("firstCommodity.BY_Description", "DESC", firstCommodity.BY_Description);
				AssertEquals("shipment.Commodities[0] Description", "DESC", shipment.Commodities[0].BY_Description);
			}
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<Trip>().Shipments.AddNew();

		void AssertFirstCommodityPropertyValueChangedWorkCorrectly(ZPropertyInfo info, Action action, ZString firstValueChange, ZString secondValueChange)
		{
			var result = new ZStringBuilder();
			info.ValueChanged += (object sender, EventArgs e) =>
			{
				if (e is ValueChangedEventArgs va)
				{
					result.AppendLine($"Old='{va.OldValue}',New='{va.NewValue}'");
				}
			};
			var originalValue = info.Value;
			action.Invoke();
			AssertEquals("ValueChanged should be fired 4 times with correct parameters", $"Old='{originalValue}',New='{firstValueChange}'\r\nOld='{firstValueChange}',New='{info.DefaultValue}'\r\nOld='{info.DefaultValue}',New='{info.DefaultValue}'\r\nOld='{info.DefaultValue}',New='{secondValueChange}'", result.ToString().Trim());
		}
	}
}
