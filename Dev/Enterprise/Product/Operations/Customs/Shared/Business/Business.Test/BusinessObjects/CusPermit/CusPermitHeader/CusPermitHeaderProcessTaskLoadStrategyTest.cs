using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPermitHeaderProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestCusPermitHeaderProcessTaskLoadStrategyGetTypeForLoad()
		{
			var strategy = new CusPermitHeaderProcessTaskLoadStrategy();
			var header = (BusinessObject)Factory.New<Integration.Customs.BR.ICusLPCOHeader>();

			AssertNull(strategy.GetTypeForLoad(CusPermitHeaderSchema.Constants.Prefix, ZGuid.Empty, Factory));
			AssertNull(strategy.GetTypeForLoad(CusPermitHeaderSchema.Constants.Prefix, ZGuid.Invalid, Factory));
			AssertNull(strategy.GetTypeForLoad("Z@", header.PK, Factory));
			AssertEquals(ObjectFactory.GetType<Integration.Customs.BR.ICusLPCOHeaderProcessTask>(), strategy.GetTypeForLoad(CusPermitHeaderSchema.Constants.Prefix, header.PK, Factory));
		}
	}
}

