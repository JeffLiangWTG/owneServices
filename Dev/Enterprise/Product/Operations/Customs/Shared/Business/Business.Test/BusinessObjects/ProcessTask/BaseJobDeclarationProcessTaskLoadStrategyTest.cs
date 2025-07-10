using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestJobDeclarationProcessTaskLoadStrategyGetTypeForLoad()
		{
			var jobDec = Factory.New<IBaseJobDeclaration>();

			var loadStrategy = new BaseJobDeclarationProcessTaskLoadStrategy();
			AssertEquals(typeof(BaseJobDeclarationProcessTask<>).MakeGenericType(jobDec.GetType()), loadStrategy.GetTypeForLoad(JobDeclarationSchema.Constants.Prefix, jobDec.PK, Factory));
		}
	}
}
