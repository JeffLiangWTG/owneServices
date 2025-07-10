using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class FWSProcessingCodeListTest : TestCaseWithFactory
	{
		public void TestIsEDS()
		{
			Assert(FWSProcessingCodeList.IsEDS(FWSProcessingCodeList.Codes.EDS));
		}

		public void TestIsLDS()
		{
			Assert(FWSProcessingCodeList.IsLDS(FWSProcessingCodeList.Codes.LDS));
		}
	}
}
