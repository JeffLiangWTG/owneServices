using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSCAHouseProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestCusSCAHouseProcessTaskLoadStrategyGetTypeForLoad()
		{
			var strategy = new CusSCAHouseProcessTaskLoadStrategy();
			var auOceanBill = Factory.New<Integration.Customs.AU.ICusSCAOceanBill>();
			var auHouse = Factory.New<Integration.Customs.AU.ICusSCAHouse>();
			auHouse.CA_CB = auOceanBill.PK;
			AssertNull(strategy.GetTypeForLoad(CusSCAHouseSchema.Constants.Prefix, ZGuid.Empty, Factory));
			AssertNull(strategy.GetTypeForLoad(CusSCAHouseSchema.Constants.Prefix, ZGuid.Invalid, Factory));
			AssertEquals(ObjectFactory.GetType<Integration.Customs.AU.ICusSCAHouseProcessTask>(), strategy.GetTypeForLoad(CusSCAHouseSchema.Constants.Prefix, auHouse.PK, Factory));
			AssertNull(strategy.GetTypeForLoad("Z@", auHouse.PK, Factory));

			var nzOceanBill = Factory.New<Integration.Customs.NZ.ICusSCAOceanBill>();
			nzOceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			var nzHouse = Factory.New<Integration.Customs.NZ.ICusSCAHouse>();
			nzHouse.CA_CB = nzOceanBill.PK;
			AssertEquals(typeof(CusSCAHouseProcessTask), strategy.GetTypeForLoad(CusSCAHouseSchema.Constants.Prefix, nzHouse.PK, Factory));
		}
	}
}
