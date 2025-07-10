using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Packing.Business.Testing
{
	public class BreakDownPackageValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateSplitQuantity

		public void TestValidateSplitQuantity()
		{
			var packages = new PkgPackageCollection(Factory.New<PkgPackageJob>());
			packages.AddNew("BOX", 5);
			var breakdownPackageBO = new BreakDownPackage(packages);

			AssertNoErrors(breakdownPackageBO.SplitQuantityInfo);

			breakdownPackageBO.SplitQuantity = 0;
			AssertHasErrors(breakdownPackageBO.SplitQuantityInfo);

			breakdownPackageBO.SplitQuantity = -1;
			AssertHasErrors(breakdownPackageBO.SplitQuantityInfo);

			breakdownPackageBO.SplitQuantity = 4;
			AssertNoErrors(breakdownPackageBO.SplitQuantityInfo);

			breakdownPackageBO.SplitQuantity = 5;
			AssertHasErrors(breakdownPackageBO.SplitQuantityInfo);

			breakdownPackageBO.SplitQuantity = 6;
			AssertHasErrors(breakdownPackageBO.SplitQuantityInfo);

			var pack2 = packages.AddNew("BOX", 10);
			breakdownPackageBO = new BreakDownPackage(packages);

			breakdownPackageBO.SplitQuantity = 6;
			AssertNoErrors(breakdownPackageBO.SplitQuantityInfo);

			breakdownPackageBO.SplitQuantity = 9;
			AssertNoErrors(breakdownPackageBO.SplitQuantityInfo);

			breakdownPackageBO.SplitQuantity = 10;
			AssertHasError(breakdownPackageBO.SplitQuantityInfo, "You cannot break down Packages into a quantity greater than or equal to the highest number of items in each Package.");

			breakdownPackageBO.SplitQuantity = 11;
			AssertHasErrors(breakdownPackageBO.SplitQuantityInfo);
		}

		public void TestValidateSplitQuantity_Int16()
		{
			var packages = new PkgPackageCollection(Factory.New<PkgPackageJob>());
			packages.AddNew("BOX", 40000);
			var breakdownPackageBO = new BreakDownPackage(packages);

			breakdownPackageBO.SplitQuantity = Int16.MaxValue + 1;
			AssertHasError(breakdownPackageBO.SplitQuantityInfo, "You cannot break down Packages into a quantity greater than 32767.");
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var collection = new PkgPackageCollection(Factory.New<PkgPackageJob>());
			var breakdownPackageBO = new BreakDownPackage(collection);

			using (breakdownPackageBO.GetValidationSuspender())
			{
				breakdownPackageBO.SplitQuantity = -1;
			}

			AssertNoErrors("PreCondition", breakdownPackageBO.SplitQuantityInfo);
			breakdownPackageBO.Validation.ValidateAll();

			AssertHasErrors(breakdownPackageBO.SplitQuantityInfo);
		}

		#endregion
	}
}
