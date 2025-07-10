using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ConsolidatedDeclarationTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTSWDecided()
		{
			var row = (Factory as IBusinessObjectFactoryInternals).RowFactory.New(ConsolidatedDeclaration.Schema.TableName);
			row[ConsolidatedDeclaration.Schema.CRD_ApplicationCode] = ConsolidatedDeclaration.ApplicationCodes.TSW;
			AssertEquals(new ConsolidatedDeclarationTypeDecider().GetTypeForLoad(row, Factory), ObjectFactory.GetType<Integration.Customs.NZ.IConsolidatedDeclaration>());
		}

		public void TestCMRDecided()
		{
			var row = (Factory as IBusinessObjectFactoryInternals).RowFactory.New(ConsolidatedDeclaration.Schema.TableName);
			row[ConsolidatedDeclaration.Schema.CRD_ApplicationCode] = ConsolidatedDeclaration.ApplicationCodes.CMR;
			AssertEquals(new ConsolidatedDeclarationTypeDecider().GetTypeForLoad(row, Factory), ObjectFactory.GetType<Integration.Customs.AU.IConsolidatedDeclaration>());
		}
	}
}
