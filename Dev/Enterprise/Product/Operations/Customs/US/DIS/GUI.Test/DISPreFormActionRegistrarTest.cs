using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.GUI.Testing
{
	sealed class DISPreFormActionRegistrarTest : TestCaseWithFactory
	{
		public void TestGetDISPreFormActionRunner()
		{
			var declaration = (IDISHost)Factory.New<Integration.Customs.US.IJobDeclaration>();
			var runner = new DISPreFormActionRegistrar().GetDISPreFormActionRunner(declaration);
			Assert(runner is JobDeclarationDISPreFormActionRunner);
		}
	}
}
