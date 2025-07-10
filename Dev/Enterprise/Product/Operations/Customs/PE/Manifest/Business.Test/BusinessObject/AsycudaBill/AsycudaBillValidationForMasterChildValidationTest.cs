using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	sealed class AsycudaBillValidationForMasterChildValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_BillIssueDate()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.AMA_MasterBillIssueDateInfo);
		}
	}
}
