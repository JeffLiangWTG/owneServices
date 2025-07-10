using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignItemScheduleItemsCollection : NonPersistentBusinessObjectCollection<ScheduleCampaignItems>
	{
		public GlbCompanyCampaignItemScheduleItemsCollection(GlbCompanyCampaign campaign)
			: base()
		{
			this.Campaign = campaign;
		}

		public void LoadData(GlbCompanyCampaignItemSchedule itemSchedule, ZDateTime scheduleDateTimeLocal, bool isEditing = false)
		{
			RemoveAll();
			AddRange(new ScheduleItemDataLoader(itemSchedule).LoadScheduleItems(scheduleDateTimeLocal, isEditing));
		}

		readonly GlbCompanyCampaign Campaign;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ScheduleCampaignItems();
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			RemoveAll();
			foreach (GlbCompanyCampaignItem item in Campaign.CampaignsItemsSent)
			{
				item.ReloadSafe();
			}
			Campaign.CampaignItemSchedule.SelectedScheduleItems.UnionWith(Campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Where(item => item.IsInDatabase && !item.G8_ScheduleTimeUtc.IsEmpty));
			AddRange(new ScheduleItemDataLoader(Campaign.CampaignItemSchedule).LoadScheduleItems(ZDateTime.Empty));
		}
	}
}
