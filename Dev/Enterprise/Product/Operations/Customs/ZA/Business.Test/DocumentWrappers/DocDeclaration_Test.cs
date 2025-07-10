using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class DocDeclaration_Test : TestCaseWithFactory
	{
		public void TestGetCargoStatusCodeForContainerMode()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			DocDeclaration declarationWrapper = DocDeclaration.New(dec, Factory);
			AssertEquals("8", declarationWrapper.GetCargoStatusCodeForContainerMode("FCL"));
			AssertEquals("7", declarationWrapper.GetCargoStatusCodeForContainerMode("LCL"));
			AssertEquals("5", declarationWrapper.GetCargoStatusCodeForContainerMode("FCG"));
			AssertEquals("4", declarationWrapper.GetCargoStatusCodeForContainerMode("EMP"));
			AssertEquals("11", declarationWrapper.GetCargoStatusCodeForContainerMode("BBK"));
			AssertEquals("10", declarationWrapper.GetCargoStatusCodeForContainerMode("BLK"));
			AssertEquals("10", declarationWrapper.GetCargoStatusCodeForContainerMode("LQD"));
		}
	}
}
