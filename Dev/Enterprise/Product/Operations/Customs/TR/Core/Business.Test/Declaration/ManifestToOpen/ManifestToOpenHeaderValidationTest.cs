using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ManifestToOpenHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCE_IssueDate_MadantoryValidation()
		{
			var bill = Manifest.Bills.AddNew();
			bill.TPD_DocumentNumber = "001";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Manifest.CE_IssueDateInfo);
		}

		public void TestCheckCE_ExpiryDate_MadantoryValidation()
		{
			var bill = Manifest.Bills.AddNew();
			bill.TPD_DocumentNumber = "001";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Manifest.CE_ExpiryDateInfo);
		}

		ManifestToOpenHeader Manifest => manifest ?? (manifest = Factory.New<ManifestToOpenHeader>());
		ManifestToOpenHeader manifest;
	}
}
