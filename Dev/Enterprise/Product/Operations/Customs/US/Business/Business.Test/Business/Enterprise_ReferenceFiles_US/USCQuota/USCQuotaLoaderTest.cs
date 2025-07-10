using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCQuota.Loader))]
	class USCQuotaLoaderTest : LoaderTestCase
	{
		public void TestLoadWithCodeCountryBeginDateAndEndDate()
		{
			AssertEquals(quota1, new USCQuota.Loader(Factory).Load("100", "KR", "", "", new ZDateTime(2006, 1, 1), new ZDateTime(2006, 12, 31)));

			USCQuota[] quotas = new USCQuota.Loader(Factory).LoadAll("100", "KR", "", "", new ZDateTime(2006, 1, 1), new ZDateTime(2006, 12, 31));
			AssertEquals("All quotas", 1, quotas.Length);
			AssertEquals(quota1, quotas[0]);

			quotas = new USCQuota.Loader(Factory).LoadAll("100", "JP", "first", "second", new ZDateTime(2006, 1, 1), new ZDateTime(2006, 12, 31));
			AssertEquals("All quotas", 1, quotas.Length);
			AssertEquals(quota6, quotas[0]);
		}

		public void TestLoadBestMatch()
		{
			AssertNull(new USCQuota.Loader(Factory).LoadBestMatchFor("100", "", "KR", new ZDateTime(2006, 1, 1), ZDateTime.Empty));

			AssertEquals(quota1, new USCQuota.Loader(Factory).LoadBestMatchFor("100", "", "KR", ZDateTime.Empty, new ZDateTime(2006, 1, 1)));

			AssertEquals(quota1, new USCQuota.Loader(Factory).LoadBestMatchFor("100", "", "KR", ZDateTime.Empty, new ZDateTime(2006, 12, 31)));

			AssertEquals(quota2, new USCQuota.Loader(Factory).LoadBestMatchFor("100", "", "KR", new ZDateTime(2006, 1, 1), new ZDateTime(2007, 1, 1)));

			AssertEquals(quota5, new USCQuota.Loader(Factory).LoadBestMatchFor("100", "123", "JP", new ZDateTime(2006, 1, 1), new ZDateTime(2006, 12, 31)));

			AssertEquals(quota8, new USCQuota.Loader(Factory).LoadBestMatchFor("1806320400", "", "JP", new ZDateTime(2006, 1, 1), new ZDateTime(2006, 12, 31)));
		}

		USCQuota quota1;
		USCQuota quota2;
		USCQuota quota3;
		USCQuota quota4;
		USCQuota quota5;
		USCQuota quota6;
		USCQuota quota7;
		USCQuota quota8;

		protected override void SetUp()
		{
			base.SetUp();
			quota1 = Factory.New<USCQuota>();
			quota1.UT_Code = "100";
			quota1.UT_UC_NKOriginCountry = "KR";
			quota1.UT_BeginDate = new ZDateTime(2006, 1, 1);
			quota1.UT_EndDate = new ZDateTime(2006, 12, 31);
			quota1.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.ExportDate;

			quota2 = Factory.New<USCQuota>();
			quota2.UT_Code = "100";
			quota2.UT_UC_NKOriginCountry = "KR";
			quota2.UT_BeginDate = new ZDateTime(2007, 1, 1);
			quota2.UT_EndDate = new ZDateTime(2007, 12, 31);
			quota2.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.ExportDate;

			quota3 = Factory.New<USCQuota>();
			quota3.UT_Code = "200";
			quota3.UT_UC_NKOriginCountry = "KR";
			quota3.UT_BeginDate = new ZDateTime(2006, 1, 1);
			quota3.UT_EndDate = new ZDateTime(2006, 12, 31);
			quota3.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.ExportDate;

			quota4 = Factory.New<USCQuota>();
			quota4.UT_Code = "100";
			quota4.UT_UC_NKOriginCountry = "JP";
			quota4.UT_BeginDate = new ZDateTime(2006, 1, 1);
			quota4.UT_EndDate = new ZDateTime(2006, 12, 31);
			quota4.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.ExportDate;

			quota5 = Factory.New<USCQuota>();
			quota5.UT_Code = "100";
			quota5.UT_UC_NKOriginCountry = "JP";
			quota5.UT_BeginDate = new ZDateTime(2006, 1, 1);
			quota5.UT_EndDate = new ZDateTime(2006, 12, 31);
			quota5.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.ExportDate;
			quota5.UT_SecondTariffNo = "123";

			quota6 = Factory.New<USCQuota>();
			quota6.UT_Code = "100";
			quota6.UT_FirstNamesake = "first";
			quota6.UT_SecondNamesake = "second";
			quota6.UT_UC_NKOriginCountry = "JP";
			quota6.UT_BeginDate = new ZDateTime(2006, 1, 1);
			quota6.UT_EndDate = new ZDateTime(2006, 12, 31);
			quota6.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.ExportDate;

			quota7 = Factory.New<USCQuota>();
			quota7.UT_Code = "100";
			quota7.UT_FirstNamesake = "first";
			quota7.UT_SecondNamesake = "";
			quota7.UT_UC_NKOriginCountry = "JP";
			quota7.UT_BeginDate = new ZDateTime(2006, 1, 1);
			quota7.UT_EndDate = new ZDateTime(2006, 12, 31);
			quota7.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.ExportDate;

			quota8 = Factory.New<USCQuota>();
			quota8.UT_Code = "1806320400";
			quota8.UT_UC_NKOriginCountry = "";
			//this is not part of job data, and if searched by tariff, namesakes are not important. It is applied to categories
			//example messages received have these namesakes for tariff.
			quota8.UT_FirstNamesake = "806320406";
			quota8.UT_SecondNamesake = "806320407";
			quota8.UT_BeginDate = new ZDateTime(2006, 1, 1);
			quota8.UT_EndDate = new ZDateTime(2006, 12, 31);
			quota8.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.ExportDate;
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new USCQuota.Loader(Factory);
		}
	}
}
