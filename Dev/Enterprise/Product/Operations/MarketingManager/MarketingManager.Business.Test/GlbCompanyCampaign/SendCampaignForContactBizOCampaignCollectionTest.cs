
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.MarketingManager.Business.SendCampaignForContactBizO;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SendCampaignForContactBizOCampaignCollection))]
	sealed class SendCampaignForContactBizOCampaignCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SendCampaignForContactBizOCampaignCollection(Factory, new ZQuery());
		}
	}
}
