using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TypeSafePackageTest : TestCaseWithFactory
	{
		public void TestNewedProperties()
		{
			AssertEquals(typeof(PackageLookups), package.Lookups.GetType());
			AssertEquals(typeof(PackageValidation), package.Validation.GetType());
			AssertEquals(typeof(Bill), package.Bill.GetType());
		}

		JobDeclaration declaration;
		Package package;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			package = declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		}
	}
}
