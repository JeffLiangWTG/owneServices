using CargoWise.EntityFramework.Testing;

namespace Enterprise.Packing.Business.Testing
{
	class PkgPackageHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckKPH_PackageID()
		{
			var packageID = Factory.New<PkgPackageHeader>();
			AssertNoErrors("Precondition", packageID.KPH_PackageIDInfo);

			packageID.Validation.ValidateKPH_PackageID();
			AssertHasError(packageID.KPH_PackageIDInfo, "Please enter a Package ID.");

			packageID.KPH_PackageID = "ZZZ";
			AssertNoErrors(packageID.KPH_PackageIDInfo);

			packageID.KPH_PackageID = "";
			AssertHasError(packageID.KPH_PackageIDInfo, "Please enter a Package ID.");
		}
	}
}
