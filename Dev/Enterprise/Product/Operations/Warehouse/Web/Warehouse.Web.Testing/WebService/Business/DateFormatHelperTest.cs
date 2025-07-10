using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class DateFormatHelperTest : WhsTestCaseWithFactory
	{
		#region TestGetDateFormatForCountry

		public void TestGetDateFormatForCountry()
		{
			AssertEquals("ddMMyy", DateFormatHelper.GetDateFormatForCountry("AU"));
			AssertEquals("MMddyy", DateFormatHelper.GetDateFormatForCountry("US"));
			AssertEquals("yyMMdd", DateFormatHelper.GetDateFormatForCountry("CN"));
			AssertEquals("ddMMyy", DateFormatHelper.GetDateFormatForCountry("HK"));
			AssertEquals("ddMMyy", DateFormatHelper.GetDateFormatForCountry("GB"));

			AssertEquals("CultureInfoHelper.GetCulture returns EN-US by default for unrecognised countries.", "MMddyy", DateFormatHelper.GetDateFormatForCountry("INVALIDCOUNTRY"));
		}

		#endregion
	}
}
