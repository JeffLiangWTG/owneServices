using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusDecHouseContainerPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLowestBills()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackage package = declaration.Packages.AddNew();
			AssertEquals(declaration.LowestBills, package.Lookups.LowestBills);
		}

		public void TestIrrelevantPackages()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = "BAG";
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = "PKG";
			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 3;
			package3.CW_PackType = "NUM";
			package3.CW_CW_Parent = package1.PK;
			AssertEquals("IrrelevantPackages count", 1, package1.Lookups.IrrelevantPackages.Count);
			AssertEquals("Contains package 2", "2:PKG", package1.Lookups.IrrelevantPackages[0].Code);
			AssertEquals("IrrelevantPackages count", 2, package2.Lookups.IrrelevantPackages.Count);
			AssertEquals("Contains package 1", "1:BAG", package2.Lookups.IrrelevantPackages[0].Code);
			AssertEquals("Contains package 3", "3:NUM", package2.Lookups.IrrelevantPackages[1].Code);
			AssertEquals("IrrelevantPackages count", 2, package3.Lookups.IrrelevantPackages.Count);
			AssertEquals("Contains package 1", "1:BAG", package3.Lookups.IrrelevantPackages[0].Code);
			AssertEquals("Contains package 2", "2:PKG", package3.Lookups.IrrelevantPackages[1].Code);
		}
	}
}
