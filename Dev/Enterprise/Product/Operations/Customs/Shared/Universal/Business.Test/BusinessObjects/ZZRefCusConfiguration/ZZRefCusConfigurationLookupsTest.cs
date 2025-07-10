using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	class ZZRefCusConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestValuationDateDefaultTypeList()
		{
			var valuationDateDefaultTypeList = lookups.ValuationDateDefaultTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DOA, DOE", valuationDateDefaultTypeList.CodesAsString);
				AssertSame("Cached", valuationDateDefaultTypeList, lookups.ValuationDateDefaultTypeList);
			});
		}

		public void TestValuationDateExportDefaultTypeList()
		{
			var valuationDateExportDefaultTypeList = lookups.ValuationDateExportDefaultTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DOE", valuationDateExportDefaultTypeList.CodesAsString);
				AssertSame("Cached", valuationDateExportDefaultTypeList, lookups.ValuationDateExportDefaultTypeList);
			});
		}

		public void TestVATValueCodeList()
		{
			var vatValueCodeList = lookups.VATValueCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "CIF, FOB", vatValueCodeList.CodesAsString);
				AssertSame("Cached", vatValueCodeList, lookups.VATValueCodeList);
			});
		}

		public void TestCustomsValueCodeList()
		{
			var customsValueCodeList = lookups.CustomsValueCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "CIF, FOB", customsValueCodeList.CodesAsString);
				AssertSame("Cached", customsValueCodeList, lookups.CustomsValueCodeList);
			});
		}

		public void TestIsReciprocalExchangeRateList()
		{
			var isReciprocalExchangeRateList = lookups.IsReciprocalExchangeRateList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", ", N, Y", isReciprocalExchangeRateList.CodesAsString);
				AssertSame("Cached", isReciprocalExchangeRateList, lookups.IsReciprocalExchangeRateList);
			});
		}

		protected override void SetUp()
		{
			var config = Factory.New<ZZRefCusConfiguration>();
			lookups = config.Lookups;
			base.SetUp();
		}

		ZZRefCusConfigurationLookups lookups;
	}
}
