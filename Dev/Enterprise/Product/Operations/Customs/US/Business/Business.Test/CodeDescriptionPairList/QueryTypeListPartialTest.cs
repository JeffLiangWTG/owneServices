namespace Enterprise.Customs.US.Business.Testing
{
	sealed class QueryTypeListTest : NUnit.Framework.TestCase
	{
		public void TestIsBulkQuery()
		{
			AssertEquals(false, QueryTypeList.IsBulkQuery(QueryTypeList.Codes.AllVisaRecordsForACountry));
			AssertEquals(true, QueryTypeList.IsBulkQuery(QueryTypeList.Codes.AllVisaRecordsForAllCountries));
		}

		public void TestIsTariffCategoryOrVisaNumberRequired()
		{
			AssertEquals(false, QueryTypeList.IsTariffCategoryOrVisaNumberRequired(QueryTypeList.Codes.AllVisaRecordsForACountry));
			AssertEquals(true, QueryTypeList.IsTariffCategoryOrVisaNumberRequired(QueryTypeList.Codes.AllVisaRecordsForCountryCategoryOrCountryTariff));

			AssertEquals(false, QueryTypeList.IsTariffCategoryOrVisaNumberRequired(QueryTypeList.Codes.AllVisaRecordsForAllCountries));
			AssertEquals(true, QueryTypeList.IsTariffCategoryOrVisaNumberRequired(QueryTypeList.Codes.QuotaRecords));
			AssertEquals(true, QueryTypeList.IsTariffCategoryOrVisaNumberRequired(QueryTypeList.Codes.SpecificVisaNumber));
		}

		public void TestGetCustomsCode()
		{
			AssertEquals(QueryTypeList.Codes.AllVisaRecordsForACountry, QueryTypeList.GetCustomsCode(QueryTypeList.Codes.AllVisaRecordsForACountry));
			AssertEquals("", QueryTypeList.GetCustomsCode(QueryTypeList.Codes.QuotaRecords));
		}
	}
}
