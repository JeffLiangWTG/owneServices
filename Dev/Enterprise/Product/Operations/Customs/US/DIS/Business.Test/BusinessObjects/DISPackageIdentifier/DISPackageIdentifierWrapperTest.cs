using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISPackageIdentifierWrapperTest : TestCaseWithFactory
	{
		public void TestIDISPackageIdentifier()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("TEST1", "ABCDE", "", PackageCategoryTypeList.Codes.GEN);
			helper.CreateDisCodeEntry("TEST2", "FGHIJ", "", PackageCategoryTypeList.Codes.CBMA);
			var iPackageIdentifier = (IDISPackageIdentifier)new DISPackageIdentifierWrapper(Factory, "TEST1", "012345678");
			AssertEquals(PackageCategoryTypeList.Codes.GEN, iPackageIdentifier.PackageCategory);
			AssertEquals("012345678", iPackageIdentifier.ImporterOfRecordNumber);
			iPackageIdentifier = new DISPackageIdentifierWrapper(Factory, "TEST2", "0123456789");
			AssertEquals(PackageCategoryTypeList.Codes.CBMA, iPackageIdentifier.PackageCategory);
			AssertEquals("0123456789", iPackageIdentifier.ImporterOfRecordNumber);
		}
	}
}
