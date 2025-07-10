using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(BreakDownPackage))]
	public class BreakDownPackageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var packages = new PkgPackageCollection(Factory.New<PkgPackageJob>());
			var pack1 = packages.AddNew("BOX", 5);
			var breakdownPackageBO = new BreakDownPackage(packages);

			AssertEquals("breakdownPackageBO.SplitQuantity", 1, breakdownPackageBO.SplitQuantity);
			AssertEquals("breakdownPackageBO.MaxSplit", 4, breakdownPackageBO.MaxSplit);
			AssertEquals("breakdownPackageBO.Validation.GetType()", typeof(BreakDownPackageValidation), breakdownPackageBO.Validation.GetType());

			var pack2 = packages.AddNew("BOX", 10);
			breakdownPackageBO = new BreakDownPackage(packages);

			AssertEquals("breakdownPackageBO.MaxSplit", 9, breakdownPackageBO.MaxSplit);
		}

		public void TestDescription()
		{
			AssertEquals("BreakDownPackage.BreakDownPackageDescription", "Break Down Package", BreakDownPackage.BreakDownPackageDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BreakDownPackage(new PkgPackageCollection(Factory.New<PkgPackageJob>()));
		}
	}
}
