using System.Collections.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	[CodeProperty("Id"), DescriptionProperty("Name")]
	public class CampaignHorizontal
	{
		readonly ObservableCollection<GlbCompanyCampaign> campaigns;
		public CampaignHorizontal(ZByte id)
		{
			Id = id;
			campaigns = new ObservableCollection<GlbCompanyCampaign>();
		}

		public ZByte Id { get; set; }

		public ZString Name
		{
			get { return Res.GetString("5492ebda-10cf-4ffd-9448-2ac5b1f76195", "Touch {0}", Id); }
		}

		public ObservableCollection<GlbCompanyCampaign> Campaigns
		{
			get { return campaigns; }
		}

		public GlbCompanyCampaign CurrentCampaign { get; set; }

		public void AddCampaign(GlbCompanyCampaign campaign)
		{
			if (!campaigns.Contains(campaign))
			{
				campaigns.Add(campaign);
			}
		}
	}
}
