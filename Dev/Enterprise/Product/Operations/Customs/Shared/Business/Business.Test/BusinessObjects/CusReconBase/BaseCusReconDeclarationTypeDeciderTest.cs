using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class BaseCusReconDeclarationTypeDeciderTest : TestCaseWithFactory
	{
		public void TestDelegateToCusReconDeclarationTypeDecider()
		{
			var cusReconDeclaration = Factory.New<CusReconDeclaration>();
			var row = (cusReconDeclaration as IBusinessObjectInternals).Row;
			AssertEquals(new CusReconBase.CusReconDeclarationTypeDecider().GetTypeForLoad(row, Factory), new CusReconDeclarationTypeDecider().GetTypeForLoad(row, Factory));
		}

		public void TestDelegateToConsolidatedDeclarationTypeDecider()
		{
			var cusReconDeclaration = Factory.New<CusReconDeclaration>();
			cusReconDeclaration.CRD_ApplicationCode = ConsolidatedDeclaration.ApplicationCodes.TSW;
			var row = (cusReconDeclaration as IBusinessObjectInternals).Row;
			AssertEquals(new CusReconBase.CusReconDeclarationTypeDecider().GetTypeForLoad(row, Factory), new ConsolidatedDeclarationTypeDecider().GetTypeForLoad(row, Factory));
		}
	}
}
