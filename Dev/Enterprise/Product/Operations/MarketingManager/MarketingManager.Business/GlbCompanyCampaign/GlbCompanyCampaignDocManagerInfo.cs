using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignDocManagerInfo : DocManagerInfo
	{
		public GlbCompanyCampaignDocManagerInfo(GlbCompanyCampaign campaign)
			: base(campaign, Core.Constants.DocManagerCodes.CompanyCampaign)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var businessObjects = new List<BusinessObject>();
			businessObjects.AddRange(base.GetRelatedObjects());

			var campaign = (GlbCompanyCampaign)BusinessEntity;
			var relatedContacts = campaign.CampaignsItemsSent.ToArray();
			if (relatedContacts.Length > 0)
			{
				businessObjects.AddRange(relatedContacts);
			}

			return businessObjects.ToArray();
		}
	}
}
