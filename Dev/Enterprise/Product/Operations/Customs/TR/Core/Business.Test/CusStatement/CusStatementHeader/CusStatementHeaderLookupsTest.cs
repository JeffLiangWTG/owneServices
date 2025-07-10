using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	internal class CusStatementHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPaymentTypeList()
		{
			AssertEquals(typeof(PaymentTypesList), lookups.PaymentTypesList.GetType());

			var list = lookups.PaymentTypesList;
			CombineAssertions(() =>
			{
				AssertType<PaymentTypesList>(lookups.PaymentTypesList);
				AssertEquals("CodeAsString", "1, 2", list.CodesAsString);
			});
		}

		public void TestStatementHeaderStatusList()
		{
			AssertEquals(typeof(StatementHeaderStatusList), lookups.StatementHeaderStatusList.GetType());

			var list = lookups.StatementHeaderStatusList;
			CombineAssertions(() =>
			{
				AssertType<StatementHeaderStatusList>(lookups.StatementHeaderStatusList);
				AssertEquals("CodeAsString", "AWA, FIN, PRE", list.CodesAsString);
			});
		}

		public void TestPaymentStatusList()
		{
			AssertEquals(typeof(PaymentStatusList), lookups.PaymentStatusList.GetType());

			var list = lookups.PaymentStatusList;
			CombineAssertions(() =>
			{
				AssertType<PaymentStatusList>(lookups.PaymentStatusList);
				AssertEquals("CodeAsString", "PAD", list.CodesAsString);
			});
		}

		public void TestPaymentPartyList()
		{
			AssertEquals(typeof(PaymentPartyList), lookups.PaymentPartyList.GetType());

			var list = lookups.PaymentPartyList;
			CombineAssertions(() =>
			{
				AssertType<PaymentPartyList>(lookups.PaymentPartyList);
				AssertEquals("CodeAsString", "BRK", list.CodesAsString);
			});
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
