using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusPollingTransactionTypeDeciderTest : TestCaseWithFactory
	{
		public void TestDefaultTypeDecided()
		{
			var row = (Factory as IBusinessObjectFactoryInternals).RowFactory.New(CusPollingTransaction.Schema.TableName);
			row[CusPollingTransaction.Schema.CPT_ApplicationCode] = CusPollingTransaction.ApplicationCodes.TRCustoms;
			AssertEquals(typeof(CusPollingTransaction), new CusPollingTransactionTypeDecider().GetTypeForLoad(row, Factory));
		}

		public void TestKRCDecided()
		{
			var row = (Factory as IBusinessObjectFactoryInternals).RowFactory.New(CusPollingTransaction.Schema.TableName);
			row[CusPollingTransaction.Schema.CPT_ApplicationCode] = CusPollingTransaction.ApplicationCodes.KRCustoms;
			AssertEquals(ObjectFactory.GetType<Integration.Customs.KR.ICusPollingTransaction>(), new CusPollingTransactionTypeDecider().GetTypeForLoad(row, Factory));
		}
	}
}
