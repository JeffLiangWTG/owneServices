using CargoWise.EntityFramework.Testing;

namespace Enterprise.Packing.Business.Testing
{
	class PkgPackageJobPackageHeaderPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckKPJ_KPH_PackageHeader()
		{
			var pivot = Factory.New<PkgPackageJobPackageHeaderPivot>();
			AssertNoErrors("Precondition", pivot.KPJ_KPH_PackageHeaderInfo);

			pivot.Validation.ValidateKPJ_KPH_PackageHeader();
			AssertHasError(pivot.KPJ_KPH_PackageHeaderInfo, "Please enter a value.");

			var packageID = Factory.New<PkgPackageHeader>();
			pivot.KPJ_KPH_PackageHeader = packageID.PK;
			AssertNoErrors(pivot.KPJ_KPH_PackageHeaderInfo);
		}

		public void TestCheckKPJ_KJ_PackageJob()
		{
			var pivot = Factory.New<PkgPackageJobPackageHeaderPivot>();
			AssertNoErrors("Precondition", pivot.KPJ_KJ_PackageJobInfo);

			pivot.Validation.ValidateKPJ_KJ_PackageJob();
			AssertHasError(pivot.KPJ_KJ_PackageJobInfo, "Please enter a value.");

			var packageJob = Factory.New<PkgPackageJob>();
			pivot.KPJ_KJ_PackageJob = packageJob.PK;
			AssertNoErrors(pivot.KPJ_KJ_PackageJobInfo);
		}
	}
}
