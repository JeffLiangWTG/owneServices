using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCQuota))]
	sealed class USCQuotaTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDescriptionProperties()
		{
			USCQuota quota = Factory.New<USCQuota>();
			quota.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.ExportDate;
			quota.UT_QuotaLimitType = QuotaLimitTypeList.Codes.Absolute;
			quota.UT_QuotaStatus = QuotaStatusList.Codes.Banned;
			quota.UT_QuotaType = QuotaTypeList.Codes.TariffNumber;

			AssertEquals(PeriodProcessingDateIndicatorList.Descriptions.ExportDate, quota.PeriodProcessDateIndicatorDesc);
			AssertEquals(QuotaLimitTypeList.Descriptions.Absolute, quota.QuotaLimitTypeDesc);
			AssertEquals(QuotaStatusList.Descriptions.Banned, quota.QuotaStatusDesc);
			AssertEquals(QuotaTypeList.Descriptions.TariffNumber, quota.QuotaTypeDesc);
		}
	}
}
