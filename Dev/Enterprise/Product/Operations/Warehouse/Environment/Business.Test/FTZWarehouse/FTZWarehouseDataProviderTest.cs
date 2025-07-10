using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class FTZWarehouseDataProviderTest : TestCaseWithFactory
	{
		#region TestFTZWarehouseDataProviderInstantiatedByObjectFactory

		public void TestFTZWarehouseDataProviderInstantiatedByObjectFactory()
		{
			var result = ObjectFactory.Get<IFTZWarehouseDataProvider>();
			AssertEquals(typeof(FTZWarehouseDataProvider), result.GetType());
		}

		#endregion

		#region TestGetFTZWarehouseQuery

		public void TestGetFTZWarehouseQuery()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var nonFTZWhs = Helper.CreateWarehouse("WH1");
			var inactiveFTZWhs = Helper.CreateFTZWarehouseInUS(warehouseCode: "WH2");
			inactiveFTZWhs.WW_IsActive = false;
			var ftzWhs = helper.CreateFTZWarehouseInUS(warehouseCode: "WH3");
			Factory.Save();

			var query1 = FTZWarehouseDataProvider.GetFTZWarehouseQuery(address);
			AssertNull("Address is not a warehouse address, should not find out warehouse", Factory.LoadTop1<WhsWarehouse>(query1));

			var query2 = FTZWarehouseDataProvider.GetFTZWarehouseQuery(nonFTZWhs.WarehouseAddress);
			AssertNull("Address is not a FTZ warehouse address, should not find out warehouse", Factory.LoadTop1<WhsWarehouse>(query1));

			var query3 = FTZWarehouseDataProvider.GetFTZWarehouseQuery(inactiveFTZWhs.WarehouseAddress);
			AssertNull("Address is a FTZ warehouse address but inactive, should not find out warehouse", Factory.LoadTop1<WhsWarehouse>(query1));

			var query4 = FTZWarehouseDataProvider.GetFTZWarehouseQuery(ftzWhs.WarehouseAddress);
			AssertEquals("Can find out FTZ warehouse", ftzWhs, Factory.LoadTop1<WhsWarehouse>(query4));
		}

		#endregion

		#region TestIsFTZWarehouseDetailedTrackingEnabled_AddressIsNull

		public void TestIsFTZWarehouseDetailedTrackingEnabled_AddressIsNull()
		{
			var provider = new FTZWarehouseDataProvider();
#if NETFRAMEWORK
			AssertExceptionThrown(typeof(ArgumentNullException), @"Value cannot be null.
Parameter name: warehouseAddress", () => provider.IsFTZWarehouseDetailedTrackingEnabled(null));
#elif NET
			AssertExceptionThrown(typeof(ArgumentNullException), @"Value cannot be null. (Parameter 'warehouseAddress')", () => provider.IsFTZWarehouseDetailedTrackingEnabled(null));
#endif
		}

		#endregion

		#region TestIsFTZWarehouseDetailedTrackingEnabled_Inactive

		public void TestIsFTZWarehouseDetailedTrackingEnabled_Inactive()
		{
			var inactiveWhs = Helper.CreateFTZWarehouseInUS();
			inactiveWhs.WW_IsActive = false;
			Factory.Save();

			var provider = new FTZWarehouseDataProvider();
			AssertEquals("When warehouse is inactive, the result shoulde be false", false, provider.IsFTZWarehouseDetailedTrackingEnabled(inactiveWhs.WarehouseAddress));
		}

		#endregion

		#region TestIsFTZWarehouseDetailedTrackingEnabled_NonWarehouseAddress

		public void TestIsFTZWarehouseDetailedTrackingEnabled_NonWarehouseAddress()
		{
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			AssertEquals("Precondition: New Address is different to WarehouseAddress", false, ftzWhs.WarehouseAddress.PK == address.PK);

			var provider = new FTZWarehouseDataProvider();
			AssertEquals("The address is not a FTZ warehouse address, the result should be false", false, provider.IsFTZWarehouseDetailedTrackingEnabled(address));
		}

		#endregion

		#region TestIsFTZWarehouseDetailedTrackingEnabled_SameWarehouseAddress

		public void TestIsFTZWarehouseDetailedTrackingEnabled_SameWarehouseAddress()
		{
			var nonFTZWhs = Helper.CreateWarehouse("WH1");
			var ftzWhs = Helper.CreateFTZWarehouseInUS(warehouseCode: "WH2");
			nonFTZWhs.WW_OA_WarehouseAddress = ftzWhs.WW_OA_WarehouseAddress;
			Factory.Save(); //Should be failed if we add an UX for WW_OA_WarehouseAddress in the future

			var provider = new FTZWarehouseDataProvider();
			AssertEquals("Should only get IsDetailedTrackingEnabled of FTZ warehouse", true, provider.IsFTZWarehouseDetailedTrackingEnabled(ftzWhs.WarehouseAddress));
		}

		#endregion

		#region TestIsFTZWarehouseDetailedTrackingEnabled

		public void TestIsFTZWarehouseDetailedTrackingEnabled()
		{
			var enabledWhs = Helper.CreateFTZWarehouseInUS(warehouseCode: "WH1", isDetailedTrackingEnabled: true);
			var nonEnabledWhs = Helper.CreateFTZWarehouseInUS(warehouseCode: "WH2", isDetailedTrackingEnabled: false);
			Factory.Save();

			var provider = new FTZWarehouseDataProvider();
			AssertEquals(true, provider.IsFTZWarehouseDetailedTrackingEnabled(enabledWhs.WarehouseAddress));
			AssertEquals(false, provider.IsFTZWarehouseDetailedTrackingEnabled(nonEnabledWhs.WarehouseAddress));
		}

		#endregion

		#region Implementation

		#region Helper

		WhsTestHelperFunctionsEnv helper;
		WhsTestHelperFunctionsEnv Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new WhsTestHelperFunctionsEnv(Factory);
				}
				return helper;
			}
		}

		#endregion

		#endregion
	}
}
