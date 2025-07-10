using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusDispositionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCDI_NoteShouldAllowEmpty()
		{
			var disposition = Factory.New<CusDisposition>();
			AssertEquals("Should have been defaulted", "", disposition.CDI_Notes);
			Assert(!disposition.HasErrors);
		}
	}
}
