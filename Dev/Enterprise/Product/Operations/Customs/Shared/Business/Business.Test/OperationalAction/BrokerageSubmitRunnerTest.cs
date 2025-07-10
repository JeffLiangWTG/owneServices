using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BrokerageSubmitRunnerTest : TestCaseWithFactory
	{
		public void TestExecute()
		{
			var declaration1 = new BusinessObjectFactory().New<BaseJobDeclaration>();
			declaration1.FillWithValidTestData();
			declaration1.JE_DeclarationReference = "Z00000001";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.FillWithValidTestData();
			declaration2.JE_DeclarationReference = "Z00000002";
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.FillWithValidTestData();
			declaration3.JE_DeclarationReference = "Z00000003";
			declaration3.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var declaration4 = Factory.New<BaseJobDeclaration>();
			declaration4.FillWithValidTestData();
			declaration4.JE_DeclarationReference = "Z00000004";
			declaration4.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var runner = new BrokerageSubmitRunnerForTesting();
			runner.Log = log;
			runner.Execute(declaration1);
			runner.Execute(declaration2);
			runner.Execute(declaration3);
			runner.Execute(declaration4);

			AssertEquals("WARNING: [HL Z00000001]: This Declaration does not exist or has already been deleted.", log.messages[0]);
			AssertEquals("WARNING: [HL Z00000002]: Not Submitted.", log.messages[1]);
			AssertEquals("INFO: [HL Z00000003]: Submit Succeeded.", log.messages[2]);
			AssertEquals("INFO: [HL Z00000004]: Submit Succeeded.", log.messages[3]);
		}

		public class BrokerageSubmitRunnerForTesting : BrokerageSubmitRunner
		{
			protected override ICusIntegration GetCusIntegrationProvider(BaseJobDeclaration declaration)
			{
				if (!declaration.IsAir)
				{
					return new CusIntegrationProviderForTesting();
				}
				return null;
			}
		}

		class CusIntegrationProviderForTesting : ICusIntegration
		{
			#region ICusIntegration Members

			public ZString Execute(Integration.Customs.IBaseJobDeclaration declaration)
			{
				return "Submit Succeeded.";
			}

			#endregion
		}
	}
}
