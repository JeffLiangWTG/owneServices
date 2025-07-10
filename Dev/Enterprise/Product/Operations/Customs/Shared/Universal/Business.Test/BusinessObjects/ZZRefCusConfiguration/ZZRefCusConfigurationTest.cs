using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusConfiguration))]
	class ZZRefCusConfigurationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetConfiguration_CompanyIsNull()
		{
			AssertNull(ZZRefCusConfiguration.Get(null));
		}

		public void TestGetConfiguration_IfConfigurationDoesNotExist()
		{
			AssertNull(ZZRefCusConfiguration.Get(GlbCompany.CurrentCompany));
		}

		public void TestGetConfiguration_IfConfigurationExists()
		{
			var configuration = ZZRefCusConfiguration.New(GlbCompany.CurrentCompany);
			Factory.Save();
			AssertEquals(configuration.PK, ZZRefCusConfiguration.Get(GlbCompany.CurrentCompany).PK);
		}

		public void TestIsReciprocalExchangeRateDescription()
		{
			var configuration = Factory.New<ZZRefCusConfiguration>();
			configuration.ZZC_IsReciprocalExchangeRate = "Y";
			AssertEquals(IsReciprocalExchangeRateList.Descriptions.Yes, configuration.IsReciprocalExchangeRateDescription);
			configuration.ZZC_IsReciprocalExchangeRate = "N";
			AssertEquals(IsReciprocalExchangeRateList.Descriptions.No, configuration.IsReciprocalExchangeRateDescription);
			configuration.ZZC_IsReciprocalExchangeRate = "";
			AssertEquals(IsReciprocalExchangeRateList.Descriptions.Blank, configuration.IsReciprocalExchangeRateDescription);
		}

		public void TestNew()
		{
			var company = Factory.New<GlbCompany>();
			var configuration = ZZRefCusConfiguration.New(company);
			AssertEquals(company.PK, configuration.ZZC_GC);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<ZZRefCusConfiguration>();
		}
	}
}
