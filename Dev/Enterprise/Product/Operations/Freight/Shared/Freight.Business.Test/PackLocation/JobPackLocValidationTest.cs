using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobPackLocValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLocationWhsGuid()
		{
			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("ZZZ");
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);

			var location = Factory.NewWithValidTestData<PackLocation>();
			location.LocationWhsGuid = ZGuid.Empty;
			AssertNoError(location.LocationWhsGuidInfo, "The code you have selected is not in the list.");

			location.LocationWhsGuid = ZGuid.Invalid;
			AssertHasError(location.LocationWhsGuidInfo, "The code you have selected is not in the list.");

			location.LocationWhsGuid = warehouse.PK;
			AssertNoError(location.LocationWhsGuidInfo, "The code you have selected is not in the list.");

			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			location.LocationWhsGuid = ZGuid.Invalid;
			AssertNoError(location.LocationWhsGuidInfo, "The code you have selected is not in the list.");
		}

		public void TestLocationStringValidation()
		{
			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var location = Factory.NewWithValidTestData<PackLocation>();

			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("1");
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);

			location.LocationWhsGuid = warehouse.PK;

			location.LocationString = "";
			location.Validation.ValidateAll();
			AssertHasError(location.LocationStringInfo, "Please enter a valid Location.");

			location.LocationString = "HI";
			AssertHasError(location.LocationStringInfo, "Please enter a valid Location Row.");

			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			location.LocationString = "ZZ";
			AssertNoError(location.LocationStringInfo, "Please enter a valid Location Row.");

			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			location.LocationString = "A-1-1-1";
			AssertNoError(location.LocationStringInfo, "Please enter a valid Location Row.");
		}

		IWhsTransactionTestHelper Helper => helper ?? (helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory));
		IWhsTransactionTestHelper helper;
	}
}
