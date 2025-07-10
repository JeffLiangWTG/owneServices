using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WhsWarehouseDO = CargoWise.Database.TestFramework.ObjectModel.WhsWarehouse;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsWarehouse))]
	class WhsWarehouseTestCase : WhsEnvBusinessObjectTestCase
	{
		#region TestGetSSCCPrefix

		public void TestGetSSCCPrefix()
		{
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			warehouse.WW_OA_WarehouseAddress = mainAddress.PK;

			AssertEquals("Precondition: Warehouse.GetSSCCPrefix()", "", warehouse.GetSSCCPrefix());

			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1111111");
			AssertEquals("1111111", warehouse.GetSSCCPrefix());

			cusCode.OK_OA_PremisesAddress = ZGuid.NewZGuid();
			AssertEquals("", warehouse.GetSSCCPrefix());

			cusCode.OK_OA_PremisesAddress = mainAddress.PK;
			AssertEquals("1111111", warehouse.GetSSCCPrefix());

			cusCode.OK_OA_PremisesAddress = ZGuid.NewZGuid();
			var cusCode2 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "2222222");
			AssertEquals("2222222", warehouse.GetSSCCPrefix());

			cusCode.OK_OA_PremisesAddress = mainAddress.PK;
			AssertEquals("1111111", warehouse.GetSSCCPrefix());
		}

		#endregion

		#region TestCreateAndSaveState

		public void TestCreateAndSaveState()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals("Precondition", 0, warehouse.Logs.GetAllLogs().Count);
			Factory.Save();

			Assert("Auto Log must be created", warehouse.Logs.GetAllLogs().Count > 0);
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			var row1 = warehouse.Rows.AddNew();
			var row2 = warehouse.Rows.AddNew();
			var area1 = warehouse.Areas.AddNew();
			var area2 = warehouse.Areas.AddNew();
			var param1 = warehouse.ClientParameters.AddNew();
			var param2 = warehouse.ClientParameters.AddNew();

			warehouse.Delete();

			AssertEquals(true, warehouse.IsDeleted);
			AssertEquals(true, row1.IsDeleted);
			AssertEquals(true, row2.IsDeleted);
			AssertEquals(true, area1.IsDeleted);
			AssertEquals(true, area2.IsDeleted);
			AssertEquals(true, param1.IsDeleted);
			AssertEquals(true, param2.IsDeleted);
			AssertEquals(0, warehouse.Rows.Count);
			AssertEquals(0, warehouse.Areas.Count);
			AssertEquals(0, warehouse.ClientParameters.Count);
		}

		#endregion

		#region TestFetchStrategy

		public void TestFetchStrategy()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			AssertEquals(typeof(WhsWarehouseFetchStrategy), warehouse.FetchStrategy.GetType());
		}

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals("Default Area should be created", 1, warehouse.Areas.Count);
			AssertEquals("DEFAULT", warehouse.Areas[0].WA_Name);
			AssertEquals(CodeLists.AreaTypes.Codes.FreeStore, warehouse.Areas[0].WA_AreaType);
			AssertEquals("Packing Slip", warehouse.PackingSlipTitle);
		}

		#endregion

		#region Properties

		#region TestWW_ABCAnalysisEnabled

		public void TestWW_ABCAnalysisEnabled()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals(false, warehouse.WW_ABCAnalysisEnabled);

			warehouse.WW_ABCAnalysisEnabled = true;
			AssertEquals(true, warehouse.WW_ABCAnalysisEnabled);
		}

		#endregion

		#region TestBondedText

		public void TestBondedText()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertEquals("Should not be bond enabled.", "Not enabled for Bonded", warehouse.BondedText);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;
			AssertEquals("Should be bond enabled.", "Bonded", warehouse.BondedText);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Excise;
			AssertEquals("Should not be bond enabled.", "Not enabled for Bonded", warehouse.BondedText);
		}

		#endregion

		#region TestBondedTextInfo

		public void TestBondedTextInfo()
		{
			AssertEquals(256, Factory.New<WhsWarehouse>().BondedTextInfo.MaxLength);
		}

		#endregion

		#region TestCountryCode

		public void TestCountryCode()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			warehouse.WW_OA_WarehouseAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			AssertEquals(Enterprise.Core.Constants.CountryCodes.Australia, warehouse.CountryCode);

			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(Enterprise.Core.Constants.CountryCodes.UnitedStates, warehouse.CountryCode);

			warehouse.WarehouseAddress.OA_RN_NKCountryCode = ZString.Empty;
			AssertEquals(ZString.Empty, warehouse.CountryCode);

			warehouse.WW_OA_WarehouseAddress = ZGuid.Empty;
			AssertEquals(ZString.Empty, warehouse.CountryCode);
		}

		#endregion

		#region TestDGPhoneNumber()

		public void TestDGPhoneNumber()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertEquals("Precondition", "", warehouse.DGPhoneNumber);

			var org = warehouse.WarehouseAddress.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_HomePhone = "123";
			contact.OC_Mobile = "456";
			contact.OC_OtherPhone = "789";
			contact.OC_Phone = "101";

			warehouse.WW_OC_DGContact = contact.PK;
			AssertPhoneNumber(warehouse, PhoneTypeList.Codes.HOM, "123");
			AssertPhoneNumber(warehouse, PhoneTypeList.Codes.MOB, "456");
			AssertPhoneNumber(warehouse, PhoneTypeList.Codes.OTH, "789");
			AssertPhoneNumber(warehouse, PhoneTypeList.Codes.WRK, "101");
			AssertPhoneNumber(warehouse, "BAD", "");
		}

		void AssertPhoneNumber(WhsWarehouse whs, ZString phoneCode, ZString phoneNumber)
		{
			whs.WW_DGContactPhoneType = phoneCode;
			AssertEquals(phoneNumber, whs.DGPhoneNumber);
		}

		#endregion

		#region TestPackingSlipTitle

		public void TestPackingSlipTitle()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			warehouse.WW_GB_RelatedCompanyBranch = ZGuid.Empty;
			AssertEquals("Packing Slip", warehouse.PackingSlipTitle);

			var branchPK = Factory.NewWithValidTestData<GlbBranch>().PK;
			WarehouseDataRegistry.Instance.PackingSlipTitles.SetValue(Guid.Empty, branchPK.ToGuid(), Guid.Empty, "Packing Slip for test branch");
			warehouse.WW_GB_RelatedCompanyBranch = branchPK;
			AssertEquals("Packing Slip for test branch", warehouse.PackingSlipTitle);
		}

		#endregion

		#region TestExciseText

		public void TestExciseText()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertEquals("Should not be excise enabled.", "Not enabled for Excise", warehouse.ExciseText);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Excise;
			AssertEquals("Should be excise enabled.", "Excise", warehouse.ExciseText);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;
			AssertEquals("Should not be excise enabled.", "Not enabled for Excise", warehouse.ExciseText);
		}

		#endregion

		#region TestExciseTextInfo

		public void TestExciseTextInfo()
		{
			AssertEquals(256, Factory.New<WhsWarehouse>().ExciseTextInfo.MaxLength);
		}

		#endregion

		#region TestFreeStoreText

		public void TestFreeStoreText()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Excise;
			AssertEquals("Should not be Free Store enabled.", "Not enabled for Free Store", warehouse.FreeStoreText);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertEquals("Should be Free Store enabled.", "Free Store", warehouse.FreeStoreText);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;
			AssertEquals("Should not be Free Store enabled.", "Not enabled for Free Store", warehouse.FreeStoreText);
		}

		#endregion

		#region TestFreeStoreTextInfo

		public void TestFreeStoreTextInfo()
		{
			AssertEquals(256, Factory.New<WhsWarehouse>().FreeStoreTextInfo.MaxLength);
		}

		#endregion

		#region TestInwardProcessingText

		public void TestInwardProcessingText()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Excise;
			AssertEquals("Should not be Inward Processing enabled.", "Not enabled for Inward Processing", warehouse.InwardProcessingText);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.InwardProcessing;
			AssertEquals("Should be Inward Processing enabled.", "Inward Processing", warehouse.InwardProcessingText);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;
			AssertEquals("Should not be Inwards Processing enabled.", "Not enabled for Inward Processing", warehouse.InwardProcessingText);
		}

		#endregion

		#region TestInwardProcessingTextInfo

		public void TestInwardProcessingTextInfo()
		{
			AssertEquals(256, Factory.New<WhsWarehouse>().InwardProcessingTextInfo.MaxLength);
		}

		#endregion

		#region TestVATFiscalText

		public void TestVATFiscalText()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Excise;
			AssertEquals("Should not be VAT fiscal enabled.", "Not enabled for VAT Fiscal", warehouse.VATFiscalText);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.VATFiscal;
			AssertEquals("Should be VAT fiscal enabled.", "VAT Fiscal", warehouse.VATFiscalText);
		}

		#endregion

		#region TestVATFiscalTextInfo

		public void TestVATFiscalTextInfo()
		{
			AssertEquals(256, Factory.New<WhsWarehouse>().VATFiscalTextInfo.MaxLength);
		}

		#endregion

		#region TestWW_IsVirtualWarehouse_ReadOnly

		public void TestWW_IsVirtualWarehouse_ReadOnly()
		{
			var warehouse = Helper.CreateWarehouse("WHS");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertEquals(nameof(warehouse.WW_IsVirtualWarehouse) + " should be read only for Warehouse that is Container Yard type.", true, warehouse.WW_IsVirtualWarehouseInfo.ReadOnly);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertEquals(nameof(warehouse.WW_IsVirtualWarehouse) + " should NOT be read only for Warehouse that is Product type.", false, warehouse.WW_IsVirtualWarehouseInfo.ReadOnly);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertEquals(nameof(warehouse.WW_IsVirtualWarehouse) + " should be read only for Warehouse that is Transit Warehouse type.", true, warehouse.WW_IsVirtualWarehouseInfo.ReadOnly);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertEquals(nameof(warehouse.WW_IsVirtualWarehouse) + " should NOT be read only for Warehouse that is Product type.", false, warehouse.WW_IsVirtualWarehouseInfo.ReadOnly);
		}

		#endregion

		#region DetailTracking

		#region TestWW_FTZDetailedTrackingWarningPercentage

		public void TestWW_FTZDetailedTrackingWarningPercentage()
		{
			var warehouse = Helper.CreateWarehouse("WHS");

			warehouse.WW_FTZDetailedTrackingWarningPercentage = 10;
			AssertEquals("10", warehouse.WW_FTZDetailedTrackingWarningPercentage.ToString());

			warehouse.WW_FTZDetailedTrackingWarningPercentage = -1;
			AssertEquals("0", warehouse.WW_FTZDetailedTrackingWarningPercentage.ToString());

			warehouse.WW_FTZDetailedTrackingWarningPercentage = 50;
			AssertEquals("50", warehouse.WW_FTZDetailedTrackingWarningPercentage.ToString());

			warehouse.WW_FTZDetailedTrackingWarningPercentage = 100;
			AssertEquals("0", warehouse.WW_FTZDetailedTrackingWarningPercentage.ToString());
		}

		#endregion

		#region TestWW_WarehouseType_DisableDetailedTracking

		public void TestWW_WarehouseType_DisableDetailedTracking()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			warehouse.WW_FTZIsDetailedTrackingEnabled = true;

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertEquals(true, warehouse.WW_FTZIsDetailedTrackingEnabled);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertEquals(false, warehouse.WW_FTZIsDetailedTrackingEnabled);

			warehouse.WW_FTZIsDetailedTrackingEnabled = true;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertEquals(false, warehouse.WW_FTZIsDetailedTrackingEnabled);
		}

		#endregion

		#region TestWW_OA_WarehouseAddress_DisableDetailedTracking

		public void TestWW_OA_WarehouseAddress_DisableDetailedTracking()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			warehouse.WW_FTZIsDetailedTrackingEnabled = true;

			var uSAddress = Factory.New<OrgAddress>();
			uSAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			warehouse.WW_OA_WarehouseAddress = uSAddress.PK;
			AssertEquals(true, warehouse.WW_FTZIsDetailedTrackingEnabled);

			var pRAddress = Factory.New<OrgAddress>();
			pRAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.PuertoRico;
			warehouse.WW_OA_WarehouseAddress = pRAddress.PK;
			AssertEquals(true, warehouse.WW_FTZIsDetailedTrackingEnabled);

			var cNAddress = Factory.New<OrgAddress>();
			cNAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.China;
			warehouse.WW_OA_WarehouseAddress = cNAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertEquals(false, warehouse.WW_FTZIsDetailedTrackingEnabled);
		}

		#endregion

		#region TestIsFTZWarehouseInCountryThatUsesPermits

		public void TestIsFTZWarehouseInCountryThatUsesPermits()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertEquals(false, warehouse.IsFTZWarehouseInCountryThatUsesPermits);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(true, warehouse.IsFTZWarehouseInCountryThatUsesPermits);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.PuertoRico;
			AssertEquals(true, warehouse.IsFTZWarehouseInCountryThatUsesPermits);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.China;
			AssertEquals(false, warehouse.IsFTZWarehouseInCountryThatUsesPermits);
		}

		#endregion

		#region TestIsFTZWarehouse

		public void TestIsFTZWarehouse()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertEquals(false, warehouse.IsFTZWarehouse);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertEquals(true, warehouse.IsFTZWarehouse);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertEquals(false, warehouse.IsFTZWarehouse);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertEquals(false, warehouse.IsFTZWarehouse);
		}

		#endregion

		#endregion

		#region TestWW_IsDangerousGoodsManagementEnabled

		public void TestWW_IsDangerousGoodsManagementEnabled()
		{
			var trwWhs = Helper.CreateTRWWarehouse("AAA");
			AssertEquals("Precondition", false, trwWhs.WW_IsDangerousGoodsManagementEnabled);

			trwWhs.WW_IsDangerousGoodsManagementEnabled = true;
			AssertEquals("UNDG Limits should not be readonly", false, trwWhs.UNDGLimits.ReadOnly);

			trwWhs.WW_IsDangerousGoodsManagementEnabled = false;
			AssertEquals("UNDG Limits should be readonly", true, trwWhs.UNDGLimits.ReadOnly);
		}

		#endregion

		#region TestWW_DGThresholdPercentage

		public void TestWW_DGThresholdPercentage()
		{
			var trwWhs = Helper.CreateTRWWarehouse("AAA");
			AssertEquals("Precondition:", (ZByte)0, trwWhs.WW_DGThresholdPercentage);

			trwWhs.WW_DGThresholdPercentage = 50;
			AssertEquals((ZByte)50, trwWhs.WW_DGThresholdPercentage);

			trwWhs.WW_DGThresholdPercentage = 100;
			AssertEquals((ZByte)0, trwWhs.WW_DGThresholdPercentage);
		}

		public void TestWW_DGThresholdPercentage_ReadOnly()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			AssertEquals("Precondition:", false, warehouse.WW_IsDangerousGoodsManagementEnabled);
			AssertEquals(true, warehouse.WW_DGThresholdPercentageInfo.ReadOnly);

			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			AssertEquals(false, warehouse.WW_DGThresholdPercentageInfo.ReadOnly);
		}

		#endregion

		#region Flags

		#region TestIsApprovedKnown

		public void TestIsApprovedKnown()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals(false, warehouse.IsApprovedKnown);
		}

		#endregion

		#region TestIsApprovedKnownUS

		public void TestIsApprovedKnownUS()
		{
			US.Testing.WhsTestHelperFunctionsEnvUS helper = new US.Testing.WhsTestHelperFunctionsEnvUS(Factory);
			WhsWarehouse warehouse = helper.CreateWarehouse("TST");

			helper.SetWarehouseTSAStatus(warehouse, CodeLists.US.TSAStatus.Codes.Unknown);
			AssertEquals(nameof(warehouse.IsApprovedKnown), false, warehouse.IsApprovedKnown);
			AssertEquals(nameof(warehouse.IsApprovedKnown), false, warehouse.IsApprovedKnown);

			helper.SetWarehouseTSAStatus(warehouse, CodeLists.US.TSAStatus.Codes.Known);
			AssertEquals(nameof(warehouse.IsApprovedKnown), true, warehouse.IsApprovedKnown);
			AssertEquals(nameof(warehouse.IsApprovedKnown), true, warehouse.IsApprovedKnown);
		}

		#endregion

		#region TestIsUsingDockDoorLocation

		public void TestIsUsingDockDoorLocation()
		{
			var whs = Helper.CreateWarehouse("WH1");
			AssertEquals("Product warehouses must have a dock door location.", true, whs.IsUsingDockDoorLocation);

			// GE remove this, should not check isactive to determine whether or no a whs is using DDL.
			// AV I just extracted it out, i dont want to change it in this WI as it will need a transform (for trigger).
			whs.WW_IsActive = false;
			AssertEquals("For not active warehouses default dock door location is not enforced.", false, whs.IsUsingDockDoorLocation);
			whs.WW_IsActive = true; // clean up

			whs.WW_IsVirtualWarehouse = true;
			AssertEquals("For virtual warehouses default dock door location is not enforced.", false, whs.IsUsingDockDoorLocation);
			whs.WW_IsVirtualWarehouse = false; // clean up

			whs.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertEquals("Free Trade Zone warehouses must have dock door location.", true, whs.IsUsingDockDoorLocation);

			whs.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertEquals("For transit warehouses default dock door location is not enforced.", false, whs.IsUsingDockDoorLocation);
		}

		#endregion

		#region TestHasPackingConsolidationLocations

		public void TestHasPackingConsolidationLocations()
		{
			var whs = Helper.CreateWarehouse("WH1");
			AssertEquals("Should not have packing consolidation locations.", false, whs.HasPackingConsolidationLocations);

			var packingConsolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocationRow = Helper.CreateRowAndGenerateLocations(whs, "PS", 1, 1);
			var packingLocation = packingLocationRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;
			Factory.Save();
			AssertEquals("Should have packing consolidation locations.", true, whs.HasPackingConsolidationLocations);

			packingLocationRow.Delete();
			Factory.Save();
			AssertEquals("Should not have packing consolidation locations.", false, whs.HasPackingConsolidationLocations);
		}

		#endregion

		#endregion

		#region DefaultDockDoor

		public void TestAccessDefaultInboundDockDoor()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "OTHERDOCK");
			Factory.Save();

			var location = row.Locations.Single();
			location.WLV_WLT_LocationType = warehouse.DefaultInboundDockDoorLocation.WLV_WLT_LocationType;
			warehouse.WW_DefaultInboundDockDoor = location.PK;

			AssertEquals("DefaultInboundDockDoorLocation should return a WhsLocation", typeof(WhsLocation), warehouse.DefaultInboundDockDoorLocation.GetType());
			AssertEquals("DefaultInboundDockDoorLocation should return correct Location", location, warehouse.DefaultInboundDockDoorLocation);
		}

		public void TestAccessDefaultOutboundDockDoor()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "OTHERDOCK");
			Factory.Save();

			var location = row.Locations.Single();
			location.WLV_WLT_LocationType = warehouse.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;
			warehouse.WW_DefaultOutboundDockDoor = location.PK;

			AssertEquals("DefaultOutboundDockDoorLocation should return a WhsLocation", typeof(WhsLocation), warehouse.DefaultOutboundDockDoorLocation.GetType());
			AssertEquals("DefaultOutboundDockDoorLocation should return correct Location", location, warehouse.DefaultOutboundDockDoorLocation);
		}

		#endregion

		#region TestCreateDefaultDockDoorAreaRowAndLocation

		public void TestCreateDefaultDockDoorAreaRowAndLocation_ProductWarehouse()
		{
			TestCreateDefaultDockDoorAreaRowAndLocationCore(WarehouseTypes.Codes.Product, true);
		}

		public void TestCreateDefaultDockDoorAreaRowAndLocation_FreeTradeZoneWarehouse()
		{
			TestCreateDefaultDockDoorAreaRowAndLocationCore(WarehouseTypes.Codes.FreeTradeZone, true);
		}

		public void TestCreateDefaultDockDoorAreaRowAndLocation_TransitWarehouse()
		{
			TestCreateDefaultDockDoorAreaRowAndLocationCore(WarehouseTypes.Codes.Transit, false);
		}

		public void TestCreateDefaultDockDoorAreaRowAndLocationCore(string warehouseType, bool shouldGenerateDockDoor)
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: shouldGenerateDockDoor);
			warehouse.WW_WarehouseType = warehouseType;
			Factory.Save();

			if (shouldGenerateDockDoor)
			{
				AssertEquals("Default Dock Door Area has name '[DEFAULT DOCK DOOR]'", 1, warehouse.Areas.Count(a => a.WA_Name == "[DEFAULT DOCK DOOR]"));

				var dockDoorArea = warehouse.Areas.Single(a => a.WA_Name == "[DEFAULT DOCK DOOR]" && a.WA_AreaType == AreaTypes.Codes.DockDoor);
				var row = warehouse.Rows.Single(r => r.WR_Name == "DOCKDOOR");
				AssertEquals("Default Dock Door Row should have only 1 Location.", (ZShort)1, row.WR_Columns);
				AssertEquals("Default Dock Door Row should have only 1 Location.", (ZShort)1, row.WR_Levels);
				AssertEquals("Default Dock Door Row should have only 1 Location.", (ZShort)1, row.WR_Trays);
				AssertEquals("Default Dock Door Row should have only 1 Location.", 1, row.Locations.Count);

				var dockDoorLocation = row.Locations.Single();
				AssertEquals("Default Dock Door Location should be Column 1.", (ZShort)1, dockDoorLocation.WLV_Column);
				AssertEquals("Default Dock Door Location should be Level 1.", (ZShort)1, dockDoorLocation.WLV_Level);
				AssertEquals("Default Dock Door Location should be Tray 1.", (ZShort)1, dockDoorLocation.WLV_Tray);
				AssertEquals("Default Dock Door Location should have Location Type with DDL Location Class.", LocationClasses.Codes.DDL, dockDoorLocation.LocationType.WLT_LocationClass);
				AssertEquals("Default Dock Door Location should be assigned as Pick Area.", dockDoorArea.PK, dockDoorLocation.WLV_WA_PickingArea);
				AssertEquals("Default Dock Door Location should be assigned as Putaway Area.", dockDoorArea.PK, dockDoorLocation.WLV_WA_PutawayArea);
			}
			else
			{
				AssertEquals("Default Dock Door Area should not be created", 0, warehouse.Areas.Count(a => a.WA_Name == "[DEFAULT DOCK DOOR]"));
				AssertEquals("Default Dock Door Row should not be created", 0, warehouse.Rows.Count(r => r.WR_Name == "DOCKDOOR"));
			}
		}

		#region TestCreateDefaultDockDoorAreaRowAndLocation_NoLocationTypeWithClassDDL

		public void TestCreateDefaultDockDoorAreaRowAndLocation_NoLocationTypeWithClassDDL()
		{
			var query = new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL);
			var ddlLocationTypes = Factory.Load<WhsLocationType>(query);
			foreach (var lt in ddlLocationTypes)
			{
				lt.WLT_LocationClass = LocationClasses.Codes.NOR;
			}
			Factory.Save();

			var warehouse = Helper.CreateWarehouse("1");
			try
			{
				Factory.Save();
				Assert(false);
			}
			catch (Exception e)
			{
				AssertHasRowError("Should Not save warehouse without DDL Location Type", warehouse, "Please create a Location Type that has a class of DDL.");
				AssertEquals(true, e.Message.Contains("Constraint_OutboundDockDoorForActiveProductWarehouse") || e.Message.Contains("Constraint_InboundDockDoorForActiveProductWarehouse"));
			}
		}

		#endregion

		#endregion

		#region TestCreateDefaultArea

		public void TestCreateDefaultArea()
		{
			var warehouse = Helper.CreateWarehouse("1");

			((IWhsWarehouseInternals)warehouse).CreateDefaultArea();

			var defaultArea = warehouse.Areas.FirstOrDefault(a => a.WA_Name == "DEFAULT");
			AssertNotNull("Default Area has name 'DEFAULT'", defaultArea);
			AssertEquals(true, defaultArea.WA_IsPickingArea);
			AssertEquals(true, defaultArea.WA_IsPutawayArea);
			AssertEquals(true, defaultArea.WA_IsDefaultPickArea);
			AssertEquals(true, defaultArea.WA_IsDefaultPutawayArea);
		}

		#endregion

		#region TestDefaultLocation

		public void TestDefaultLocation()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);
			AssertEquals("Default Location is first", row.Locations[0], warehouse.DefaultLocation);

			row.Locations[0].WLV_LocationStatus = LocationStatus.Codes.Damaged;
			row.Locations[1].WLV_LocationStatus = LocationStatus.Codes.Void;
			row.Locations[2].WLV_LocationStatus = LocationStatus.Codes.Held;
			AssertEquals("Default Location is fourth", row.Locations[3], warehouse.DefaultLocation);

			var dockDoorLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC"));
			row.Locations[3].WLV_WLT_LocationType = dockDoorLocationType.PK;
			AssertEquals("No Default Location - Dock door is not valid for Default Location", null, warehouse.DefaultLocation);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.DDL;
			AssertEquals("No Default Location - Dock door is not valid for Default Location", null, warehouse.DefaultLocation);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.PST;
			AssertEquals("No Default Location - Packing station is not valid for Default Location", null, warehouse.DefaultLocation);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			AssertEquals("No Default Location - Packing consolidation location is not valid for Default Location", null, warehouse.DefaultLocation);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.FIX;
			AssertEquals("No Default Location - Fixed Pick Face location is not valid for Default Location", null, warehouse.DefaultLocation);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.DPF;
			AssertEquals("No Default Location - Dynamic Pick Face location is not valid for Default Location", null, warehouse.DefaultLocation);
		}

		#endregion

		#region TestDefaultLocation_ExcludesInwardProcessingAreas

		public void TestDefaultLocation_ExcludesInwardProcessingAreas()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2);
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");

			AssertEquals("Default Location returns first location.", location1, warehouse.DefaultLocation);

			var inwardProcessingArea = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			location1.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location1.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			AssertEquals("Default Location returns second location.", location2, warehouse.DefaultLocation);

			location2.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location2.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			AssertNull("Default Location returns null.", warehouse.DefaultLocation);
		}

		#endregion

		#region TestDefaultLocationInNonBondedArea

		public void TestDefaultLocationInNonBondedArea()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);
			AssertEquals("Whs without Bonded Area returns first location", row.Locations[0], warehouse.DefaultLocationInNonBondedArea);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;
			Factory.Save();
			row.Locations[0].WLV_LocationStatus = LocationStatus.Codes.Damaged;
			row.Locations[1].WLV_LocationStatus = LocationStatus.Codes.Void;
			row.Locations[2].WLV_LocationStatus = LocationStatus.Codes.Held;
			AssertEquals("Whs with Bonded Area returns null", null, warehouse.DefaultLocationInNonBondedArea);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.DDL;
			AssertEquals("No Default Location - Dock door is not valid for Default Location", null, warehouse.DefaultLocationInNonBondedArea);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.PST;
			AssertEquals("No Default Location - Packing station is not valid for Default Location", null, warehouse.DefaultLocationInNonBondedArea);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			AssertEquals("No Default Location - Packing consolidation location is not valid for Default Location", null, warehouse.DefaultLocationInNonBondedArea);
		}

		#endregion

		#region TestDefaultLocationInNonBondedArea_ExcludesInwardProcessingAreas

		public void TestDefaultLocationInNonBondedArea_ExcludesInwardProcessingAreas()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2);
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");

			AssertEquals("Default Location In Non Bonded Area returns first location.", location1, warehouse.DefaultLocationInNonBondedArea);

			var inwardProcessingArea = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			location1.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location1.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			AssertEquals("Default Location In Non Bonded Area returns second location.", location2, warehouse.DefaultLocationInNonBondedArea);

			location2.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location2.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			AssertNull("Default Location In Non Bonded Area returns null.", warehouse.DefaultLocationInNonBondedArea);
		}

		#endregion

		#region TestDefaultLocationInBondedArea

		public void TestDefaultLocationInBondedArea()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);
			AssertEquals("Whs without Bonded Area returns null", null, warehouse.DefaultLocationInBondedArea);

			warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;
			Factory.Save();
			row.Locations[0].WLV_LocationStatus = LocationStatus.Codes.Damaged;
			row.Locations[1].WLV_LocationStatus = LocationStatus.Codes.Void;
			row.Locations[2].WLV_LocationStatus = LocationStatus.Codes.Held;
			AssertEquals("Default Location is fourth", row.Locations[3], warehouse.DefaultLocationInBondedArea);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.DDL;
			AssertEquals("No Default Location - Dock door is not valid for Default Location", null, warehouse.DefaultLocationInBondedArea);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.PST;
			AssertEquals("No Default Location - Packing station is not valid for Default Location", null, warehouse.DefaultLocationInBondedArea);

			row.Locations[3].LocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			AssertEquals("No Default Location - Packing consolidation location is not valid for Default Location", null, warehouse.DefaultLocationInBondedArea);
		}

		#endregion

		#region TestDefaultLocationInInwardProcessingArea

		public void TestDefaultLocationInInwardProcessingArea()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 5);
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			var location3 = warehouse.FindLocation("A-3");
			var location4 = warehouse.FindLocation("A-4");
			var location5 = warehouse.FindLocation("A-5");

			AssertNull("Default Location In Inward Processing Area returns null.", warehouse.DefaultLocationInInwardProcessingArea);

			var bondedArea = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.Bonded);
			var inwardProcessingArea = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			location2.WLV_WA_PickingArea = bondedArea.PK;
			location2.WLV_WA_PutawayArea = bondedArea.PK;
			AssertNull("Default Location In Inward Processing Area returns null.", warehouse.DefaultLocationInInwardProcessingArea);

			location3.WLV_LocationStatus = LocationStatus.Codes.Held;
			location3.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location3.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			AssertNull("Default Location In Inward Processing Area returns null.", warehouse.DefaultLocationInInwardProcessingArea);

			location4.WLV_LocationStatus = LocationStatus.Codes.Damaged;
			location4.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location4.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			AssertNull("Default Location In Inward Processing Area returns null.", warehouse.DefaultLocationInInwardProcessingArea);

			location5.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location5.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			AssertEquals("Default Location In Inward Processing Area returns location 5.", location5, warehouse.DefaultLocationInInwardProcessingArea);

			location5.LocationType.WLT_LocationClass = LocationClasses.Codes.DDL;
			AssertEquals("No Default Location - Dock door is not valid for Default Location", null, warehouse.DefaultLocationInInwardProcessingArea);

			location5.LocationType.WLT_LocationClass = LocationClasses.Codes.PST;
			AssertEquals("No Default Location - Packing station is not valid for Default Location", null, warehouse.DefaultLocationInInwardProcessingArea);

			location5.LocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			AssertEquals("No Default Location - Packing consolidation location is not valid for Default Location", null, warehouse.DefaultLocationInInwardProcessingArea);
		}

		#endregion

		#region TestIsWarehouseFreeStoreEnabled

		public void TestIsWarehouseFreeStoreEnabled()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			warehouse.Areas.DeleteAll();
			AssertEquals("Should be freestore enabled", true, warehouse.IsWarehouseFreeStoreEnabled);

			warehouse.Areas.AddNew().WA_AreaType = CodeLists.AreaTypes.Codes.Bonded;
			AssertEquals("Should not be freestore enabled", false, warehouse.IsWarehouseFreeStoreEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.InwardProcessing;
			AssertEquals("Should not be freestore enabled", false, warehouse.IsWarehouseFreeStoreEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.FreeStore;
			AssertEquals("Should be freestore enabled", true, warehouse.IsWarehouseFreeStoreEnabled);

			warehouse.Areas[0].WA_AreaType = "fRe";
			AssertEquals("Should be freestore enabled", true, warehouse.IsWarehouseFreeStoreEnabled);
		}

		#endregion

		#region TestIsWarehouseBondEnabled

		public void TestIsWarehouseBondEnabled()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals("Should not be bond enabled", false, warehouse.IsWarehouseBondEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.Bonded;
			AssertEquals("Should be bond enabled", true, warehouse.IsWarehouseBondEnabled);

			warehouse.Areas[0].WA_AreaType = "bOn";
			AssertEquals("Should be bond enabled", true, warehouse.IsWarehouseBondEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.InwardProcessing;
			AssertEquals("Should not be bond enabled", false, warehouse.IsWarehouseBondEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.Excise;
			AssertEquals("Should not be bond enabled", false, warehouse.IsWarehouseBondEnabled);
		}

		#endregion

		#region TestIsWarehouseExciseEnabled

		public void TestIsWarehouseExciseEnabled()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals("Should not be excise enabled", false, warehouse.IsWarehouseExciseEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.Excise;
			AssertEquals("Should be excise enabled", true, warehouse.IsWarehouseExciseEnabled);

			warehouse.Areas[0].WA_AreaType = "eXc";
			AssertEquals("Should be excise enabled", true, warehouse.IsWarehouseExciseEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.Bonded;
			AssertEquals("Should not be excise enabled", false, warehouse.IsWarehouseExciseEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.InwardProcessing;
			AssertEquals("Should not be excise enabled", false, warehouse.IsWarehouseExciseEnabled);
		}

		#endregion

		#region TestIsInwardProcessingEnabled

		public void TestIsInwardProcessingEnabled()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals("Should not be inward processing enabled", false, warehouse.IsInwardProcessingEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.InwardProcessing;
			AssertEquals("Should be inward processing enabled", true, warehouse.IsInwardProcessingEnabled);

			warehouse.Areas[0].WA_AreaType = "iPr";
			AssertEquals("Should be inward processing enabled", true, warehouse.IsInwardProcessingEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.Bonded;
			AssertEquals("Should not be inward processing enabled", false, warehouse.IsInwardProcessingEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.Excise;
			AssertEquals("Should not be inward processing enabled", false, warehouse.IsInwardProcessingEnabled);
		}

		#endregion

		#region TestIsVATFiscalEnabled

		public void TestIsVATFiscalEnabled()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals("Should not be VAT fiscal enabled", false, warehouse.IsVATFiscalEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.VATFiscal;
			AssertEquals("Should be VAT fiscal enabled", true, warehouse.IsVATFiscalEnabled);

			warehouse.Areas[0].WA_AreaType = "vAt";
			AssertEquals("Should be VAT fiscal enabled", true, warehouse.IsVATFiscalEnabled);

			warehouse.Areas[0].WA_AreaType = CodeLists.AreaTypes.Codes.Bonded;
			AssertEquals("Should not be VAT fiscal enabled", false, warehouse.IsVATFiscalEnabled);
		}

		#endregion

		#region Location Alpha

		public void TestWW_LocationColumnsAlphaInfo()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals(false, warehouse.WW_LocationColumnsAlphaInfo.ReadOnly);
			warehouse.WW_LocationColumnsZeroBased = true;
			AssertEquals(true, warehouse.WW_LocationColumnsAlphaInfo.ReadOnly);
		}

		public void TestWW_LocationLevelsAlphaInfo()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals(false, warehouse.WW_LocationLevelsAlphaInfo.ReadOnly);
			warehouse.WW_LocationLevelsZeroBased = true;
			AssertEquals(true, warehouse.WW_LocationLevelsAlphaInfo.ReadOnly);
		}

		public void TestWW_LocationTraysAlphaInfo()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals(false, warehouse.WW_LocationTraysAlphaInfo.ReadOnly);
			warehouse.WW_LocationTraysZeroBased = true;
			AssertEquals(true, warehouse.WW_LocationTraysAlphaInfo.ReadOnly);
		}

		#endregion

		#region Location Zero Based

		public void TestWW_LocationColumnsZeroBasedInfo()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals(false, warehouse.WW_LocationColumnsZeroBasedInfo.ReadOnly);
			warehouse.WW_LocationColumnsAlpha = true;
			AssertEquals(true, warehouse.WW_LocationColumnsZeroBasedInfo.ReadOnly);
		}

		public void TestWW_LocationLevelsZeroBasedInfo()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals(false, warehouse.WW_LocationLevelsZeroBasedInfo.ReadOnly);
			warehouse.WW_LocationLevelsAlpha = true;
			AssertEquals(true, warehouse.WW_LocationLevelsZeroBasedInfo.ReadOnly);
		}

		public void TestWW_LocationTraysZeroBasedInfo()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals(false, warehouse.WW_LocationTraysZeroBasedInfo.ReadOnly);
			warehouse.WW_LocationTraysAlpha = true;
			AssertEquals(true, warehouse.WW_LocationTraysZeroBasedInfo.ReadOnly);
		}

		#endregion

		#region TestRFPickPackPrinterPK

		public void TestRFPickPackPrinterPK()
		{
			var printer = Helper.CreatePrintQueue("PRINTER");
			var warehouse = Factory.New<WhsWarehouse>();
			AssertEquals("RF Pick Pack Printer should be empty if there is no default printer created.", ZGuid.Empty, warehouse.RFPickPackPrinterPK);

			warehouse.RFPickPackPrinterPK = printer.PK;
			AssertEquals("RF Pick Pack Printer should return the printer that was set.", printer.PK, warehouse.RFPickPackPrinterPK);

			warehouse.RFPickPackPrinterPK = ZGuid.Invalid;
			AssertNoErrors("Validation should be suspended for setting the Printer PK on the StmDefaultPrinter.", StmDefaultPrinter.LoadDefaultPrinter(Factory, warehouse).SDP_SQ_PrinterInfo);
			AssertEquals("If Invalid Guid is set, the property should return Invalid Guid.", ZGuid.Invalid, warehouse.RFPickPackPrinterPK);
		}

		#endregion

		#region TestWarehouseTypeDescription

		public void TestWarehouseTypeDescription()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertEquals("Warehouse Type is 'PRW'", "Product Warehouse", warehouse.WarehouseTypeDescription);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertEquals("Warehouse Type is 'FTZ'", "Product Warehouse (FTZ)", warehouse.WarehouseTypeDescription);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertEquals("Warehouse Type is 'TRW'", "Transit Warehouse", warehouse.WarehouseTypeDescription);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertEquals("Warehouse Type is 'CYD'", "Container Yard", warehouse.WarehouseTypeDescription);
		}

		#endregion

		#region TestWW_WarehouseNameTranslatable

		public void TestWW_WarehouseNameTranslatable()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			AssertEquals("Pre-condition: WW_WarehouseName.", "W1", warehouse.WW_WarehouseName);

			string resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "W1").ResourceKey;
			AssertEquals("W1", warehouse.WW_WarehouseNameMultilingual);
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试区"));
				AssertEquals("测试区", warehouse.WW_WarehouseNameMultilingual);
			}
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var whs = Factory.New<WhsWarehouse>();
			AssertEquals("Warehouse", whs.HumanReadableName);

			whs.WW_WarehouseName = "whs1";
			AssertEquals("Warehouse whs1", whs.HumanReadableName);

			string resKey = whs.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(whs, "whs1").ResourceKey;
			AssertEquals("whs1", whs.WW_WarehouseNameMultilingual);

			using (Res.TemporarilySwitchLanguage("CHS"))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试区"));
				AssertEquals("Warehouse 测试区", whs.HumanReadableName); //mock translation, "Warehouse" is not translated
			}
		}

		#endregion

		#region TestWW_WarehouseType

		public void TestWW_WarehouseType_ProductWarehouse_UnableToSwitchToContainerYardWithUNDGs()
		{
			TestWW_WarehouseType_UnableToSwitchToContainerYardWithUNDGsCore(WarehouseTypes.Codes.Product);
		}

		public void TestWW_WarehouseType_FreeTradeZone_UnableToSwitchToContainerYardWithUNDGs()
		{
			TestWW_WarehouseType_UnableToSwitchToContainerYardWithUNDGsCore(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestWW_WarehouseType_TransitWarehouse_UnableToSwitchToContainerYardWithUNDGs()
		{
			TestWW_WarehouseType_UnableToSwitchToContainerYardWithUNDGsCore(WarehouseTypes.Codes.Transit);
		}

		public void TestWW_WarehouseType_UnableToSwitchToContainerYardWithUNDGsCore(string startingWarehouseType)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = startingWarehouseType;
			AssertEquals("Precondition", startingWarehouseType, warehouse.WW_WarehouseType);

			warehouse.WW_DGThresholdPercentage = 20;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			AssertEquals("Could not switch types due to non-zero threshold percentage.", startingWarehouseType, warehouse.WW_WarehouseType);

			warehouse.WW_DGThresholdPercentage = 0;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			AssertEquals("Could not switch types due to enabling UNDG Thresholds.", startingWarehouseType, warehouse.WW_WarehouseType);

			warehouse.WW_IsDangerousGoodsManagementEnabled = false;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			AssertEquals("Should switch types due to disabling UNDG Thresholds.", WarehouseTypes.Codes.ContainerYard, warehouse.WW_WarehouseType);
		}

		public void TestWW_WarehouseType_Contraint_WW_WarehouseType()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			Factory.Save();

			var dbConnected = (IDbConnected)Factory;

			AssertNoExceptionThrown(() => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseType(WarehouseTypes.Codes.ContainerYard, warehouse.PK)));
			AssertNoExceptionThrown(() => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseType(WarehouseTypes.Codes.FreeTradeZone, warehouse.PK)));
			AssertNoExceptionThrown(() => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseType(WarehouseTypes.Codes.Product, warehouse.PK)));
			AssertNoExceptionThrown(() => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseType(WarehouseTypes.Codes.Transit, warehouse.PK)));

			AssertExceptionThrown(typeof(SqlException), () => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseType("XYZ", warehouse.PK)));    // invalid type
		}

		string UpdateWarehouseType(string warehouseType, ZGuid warehousePK)
			=> WhsWarehouseDO.UpdateWhere(warehousePK.ToGuid())
				.Set(w => w.WW_WarehouseType, warehouseType).AsSQL();

		public void TestWW_WarehouseType_ChangeClearsReleaseGroup_PWH_TWH()
			=> TestWW_WarehouseType_ChangeClearsReleaseGroup(WarehouseTypes.Codes.Product, WarehouseTypes.Codes.Transit, shouldClear: true);

		public void TestWW_WarehouseType_ChangeClearsReleaseGroup_FTZ_CYD()
			=> TestWW_WarehouseType_ChangeClearsReleaseGroup(WarehouseTypes.Codes.FreeTradeZone, WarehouseTypes.Codes.ContainerYard, shouldClear: true);

		public void TestWW_WarehouseType_ChangeDoesNotClearReleaseGroup_PWH_FTZ()
			=> TestWW_WarehouseType_ChangeClearsReleaseGroup(WarehouseTypes.Codes.Product, WarehouseTypes.Codes.FreeTradeZone, shouldClear: false);

		public void TestWW_WarehouseType_ChangeDoesNotClearReleaseGroup_FTZ_PWH()
			=> TestWW_WarehouseType_ChangeClearsReleaseGroup(WarehouseTypes.Codes.FreeTradeZone, WarehouseTypes.Codes.Product, shouldClear: false);

		void TestWW_WarehouseType_ChangeClearsReleaseGroup(string originalWarehouseType, string newWarehouseType, bool shouldClear)
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = originalWarehouseType;
			warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;

			warehouse.WW_WarehouseType = newWarehouseType;
			if (shouldClear)
			{
				AssertEquals("Should have cleared the Release Group.", ZGuid.Empty, warehouse.WW_GG_ReleaseGroup);
			}
			else
			{
				AssertEquals("Should not have cleared the Release Group.", releaseGroup.PK, warehouse.WW_GG_ReleaseGroup);
			}
		}

		#endregion

		#region TestWarehouseTypeSupportsUNDGThresholds

		public void TestWarehouseTypeSupportsUNDGThresholds()
		{
			var warehouse = Helper.CreateWarehouse("WHS");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertEquals(true, warehouse.WarehouseTypeSupportsUNDGThresholds);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertEquals(true, warehouse.WarehouseTypeSupportsUNDGThresholds);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertEquals(true, warehouse.WarehouseTypeSupportsUNDGThresholds);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertEquals(false, warehouse.WarehouseTypeSupportsUNDGThresholds);
		}

		#endregion

		#region TestWarehouseTypeSupportsTaskManagement

		public void TestWarehouseTypeSupportsTaskManagement()
		{
			var warehouse = Helper.CreateWarehouse("WHS");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertEquals(true, warehouse.WarehouseTypeSupportsTaskManagement);

			warehouse.WW_WarehouseType = "prw";
			AssertEquals(true, warehouse.WarehouseTypeSupportsTaskManagement);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertEquals(false, warehouse.WarehouseTypeSupportsTaskManagement);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertEquals(true, warehouse.WarehouseTypeSupportsTaskManagement);

			warehouse.WW_WarehouseType = "fTz";
			AssertEquals(true, warehouse.WarehouseTypeSupportsTaskManagement);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertEquals(false, warehouse.WarehouseTypeSupportsTaskManagement);
		}

		#endregion

		#region TestWW_IsVirtualWarehouse_Constraint_WW_WarehouseIsVirtual

		public void TestWW_IsVirtualWarehouse_Constraint_WW_WarehouseIsVirtual()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			Factory.Save();

			var dbConnected = (IDbConnected)Factory;

			AssertNoExceptionThrown(() => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseTypeAndIsVirtualWarehouse(WarehouseTypes.Codes.ContainerYard, 0, warehouse.PK)));
			AssertNoExceptionThrown(() => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseTypeAndIsVirtualWarehouse(WarehouseTypes.Codes.FreeTradeZone, 0, warehouse.PK)));
			AssertNoExceptionThrown(() => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseTypeAndIsVirtualWarehouse(WarehouseTypes.Codes.Product, 0, warehouse.PK)));
			AssertNoExceptionThrown(() => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseTypeAndIsVirtualWarehouse(WarehouseTypes.Codes.Transit, 0, warehouse.PK)));

			AssertNoExceptionThrown(() => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseTypeAndIsVirtualWarehouse(WarehouseTypes.Codes.FreeTradeZone, 1, warehouse.PK)));
			AssertNoExceptionThrown(() => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseTypeAndIsVirtualWarehouse(WarehouseTypes.Codes.Product, 1, warehouse.PK)));

			AssertExceptionThrown(typeof(SqlException), () => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseTypeAndIsVirtualWarehouse(WarehouseTypes.Codes.ContainerYard, 1, warehouse.PK)));
			AssertExceptionThrown(typeof(SqlException), () => dbConnected.Connection.ExecuteNonQuery(UpdateWarehouseTypeAndIsVirtualWarehouse(WarehouseTypes.Codes.Transit, 1, warehouse.PK)));
		}

		string UpdateWarehouseTypeAndIsVirtualWarehouse(string warehouseType, int isVirtual, ZGuid warehousePK)
			=> WhsWarehouseDO.UpdateWhere(l => l.PK == warehousePK.ToGuid())
				.Set(w => w.WW_WarehouseType, warehouseType)
				.Set(w => w.WW_IsVirtualWarehouse, Convert.ToBoolean(isVirtual))
				.AsSQL();

		#endregion

		#region TestWW_WarehouseCode_Constraint_WW_WarehouseCode

		public void TestWW_WarehouseCode_Constraint_WW_WarehouseCode()
		{
			var dbConnected = (IDbConnected)Factory;
			AssertExceptionThrown(typeof(SqlException), () => new WhsWarehouseDO("", "PRW") { WW_WarehouseName = "TEST Empty Code Warehouse" }.Insert(dbConnected.Connection));

			var warehouse = Helper.CreateWarehouse("WHS");
			Factory.Save();

			AssertExceptionThrown(typeof(SqlException), () => WhsWarehouseDO.UpdateWhere(w => w.PK == warehouse.PK).Set(w => w.WW_WarehouseCode, "").Post(TestConnection));
		}

		#endregion

		#region TestWW_TransitSecurityProcessingRequired

		public void TestWW_TransitSecurityProcessingRequired()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals(false, warehouse.WW_TransitSecurityProcessingRequired);
		}

		#endregion

		#region TestWW_AllowPartialLoadingDefault

		public void TestWW_AllowPartialLoadingDefault()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals(false, warehouse.WW_AllowPartialLoadingDefault);
		}

		#endregion

		#region

		public void TestIsTaskManagementEnabled_Enabled() => TestIsTaskManagementEnabledCore(isTaskManagementEnabled: true);
		public void TestIsTaskManagementEnabled_Disabled() => TestIsTaskManagementEnabledCore(isTaskManagementEnabled: false);

		void TestIsTaskManagementEnabledCore(bool isTaskManagementEnabled)
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var warehouse = Helper.CreateWarehouse("WHS");
			if (isTaskManagementEnabled)
			{
				warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			}

			AssertEquals(isTaskManagementEnabled, warehouse.IsTaskManagementEnabled);
		}

		#endregion

		#endregion

		#region TestWarehouseAddress

		public void TestWarehouseAddress()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			OrgAddress address = Factory.New<OrgAddress>();
			warehouse.WW_OA_WarehouseAddress = address.PK;
			AssertEquals(address.PK, warehouse.WarehouseAddress.PK);
		}

		#endregion

		#region TestClientParameters

		public void TestClientParameters()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			var param = Factory.New<WhsClientParameterByWarehouse>();
			param.WY_WW_Whs = warehouse.PK;

			var clientParams = warehouse.ClientParameters;
			AssertNotNull(clientParams);
			AssertEquals("Should be loaded", 1, clientParams.Count);
			AssertEquals("Should be cached", clientParams, warehouse.ClientParameters);
			AssertEquals(true, warehouse.IsRegisteredEditableChildObject(clientParams));
		}

		#endregion

		#region TestRows

		public void TestRows()
		{
			var whs = Factory.New<WhsWarehouse>();
			var row1 = Factory.New<WhsRow>();
			var row2 = Factory.New<WhsRow>();
			row1.WR_WW_Whs = whs.PK;
			row2.WR_WW_Whs = whs.PK;
			AssertEquals("Should be loaded", 2, whs.Rows.Count);
		}

		#endregion

		#region TestAreas

		public void TestAreas()
		{
			int bondTextRefreshHitCount = 0;
			int exciseTextRefreshHitCount = 0;
			int freeStoreTextRefreshHitCount = 0;
			var warehouse = Helper.CreateWarehouse("1");
			warehouse.BondedTextInfo.ValueChanged += (sender, e) => bondTextRefreshHitCount++;
			warehouse.ExciseTextInfo.ValueChanged += (sender, e) => exciseTextRefreshHitCount++;
			warehouse.FreeStoreTextInfo.ValueChanged += (sender, e) => freeStoreTextRefreshHitCount++;
			var area1 = Factory.NewWithValidTestData<WhsArea>();
			var area2 = Factory.NewWithValidTestData<WhsArea>();
			area1.WA_WW_Whs = warehouse.PK;
			AssertEquals("When Area is added to Collection, Warehouse Transaction Type should refresh.", 1, bondTextRefreshHitCount);
			AssertEquals("When Area is added to Collection, Warehouse Transaction Type should refresh.", 1, exciseTextRefreshHitCount);
			AssertEquals("When Area is added to Collection, Warehouse Transaction Type should refresh.", 1, freeStoreTextRefreshHitCount);

			area2.WA_WW_Whs = warehouse.PK;
			AssertEquals("When Area is added to Collection, Warehouse Transaction Type should refresh.", 2, bondTextRefreshHitCount);
			AssertEquals("When Area is added to Collection, Warehouse Transaction Type should refresh.", 2, exciseTextRefreshHitCount);
			AssertEquals("When Area is added to Collection, Warehouse Transaction Type should refresh.", 2, freeStoreTextRefreshHitCount);
			AssertEquals(true, warehouse.IsRegisteredEditableChildObject(warehouse.Areas));

			area1.Delete();
			AssertEquals("When Area is deleted from Collection, Warehouse Transaction Type should refresh.", 3, bondTextRefreshHitCount);
			AssertEquals("When Area is deleted from Collection, Warehouse Transaction Type should refresh.", 3, exciseTextRefreshHitCount);
			AssertEquals("When Area is deleted from Collection, Warehouse Transaction Type should refresh.", 3, freeStoreTextRefreshHitCount);

			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var warehouseInOtherFactory = otherFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Should be loaded", 3, warehouseInOtherFactory.Areas.Count);
		}

		#endregion

		#region TestUNDGLimits

		public void TestUNDGLimits()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, "0004a");
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, "3005A");
			Factory.Save();

			AssertEquals("Should be loaded", 2, warehouse.UNDGLimits.Count);

			AssertEquals("Precondition:", false, warehouse.WW_IsDangerousGoodsManagementEnabled);
			AssertEquals("UNDGLimits collection should be readonly when DG Limits not enabled", true, warehouse.UNDGLimits.ReadOnly);

			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			AssertEquals("UNDGLimits collection should not be readonly when DG Limits enabled", false, warehouse.UNDGLimits.ReadOnly);
		}

		#endregion

		#region TestGenerateLocations

		public void TestGenerateLocations()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			// Create some new rows with some columns, levels & trays
			// Generate the Locations for the rows
			// Check the number of generated locations is correct

			warehouse.Rows.AddNew();
			warehouse.Rows.AddNew();

			WhsRow row;

			// create 2 new rows
			row = warehouse.Rows[0];
			row.WR_Name = "TEST1";
			row.WR_Columns = 4;
			row.WR_Levels = 3;
			row.WR_Trays = 2;
			int row1Locs = (row.WR_Columns * row.WR_Levels * row.WR_Trays);

			row = warehouse.Rows[1];
			row.WR_Name = "TEST2";
			row.WR_Columns = 1;
			row.WR_Levels = 1;
			row.WR_Trays = 1;
			int row2Locs = (row.WR_Columns * row.WR_Levels * row.WR_Trays);
			int origLocs = row1Locs + row2Locs;

			((IWhsWarehouseInternals)warehouse).GenerateLocations();

			int generatedLocs = warehouse.Rows[0].Locations.Count + warehouse.Rows[1].Locations.Count;
			AssertEquals("Test1: Locations the warehouse should contain", row1Locs + row2Locs, generatedLocs);

			// modify 1 of the rows
			row = warehouse.Rows[0];
			row.WR_Columns = 1;
			row.WR_Levels = 4;
			row.WR_Trays = 1;
			row1Locs = (row.WR_Columns * row.WR_Levels * row.WR_Trays);

			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			generatedLocs = warehouse.Rows[0].Locations.Count + warehouse.Rows[1].Locations.Count;
			AssertEquals("Test2: Locations the warehouse should contain", row1Locs + row2Locs, generatedLocs);

			// set rows back to original state
			row = warehouse.Rows[0];
			row.WR_Columns = 4;
			row.WR_Levels = 3;
			row.WR_Trays = 2;
			row1Locs = (row.WR_Columns * row.WR_Levels * row.WR_Trays);

			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			generatedLocs = warehouse.Rows[0].Locations.Count + warehouse.Rows[1].Locations.Count;
			AssertEquals("Test3: Locations the warehouse should contain", row1Locs + row2Locs, generatedLocs);
			AssertEquals("Test4: Warehouse location collection count", origLocs, generatedLocs);
		}

		#endregion

		#region TestFindLocation

		public void TestFindLocation()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);

			var diffWarehouse = Helper.CreateWarehouse("WHS");
			var rowInDiffWarehouse = Helper.CreateRowAndGenerateLocations(diffWarehouse, "A", 2, 2);
			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "A-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "A-1-2");
			var locationInDiffWarehouse = rowInDiffWarehouse.Locations.Single(l => l.WLV_LocationString == "A-1-1");

			AssertNull(warehouse.FindLocation(""));
			AssertNull(warehouse.FindLocation("A-3"));
			AssertEquals(location1.PK, warehouse.FindLocation("A-1-1").PK);
			AssertEquals(location2.PK, warehouse.FindLocation("A-1-2").PK);
			AssertEquals(locationInDiffWarehouse.PK, diffWarehouse.FindLocation("A-1-1").PK);
		}

		public void TestFindLocation_FixedWidthLocation()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);
			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "A001001");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "A001002");

			AssertNull(warehouse.FindLocation(""));
			AssertNull(warehouse.FindLocation("A-3"));
			AssertNull(warehouse.FindLocation("A3"));
			AssertEquals(location1.PK, warehouse.FindLocation("A001").PK);
			AssertEquals(location1.PK, warehouse.FindLocation("A001001").PK);
			AssertEquals(location2.PK, warehouse.FindLocation("A001002").PK);
		}

		#endregion

		#region TestFindLocationParts

		public void TestFindLocationParts()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			Factory.Save();

			AssertNull(warehouse.FindLocationParts("SHEEP"));
			AssertNull(warehouse.FindLocationParts("A-5"));
			AssertLocationParts(row, "A", 1, 1, 1, warehouse);
			AssertLocationParts(row, "A-4", 4, 1, 1, warehouse);
			AssertLocationParts(row, "A-1-2-2", 1, 2, 2, warehouse);
			AssertLocationParts(row, "A-3-1-2", 3, 1, 2, warehouse);
			AssertLocationParts(row, "A-4-2-1", 4, 2, 1, warehouse);
		}

		public void TestFindLocationParts_FixedWidthLocation()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			Factory.Save();

			AssertNull(warehouse.FindLocationParts("SHEEP"));
			AssertNull(warehouse.FindLocationParts("A-5"));
			AssertNull(warehouse.FindLocationParts("A-4"));
			AssertNull(warehouse.FindLocationParts("A4"));
			AssertLocationParts(row, "A", 1, 1, 1, warehouse);
			AssertLocationParts(row, "A004", 4, 1, 1, warehouse);
			AssertLocationParts(row, "A004002", 4, 2, 1, warehouse);
			AssertLocationParts(row, "A00100202", 1, 2, 2, warehouse);
			AssertLocationParts(row, "A00400201", 4, 2, 1, warehouse);
		}

		void AssertLocationParts(WhsRow row, ZString locationString, short? expectColumn, short? expectLevel, short? expectTray, WhsWarehouse whs)
		{
			var warehouse = whs;
			WhsWarehouse.LocationParts parts = warehouse.FindLocationParts(locationString);
			AssertEquals(parts.Row, row);
			AssertEquals(parts.Column, expectColumn);
			AssertEquals(parts.Level, expectLevel);
			AssertEquals(parts.Tray, expectTray);
		}

		#endregion

		#region TestOperationalActionsFieldVisibility

		public void TestOperationalActionsFieldVisibility()
		{
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsWarehouse).GetProperty(WhsWarehouseSchema.WW_WarehouseCode.Name)).ReadOnly);
		}

		#endregion

		#region TestSetUpBondedWarehouse

		public void TestSetUpBondedWarehouse()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.OA_Address1 = "CargoWise Warehouse";

			var warehouse = Helper.CreateWarehouse("", shouldPreGenerateDDL: false);
			AssertEquals("Precondition: Warehouse should not have any rows created.", 0, warehouse.Rows.Count);

			warehouse.SetUpBondedWarehouse(address);
			AssertEquals("Warehouse Type", WarehouseTypes.Codes.Product, warehouse.WW_WarehouseType);
			AssertEquals("Warehouse should have the same address as the one created from", warehouse.WarehouseAddress.PK, address.PK);
			AssertEquals("Warehouse name should be Address1", warehouse.WW_WarehouseName, address.OA_Address1);
			AssertEquals("Should be a bond warehouse", true, warehouse.WW_IsBondedWarehouse);
			AssertEquals("Should be a bond warehouse", true, warehouse.WW_IsVirtualWarehouse);
			AssertEquals("Should be bond enabled", true, warehouse.IsWarehouseBondEnabled);
			AssertEquals("Don't AutoPrint Picking slip", false, warehouse.WW_AutoPrintPickingSlip);
			AssertEquals("Don't AutoPrint Packing slip", false, warehouse.WW_AutoPrintPackingSlip);
			AssertEquals("One row shoud be created against this warehouse", warehouse.Rows.Count, 1);

			var row = warehouse.Rows.Single();
			AssertEquals("Row Name", "BOND", row.WR_Name);
			AssertEquals("Column", (ZShort)1, row.WR_Columns);
			AssertEquals("Level", (ZShort)1, row.WR_Levels);
			AssertEquals("Tray", (ZShort)1, row.WR_Trays);

			AssertEquals("Precondition", "", warehouse.WW_WarehouseCode);
			Factory.Save();
			AssertEquals("1", warehouse.WW_WarehouseCode);

			warehouse.Rows.DeleteAll();
			warehouse.SetUpBondedWarehouse(address, WarehouseTypes.Codes.FreeTradeZone);
			AssertEquals("1", warehouse.WW_WarehouseCode);
			AssertEquals("Warehouse Type", WarehouseTypes.Codes.FreeTradeZone, warehouse.WW_WarehouseType);
			AssertEquals("Warehouse should have the same address as the one created from", warehouse.WarehouseAddress.PK, address.PK);
			AssertEquals("Warehouse name should be Address1", warehouse.WW_WarehouseName, address.OA_Address1);
			AssertEquals("Should be a bond warehouse", true, warehouse.WW_IsBondedWarehouse);
			AssertEquals("Should be a bond warehouse", true, warehouse.WW_IsVirtualWarehouse);
			AssertEquals("Should be bond enabled", true, warehouse.IsWarehouseBondEnabled);
			AssertEquals("Don't AutoPrint Picking slip", false, warehouse.WW_AutoPrintPickingSlip);
			AssertEquals("Don't AutoPrint Packing slip", false, warehouse.WW_AutoPrintPackingSlip);
			AssertEquals("One row shoud be created against this warehouse", warehouse.Rows.Count, 1);
			row = warehouse.Rows[0];
			AssertEquals("Row Name", "BOND", row.WR_Name);
			AssertEquals("Column", (ZShort)1, row.WR_Columns);
			AssertEquals("Level", (ZShort)1, row.WR_Levels);
			AssertEquals("Tray", (ZShort)1, row.WR_Trays);
		}

		#endregion

		#region TestSave

		public void TestSave_PopulatesVirtualWarehouseCodeIfRequired()
		{
			var virtualWarehouse1 = Helper.CreateWarehouse("Virtual Warehouse 1", "", "A");
			virtualWarehouse1.WW_IsVirtualWarehouse = true;
			Factory.Save();
			AssertEquals("1", virtualWarehouse1.WW_WarehouseCode);

			var virtualWarehouse2 = Helper.CreateWarehouse("Virtual Warehouse 2", "", "A");
			virtualWarehouse2.WW_IsVirtualWarehouse = true;
			Factory.Save();
			AssertEquals("2", virtualWarehouse2.WW_WarehouseCode);
		}

		public void TestSave_PopulatesVirtualWarehouseCodeIfRequired_MaxAttemptsReached()
		{
			// create warehouses to use up codes 1 through 999
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var whsAddress = org.MainAddress;
			var currentBranch = GlbBranch.CurrentBranch;
			for (int i = 1; i < 1000; i++)
			{
				// Helper.CreateWarehouse() is too slow due to NewWithValidTestData() creating a new org/addy etc for each whs.
				var whs = Helper.CreateWarehouse(i.ToString(), whsAddress, currentBranch);
				whs.WW_IsVirtualWarehouse = true;
			}
			Factory.Save();

			var virtualWarehouse = Helper.CreateWarehouse("Virtual Warehouse", "", "A");
			virtualWarehouse.WW_IsVirtualWarehouse = true;
			AssertExceptionThrown("Codes 1-999 are already used, because constraint WW_WarehouseCode still cannot save successfully.", typeof(ZSaveException), () => Factory.Save());
		}

		public void TestSave_DefaultOutboundDockDoor()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals("DefaultOutboundDockDoor is not set yet", ZGuid.Empty, warehouse.WW_DefaultOutboundDockDoor);
			Factory.Save();
			AssertNotEquals("DefaultOutboundDockDoor should be set now", ZGuid.Empty, warehouse.WW_DefaultOutboundDockDoor);
		}

		public void TestSave_PopulatesDefaultInboundDockDoor()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals("DefaultInboundDockDoor is not set yet", ZGuid.Empty, warehouse.WW_DefaultInboundDockDoor);
			Factory.Save();
			AssertNotEquals("DefaultInboundDockDoor should be set now", ZGuid.Empty, warehouse.WW_DefaultInboundDockDoor);
		}

		public void TestSave_PopulatesDefaultOutboundDockDoor()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			AssertEquals("DefaultOutboundDockDoor is not set yet", ZGuid.Empty, warehouse.WW_DefaultOutboundDockDoor);
			Factory.Save();
			AssertNotEquals("DefaultOutboundDockDoor should be set now", ZGuid.Empty, warehouse.WW_DefaultOutboundDockDoor);
		}

		public void TestSave_TriggerUpdatePackageStateSecurityStatusIfSecurityProcessingRequiredChangesToTrue() => TestSave_TriggerUpdatePackageStateSecurityStatusIfSecurityProcessingRequiredChanges(true);

		public void TestSave_TriggerUpdatePackageStateSecurityStatusIfSecurityProcessingRequiredChangesToFalse() => TestSave_TriggerUpdatePackageStateSecurityStatusIfSecurityProcessingRequiredChanges(false);

		void TestSave_TriggerUpdatePackageStateSecurityStatusIfSecurityProcessingRequiredChanges(bool newStatus)
		{
			ErrorReporter.Clear();
			var checkProcedureName = "dbo.UpdatePackageStateSecurityStatus";
			var mockedCheckProcedure = "\r\nALTER PROC " + checkProcedureName + " (@CompanyBranchPK UNIQUEIDENTIFIER, @SystemLastEditUser VARCHAR(3), @RegistryValue BIT, @WarehouseConfigurationValue BIT, @PackageStatePKs dbo.TVP_uniqueidentifier READONLY) AS\r\nBEGIN\r\n\tDECLARE @result varchar(max) = CONCAT(CONVERT(NVARCHAR(36), @CompanyBranchPK), ',',  @SystemLastEditUser, ',', @RegistryValue, ',', @WarehouseConfigurationValue, ',', (SELECT COUNT(*) FROM @PackageStatePKs))\r\n\tRAISERROR(@result, 18, 1)\r\n\tRETURN 1\r\nEND";
			Db.Connection.ExecuteNonQuery(mockedCheckProcedure);

			var warehouse = Helper.CreateWarehouse("TWH");
			warehouse.WW_TransitSecurityProcessingRequired = !newStatus;
			AssertEquals($"Precondition: WW_TransitSecurityProcessingRequired is {!newStatus}", !newStatus, warehouse.WW_TransitSecurityProcessingRequired);
			Factory.Save();

			warehouse.WW_TransitSecurityProcessingRequired = newStatus;
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			Factory.Save();

			var branchPK = warehouse.WW_GB_RelatedCompanyBranch.IsValid ? warehouse.WW_GB_RelatedCompanyBranch.ToGuid() : Guid.Empty;
			var expectedErrorReportMessage = branchPK + "," + ((IGlbStaff)Env.CurrentUser).GS_Code + "," + "," + Convert.ToInt32(newStatus) + ",0";
			AssertEquals("Error Reported", expectedErrorReportMessage.ToUpper(), ErrorReporter.LastMessageReported.ToUpper());
			ErrorReporter.Clear();
		}

		public void TestSave_TriggerUpdatePackageStateSecurityStatusIfSecurityProcessingRequiredChanges_NoChanges()
		{
			ErrorReporter.Clear();
			var checkProcedureName = "UpdatePackageStateSecurityStatus";
			var mockedCheckProcedure = "\r\nALTER PROC " + checkProcedureName + " (@CompanyBranchPK UNIQUEIDENTIFIER, @SystemLastEditUser VARCHAR(3), @RegistryValue BIT, @WarehouseConfigurationValue BIT, @PackageStatePKs dbo.TVP_uniqueidentifier READONLY) AS\r\nBEGIN\r\n\tDECLARE @result VARCHAR(MAX) = CONCAT(CONVERT(NVARCHAR(36), @CompanyBranchPK), ',', @SystemLastEditUser, ',', @RegistryValue, ',', @WarehouseConfigurationValue, ',', (SELECT COUNT(*) FROM @PackageStatePKs))\r\n\tRAISERROR(@result, 18, 1)\r\n\tRETURN 1\r\nEND";
			Db.Connection.ExecuteNonQuery(mockedCheckProcedure);

			var warehouse = Helper.CreateWarehouse("TWH");
			warehouse.WW_TransitSecurityProcessingRequired = false;
			Factory.Save();

			warehouse.WW_TransitSecurityProcessingRequired = true;
			warehouse.WW_TransitSecurityProcessingRequired = false;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure_WhenWarehouseCustomsControlledChangedToFalse() => TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure(false, false);

		public void TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure_WhenWarehouseCustomsControlledChangedToTrue() => TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure(true, false);

		public void TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure_WhenWarehousePortAuthorityControlChangedToFalse() => TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure(false, false);

		public void TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure_WhenWarehousePortAuthorityControlChangedToTrue() => TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure(false, true);

		public void TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure_WhenIsApplyPackageQuantityCountingAlgorithmChangedToTrue() => TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure(true, true, true);

		void TestOnSave_RunUpdatePackageStateAndRCNCustomStatusStoreProcedure(bool isCustomsControlled, bool isPortAuthorityControlled, bool isApplyPackageQuantityCountingAlgorithm = false)
		{
			ErrorReporter.Clear();
			var checkProcedureName = "dbo.UpdatePackageStateAndRCNCustomStatus";
			var mockedCheckProcedure = "\r\nALTER PROC " + checkProcedureName +
				" (@CompanyBranchPK UNIQUEIDENTIFIER, " +
				"@SystemLastEditUser VARCHAR(3), " +
				"@CurrentUTC DATETIME, " +
				"@WarehouseConfigCustomControlled BIT, " +
				"@WarehouseConfigPortControlled BIT, " +
				"@IsApplyPackageQuantityCountingAlgorithm BIT, " +
				"@PackageStatePKs dbo.TVP_uniqueidentifier READONLY) " +
				"AS\r\nBEGIN\r\n\tDECLARE @result VARCHAR(MAX) = CONCAT(CONVERT(NVARCHAR(36), @CompanyBranchPK), " +
				"',', @SystemLastEditUser, " +
				"',', @CurrentUTC, " +
				"',', @WarehouseConfigCustomControlled, " +
				"',', @WarehouseConfigPortControlled, " +
				"',', @IsApplyPackageQuantityCountingAlgorithm, " +
				"',', (SELECT COUNT(*) FROM @PackageStatePKs))\r\n\tRAISERROR(@result, 18, 1)\r\n\tRETURN 1\r\nEND";

			Db.Connection.ExecuteNonQuery(mockedCheckProcedure);

			var warehouse = Helper.CreateWarehouse("TWH");
			warehouse.WW_IsCustomsControlled = !isCustomsControlled;
			warehouse.WW_IsPortAuthorityControlled = !isPortAuthorityControlled;
			AssertEquals($"Precondition: WW_IsCustomsControlled is {!isCustomsControlled}", !isCustomsControlled, warehouse.WW_IsCustomsControlled);
			AssertEquals($"Precondition: WW_IsPortAuthorityControlled is {isPortAuthorityControlled}", !isPortAuthorityControlled, warehouse.WW_IsPortAuthorityControlled);

			Factory.Save();
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			warehouse.WW_IsCustomsControlled = isCustomsControlled;
			warehouse.WW_IsPortAuthorityControlled = isPortAuthorityControlled;

			var branchPK = warehouse.WW_GB_RelatedCompanyBranch.IsValid ? warehouse.WW_GB_RelatedCompanyBranch.ToGuid() : Guid.Empty;
			WarehouseDataRegistry.Instance.ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation.SetValue(Guid.Empty, branchPK, Guid.Empty, isApplyPackageQuantityCountingAlgorithm);

			Factory.Save();

			var expectedErrorReportMessage = branchPK + "," +
				((IGlbStaff)Env.CurrentUser).GS_Code + "," +
				/* CurrentUTC */ "," +
				Convert.ToInt16(warehouse.WW_IsCustomsControlled) + "," +
				Convert.ToInt16(warehouse.WW_IsPortAuthorityControlled) + "," +
				Convert.ToInt16(isApplyPackageQuantityCountingAlgorithm) + "," +
				0;

			AssertEquals("Error Reported", expectedErrorReportMessage.ToUpper(), ErrorReporter.LastMessageReported.ToUpper());
			ErrorReporter.Clear();
		}

		#endregion

		#region TestIDocAddresses

		public void TestIDocAddresses()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			IDocAddresses docAddy = warehouse;

			AssertEquals(warehouse, docAddy.DocAddresses.Master);
			AssertEquals(Env.Security.Warehouse, docAddy.GetCanOverrideCheckpoint(null));
			AssertNull(docAddy.GetDocAddressRequirement(DocAddressType.PickUpAddress));
			AssertEquals("Warehouse " + warehouse.WW_WarehouseCode, docAddy.HumanReadableName);
			AssertNull(docAddy.PiggyBackedDocAddressValidation(null));
			AssertCollectionContains(DocAddressType.PickUpAddress, docAddy.SupportedAddressTypes);
		}

		#endregion

		#region TestWorkTime

		public void TestWorkTime()
		{
			var warehouse1 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse1.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			AssertNotNull(warehouse1.WorkTimes.ParentID);
			AssertEquals(warehouse1.PK, warehouse1.WorkTimes.ParentID);
			AssertEquals(WhsWarehouseSchema.Constants.Prefix, warehouse1.WorkTimes.ParentTableCode);

			warehouse1.WorkTimes.MondayWorkingHours = "**              ***";
			warehouse1.WorkTimes.TuesdayWorkingHours = "***********************************************";
			warehouse1.WorkTimes.WednesdayWorkingHours = "      *****     ";
			warehouse1.WorkTimes.ThursdayWorkingHours = "      *";
			warehouse1.WorkTimes.FridayWorkingHours = "           *********";
			warehouse1.WorkTimes.SaturdayWorkingHours = "           ";
			warehouse1.WorkTimes.SundayWorkingHours = "           ";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var warehouse2 = factory2.Load<WhsWarehouse>(warehouse1.PK);
			factory2.Save();
			AssertNotNull(warehouse2.WorkTimes.ParentID);
			AssertEquals(warehouse2.PK, warehouse2.WorkTimes.ParentID);
			AssertEquals(WhsWarehouseSchema.Constants.Prefix, warehouse2.WorkTimes.ParentTableCode);
			AssertEquals("**              ***", warehouse2.WorkTimes.MondayWorkingHours);
			AssertEquals("***********************************************", warehouse2.WorkTimes.TuesdayWorkingHours);
			AssertEquals("Should not trim start, but should trim the end", "      *****", warehouse2.WorkTimes.WednesdayWorkingHours);
			AssertEquals("Should not trim start", "      *", warehouse2.WorkTimes.ThursdayWorkingHours);
			AssertEquals("Should not trim start", "           *********", warehouse2.WorkTimes.FridayWorkingHours);
			AssertEquals("Should trim the end", "", warehouse2.WorkTimes.SaturdayWorkingHours);
			AssertEquals("Should trim the end", "", warehouse2.WorkTimes.SundayWorkingHours);
		}

		#endregion

		#region TestFixedWidthLocationParameters

		public void TestIsFixedWidthLocation_Enabled()
		{
			TestIsFixedWidthLocationCore(true);
		}

		public void TestIsFixedWidthLocation_Disabled()
		{
			TestIsFixedWidthLocationCore(false);
		}

		void TestIsFixedWidthLocationCore(bool isEnabled)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.IsFixedWidthLocation = isEnabled;

			AssertEquals("Property readonly config is correct.", !isEnabled, warehouse.WW_LocationColumnsFixedWidthInfo.ReadOnly);
			AssertEquals("Property readonly config is correct.", !isEnabled, warehouse.WW_LocationLevelsFixedWidthInfo.ReadOnly);
			AssertEquals("Property readonly config is correct.", !isEnabled, warehouse.WW_LocationTraysFixedWidthInfo.ReadOnly);
		}

		public void TestIsFixedWidthLocation_LocationsHaveLeadingZeros()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertEquals("Precondition", false, warehouse.WW_LocationsHaveLeadingZerosInfo.ReadOnly);
			AssertEquals("Precondition", false, warehouse.WW_LocationsHaveLeadingZeros);
			AssertEquals("Precondition", false, warehouse.IsFixedWidthLocation);

			warehouse.IsFixedWidthLocation = true;
			AssertEquals("Precondition", true, warehouse.WW_LocationsHaveLeadingZerosInfo.ReadOnly);
			AssertEquals("Precondition", true, warehouse.WW_LocationsHaveLeadingZeros);
		}

		public void TestIsFixedWidthLocationFromDB()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationColumnsFixedWidth = 3;
			warehouse.WW_LocationLevelsFixedWidth = 2;
			warehouse.WW_LocationTraysFixedWidth = 1;
			warehouse.WW_LocationsHaveLeadingZeros = true;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var warehouseInNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Fixed width parameter is not 0.", (ZByte)3, warehouse.WW_LocationColumnsFixedWidth);
			AssertEquals("Fixed width parameter is not 0.", (ZByte)2, warehouse.WW_LocationLevelsFixedWidth);
			AssertEquals("Fixed width parameter is not 0.", (ZByte)1, warehouse.WW_LocationTraysFixedWidth);
			Assert("Fixed Width Location info is correct for warehouse loaded from DB.", warehouseInNewFactory.IsFixedWidthLocation);
		}

		public void TestIsFixedWidthLocationReset()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationColumnsFixedWidth = 3;
			warehouse.WW_LocationLevelsFixedWidth = 2;
			warehouse.WW_LocationTraysFixedWidth = 1;
			AssertNotEquals("Precondition", ZByte.Zero, warehouse.WW_LocationColumnsFixedWidth);
			AssertNotEquals("Precondition", ZByte.Zero, warehouse.WW_LocationLevelsFixedWidth);
			AssertNotEquals("Precondition", ZByte.Zero, warehouse.WW_LocationTraysFixedWidth);

			warehouse.IsFixedWidthLocation = false;
			AssertEquals("Fixed width parameter is reset to 0.", ZByte.Zero, warehouse.WW_LocationColumnsFixedWidth);
			AssertEquals("Fixed width parameter is reset to 0.", ZByte.Zero, warehouse.WW_LocationLevelsFixedWidth);
			AssertEquals("Fixed width parameter is reset to 0.", ZByte.Zero, warehouse.WW_LocationTraysFixedWidth);
		}

		public void TestFixedWidthLocationEnabled_AlphaColumns()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationColumnsFixedWidth = 3;
			AssertEquals("Precondition", false, warehouse.WW_LocationColumnsAlpha);
			AssertEquals("Precondition", (ZByte)3, warehouse.WW_LocationColumnsFixedWidth);
			AssertEquals("Precondition", false, warehouse.WW_LocationColumnsFixedWidthInfo.ReadOnly);

			warehouse.WW_LocationColumnsAlpha = true;
			AssertEquals("Fixed width is set to 1 when component is configured as Alpha.", (ZByte)1, warehouse.WW_LocationColumnsFixedWidth);
			AssertEquals("Fixed width is set as readonly.", true, warehouse.WW_LocationColumnsFixedWidthInfo.ReadOnly);
		}

		public void TestFixedWidthLocationEnabled_AlphaColumnsThenEnableFixedWidthLocation()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_LocationColumnsAlpha = true;
			AssertEquals("Precondition", true, warehouse.WW_LocationColumnsAlpha);
			AssertEquals("Precondition", ZByte.Zero, warehouse.WW_LocationColumnsFixedWidth);
			AssertEquals("Precondition", false, warehouse.IsFixedWidthLocation);

			warehouse.IsFixedWidthLocation = true;
			AssertEquals("Fixed width is set to 1 when component is configured as Alpha.", (ZByte)1, warehouse.WW_LocationColumnsFixedWidth);
			AssertEquals("Fixed width is set as readonly.", true, warehouse.WW_LocationColumnsFixedWidthInfo.ReadOnly);
			AssertNoErrors(warehouse.WW_LocationColumnsFixedWidthInfo);
			AssertHasError("Validation called after setting IsFixedWidthLocation to true.", warehouse.WW_LocationLevelsFixedWidthInfo, "Levels Fixed Width cannot be 0 when Location Fixed Width is enabled.");
			AssertHasError("Validation called after setting IsFixedWidthLocation to true.", warehouse.WW_LocationTraysFixedWidthInfo, "Trays Fixed Width cannot be 0 when Location Fixed Width is enabled.");
		}

		public void TestFixedWidthLocationEnabled_AlphaLevels()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationLevelsFixedWidth = 2;
			AssertEquals("Precondition", false, warehouse.WW_LocationLevelsAlpha);
			AssertEquals("Precondition", (ZByte)2, warehouse.WW_LocationLevelsFixedWidth);
			AssertEquals("Precondition", false, warehouse.WW_LocationLevelsFixedWidthInfo.ReadOnly);

			warehouse.WW_LocationLevelsAlpha = true;
			AssertEquals("Fixed width is set to 1 when component is configured as Alpha.", (ZByte)1, warehouse.WW_LocationLevelsFixedWidth);
			AssertEquals("Fixed width is set as readonly.", true, warehouse.WW_LocationLevelsFixedWidthInfo.ReadOnly);
		}

		public void TestFixedWidthLocationEnabled_AlphaLevelsThenEnableFixedWidthLocation()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_LocationLevelsAlpha = true;
			AssertEquals("Precondition", true, warehouse.WW_LocationLevelsAlpha);
			AssertEquals("Precondition", ZByte.Zero, warehouse.WW_LocationLevelsFixedWidth);
			AssertEquals("Precondition", false, warehouse.IsFixedWidthLocation);

			warehouse.IsFixedWidthLocation = true;
			AssertEquals("Fixed width is set to 1 when component is configured as Alpha.", (ZByte)1, warehouse.WW_LocationLevelsFixedWidth);
			AssertEquals("Fixed width is set as readonly.", true, warehouse.WW_LocationLevelsFixedWidthInfo.ReadOnly);
			AssertNoErrors(warehouse.WW_LocationLevelsFixedWidthInfo);
			AssertHasError("Validation called after setting IsFixedWidthLocation to true.", warehouse.WW_LocationColumnsFixedWidthInfo, "Columns Fixed Width cannot be 0 when Location Fixed Width is enabled.");
			AssertHasError("Validation called after setting IsFixedWidthLocation to true.", warehouse.WW_LocationTraysFixedWidthInfo, "Trays Fixed Width cannot be 0 when Location Fixed Width is enabled.");
		}

		public void TestFixedWidthLocationEnabled_AlphaTrays()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationTraysFixedWidth = 2;
			AssertEquals("Precondition", false, warehouse.WW_LocationTraysAlpha);
			AssertEquals("Precondition", (ZByte)2, warehouse.WW_LocationTraysFixedWidth);
			AssertEquals("Precondition", false, warehouse.WW_LocationTraysFixedWidthInfo.ReadOnly);

			warehouse.WW_LocationTraysAlpha = true;
			AssertEquals("Fixed width is set to 1 when component is configured as Alpha.", (ZByte)1, warehouse.WW_LocationTraysFixedWidth);
			AssertEquals("Fixed width is set as readonly.", true, warehouse.WW_LocationTraysFixedWidthInfo.ReadOnly);
		}

		public void TestFixedWidthLocationEnabled_AlphaTraysThenEnableFixedWidthLocation()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_LocationTraysAlpha = true;
			AssertEquals("Precondition", true, warehouse.WW_LocationTraysAlpha);
			AssertEquals("Precondition", ZByte.Zero, warehouse.WW_LocationTraysFixedWidth);
			AssertEquals("Precondition", false, warehouse.IsFixedWidthLocation);

			warehouse.IsFixedWidthLocation = true;
			AssertEquals("Fixed width is set to 1 when component is configured as Alpha.", (ZByte)1, warehouse.WW_LocationTraysFixedWidth);
			AssertEquals("Fixed width is set as readonly.", true, warehouse.WW_LocationTraysFixedWidthInfo.ReadOnly);
			AssertNoErrors(warehouse.WW_LocationTraysFixedWidthInfo);
			AssertHasError("Validation called after setting IsFixedWidthLocation to true.", warehouse.WW_LocationLevelsFixedWidthInfo, "Levels Fixed Width cannot be 0 when Location Fixed Width is enabled.");
			AssertHasError("Validation called after setting IsFixedWidthLocation to true.", warehouse.WW_LocationColumnsFixedWidthInfo, "Columns Fixed Width cannot be 0 when Location Fixed Width is enabled.");
		}

		#endregion

		#region TestUniqueIndexFailureHandlers

		public void TestUniqueIndexFailureHandlers()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertType<LocationStringUniqueIndexValidationHandler>(((IBusinessObjectInternals)warehouse).UniqueIndexFailureHandlers.Single());

			Helper.CreateRowAndGenerateLocations(warehouse, "B", cols: 130);
			Helper.CreateRowAndGenerateLocations(warehouse, "B006");

			Factory.Save();

			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationColumnsFixedWidth = 3;
			warehouse.WW_LocationLevelsFixedWidth = 1;
			warehouse.WW_LocationTraysFixedWidth = 1;

			var notifier = new TestNotificationHandler();
			var saveHitCount = 0;
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
			{
				// only try to save once, to check unique index handler.
				if (saveHitCount++ == 0)
				{
					Factory.Save();
				}
			}, null, notifier: notifier);
			AssertEquals("Should have correct error message from unique index validation handler.",
				"The changes to the location configurations on the warehouse have resulted in a duplicate Location Barcode in the warehouse. Ensure Row Names for this warehouse will not result in Duplicate Location Barcodes if enabling Fixed Width Locations.", notifier.Messages.Single());
		}

		class TestNotificationHandler : INotificationHandler
		{
			public IEnumerable<string> Messages => messages;
			readonly List<string> messages = new();

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				messages.Add(message);
			}

			public void ReportInformation(string message, string caption)
			{
				messages.Add(message);
			}
		}

		#endregion

		#region TestAreasFromIWhsWarehouse

		public void TestAreas_FromIWhsWarehouse()
		{
			var whs = Helper.CreateWarehouse("1");
			var area1 = Factory.NewWithValidTestData<WhsArea>();
			area1.WA_WW_Whs = whs.PK;
			var area2 = Factory.NewWithValidTestData<WhsArea>();
			area2.WA_WW_Whs = whs.PK;
			var area3 = Factory.NewWithValidTestData<WhsArea>();
			area3.WA_WW_Whs = whs.PK;

			var areas = ((IWhsWarehouse)whs).Areas;

			AssertNotNull(areas);
			AssertEquals(areas, whs.Areas);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var whs = Helper.CreateWarehouse("WHS DEL", shouldPreGenerateDDL: false);
			whs.WW_IsVirtualWarehouse = true;
			return whs;
		}

		#endregion
	}

	class CannotChangeWarehouseTypeToOrFromTransitIfAlreadyReferencedTest : WhsTestCaseWithFactoryEnv
	{
		#region TestChangeTypeFromProductToTransit

		public void TestChangeTypeFromProductToTransit()
		{
			// specifically create rows on each warehouse to test Validation ignores Rows & Areas
			var warehouseWithProductTransactions = Helper.CreateWarehouse("WHS", "A");
			var client = Helper.CreateClient();
			Factory.Save();

			// add product transaction that references the warehouse
			var sqlWarehouseWithProductTransactions = WhsWarehouseDO.ShallowLoadFromDB(TestConnection, w => w.PK == warehouseWithProductTransactions.PK)[0];
			var sqlClient = CargoWise.Database.TestFramework.ObjectModel.OrgHeader.ShallowLoadFromDB(TestConnection, c => c.PK == client.PK)[0];
			AssertNoExceptionThrown(() => new CargoWise.Database.TestFramework.ObjectModel.JobStorage("ID123", sqlWarehouseWithProductTransactions, sqlClient, DateTime.Now, DateTime.Now).Insert(TestConnection));

			warehouseWithProductTransactions.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertNoExceptionThrown(() => Factory.Save());

			warehouseWithProductTransactions.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		#region TestChangeTypeFromTransitToProduct

		public void TestChangeTypeFromTransitToProduct()
		{
			// specifically create rows on each warehouse to test that Validation ignores Rows & Areas
			var warehouseWithTransitTransactions = Helper.CreateWarehouse("TRA", "A");
			warehouseWithTransitTransactions.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			// add transit transaction that references the warehouse
			var sqlWarehouseWithTransitTransactions = WhsWarehouseDO.ShallowLoadFromDB(TestConnection, w => w.PK == warehouseWithTransitTransactions.PK)[0];
			var sqlWarehouseWithTransitTransactionsLoc = CargoWise.Database.TestFramework.ObjectModel.WhsLocation.ShallowLoadFromDB(TestConnection, w => w.PK == warehouseWithTransitTransactions.DefaultLocation.PK)[0];
			new CargoWise.Database.TestFramework.ObjectModel.WhsItemReceiveTransportationUnit(sqlWarehouseWithTransitTransactions, "R1", sqlWarehouseWithTransitTransactionsLoc, "car123").Insert(TestConnection);

			warehouseWithTransitTransactions.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouseWithTransitTransactions.WW_IsVirtualWarehouse = true;
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		#region TestChangeWarehouseTypeToContainerYard

		public void TestChangeTypeFromProductToContainerYard()
		{
			TestChangeWarehouseTypeToContainerYardCore(WarehouseTypes.Codes.Product);
		}

		public void TestChangeTypeFromFreeTradeZoneToContainerYard()
		{
			TestChangeWarehouseTypeToContainerYardCore(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestChangeTypeFromTransitToContainerYard()
		{
			TestChangeWarehouseTypeToContainerYardCore(WarehouseTypes.Codes.Transit);
		}

		public void TestChangeWarehouseTypeToContainerYardCore(string originalWarehouseType)
		{
			var warehouseTransactions = Helper.CreateWarehouse("WHS", "A");
			warehouseTransactions.WW_WarehouseType = originalWarehouseType;
			var client = Helper.CreateClient();
			Factory.Save();

			// add product transaction that references the warehouse
			var sqlWarehouseTransactions = WhsWarehouseDO.ShallowLoadFromDB(TestConnection, w => w.PK == warehouseTransactions.PK)[0];

			if (originalWarehouseType == WarehouseTypes.Codes.Transit)
			{
				var sqlWarehouseTransactionsLoc = CargoWise.Database.TestFramework.ObjectModel.WhsLocation.ShallowLoadFromDB(TestConnection, w => w.PK == warehouseTransactions.DefaultLocation.PK)[0];
				new CargoWise.Database.TestFramework.ObjectModel.WhsItemReceiveTransportationUnit(sqlWarehouseTransactions, "R1", sqlWarehouseTransactionsLoc, "car123").Insert(TestConnection);
			}
			else
			{
				var sqlClient = CargoWise.Database.TestFramework.ObjectModel.OrgHeader.ShallowLoadFromDB(TestConnection, c => c.PK == client.PK)[0];
				new CargoWise.Database.TestFramework.ObjectModel.JobStorage("ID123", sqlWarehouseTransactions, sqlClient, DateTime.Now, DateTime.Now).Insert(TestConnection);
			}

			AssertEquals("Precondition", originalWarehouseType, warehouseTransactions.WW_WarehouseType);

			warehouseTransactions.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		#region TestChangeTypeWhenWarehouseHasNoTransactions

		public void TestChangeTypeWhenWarehouseHasNoTransactions()
		{
			var warehouseWithNoTransactions = Helper.CreateWarehouse("NON", "A");
			Factory.Save();

			warehouseWithNoTransactions.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertNoExceptionThrown(() => Factory.Save());

			warehouseWithNoTransactions.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertNoExceptionThrown(() => Factory.Save());

			warehouseWithNoTransactions.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertNoExceptionThrown(() => Factory.Save());

			warehouseWithNoTransactions.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region Implementation

		static void AssertTriggerPreventsSave(BusinessObjectFactory factory)
		{
			bool exceptionThrown = false;
			try
			{
				factory.Save();
			}
			catch (ZSaveException e)
			{
				AssertEquals(WhsWarehouse.CannotChangeWarehouseTypeToOrFromTransitIfAlreadyReferencedTriggerID, e.InnerException.InnerException.Message);
				exceptionThrown = true;
			}

			AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
		}

		#endregion
	}

	#region WhsWarehouseTriggersTest

	internal class WhsWarehouseTriggersTest : TestCaseWithFactory
	{
		#region Test trigger prevents wrong Alpha values

		public void TestTriggerPreventsSettingLocationColumnsAlphaIfThereAreWrongRows()
		{
			TestTriggerPreventsSettingLocationAlphaIfThereAreWrongRows(row => row.WR_Columns = 27, warehouse => warehouse.WW_LocationColumnsAlpha = true, "Unable to turn on Alpha for Columns as there is Row with more than 26 Columns in it.");
		}

		public void TestTriggerPreventsSettingLocationLevelsAlphaIfThereAreWrongRows()
		{
			TestTriggerPreventsSettingLocationAlphaIfThereAreWrongRows(row => row.WR_Levels = 27, warehouse => warehouse.WW_LocationLevelsAlpha = true, "Unable to turn on Alpha for Levels as there is Row with more than 26 Levels in it.");
		}

		public void TestTriggerPreventsSettingLocationTraysAlphaIfThereAreWrongRows()
		{
			TestTriggerPreventsSettingLocationAlphaIfThereAreWrongRows(row => row.WR_Trays = 27, warehouse => warehouse.WW_LocationTraysAlpha = true, "Unable to turn on Alpha for Trays as there is Row with more than 26 Trays in it.");
		}

		public void TestTriggerPreventsSettingLocationAlphaIfThereAreWrongRows(Action<WhsRow> whsRowPartSetter, Action<WhsWarehouse> whsLocationAlphaPropertySet, ZString expectedMessage)
		{
			var warehouse = Helper.CreateWarehouse("WH1", "A");
			var row = warehouse.Rows.Single(r => r.WR_Name == "A");
			whsRowPartSetter(row);
			Factory.Save();

			whsLocationAlphaPropertySet(warehouse);
			try
			{
				Factory.Save();
				Fail("Trigger should prevent save.");
			}
			catch (ZSaveException ex)
			{
				AssertEquals(expectedMessage, ex.FriendlyMessage);
			}
		}

		#endregion

		#region Test trigger prevents wrong fixed width

		public void TestTriggerPreventsSettingLocationColumnsFixedWidthIfThereAreWrongRows_RowColumnsInHundreds()
		{
			TestTriggerPreventsSettingLocationFixedWidthIfThereAreWrongRows(
				(row, warehouse) =>
				{
					row.WR_Columns = 100;
					warehouse.WW_LocationColumnsFixedWidth = 3;
					warehouse.WW_LocationLevelsFixedWidth = 1;
					warehouse.WW_LocationTraysFixedWidth = 1;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => warehouse.WW_LocationColumnsFixedWidth = 2,
				"Unable to update the location Columns Fixed Width as there is a Row with more than the maximum allowed Columns with the updated fixed width.");
		}

		public void TestTriggerPreventsSettingLocationColumnsFixedWidthIfThereAreWrongRows_RowColumnsInTens()
		{
			TestTriggerPreventsSettingLocationFixedWidthIfThereAreWrongRows(
				(row, warehouse) =>
				{
					row.WR_Columns = 10;
					warehouse.WW_LocationColumnsFixedWidth = 2;
					warehouse.WW_LocationLevelsFixedWidth = 1;
					warehouse.WW_LocationTraysFixedWidth = 1;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => warehouse.WW_LocationColumnsFixedWidth = 1,
				"Unable to update the location Columns Fixed Width as there is a Row with more than the maximum allowed Columns with the updated fixed width.");
		}

		public void TestTriggerPreventsSettingLocationLevelsFixedWidthIfThereAreWrongRows_RowLevelsInHundreds()
		{
			TestTriggerPreventsSettingLocationFixedWidthIfThereAreWrongRows(
				(row, warehouse) =>
				{
					row.WR_Levels = 100;
					warehouse.WW_LocationColumnsFixedWidth = 1;
					warehouse.WW_LocationLevelsFixedWidth = 3;
					warehouse.WW_LocationTraysFixedWidth = 1;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => warehouse.WW_LocationLevelsFixedWidth = 2,
				"Unable to update the location Levels Fixed Width as there is a Row with more than the maximum allowed Levels with the updated fixed width.");
		}

		public void TestTriggerPreventsSettingLocationLevelsFixedWidthIfThereAreWrongRows_RowLevelsInTens()
		{
			TestTriggerPreventsSettingLocationFixedWidthIfThereAreWrongRows(
				(row, warehouse) =>
				{
					row.WR_Levels = 10;
					warehouse.WW_LocationColumnsFixedWidth = 1;
					warehouse.WW_LocationLevelsFixedWidth = 2;
					warehouse.WW_LocationTraysFixedWidth = 1;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => warehouse.WW_LocationLevelsFixedWidth = 1,
				"Unable to update the location Levels Fixed Width as there is a Row with more than the maximum allowed Levels with the updated fixed width.");
		}

		public void TestTriggerPreventsSettingLocationTraysFixedWidthIfThereAreWrongRows()
		{
			TestTriggerPreventsSettingLocationFixedWidthIfThereAreWrongRows(
				(row, warehouse) =>
				{
					row.WR_Trays = 10;
					warehouse.WW_LocationColumnsFixedWidth = 1;
					warehouse.WW_LocationLevelsFixedWidth = 1;
					warehouse.WW_LocationTraysFixedWidth = 2;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => warehouse.WW_LocationTraysFixedWidth = 1,
				"Unable to update the location Trays Fixed Width as there is a Row with more than the maximum allowed Trays with the updated fixed width.");
		}

		public void TestTriggerPreventsSettingLocationFixedWidthIfThereAreWrongRows(Action<WhsRow, WhsWarehouse> warehouseAndRowConfigSetter, Action<WhsWarehouse> fixedWidthPropertySet, ZString expectedMessage)
		{
			var warehouse = Helper.CreateWarehouse("WH1", "A");
			var row = warehouse.Rows.Single(r => r.WR_Name == "A");
			warehouseAndRowConfigSetter(row, warehouse);
			Factory.Save();

			fixedWidthPropertySet(warehouse);
			try
			{
				Factory.Save();
				Fail("Trigger should prevent save.");
			}
			catch (ZSaveException ex)
			{
				AssertEquals(expectedMessage, ex.FriendlyMessage);
			}
		}

		public void TestTriggerSettingLocationFixedWidth_AlphaColumns()
		{
			TestTriggerSettingLocationFixedWidth_Alpha(
				(row, warehouse) =>
				{
					row.WR_Columns = 20;
					warehouse.WW_LocationColumnsFixedWidth = 2;
					warehouse.WW_LocationLevelsFixedWidth = 1;
					warehouse.WW_LocationTraysFixedWidth = 1;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => { warehouse.WW_LocationColumnsFixedWidth = 1; warehouse.WW_LocationColumnsAlpha = true; });
		}

		public void TestTriggerSettingLocationFixedWidth_AlphaLevels()
		{
			TestTriggerSettingLocationFixedWidth_Alpha(
				(row, warehouse) =>
				{
					row.WR_Levels = 20;
					warehouse.WW_LocationColumnsFixedWidth = 1;
					warehouse.WW_LocationLevelsFixedWidth = 2;
					warehouse.WW_LocationTraysFixedWidth = 1;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => { warehouse.WW_LocationLevelsFixedWidth = 1; warehouse.WW_LocationLevelsAlpha = true; });
		}

		public void TestTriggerSettingLocationFixedWidth_AlphaTrays()
		{
			TestTriggerSettingLocationFixedWidth_Alpha(
				(row, warehouse) =>
				{
					row.WR_Trays = 20;
					warehouse.WW_LocationColumnsFixedWidth = 1;
					warehouse.WW_LocationLevelsFixedWidth = 1;
					warehouse.WW_LocationTraysFixedWidth = 2;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => { warehouse.WW_LocationTraysFixedWidth = 1; warehouse.WW_LocationTraysAlpha = true; });
		}

		void TestTriggerSettingLocationFixedWidth_Alpha(Action<WhsRow, WhsWarehouse> warehouseAndRowConfigSetter, Action<WhsWarehouse> whsLocationFixedWidthPropertySet)
		{
			var warehouse = Helper.CreateWarehouse("WH1", "A");
			var row = warehouse.Rows.Single(r => r.WR_Name == "A");
			warehouseAndRowConfigSetter(row, warehouse);
			Factory.Save();

			whsLocationFixedWidthPropertySet(warehouse);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctionsEnv helper;
		protected WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));

		#endregion
	}

	#endregion

	#region WhsWarehouseAffectLocationStringTestCase

	[TestedType(typeof(WhsWarehouse))]
	class WhsWarehouseAffectLocationStringTestCase : AffectLocationStringTestCase
	{
		#region TestReloadLocations_DBHits

		public void TestReloadLocations_DBHits()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");

			for (int index = 0; index < 10; index++)
			{
				helper.CreateRowAndGenerateLocations(warehouse, ('0' + index).ToString(), 5, 5);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var warehouseInNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			warehouseInNewFactory.WW_LocationColumnsAlpha = true;
			newFactory.Save();

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsRowSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);

			AssertDbHits(expectedDbHits, newFactory);
		}

		public void TestReloadLocations_WW_WarehouseType()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "WHS", 2);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { WarehouseTypes.Codes.Product, WarehouseTypes.Codes.Product }, row.Locations.Select(l => l.WLV_WarehouseType));

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { WarehouseTypes.Codes.FreeTradeZone, WarehouseTypes.Codes.FreeTradeZone }, row.Locations.Select(l => l.WLV_WarehouseType));
		}

		public void TestReloadLocations_WW_IsVirtualWarehouse()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "WHS", 2);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { ZBool.False, ZBool.False }, row.Locations.Select(l => l.WLV_IsVirtualWarehouse));

			warehouse.WW_IsVirtualWarehouse = true;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { ZBool.True, ZBool.True }, row.Locations.Select(l => l.WLV_IsVirtualWarehouse));
		}

		#endregion

		#region Implementation

		protected override IEnumerable<WhsLocation> GetLocations(IAffectLocationView p)
		{
			var parent = (WhsWarehouse)p;
			return parent.Rows.SelectMany(r => r.Locations).Where(l => !l.IsDockDoorLocation);
		}

		protected override IAffectLocationView GetNewParent()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);
			return warehouse;
		}

		protected override void ModifyColumnsThatAffectionLocationView(IAffectLocationView w)
		{
			var warehouse = (WhsWarehouse)w;
			warehouse.WW_LocationColumnsAlpha = true;
		}

		protected override IEnumerable<SchemaColumn> ExpectedColumnsThatAffectLocationView
		{
			get
			{
				return new SchemaColumn[]
				{
					WhsWarehouseSchema.WW_LocationComponentDelimiter,
					WhsWarehouseSchema.WW_LocationColumnsAlpha,
					WhsWarehouseSchema.WW_LocationColumnsFixedWidth,
					WhsWarehouseSchema.WW_LocationColumnsZeroBased,
					WhsWarehouseSchema.WW_LocationLevelsAlpha,
					WhsWarehouseSchema.WW_LocationLevelsFixedWidth,
					WhsWarehouseSchema.WW_LocationLevelsZeroBased,
					WhsWarehouseSchema.WW_LocationTraysAlpha,
					WhsWarehouseSchema.WW_LocationTraysFixedWidth,
					WhsWarehouseSchema.WW_LocationTraysZeroBased,
					WhsWarehouseSchema.WW_IsVirtualWarehouse,
					WhsWarehouseSchema.WW_WarehouseType,
				};
			}
		}

		#endregion
	}

	#endregion
}
