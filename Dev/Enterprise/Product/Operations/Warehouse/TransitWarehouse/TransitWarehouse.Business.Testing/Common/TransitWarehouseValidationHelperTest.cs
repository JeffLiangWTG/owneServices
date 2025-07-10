using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business.Common;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TransitWarehouseValidationHelperTest : TestCaseWithFactory
	{
		#region GetWhsUNDGLimitValidationMessage
		public void TestGetWhsUNDGLimitValidationMessage_LimitHasError()
		{
			var warehouse = CreateTestData();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100, totalVolumeLimit: 100);
			Factory.Save();

			undgLimit.AddRowError("TestError");
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage()
		{
			var warehouse = CreateTestData();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals(@"DG '0004a' is at 50% weight capacity.
DG '0004a' is at 50% volume capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_DGIsNotAllowed()
		{
			var warehouse = CreateTestData();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 0, totalVolumeLimit: 0);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("This Warehouse cannot store DG '0004a', but there are already DG '0004a' packages in warehouse.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_OnlyVolumeLimited()
		{
			var warehouse = CreateTestData();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 0, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("DG '0004a' is at 50% volume capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_OnlyWeightLimited()
		{
			var warehouse = CreateTestData();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100, totalVolumeLimit: 0);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("DG '0004a' is at 50% weight capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_NotExceedLimitAndLessThanDGThresholdPercentage()
		{
			var warehouse = CreateTestData();
			warehouse.WW_DGThresholdPercentage = 80;

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_NotExceedLimitAndDGThresholdPercentageIsZero()
		{
			var warehouse = CreateTestData();
			warehouse.WW_DGThresholdPercentage = 0;

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_CountryReference()
		{
			var warehouse = CreateTestData();
			var countryReference = Helper.CreateCountryReference("4567");
			Helper.CreateCountryReferencePivot(countryReference, "0004a");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 100, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals(@"Country Reference '4567' is at 50% weight capacity.
Country Reference '4567' is at 50% volume capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_CountryReference_CountryReferenceIsNotAllowed()
		{
			var warehouse = CreateTestData();
			var countryReference = Helper.CreateCountryReference("4567");
			Helper.CreateCountryReferencePivot(countryReference, "0004a");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 0, totalVolumeLimit: 0);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("This Warehouse cannot store Country Reference '4567', but there are already Country Reference '4567' packages in warehouse.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_CountryReference_OnlyVolumeLimited()
		{
			var warehouse = CreateTestData();
			var countryReference = Helper.CreateCountryReference("4567");
			Helper.CreateCountryReferencePivot(countryReference, "0004a");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 0, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("Country Reference '4567' is at 50% volume capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_CountryReference_OnlyWeightLimited()
		{
			var warehouse = CreateTestData();
			var countryReference = Helper.CreateCountryReference("4567");
			Helper.CreateCountryReferencePivot(countryReference, "0004a");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 100, totalVolumeLimit: 0);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("Country Reference '4567' is at 50% weight capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_CountryReference_NotExceedLimitAndLessThanDGThresholdPercentage()
		{
			var warehouse = CreateTestData();
			warehouse.WW_DGThresholdPercentage = 80;

			var countryReference = Helper.CreateCountryReference("4567");
			Helper.CreateCountryReferencePivot(countryReference, "0004a");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 100, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_CountryReference_NotExceedLimitAndDGThresholdPercentageIsZero()
		{
			var warehouse = CreateTestData();
			warehouse.WW_DGThresholdPercentage = 0;

			var countryReference = Helper.CreateCountryReference("4567");
			Helper.CreateCountryReferencePivot(countryReference, "0004a");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 100, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_LimitIsChanged_DGIsNotAllowed()
		{
			var warehouse = CreateTestData();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004b", totalWeightLimit: 0, totalVolumeLimit: 0);
			Factory.Save();

			undgLimit.WWD_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("This Warehouse cannot store DG '0004a', but there are already DG '0004a' packages in warehouse.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_LimitIsAdded_DGIsNotAllowed()
		{
			var warehouse = CreateTestData();
			Factory.Save();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 0, totalVolumeLimit: 0);
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("This Warehouse cannot store DG '0004a', but there are already DG '0004a' packages in warehouse.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_LimitIsAdded_OnlyVolumeLimited()
		{
			var warehouse = CreateTestData();
			Factory.Save();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 0, totalVolumeLimit: 100);
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("DG '0004a' is at 50% volume capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_LimitIsAdded_OnlyWeightLimited()
		{
			var warehouse = CreateTestData();
			Factory.Save();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100, totalVolumeLimit: 0);
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("DG '0004a' is at 50% weight capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_CountryReference_LimitIsChanged_CountryReferenceIsNotAllowed()
		{
			var warehouse = CreateTestData();
			var countryReference = Helper.CreateCountryReference("1234");
			var countryReference2 = Helper.CreateCountryReference("4567");
			Helper.CreateCountryReferencePivot(countryReference2, "0004a");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 0, totalVolumeLimit: 0);
			Factory.Save();

			undgLimit.WWD_DCR_UNDGCountryReference = countryReference2.PK;
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("This Warehouse cannot store Country Reference '4567', but there are already Country Reference '4567' packages in warehouse.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_CountryReference_LimitIsAdded_CountryReferenceIsNotAllowed()
		{
			var warehouse = CreateTestData();
			var countryReference = Helper.CreateCountryReference("4567");
			Helper.CreateCountryReferencePivot(countryReference, "0004a");
			Factory.Save();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 0, totalVolumeLimit: 0);
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("This Warehouse cannot store Country Reference '4567', but there are already Country Reference '4567' packages in warehouse.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_CountryReference_LimitIsAdded_OnlyVolumeLimited()
		{
			var warehouse = CreateTestData();
			var countryReference = Helper.CreateCountryReference("4567");
			Helper.CreateCountryReferencePivot(countryReference, "0004a");
			Factory.Save();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 0, totalVolumeLimit: 100);
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("Country Reference '4567' is at 50% volume capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_CountryReference_LimitIsAdded_OnlyWeightLimited()
		{
			var warehouse = CreateTestData();
			var countryReference = Helper.CreateCountryReference("4567");
			Helper.CreateCountryReferencePivot(countryReference, "0004a");
			Factory.Save();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 100, totalVolumeLimit: 0);
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("Country Reference '4567' is at 50% weight capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_UNDGClass()
		{
			var warehouse = CreateTestData();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1", totalWeightLimit: 100, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals(@"UNDG Class '1' is at 50% weight capacity.
UNDG Class '1' is at 50% volume capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_UNDGClass_UNDGClassIsNotAllowed()
		{
			var warehouse = CreateTestData();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1", totalWeightLimit: 0, totalVolumeLimit: 0);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("This Warehouse cannot store UNDG Class '1', but there are already UNDG Class '1' packages in warehouse.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_UNDGClass_OnlyVolumeLimited()
		{
			var warehouse = CreateTestData();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1", totalWeightLimit: 0, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("UNDG Class '1' is at 50% volume capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_UNDGClass_OnlyWeightLimited()
		{
			var warehouse = CreateTestData();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1", totalWeightLimit: 100, totalVolumeLimit: 0);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("UNDG Class '1' is at 50% weight capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_UNDGClass_NotExceedLimitAndLessThanDGThresholdPercentage()
		{
			var warehouse = CreateTestData();
			warehouse.WW_DGThresholdPercentage = 80;

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1", totalWeightLimit: 100, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_UNDGClass_NotExceedLimitAndDGThresholdPercentageIsZero()
		{
			var warehouse = CreateTestData();
			warehouse.WW_DGThresholdPercentage = 0;

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1", totalWeightLimit: 100, totalVolumeLimit: 100);
			Factory.Save();

			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_UNDGClass_LimitIsChanged_UNDGClassIsNotAllowed()
		{
			var warehouse = CreateTestData();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "7", totalWeightLimit: 0, totalVolumeLimit: 0);
			Factory.Save();

			undgLimit.WWD_UNDGClass = "1";
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("This Warehouse cannot store UNDG Class '1', but there are already UNDG Class '1' packages in warehouse.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_UNDGClass_LimitIsAdded_UNDGClassIsNotAllowed()
		{
			var warehouse = CreateTestData();
			Factory.Save();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1", totalWeightLimit: 0, totalVolumeLimit: 0);
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("This Warehouse cannot store UNDG Class '1', but there are already UNDG Class '1' packages in warehouse.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_UNDGClass_LimitIsAdded_OnlyVolumeLimited()
		{
			var warehouse = CreateTestData();
			Factory.Save();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1", totalWeightLimit: 0, totalVolumeLimit: 100);
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("UNDG Class '1' is at 50% volume capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		public void TestGetWhsUNDGLimitValidationMessage_UNDGClass_LimitIsAdded_OnlyWeightLimited()
		{
			var warehouse = CreateTestData();
			Factory.Save();

			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1", totalWeightLimit: 100, totalVolumeLimit: 0);
			var helper = new TransitWarehouseValidationHelper();
			AssertEquals("UNDG Class '1' is at 50% weight capacity.", helper.GetWhsUNDGLimitValidationMessage(undgLimit));
		}

		#endregion

		#region Implementation

		WhsWarehouse CreateTestData(string warehouseCode = "WHS", bool createDGItem = true)
		{
			var warehouse = Helper.CreateTRWWarehouse(warehouseCode);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1" + warehouseCode == "WHS" ? string.Empty : warehouseCode, warehouse.PK, location.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "REF1" + warehouseCode == "WHS" ? string.Empty : warehouseCode);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package = packageState.Package;
			if (createDGItem)
			{
				var dgItem1 = package.UNDGs.AddNew();
				dgItem1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				dgItem1.DI_DGWeight = 50m;
				dgItem1.DI_UnitOfWeight = Constants.Weight.Kilograms;
				dgItem1.DI_DGVolume = 50m;
				dgItem1.DI_UnitOfVolume = Constants.Volume.CubicMetres;
			}
			Factory.Save();

			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_DGThresholdPercentage = 10;

			return warehouse;
		}

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
