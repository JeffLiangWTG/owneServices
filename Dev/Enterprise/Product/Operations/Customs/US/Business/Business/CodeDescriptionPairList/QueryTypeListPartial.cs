
namespace Enterprise.Customs.US.Business
{
	partial class QueryTypeList
	{
		public static bool IsBulkQuery(string code)
		{
			return code == Codes.AllVisaRecordsForAllCountries;
		}

		public static bool IsTariffCategoryOrVisaNumberRequired(string code)
		{
			return code == Codes.AllVisaRecordsForCountryCategoryOrCountryTariff ||
				code == Codes.QuotaRecords ||
				code == Codes.SpecificVisaNumber;
		}

		public static string GetCustomsCode(string code)
		{
			if (code == Codes.QuotaRecords)
			{
				return "";
			}

			return code;
		}
	}
}
