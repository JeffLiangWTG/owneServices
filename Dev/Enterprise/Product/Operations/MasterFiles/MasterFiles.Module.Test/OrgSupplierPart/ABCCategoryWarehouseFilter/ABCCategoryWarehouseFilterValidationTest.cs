using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ABCCategoryWarehouseFilterValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateWJ_Category

		public void TestValidateWJ_Category()
		{
			AssertNoErrors("Precondition", Filter.WJ_CategoryInfo);

			Filter.WJ_Category = "XXX";
			AssertNoErrors(Filter.WJ_CategoryInfo);
			AssertHasWarning(Filter.WJ_CategoryInfo, "You have not entered a valid code.");

			Filter.WJ_Category = "A B";
			AssertNoErrors(Filter.WJ_CategoryInfo);
			AssertHasWarning(Filter.WJ_CategoryInfo, "You have not entered a valid code.");

			Filter.WJ_Category = "A";
			AssertNoErrors(Filter.WJ_CategoryInfo);
			AssertNoWarnings(Filter.WJ_CategoryInfo);

			Filter.WJ_Category = "B";
			AssertNoErrors(Filter.WJ_CategoryInfo);
			AssertNoWarnings(Filter.WJ_CategoryInfo);
		}

		#endregion

		#region TestValidateWJ_WW_Warehouse

		public void TestValidateWJ_WW_Warehouse()
		{
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			Factory.Save();

			AssertNoErrors("Precondition", Filter.WJ_WW_WarehouseInfo);

			Filter.WJ_WW_Warehouse = ZGuid.Invalid;
			AssertHasError(Filter.WJ_WW_WarehouseInfo, "Enter a valid Warehouse.");

			Filter.WJ_WW_Warehouse = warehouse.PK;
			AssertNoErrors(Filter.WJ_WW_WarehouseInfo);

			Filter.WJ_WW_Warehouse = ZGuid.Empty;
			AssertNoErrors(Filter.WJ_WW_WarehouseInfo);
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			AssertEquals("Precondition", false, Filter.HasErrors);
			AssertEquals("Precondition", false, Filter.HasWarnings);

			using (Filter.GetValidationSuspender())
			{
				Filter.WJ_Category = "XXX";
				AssertEquals(false, Filter.HasErrors);
				AssertEquals(false, Filter.HasWarnings);
			}

			Filter.Validation.ValidateAll();
			AssertEquals(false, Filter.HasErrors);
			AssertEquals(true, Filter.HasWarnings);

			Filter.WJ_Category = "";
			AssertEquals(false, Filter.HasErrors);
			AssertEquals(false, Filter.HasWarnings);

			using (Filter.GetValidationSuspender())
			{
				Filter.WJ_WW_Warehouse = ZGuid.Invalid;
				AssertEquals(false, Filter.HasErrors);
			}

			Filter.Validation.ValidateAll();
			AssertEquals(true, Filter.HasErrors);
		}

		#endregion

		#region Implementation

		ABCCategoryWarehouseFilter Filter
		{
			get { return filter ?? (filter = new ABCCategoryWarehouseFilter()); }
		}

		ABCCategoryWarehouseFilter filter;

		#endregion
	}
}
