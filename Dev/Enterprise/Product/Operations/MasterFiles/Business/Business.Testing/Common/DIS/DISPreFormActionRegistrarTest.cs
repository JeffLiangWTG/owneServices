using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.MasterFiles.Business.DIS.Testing
{
	sealed class DISPreFormActionRegistrarTest : TestCaseWithFactory
	{
		public void TestGetDISPreFormActionRunner()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var disHostMock = new Mock<IDISHost>();
				var runner = DISPreFormActionRegistrar.GetDISPreFormActionRunner(disHostMock.Object);
				Assert("Should be a DISPreFormActionRunner", runner is DISPreFormActionRunner);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = (IDISHost)Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
				var runner = DISPreFormActionRegistrar.GetDISPreFormActionRunner(declaration);
				AssertEquals("Should be a JobDeclarationDISPreFormActionRunner", "Enterprise.Customs.US.DIS.GUI.JobDeclarationDISPreFormActionRunner", runner.GetType().FullName);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declaration = (IDISHost)Factory.New<Enterprise.Integration.Customs.CA.IJobDeclaration>();
				var runner = DISPreFormActionRegistrar.GetDISPreFormActionRunner(declaration);
				AssertEquals("Should be a JobDeclarationDISPreFormActionRunner", "Enterprise.Customs.CA.DIF.GUI.JobDeclarationDISPreFormActionRunner", runner.GetType().FullName);
			}
		}
	}
}
