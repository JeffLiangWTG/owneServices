using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusSCAOceanBillProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestBaseCusSCAOceanBillProcessTaskLoadStrategyGetTypeForLoad()
		{
			var caOceanBill = Factory.New<Integration.Customs.CA.ICusSCAOceanBill>();
			var auOceanBill = Factory.New<Integration.Customs.AU.ICusSCAOceanBill>();
			var nzOceanBill = Factory.New<Integration.Customs.NZ.ICusSCAOceanBill>();

			var loadStrategy = new CusSCAOceanBillProcessTaskLoadStrategy();
			AssertEquals(ObjectFactory.GetType<Integration.Customs.CA.ICusSCAOceanBillProcessTask>(), loadStrategy.GetTypeForLoad(CusSCAOceanBillSchema.Constants.Prefix, caOceanBill.PK, Factory));
			AssertEquals(ObjectFactory.GetType<Integration.Customs.AU.ICusSCAOceanBillProcessTask>(), loadStrategy.GetTypeForLoad(CusSCAOceanBillSchema.Constants.Prefix, auOceanBill.PK, Factory));
			AssertEquals(ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAOceanBillProcessTask>(), loadStrategy.GetTypeForLoad(CusSCAOceanBillSchema.Constants.Prefix, nzOceanBill.PK, Factory));
		}
	}
}
