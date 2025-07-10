using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsUNDGLimitValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestTransitWarehouse

		public void TestTransitWarehouse()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			AssertEquals("TRW", warehouse.WW_WarehouseType);

			TestWarehouseUNDGLimitConfig(warehouse);
		}

		#endregion

		#region TestProductWarehouse

		public void TestProductWarehouseUNDGLimitConfig()
		{
			var warehouse = Helper.CreateWarehouse("AAA");
			TestWarehouseUNDGLimitConfig(warehouse);
		}

		public void TestProductWarehouseHasUNDGProductWarning()
		{
			TestValidateAll_HasUNDGProductWarning(WarehouseTypes.Codes.Product);
		}

		#endregion

		#region TestFTZWarehouse

		public void TestFTZWarehouseUNDGLimitConfig()
		{
			var warehouse = Helper.CreateWarehouse("AAA");
			warehouse.WW_WarehouseType = "FTZ";
			TestWarehouseUNDGLimitConfig(warehouse);
		}

		public void TestFTZWarehouseHasUNDGProductWarning()
		{
			TestValidateAll_HasUNDGProductWarning(WarehouseTypes.Codes.FreeTradeZone);
		}

		#endregion

		#region TestWarehouseUNDGLimitConfig

		void TestWarehouseUNDGLimitConfig(WhsWarehouse warehouse)
		{
			TestCheckWWD_DG(warehouse);
			TestCheckWWD_DCR_UNDGCountryReference(warehouse);
			TestCheckWWD_UNDGClass(warehouse);
			TestCheckWWD_TotalWeightLimit(warehouse);
			TestCheckWWD_TotalWeightLimitUQ(warehouse);
			TestCheckWWD_TotalVolumeLimit(warehouse);
			TestCheckWWD_TotalVolumeLimitUQ(warehouse);
		}

		void TestCheckWWD_DG(WhsWarehouse warehouse)
		{
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, "0004a");
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, "3005A");
			AssertNoErrors(undgLimit1.WWD_DGInfo);
			AssertNoErrors(undgLimit2.WWD_DGInfo);
			var undg = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();

			undgLimit1.WWD_DG = ZGuid.Empty;
			AssertHasError(undgLimit1.WWD_DGInfo, "You need to enter either DG Substance or UNDG Country/Region Reference or UNDG Class.");

			undgLimit1.WWD_DG = undg.PK;
			undgLimit2.WWD_DG = undg.PK;
			AssertNoErrors(undgLimit1.WWD_DGInfo);
			AssertHasError(undgLimit2.WWD_DGInfo, "UNDG Substance is duplicated.");
			undgLimit2.WWD_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3005", "A", "IMO").First().PK;
			AssertNoErrors(undgLimit1.WWD_DGInfo);
			AssertNoErrors(undgLimit2.WWD_DGInfo);
		}

		void TestCheckWWD_DCR_UNDGCountryReference(WhsWarehouse warehouse)
		{
			var countryReference1 = Helper.CreateCountryReference();
			var countryReference2 = Helper.CreateCountryReference();
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference1);
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference2);
			AssertNoErrors(undgLimit1.WWD_DCR_UNDGCountryReferenceInfo);
			AssertNoErrors(undgLimit2.WWD_DCR_UNDGCountryReferenceInfo);

			undgLimit1.WWD_DCR_UNDGCountryReference = ZGuid.Empty;
			AssertHasError(undgLimit1.WWD_DCR_UNDGCountryReferenceInfo, "You need to enter either DG Substance or UNDG Country/Region Reference or UNDG Class.");

			undgLimit1.WWD_DCR_UNDGCountryReference = countryReference1.PK;
			undgLimit2.WWD_DCR_UNDGCountryReference = countryReference1.PK;
			AssertNoErrors(undgLimit1.WWD_DCR_UNDGCountryReferenceInfo);
			AssertHasError(undgLimit2.WWD_DCR_UNDGCountryReferenceInfo, "UNDG Country/Region Reference is duplicated.");

			undgLimit2.WWD_DCR_UNDGCountryReference = countryReference2.PK;
			AssertNoErrors(undgLimit1.WWD_DCR_UNDGCountryReferenceInfo);
			AssertNoErrors(undgLimit2.WWD_DCR_UNDGCountryReferenceInfo);
		}

		void TestCheckWWD_UNDGClass(WhsWarehouse warehouse)
		{
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1");
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "7");
			AssertNoErrors(undgLimit1.WWD_UNDGClassInfo);
			AssertNoErrors(undgLimit2.WWD_UNDGClassInfo);

			undgLimit1.WWD_UNDGClass = ZString.Empty;
			AssertHasError(undgLimit1.WWD_UNDGClassInfo, "You need to enter either DG Substance or UNDG Country/Region Reference or UNDG Class.");

			undgLimit1.WWD_UNDGClass = "1";
			undgLimit2.WWD_UNDGClass = "1";
			AssertNoErrors(undgLimit1.WWD_UNDGClassInfo);
			AssertHasError(undgLimit2.WWD_UNDGClassInfo, "UNDG Class is duplicated.");

			undgLimit2.WWD_UNDGClass = "2";
			AssertNoErrors(undgLimit1.WWD_UNDGClassInfo);
			AssertNoErrors(undgLimit2.WWD_UNDGClassInfo);

			undgLimit2.WWD_UNDGClass = "7";
			AssertNoErrors(undgLimit1.WWD_UNDGClassInfo);
			AssertNoErrors(undgLimit2.WWD_UNDGClassInfo);

			undgLimit2.WWD_UNDGClass = "Comb";
			AssertNoErrors(undgLimit1.WWD_UNDGClassInfo);
			AssertNoErrors(undgLimit2.WWD_UNDGClassInfo);

			undgLimit2.WWD_UNDGClass = "0";
			AssertNoErrors(undgLimit1.WWD_UNDGClassInfo);
			AssertHasError(undgLimit2.WWD_UNDGClassInfo, "UNDG Class is invalid.");
		}

		void TestCheckWWD_TotalWeightLimit(WhsWarehouse warehouse)
		{
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 10, totalVolumeLimit: 0m);
			AssertNoErrors(undgLimit.WWD_TotalWeightLimitInfo);

			undgLimit.WWD_TotalWeightLimit = -1m;
			AssertHasError(undgLimit.WWD_TotalWeightLimitInfo, "Cannot enter a negative Total Weight Limit.");

			undgLimit.WWD_TotalWeightLimit = 10m;
			AssertNoErrors(undgLimit.WWD_TotalWeightLimitInfo);
		}

		void TestCheckWWD_TotalWeightLimitUQ(WhsWarehouse warehouse)
		{
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 0, totalWeightLimitUQ: "");
			AssertNoErrors(undgLimit.WWD_TotalWeightLimitUQInfo);

			undgLimit.WWD_TotalWeightLimit = 10m;
			AssertHasError(undgLimit.WWD_TotalWeightLimitUQInfo, "Please enter a Total Weight Limit UQ.");

			undgLimit.WWD_TotalWeightLimitUQ = Constants.Weight.Kilograms;
			AssertNoErrors(undgLimit.WWD_TotalWeightLimitUQInfo);

			undgLimit.WWD_TotalWeightLimitUQ = "";
			AssertHasError("Precondition", undgLimit.WWD_TotalWeightLimitUQInfo, "Please enter a Total Weight Limit UQ.");
			undgLimit.WWD_TotalWeightLimit = 0m;
			AssertNoErrors(undgLimit.WWD_TotalWeightLimitUQInfo);

			undgLimit.WWD_TotalWeightLimitUQ = "XX";
			AssertHasError(undgLimit.WWD_TotalWeightLimitUQInfo, "Enter a valid Total Weight Limit UQ.");
		}

		void TestCheckWWD_TotalVolumeLimit(WhsWarehouse warehouse)
		{
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 0, totalVolumeLimit: 10m);
			AssertNoErrors(undgLimit.WWD_TotalVolumeLimitInfo);

			undgLimit.WWD_TotalVolumeLimit = -1m;
			AssertHasError(undgLimit.WWD_TotalVolumeLimitInfo, "Cannot enter a negative Total Volume Limit.");

			undgLimit.WWD_TotalVolumeLimit = 10m;
			AssertNoErrors(undgLimit.WWD_TotalVolumeLimitInfo);
		}

		void TestCheckWWD_TotalVolumeLimitUQ(WhsWarehouse warehouse)
		{
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalVolumeLimit: 0, totalVolumeLimitUQ: "");
			AssertNoErrors(undgLimit.WWD_TotalVolumeLimitUQInfo);

			undgLimit.WWD_TotalVolumeLimit = 10m;
			AssertHasError(undgLimit.WWD_TotalVolumeLimitUQInfo, "Please enter a Total Volume Limit UQ.");

			undgLimit.WWD_TotalVolumeLimitUQ = Constants.Volume.CubicMetres;
			AssertNoErrors(undgLimit.WWD_TotalVolumeLimitUQInfo);

			undgLimit.WWD_TotalVolumeLimitUQ = "";
			AssertHasError("Precondition", undgLimit.WWD_TotalVolumeLimitUQInfo, "Please enter a Total Volume Limit UQ.");
			undgLimit.WWD_TotalVolumeLimit = 0m;
			AssertNoErrors(undgLimit.WWD_TotalVolumeLimitUQInfo);

			undgLimit.WWD_TotalVolumeLimitUQ = "XX";
			AssertHasError(undgLimit.WWD_TotalVolumeLimitUQInfo, "Enter a valid Total Volume Limit UQ.");
		}

		#endregion

		#region TestValidateAll_HasUNDGProductWarning

		void TestValidateAll_HasUNDGProductWarning(ZString warehouseType)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 4, 1);
			var warehouse = data.Whs1;
			warehouse.WW_WarehouseType = warehouseType;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = helper.CreateClient("WHS1TST");
			var part = helper.CreateProduct(orgPK, "P1");
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(part.PK, "OP", substance, 50m, 50m);

			var receivePK = helper.CreateWhsReceive(orgPK, warehouse.PK, "1", Notify);
			helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 1m, "A-1");
			helper.FinaliseDocket(receivePK);
			Factory.Save();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, substance, totalVolumeLimit: 10m, totalWeightLimit: 10m);
			undgLimit.Validation.ValidateAll();
			AssertHasRowWarning(undgLimit, "DG '0004a' is at 500% weight capacity.\r\nDG '0004a' is at 500% volume capacity.");
		}

		public void TestValidateAll_HasTransitPackageWarning()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a");
			Factory.Save();

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var packageJob = Helper.CreatePackageJob(rtu);
			var package = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = Helper.CreateWhsItemPackageState("ARV", location, rtu, package);

			var dgItem = Factory.NewWithValidTestData<UNDGDataItem>();
			dgItem.DI_ParentID = package.PK;
			dgItem.DI_ParentTableCode = "KP";
			dgItem.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			dgItem.DI_DGWeight = 50m;
			dgItem.DI_UnitOfWeight = Constants.Weight.Kilograms;
			dgItem.DI_DGVolume = 50m;
			dgItem.DI_UnitOfVolume = Constants.Volume.CubicMetres;
			Factory.Save();

			undgLimit.Validation.ValidateAll();
			AssertHasRowWarning(undgLimit, "DG '0004a' is at 500% weight capacity.\r\nDG '0004a' is at 500% volume capacity.");
		}

		#endregion
	}
}
