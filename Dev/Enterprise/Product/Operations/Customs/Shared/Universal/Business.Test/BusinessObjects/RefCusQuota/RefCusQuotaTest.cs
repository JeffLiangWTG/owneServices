using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusQuota))]
	class RefCusQuotaTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDataGrouping()
		{
			CombineAssertions(() =>
			{
				AssertNull("Default", Factory.New<RefCusQuota>().DataGrouping);
				AssertNotNull("HasDataGrouping", ((RefCusQuota)GetBusinessObjectForFetchForLoad()).DataGrouping);
			});
		}

		public void TestSetDefaultValues()
		{
			var refCusQuota = Factory.New<RefCusQuota>();
			CombineAssertions(() =>
			{
				AssertEquals("StartDate", ZDateTime.MinSmallDateTimeValue, refCusQuota.ZXQ_StartDate);
				AssertEquals("EndDate", ZDateTime.MaxSmallDateTimeValue, refCusQuota.ZXQ_EndDate);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland);
			var bo = (RefCusQuota)base.GetNewBusinessObjectForDeleteTest(factory);
			bo.ZXQ_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Poland;
			return bo;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);
	}
}
