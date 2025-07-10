using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class UnsubscribeFilterHelperTest : TestCaseWithFactory
	{
		public void TestGetUnsubscribeFilterNullCampaign()
		{
			AssertExceptionThrown<ArgumentNullException>(() => UnsubscribeFilterHelper.GetUnsubscribeFilter(null));
		}

		public void TestGetUnsubscribeFilter()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CAT";
			campaign.G0_Type = "TYPE";

			const string expected = @"(
	GCS_MediaCategory in 
	(
		'', 'CAT'
	)
)
AND
(
	GCS_MediaType in 
	(
		'', 'TYPE'
	)
)
";
			AssertEquals(expected, campaign.GetUnsubscribeFilter().LiteralTextADOFormatted);
		}
	}
}
