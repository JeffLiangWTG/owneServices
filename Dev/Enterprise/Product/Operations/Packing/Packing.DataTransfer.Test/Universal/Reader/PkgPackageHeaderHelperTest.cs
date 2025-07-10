using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Core.Testing;

namespace Enterprise.Packing.DataTransfer.Testing
{
	class PkgPackageHeaderHelperTest : PkgPackageDataObjectReaderTestCase
	{
		public void TestCreatePackageHeaderForPackage()
		{
			var existingDummyParent = Factory.New<DummyWithPacking>();
			var existingPackageJob = PkgPackageJob.LoadOrCreatePackageJob(existingDummyParent);
			var existingPackage = existingPackageJob.Packages.AddNew();
			existingPackage.KP_PackageID = "Z456";
			Factory.SaveForTesting();

			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew();
			package1.KP_PackageQty = 0;
			PkgPackageHeaderHelper.SetPackageQuanityAndID_IfValid(Factory, Logger, package1, (ZInt)1, "Z123");
			AssertEquals("Z123", package1.KP_PackageID);
			AssertEquals(1, package1.KP_PackageQty);

			var package2 = Data.PackageJob.Packages.AddNew();
			AssertEquals("Precondition: qty should default to 1", 1, package2.KP_PackageQty);
			PkgPackageHeaderHelper.SetPackageQuanityAndID_IfValid(Factory, Logger, package2, (ZInt?)null, "Z456");
			AssertEquals("Z456", package2.KP_PackageID);
			AssertEquals("Should still be 1", 1, package2.KP_PackageQty);

			package2.KP_PackageQty = 10;
			PkgPackageHeaderHelper.SetPackageQuanityAndID_IfValid(Factory, Logger, package2, (ZInt?)null, "Z789");
			AssertEquals("Package2 was updated, but can't set ID as qty is 10.", "", package2.KP_PackageID);
			AssertEquals("Fk should have been cleared.", ZGuid.Empty, package2.KP_KPH_PackageHeader);
			AssertEquals("Should still be 10", 10, package2.KP_PackageQty);
			AssertEquals("Warning - Could not populate Package ID 'Z789' because Package Quantity '10' is not 1.", Logger.Logs);
			Logger.ClearLogs();

			PkgPackageHeaderHelper.SetPackageQuanityAndID_IfValid(Factory, Logger, package2, (ZInt)1, "Z789");
			AssertEquals("Package2 updated, and cause qty was also updated to 1, the id can now be set.", "Z789", package2.KP_PackageID);
			AssertEquals("Should be 1", 1, package2.KP_PackageQty);

			Factory.SaveForTesting(); // ensure no save exceptions (db constraint failures)

			PkgPackageHeaderHelper.SetPackageQuanityAndID_IfValid(Factory, Logger, package2, (ZInt)2, "Z111");
			AssertEquals("Warning - Could not populate Package ID 'Z111' because Package Quantity '2' is not 1.", Logger.Logs);
		}

		public void TestCreatePackageHeaderForPackage_ExistingPackage()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Z123";

			var packageIdPK = package.KP_KPH_PackageHeader;
			PkgPackageHeaderHelper.SetPackageQuanityAndID_IfValid(Factory, Logger, package, new ZLong(package.KP_PackageQty), "Y567");
			AssertEquals("Z123", package.KP_PackageID);
			AssertEquals(packageIdPK, package.KP_KPH_PackageHeader);
		}

		public void TestCreatePackageHeaderForPackage_EmptyValues()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.Outer;

			var package = Data.PackageJob.Packages.AddNew();

			PkgPackageHeaderHelper.SetPackageQuanityAndID_IfValid(Factory, Logger, package, new ZLong(package.KP_PackageQty), "");
			AssertEquals(ZGuid.Empty, package.KP_KPH_PackageHeader);

			PkgPackageHeaderHelper.SetPackageQuanityAndID_IfValid(Factory, Logger, package, new ZLong(package.KP_PackageQty), null);
			AssertEquals(ZGuid.Empty, package.KP_KPH_PackageHeader);
		}

		TestErrorLogger Logger => logger ?? (logger = new TestErrorLogger());
		TestErrorLogger logger;
	}
}
