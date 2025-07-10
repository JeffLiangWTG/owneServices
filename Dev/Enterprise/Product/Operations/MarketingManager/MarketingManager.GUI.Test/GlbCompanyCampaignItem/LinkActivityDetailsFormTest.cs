using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(LinkActivityDetailsForm))]
	public class LinkActivityDetailsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			CampaignItemClickStatModel statModel = new CampaignItemClickStatModel(campaignItem);
			return new LinkActivityDetailsForm(statModel);
		}
	}
}
