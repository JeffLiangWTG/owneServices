using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business.Testing
{
	public class TransportPackageLabelHelperTest : TestCaseWithFactory
	{
		public void TestGetNewPackageID_HandlingUnit_3PL()
		{
			var hu = Factory.NewWithValidTestData<PkgHandlingUnit>();
			hu.KPU_JobContext = "3PL";
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob.KJ_ParentID = hu.PK;
			packageJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;
			var package = Factory.NewWithValidTestData<PkgPackage>();
			package.KP_KJ_ParentPackageJob = packageJob.PK;

			TransportPackageLabelHelper.CreateDocumentWrapperForNewPackageID(hu);

			var header = package.GetPackageHeader();
			AssertNotNull("Should create PackageHeader and link to HU.", header);
			AssertNull("Should not create Pivot.", Factory.LoadTop1<PkgPackageJobPackageHeaderPivot>(new ZQuery(PkgPackageJobPackageHeaderPivotSchema.KPJ_KPH_PackageHeader, header.PK)));
		}

		public void TestGetNewPackageID_HandlingUnit_TWH()
		{
			var hu = Factory.NewWithValidTestData<PkgHandlingUnit>();
			hu.KPU_JobContext = "TWH";
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob.KJ_ParentID = hu.PK;
			packageJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;
			var package = Factory.NewWithValidTestData<PkgPackage>();
			package.KP_KJ_ParentPackageJob = packageJob.PK;

			TransportPackageLabelHelper.CreateDocumentWrapperForNewPackageID(hu);

			var pivot = Factory.LoadTop1<PkgPackageJobPackageHeaderPivot>(new ZQuery(PkgPackageJobPackageHeaderPivotSchema.KPJ_KJ_PackageJob, packageJob.PK));
			AssertNotNull("Should create Pivot for HU in TWH.", pivot);
			var header = Factory.LoadTop1<PkgPackageHeader>(new ZQuery(PkgPackageHeaderSchema.PK, pivot.KPJ_KPH_PackageHeader));
			AssertNotNull("Should create PackageHeader.", header);
			AssertEquals("Should not set KP_KPH_PackageHeader.", ZGuid.Empty, package.KP_KPH_PackageHeader);
		}
	}
}
