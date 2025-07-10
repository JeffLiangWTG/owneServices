using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsArea))]
	class WhsAreaTest : WhsEnvBusinessObjectTestCase
	{
		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			AssertEquals(CodeLists.AreaTypes.Codes.FreeStore, Area.WA_AreaType);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			AssertEquals(Whs, Area.Warehouse);
		}

		#endregion

		#region TestPickAndPutawayLocations

		public void TestPickAndPutawayLocations()
		{
			var whs = Helper.CreateWarehouse("1", "A", 2, 3);
			var area1 = Helper.CreateArea(Whs, "A1", AreaTypes.Codes.FreeStore, true, true);
			var area2 = Helper.CreateArea(Whs, "A2", AreaTypes.Codes.FreeStore, true, true);
			Factory.Save();
			var locationWithAreaUsedAsPick = whs.FindLocation("A-1-1");
			var locationWithAreaUsedAsPutaway = whs.FindLocation("A-1-2");
			locationWithAreaUsedAsPick.WLV_WA_PickingArea = area1.PK;
			locationWithAreaUsedAsPutaway.WLV_WA_PutawayArea = area1.PK;
			AssertContainsExactElementsInAnyOrder("Only locations used as pick area must be returned.", new[] { locationWithAreaUsedAsPick }, area1.PickLocations);
			AssertContainsExactElementsInAnyOrder("Only locations used as putaway area must be returned.", new[] { locationWithAreaUsedAsPutaway }, area1.PutawayLocations);

			locationWithAreaUsedAsPick.WLV_WA_PickingArea = area2.PK;
			locationWithAreaUsedAsPutaway.WLV_WA_PutawayArea = area2.PK;
			AssertEquals("Pick location cache must cleared.", 0, area1.PickLocations.Count);
			AssertEquals("Putaway location cache must cleared.", 0, area1.PutawayLocations.Count);
		}

		#endregion

		#region TestWA_AreaType_RefreshesWarehouseTransactionTypes

		public void TestWA_AreaType_RefreshesWarehouseTransactionTypes()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var area = warehouse.Areas[0];
			area.WA_AreaType = AreaTypes.Codes.FreeStore;

			var bondedTextRefreshHitCount = 0;
			var exciseTextRefreshHitCount = 0;
			var freeStoreTextRefreshHitCount = 0;
			var inwardProcessingTextRefreshHitCount = 0;
			var vatFiscalTextRefreshHitCount = 0;
			warehouse.BondedTextInfo.ValueChanged += (sender, e) => bondedTextRefreshHitCount++;
			warehouse.ExciseTextInfo.ValueChanged += (sender, e) => exciseTextRefreshHitCount++;
			warehouse.FreeStoreTextInfo.ValueChanged += (sender, e) => freeStoreTextRefreshHitCount++;
			warehouse.InwardProcessingTextInfo.ValueChanged += (sender, e) => inwardProcessingTextRefreshHitCount++;
			warehouse.VATFiscalTextInfo.ValueChanged += (sender, e) => vatFiscalTextRefreshHitCount++;
			area.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertEquals("Setting Area Type to the same Type, Warehouse Transaction Type should not refresh.", 0, bondedTextRefreshHitCount);
			AssertEquals("Setting Area Type to the same Type, Warehouse Transaction Type should not refresh.", 0, exciseTextRefreshHitCount);
			AssertEquals("Setting Area Type to the same Type, Warehouse Transaction Type should not refresh.", 0, freeStoreTextRefreshHitCount);
			AssertEquals("Setting Area Type to the same Type, Warehouse Transaction Type should not refresh.", 0, inwardProcessingTextRefreshHitCount);
			AssertEquals("Setting Area Type to the same Type, Warehouse Transaction Type should not refresh.", 0, vatFiscalTextRefreshHitCount);

			area.WA_AreaType = AreaTypes.Codes.Excise;
			AssertEquals("Setting Area Type to a different Area Type, Warehouse Transaction Type should refresh.", 1, bondedTextRefreshHitCount);
			AssertEquals("Setting Area Type to a different Area Type, Warehouse Transaction Type should refresh.", 1, exciseTextRefreshHitCount);
			AssertEquals("Setting Area Type to a different Area Type, Warehouse Transaction Type should refresh.", 1, freeStoreTextRefreshHitCount);
			AssertEquals("Setting Area Type to a different Area Type, Warehouse Transaction Type should refresh.", 1, inwardProcessingTextRefreshHitCount);
			AssertEquals("Setting Area Type to a different Area Type, Warehouse Transaction Type should refresh.", 1, vatFiscalTextRefreshHitCount);
		}

		#endregion

		#region TestWA_OH_TransitClient

		public void TestWA_OH_TransitClient()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 2);
			var area = Helper.CreateArea(whs, "Defaule");
			var pK = ZGuid.NewZGuid();
			area.WA_OH_TransitClient = pK;
			AssertEquals(pK, area.WA_OH_TransitClient);
		}

		#endregion

		#region TestWA_CalcTransitClientDescription

		public void TestWA_CalcTransitClientDescription()
		{
			var client = Helper.CreateClient("CL1", "Test Client");
			var whs = Helper.CreateWarehouse("W1", "A", 2, 2);
			var area = Helper.CreateArea(whs, "Default");

			whs.WW_WarehouseType = WarehouseTypes.Codes.Product;
			area.WA_OH_TransitClient = client.PK;
			AssertEquals("Should return '" + WhsArea.naString + "'", WhsArea.naString, area.WA_CalcTransitClientDescription);

			whs.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertEquals("Should return 'Test Client'", "Test Client", area.WA_CalcTransitClientDescription);

			area.WA_OH_TransitClient = ZGuid.Empty;
			AssertEquals("Should return empty string.", "", area.WA_CalcTransitClientDescription);
		}

		#endregion

		#region TestWA_WW_Whs

		public void TestWA_WW_Whs()
		{
			ZGuid pK = ZGuid.NewZGuid();
			Area.WA_WW_Whs = pK;
			AssertEquals(pK, Area.WA_WW_Whs);
		}

		public void TestWA_WW_Whs_ReadOnly()
		{
			var area = Factory.New<WhsArea>();
			AssertEquals("WA_WW_Whs ReadOnly should be false.", false, area.WA_WW_WhsInfo.ReadOnly);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			area.WA_WW_Whs = warehouse.PK;
			AssertEquals("WA_WW_Whs ReadOnly should be false.", false, area.WA_WW_WhsInfo.ReadOnly);

			area.WA_Name = "Test";
			Factory.Save();
			AssertEquals("Pre-condition: area is saved.", true, area.IsInDatabase);
			AssertEquals("WA_WW_Whs ReadOnly should be true.", true, area.WA_WW_WhsInfo.ReadOnly);

			var otherFactory = new BusinessObjectFactory();
			var areaInOtherFactory = otherFactory.Load<WhsArea>(area.PK);
			AssertEquals("WA_WW_Whs ReadOnly should be true.", true, areaInOtherFactory.WA_WW_WhsInfo.ReadOnly);
		}

		#region TestTrigger_WhsArea_PreventChangingWarehouse

		[ExpectNoExceptions]
		public void TestTrigger_WhsArea_PreventChangingWarehouse()
		{
			var whs1 = Helper.CreateWarehouse("W1");
			var whs2 = Helper.CreateWarehouse("W2");
			var area = Helper.CreateArea(whs1, "A");
			Factory.Save();

			area.WA_WW_Whs = whs2.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change the Warehouse for an Area that is already saved."), "Trigger error should be thrown.");
		}

		public void TestTrigger_WhsArea_PreventChangingWarehouse_NoException_WhenUpdateToSameValue()
		{
			var whs1 = Helper.CreateWarehouse("W1");
			var whs2 = Helper.CreateWarehouse("W2");
			var area = Helper.CreateArea(whs1, "A");
			Factory.Save();

			AssertEquals("Pre-condition:", whs1.PK, area.WA_WW_Whs);

			AssertNoExceptionThrown("Should not throw exception when update to same value.",
				() => CargoWise.Database.TestFramework.ObjectModel.WhsArea
					.UpdateWhere(l => l.PK == area.PK)
					.Set(l => l.WA_WW_Whs, whs1.PK.ToGuid())
					.Post(TestConnection));
		}

		#endregion

		#endregion

		#region TestWA_IsDefaultPutawayArea

		public void TestWA_IsDefaultPutawayArea_WhenSettingWithIsPutawayAreaNotSet()
		{
			var warehouse = Helper.CreateWarehouse("WHS", false);
			var area = warehouse.Areas[0];
			var expectedError = "Putaway Area must be enabled when Default Putaway Area is enabled.";
			AssertNoError("Precondition", area.WA_IsPutawayAreaInfo, expectedError);
			AssertNoError("Precondition", area.WA_IsDefaultPutawayAreaInfo, expectedError);

			area.WA_IsPutawayArea = false;
			area.WA_IsDefaultPutawayArea = true;
			AssertHasError(area.WA_IsPutawayAreaInfo, expectedError);
			AssertHasError(area.WA_IsDefaultPutawayAreaInfo, expectedError);
		}

		#endregion

		#region TestWA_IsDefaultPickArea

		public void TestWA_IsDefaultPickArea_WhenSettingWithIsPickingAreaNotSet()
		{
			var warehouse = Helper.CreateWarehouse("WHS", false);
			var area = warehouse.Areas[0];
			var expectedError = "Pick Area must be enabled when Default Pick Area is enabled.";
			AssertNoError("Precondition", area.WA_IsPickingAreaInfo, expectedError);
			AssertNoError("Precondition", area.WA_IsDefaultPickAreaInfo, expectedError);
			area.WA_IsPickingArea = false;
			area.WA_IsDefaultPickArea = true;

			AssertHasError(area.WA_IsPickingAreaInfo, expectedError);
			AssertHasError(area.WA_IsDefaultPickAreaInfo, expectedError);
		}

		#endregion

		#region TestWA_CalcMaxWeightAndVolumes

		public void TestWA_CalcMaxWeightAndVolumes()
		{
			Env.Registry.PackageWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			Env.Registry.PackageVolumeUnit = Enterprise.Core.Constants.Volume.CubicMetres;

			var whs = Helper.CreateWarehouse("W1", "A", 2, 2);
			SetupAreaMaxMeasures(whs.Areas[0]);

			AssertEquals(150m, whs.Areas[0].WA_CalcMaxWeight);
			AssertEquals(15m, whs.Areas[0].WA_CalcMaxVolume);
		}

		#endregion

		#region TestWA_CalcMaxWeightAndVolumes_LogsWarnings

		public void TestWA_CalcMaxWeightAndVolumes_LogsWarnings()
		{
			Env.Registry.PackageWeightUnit = Constants.Weight.Kilograms;
			Env.Registry.PackageVolumeUnit = Constants.Volume.CubicMetres;

			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 3);

			Factory.Save();

			var area = warehouse.Areas[0];

			var location1 = warehouse.FindLocation("A-1-1");
			Helper.SetLocationMaxWeightAndVolume(location1, 10m, "", 10m, "");

			var location2 = warehouse.FindLocation("A-1-2");
			Helper.SetLocationMaxWeightAndVolume(location2, 10m, "L", 1m, "CM");

			var location3 = warehouse.FindLocation("A-2-1");
			Helper.SetLocationMaxWeightAndVolume(location3, 10m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);

			var location4 = warehouse.FindLocation("A-2-2");
			Helper.SetLocationMaxWeightAndVolume(location4, 10m, "L", 1m, "CM");

			AssertEquals("Precondition", false, area.WeightAndVolumeHelper.HasMaxWeightWarningMessage);
			AssertEquals("Precondition", false, area.WeightAndVolumeHelper.HasMaxVolumeWarningMessage);
			AssertEquals(0m, area.WA_CalcMaxWeight);
			AssertEquals(0m, area.WA_CalcMaxVolume);
			AssertEquals(true, area.WeightAndVolumeHelper.HasMaxWeightWarningMessage);
			AssertEquals(true, area.WeightAndVolumeHelper.HasMaxVolumeWarningMessage);
			AssertMultilineASCIIEquals("MaxWeightWarningMessage", @"
	Location 'A-1-1' has no Weight Unit.
	Location 'A-1-2' has an invalid Weight Unit 'L'.
	Location 'A-2-2' has an invalid Weight Unit 'L'.
			".TrimStart('\r', '\n', ' ').TrimEnd('\r', '\n', ' ', '\t'), area.WeightAndVolumeHelper.MaxWeightWarningMessage);
			AssertMultilineASCIIEquals("MaxVolumeWarningMessage", @"
	Location 'A-1-1' has no Volume Unit.
	Location 'A-1-2' has an invalid Volume Unit 'CM'.
	Location 'A-2-2' has an invalid Volume Unit 'CM'.
			".TrimStart('\r', '\n', ' ').TrimEnd('\r', '\n', ' ', '\t'), area.WeightAndVolumeHelper.MaxVolumeWarningMessage);
		}

		#endregion

		#region TestWA_CalcWeightAndVolumes

		public void TestWA_CalcWeightAndVolumes()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = iHelper.CreateClient("WHS1TST");
			var part = iHelper.CreateProduct(orgPK, "P1");

			var whs = Helper.CreateWarehouse("W1", "A", 2, 2);
			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;
			var area1 = locations[0].PickingArea;
			var area2 = Helper.CreateArea(whs, "AREA");
			locations[2].WLV_WA_PickingArea = area2.PK;
			SetupAreaMaxMeasures(area1);
			Factory.Save();

			var receivePK = iHelper.CreateWhsReceive(orgPK, whs.PK, "1", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1");
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-2");
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-2-1");
			iHelper.FinaliseDocket(receivePK);
			Factory.Save(); // needed for DBOnly Query

			Env.Registry.PackageWeightUnit = Constants.Weight.Kilograms;
			Env.Registry.PackageVolumeUnit = Constants.Volume.CubicMetres;

			ZDecimal areaMaxWeight = 70m;
			ZDecimal areaMaxVolume = 7m;
			ZDecimal areaCurrWeight = 40m;
			ZDecimal areaCurrVolume = 0.4m;

			AssertEquals("CurrentWeight calculation is incorrect", areaCurrWeight, area1.WA_CalcCurrentWeight);
			AssertEquals("CurrentVolume calculation is incorrect", areaCurrVolume, area1.WA_CalcCurrentVolume);
			AssertEquals("AvailableWeight calculation is incorrect", areaMaxWeight - areaCurrWeight, area1.WA_CalcAvailableWeight);
			AssertEquals("AvailableVolume calculation is incorrect", areaMaxVolume - areaCurrVolume, area1.WA_CalcAvailableVolume);

			Env.Registry.PackageWeightUnit = Constants.Weight.Pounds;
			Env.Registry.PackageVolumeUnit = Constants.Volume.Litre;
			area1.ResetCalculatedTotalsCache();

			AssertEquals("CurrentWeight calculation is incorrect", Math.Round(Constants.Weight.Convert(areaCurrWeight, Constants.Weight.Kilograms, Constants.Weight.Pounds), 2), Math.Round(area1.WA_CalcCurrentWeight, 2));
			AssertEquals("CurrentVolume calculation is incorrect", Math.Round(Constants.Volume.Convert(areaCurrVolume, Constants.Volume.CubicMetres, Constants.Volume.Litre), 4), Math.Round(area1.WA_CalcCurrentVolume, 4));
			AssertEquals("AvailableWeight calculation is incorrect", Math.Round(Constants.Weight.Convert(areaMaxWeight - areaCurrWeight, Constants.Weight.Kilograms, Constants.Weight.Pounds), 2), Math.Round(area1.WA_CalcAvailableWeight, 2));
			AssertEquals("AvailableVolume calculation is incorrect", Math.Round(Constants.Volume.Convert(areaMaxVolume - areaCurrVolume, Constants.Volume.CubicMetres, Constants.Volume.Litre), 4), Math.Round(area1.WA_CalcAvailableVolume, 4));
		}

		#endregion

		#region TestWA_CalcWeightAndVolumes_LogsWarnings

		public void TestWA_CalcWeightAndVolumes_LogsWarnings()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = iHelper.CreateClient("WHS1TST");
			var part1 = (OrgSupplierPart)iHelper.CreateProduct(orgPK, "P1");
			part1.OP_CubicUQ = "CM";
			part1.OP_WeightUQ = "L";
			var part2 = (OrgSupplierPart)iHelper.CreateProduct(orgPK, "P2");
			part2.OP_CubicUQ = "CM";
			part2.OP_WeightUQ = "L";
			var part3 = (OrgSupplierPart)iHelper.CreateProduct(orgPK, "P3");
			part3.OP_CubicUQ = "M3";
			part3.OP_WeightUQ = "KG";
			var part4 = (OrgSupplierPart)iHelper.CreateProduct(orgPK, "P4");
			part4.OP_Cubic = 1m;
			part4.OP_Weight = 10m;
			part4.OP_CubicUQ = "";
			part4.OP_WeightUQ = "";
			AssertHasError(part4.OP_CubicUQInfo, "Please enter a Cubic UQ.");
			AssertHasError(part4.OP_WeightUQInfo, "Please enter a Weight UQ.");
			part4.OP_CubicUQ = "M3";
			part4.OP_WeightUQ = "KG";

			var whs = Helper.CreateWarehouse("W1", "A", 2, 2);
			var area1 = whs.DefaultLocation.PickingArea;
			Factory.Save();

			var receivePK = iHelper.CreateWhsReceive(orgPK, whs.PK, "1", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part1.PK, 10m, "A-1-1");
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part1.PK, 10m, "A-1-2");
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part2.PK, 10m, "A-2-1");
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part3.PK, 10m, "A-2-2");
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part4.PK, 10m, "A-2-2");
			iHelper.FinaliseDocket(receivePK);
			Factory.Save(); // needed for DBOnly Query

			Env.Registry.PackageWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			Env.Registry.PackageVolumeUnit = Enterprise.Core.Constants.Volume.CubicMetres;

			AssertEquals("Precondition", false, area1.WeightAndVolumeHelper.HasWeightWarningMessage);
			AssertEquals("Precondition", false, area1.WeightAndVolumeHelper.HasVolumeWarningMessage);
			AssertEquals("CurrentWeight calculation should not be able to convert", 0m, area1.WA_CalcCurrentWeight);
			AssertEquals("CurrentVolume calculation should not be able to convert", 0m, area1.WA_CalcCurrentVolume);
			AssertEquals(true, area1.WeightAndVolumeHelper.HasWeightWarningMessage);
			AssertEquals(true, area1.WeightAndVolumeHelper.HasVolumeWarningMessage);
			AssertMultilineASCIIEquals("WeightWarningMessage", @"
	Product 'P1' has an invalid Weight Unit 'L'.
	Product 'P2' has an invalid Weight Unit 'L'.
			".TrimStart('\r', '\n', ' ').TrimEnd('\r', '\n', ' ', '\t'), area1.WeightAndVolumeHelper.WeightWarningMessage);
			AssertMultilineASCIIEquals("VolumeWarningMessage", @"
	Product 'P1' has an invalid Volume Unit 'CM'.
	Product 'P2' has an invalid Volume Unit 'CM'.
			".TrimStart('\r', '\n', ' ').TrimEnd('\r', '\n', ' ', '\t'), area1.WeightAndVolumeHelper.VolumeWarningMessage);
		}

		#endregion

		#region TestWA_CalcWeightUnitInfo

		public void TestWA_CalcWeightUnitInfo()
		{
			AssertEquals(WhsArea.Schema.WA_CalcWeightUnit, Area.WA_CalcWeightUnitInfo.Name);
		}

		#endregion

		#region TestWA_CalcVolumeUnitInfo

		public void TestWA_CalcVolumeUnitInfo()
		{
			AssertEquals(WhsArea.Schema.WA_CalcVolumeUnit, Area.WA_CalcVolumeUnitInfo.Name);
		}

		#endregion

		#region TestWA_CalcMaxWeightInfo

		public void TestWA_CalcMaxWeightInfo()
		{
			AssertEquals(WhsArea.Schema.WA_CalcMaxWeight, Area.WA_CalcMaxWeightInfo.Name);
		}

		#endregion

		#region TestWA_CalcMaxVolumeInfo

		public void TestWA_CalcMaxVolumeInfo()
		{
			AssertEquals(WhsArea.Schema.WA_CalcMaxVolume, Area.WA_CalcMaxVolumeInfo.Name);
		}

		#endregion

		#region TestWA_CalcCurrentWeightInfo

		public void TestWA_CalcCurrentWeightInfo()
		{
			AssertEquals(WhsArea.Schema.WA_CalcCurrentWeight, Area.WA_CalcCurrentWeightInfo.Name);
		}

		#endregion

		#region TestWA_CalcCurrentVolumeInfo

		public void TestWA_CalcCurrentVolumeInfo()
		{
			AssertEquals(WhsArea.Schema.WA_CalcCurrentVolume, Area.WA_CalcCurrentVolumeInfo.Name);
		}

		#endregion

		#region TestWA_CalcAvailableWeightInfo

		public void TestWA_CalcAvailableWeightInfo()
		{
			AssertEquals(WhsArea.Schema.WA_CalcAvailableWeight, Area.WA_CalcAvailableWeightInfo.Name);
		}

		#endregion

		#region TestWA_CalcAvailableVolumeInfo

		public void TestWA_CalcAvailableVolumeInfo()
		{
			AssertEquals(WhsArea.Schema.WA_CalcAvailableVolume, Area.WA_CalcAvailableVolumeInfo.Name);
		}

		#endregion

		#region TestWA_Name_Translatable

		public void TestWA_Name_Translatable()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 2);
			var area = Helper.CreateArea(whs, "TestArea");
			AssertEquals("Pre-condition: WA_Name.", "TestArea", area.WA_Name);

			string resKey = area.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(area, "TestArea").ResourceKey;
			AssertEquals("TestArea", area.WA_NameMultilingual);
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试区"));
				AssertEquals("测试区", area.WA_NameMultilingual);
			}
		}

		#endregion

		#region TestRFPickPackPrinterPK

		public void TestRFPickPackPrinterPK()
		{
			var printer = Helper.CreatePrintQueue("PRINTER");
			var area = Factory.New<WhsArea>();
			AssertEquals("RF Pick Pack Printer should be empty if there is no default printer created.", ZGuid.Empty, area.RFPickPackPrinterPK);

			area.RFPickPackPrinterPK = printer.PK;
			AssertEquals("RF Pick Pack Printer should return the printer that was set.", printer.PK, area.RFPickPackPrinterPK);

			area.RFPickPackPrinterPK = ZGuid.Invalid;
			AssertNoErrors("Validation should be suspended for setting the Printer PK on the StmDefaultPrinter.", StmDefaultPrinter.LoadDefaultPrinter(Factory, area).SDP_SQ_PrinterInfo);
			AssertEquals("If Invalid Guid is set, the property should return Invalid Guid.", ZGuid.Invalid, area.RFPickPackPrinterPK);
		}

		#endregion

		#region TestCreateAndSaveState

		public void TestCreateAndSaveState()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var area = Helper.CreateArea(warehouse, "AREA");
			AssertEquals("Precondition", 0, area.Logs.GetAllLogs().Count);
			Factory.Save();
			Assert("Auto Log must be created", area.Logs.GetAllLogs().Count > 0);
		}

		public void TestCreateAndSaveState_WhenDefaultIsChanged()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var defaultArea = warehouse.Areas.First(a => a.WA_Name == "DEFAULT");
			Assert(defaultArea.WA_IsDefaultPickArea);
			Assert(defaultArea.WA_IsDefaultPutawayArea);
			Factory.Save();
			var area = Helper.CreateArea(warehouse, "AREA");
			area.WA_IsDefaultPickArea = true;
			area.WA_IsDefaultPutawayArea = true;

			Factory.Save();

			Assert(!defaultArea.WA_IsDefaultPickArea);
			Assert(!defaultArea.WA_IsDefaultPutawayArea);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var area = Factory.New<WhsArea>();
			AssertEquals("Area", area.HumanReadableName);

			area.WA_Name = "ar1";
			AssertEquals("Area ar1", area.HumanReadableName);

			var resKey = area.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(area, "ar1").ResourceKey;
			AssertEquals("ar1", area.WA_NameMultilingual);

			using (Res.TemporarilySwitchLanguage("CHS"))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试区"));
				AssertEquals("Area 测试区", area.HumanReadableName); //mock translation, "Area" is not translated
			}
		}

		#endregion

		#region TestICanDelete

		public void TestICanDelete()
		{
			var branch = Helper.CreateGlbBranch("BR1");
			var client = Helper.CreateClient("CLIENT");
			var whs = Helper.CreateWarehouse("WHS", client.MainAddress, branch, shouldPreGenerateDDL: false);
			var area1 = whs.Areas[0];
			AssertEquals("Can't delete area if warehouse only has one", false, area1.CanDelete);
			AssertEquals("Error Message", "A warehouse must have at least one area", area1.ReasonForNotAbleToDelete);

			var area2 = whs.Areas.AddNew();
			AssertEquals("Can delete this", true, area2.CanDelete);
			AssertNull("No error message", area2.ReasonForNotAbleToDelete);
		}

		#endregion

		#region Implementation

		void SetupAreaMaxMeasures(WhsArea area)
		{
			ZDecimal maxWeight = 10m;
			ZDecimal maxVolume = 1m;

			foreach (WhsLocation locn in area.PickLocations)
			{
				locn.WLV_MaxWeight = maxWeight;
				locn.WLV_MaxCubic = maxVolume;

				locn.WLV_MaxWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
				locn.WLV_MaxCubicUnit = Enterprise.Core.Constants.Volume.CubicMetres;

				maxWeight *= 2m;
				maxVolume *= 2m;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			return Helper.CreateArea(warehouse, "AREA");
		}

		WhsWarehouse whs;
		WhsWarehouse Whs
		{
			get { return whs ?? (whs = Helper.CreateWarehouse("1")); }
			set { whs = value; }
		}

		WhsArea area;
		WhsArea Area
		{
			get
			{
				if (area == null)
				{
					area = (WhsArea)GetNewBusinessObject();
					area.WA_WW_Whs = Whs.PK;
				}
				return area;
			}
			set { area = value; }
		}

		#endregion
	}

	#region WhsAreaDocumentSupporterTest

	class WhsAreaDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		#region TestGetContactOrganisation

		public override void TestGetContactOrganisation()
		{
			var area = (WhsArea)BusinessObject;
			AssertNull(area.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY));
		}

		#endregion

		#region TestGetDocBusinessObjects

		public override void TestGetDocBusinessObjects()
		{
			IBODocDataProvider[] docBizoList = DocSupporter.GetDocumentWrappers(DataContext, null);
			AssertEquals("Only one Document Wrapper should be generated, only one document should be printed.", 1, docBizoList.Length);
			AssertEquals(typeof(WhsArea), docBizoList[0].ParentBusinessObject.GetType());
		}

		#endregion

		#region TestBusinessContext

		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsArea, DocSupporter.BusinessContext);
		}

		#endregion

		#region ShowReasonForNotPrinting

		protected override bool GetExpectedShowReasonForNotPrinting()
		{
			return false;
		}

		#endregion

		#region Implementation

		#region Properties

		protected override Constants.DataContext DataContext
		{
			get { return Enterprise.Core.Constants.DataContext.GenericFreightJob; }
		}

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsConfigWarehouseCustomiseDocuments;

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return area;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 2);
			area = Helper.CreateArea(warehouse, "AREA");
		}
		WhsArea area;

		#endregion
	}

	#endregion

	#region WhsAreaDocumentSupporterDocumentSupporterTest

	[TestedType(typeof(WhsAreaDocumentSupporter))]
	class WhsAreaDocumentSupporterDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 2);
			var area = Helper.CreateArea(warehouse, "AREA");
			return area;
		}

		protected WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}
		WhsTestHelperFunctionsEnv helper;
	}

	#endregion

	#region WhsAreaAffectLocationViewTest

	[TestedType(typeof(WhsArea))]
	class WhsAreaAffectLocationViewTest : AffectLocationViewTestCase
	{
		#region TestReloadAreaTypesWhenAreaTypeChanged

		protected override void TestReloadLocationForAffectorsCore()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "R");
			var area1 = helper.CreateArea(warehouse, "A", AreaTypes.Codes.FreeStore);
			row.Locations.Single().WLV_WA_PickingArea = area1.PK;
			row.Locations.Single().WLV_WA_PutawayArea = area1.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsLocationViewSchema.Constants.TableName, 3); // One for Load from newFactory, one for WhsArea.PickLocations.RefreshFromDb, and one for WhsArea.PutawayLocations.RefreshFromDb
			expectedDbHits.Add(WhsAreaSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsRowSchema.Constants.TableName, 1);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				var warehouseInNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
				var areaInNewFactory = newFactory.Load<WhsArea>(area1.PK);
				var rowInNewFactory = newFactory.Load<WhsRow>(row.PK);
				var location = rowInNewFactory.Locations.Single();
				AssertEquals($"Precondition: WLV_PickingAreaType should be {AreaTypes.Codes.FreeStore}", AreaTypes.Codes.FreeStore, location.WLV_PickingAreaType);
				AssertEquals($"Precondition: WLV_PutawayAreaType should be {AreaTypes.Codes.FreeStore}", AreaTypes.Codes.FreeStore, location.WLV_PutawayAreaType);
				areaInNewFactory.WA_AreaType = AreaTypes.Codes.Bonded;
				newFactory.Save();
				AssertEquals($"WLV_PickingAreaType should changed to {AreaTypes.Codes.Bonded}", AreaTypes.Codes.Bonded, location.WLV_PickingAreaType);
				AssertEquals($"WLV_PutawayAreaType should changed to {AreaTypes.Codes.Bonded}", AreaTypes.Codes.Bonded, location.WLV_PutawayAreaType);
			}
		}

		#endregion

		protected override IEnumerable<SchemaColumn> ExpectedColumnsThatAffectLocationView
		{
			get
			{
				return new SchemaColumn[]
				{
					WhsAreaSchema.WA_AreaType
				};
			}
		}

		protected override IEnumerable<WhsLocation> GetLocations(IAffectLocationView p)
		{
			WhsArea parent = (WhsArea)p;

			return parent.PickLocations.Concat(parent.PutawayLocations);
		}

		protected override IAffectLocationView GetNewParent()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1);
			var area = helper.CreateArea(warehouse, "PICKING", AreaTypes.Codes.FreeStore);
			row.Locations.Single().WLV_WA_PickingArea = area.PK;
			row.Locations.Single().WLV_WA_PutawayArea = area.PK;
			return area;
		}
	}

	#endregion
}
