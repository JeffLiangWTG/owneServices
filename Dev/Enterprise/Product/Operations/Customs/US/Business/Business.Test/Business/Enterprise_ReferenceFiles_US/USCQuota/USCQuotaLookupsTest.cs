using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCQuotaLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPeriodProcessDateIndicatorList()
		{
			AssertEquals("EXD, PRE", lookups.PeriodProcessDateIndicatorList.CodesAsString);
		}

		public void TestQuotaLimitTypeList()
		{
			AssertEquals("A, C, H, L", lookups.QuotaLimitTypeList.CodesAsString);
		}

		public void TestQuotaStatusList()
		{
			AssertEquals("8, 5, 7, 1, 2", lookups.QuotaStatusList.CodesAsString);
		}

		public void TestQuotaTypeList()
		{
			AssertEquals("TAF, TCN", lookups.QuotaTypeList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var uscQuota = Factory.New<USCQuota>();
			lookups = new USCQuotaLookups(uscQuota);
		}
		USCQuotaLookups lookups;
	}
}
