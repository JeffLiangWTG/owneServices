using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusStatementHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestImportersList()
		{
			AssertEquals(typeof(ConsigneeCollection), lookups.ImportersList.GetType());
		}

		public void TestStatementHeaderStatusList()
		{
			AssertEquals(typeof(StatementHeaderStatusList), lookups.StatementHeaderStatusList.GetType());
		}

		public void TestPaymentTypeList()
		{
			AssertEquals(typeof(PaymentTypeList), lookups.PaymentTypeList.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusStatementHeader>();
			lookups = new CusStatementHeaderLookups(header);
		}

		CusStatementHeaderLookups lookups;
		CusStatementHeader header;
	}
}
