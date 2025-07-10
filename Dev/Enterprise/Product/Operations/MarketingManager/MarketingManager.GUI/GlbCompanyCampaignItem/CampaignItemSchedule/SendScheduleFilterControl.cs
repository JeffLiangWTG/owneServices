using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SendScheduleFilterControl : ZFilterStripControl
	{
		public SendScheduleFilterControl(IBusinessObjectCollection collection, SendScheduleFilterBusinessObject filterBusinessObject)
			: base(collection, filterBusinessObject)
		{
			InitializeComponent();
		}

		new public SendScheduleFilterBusinessObject FilterBusinessObject
		{
			get { return (SendScheduleFilterBusinessObject)base.FilterBusinessObject; }
		}

		void grid_DoubleClick(object sender, System.EventArgs e)
		{
			if (grid.ListManager.List.Cast<ScheduleCampaignItems>().Any())
			{
				ScheduleCampaignItems scheduledCampaignItems = (ScheduleCampaignItems)grid.ListManager.GetCurrent();
				if (scheduledCampaignItems != null)
				{
					var campaignItems = GetCampaignItems(scheduledCampaignItems);

					if (campaignItems.Any())
					{
						var collection = new GlbCompanyCampaignItemCampaignDependentCollection(scheduledCampaignItems.Campaign.Factory);
						collection.AddRange(campaignItems);
						((GlbCompanyCampaignForm)ParentForm).FocusOnCampaignItem(FocusOnTrackingTabTypes.CampaignItemList, collection);
					}
				}
			}
		}

		protected IEnumerable<GlbCompanyCampaignItem> GetCampaignItems(ScheduleCampaignItems scheduledCampaignItems)
		{
			ScheduleItemDataLoader loader = new ScheduleItemDataLoader(scheduledCampaignItems.Campaign.CampaignItemSchedule);

			return scheduledCampaignItems.Campaign.CampaignItemSchedule.SelectedScheduleItems
				.Cast<GlbCompanyCampaignItem>().Where(c =>
					c.RecipientFromView != null
					&& loader.OffsetFromUtcTimeZone(c.RecipientFromView.RelatedPortCodeForScheduling,
						loader.GetUtcFromUnlocoTime(c.RecipientFromView.RelatedPortCodeForScheduling,
							scheduledCampaignItems.ScheduleSendTimeUTC)) == scheduledCampaignItems.UtcOffset
					&& loader.CivilianTimeZoneCodeTimeZone(c.RecipientFromView.RelatedPortCodeForScheduling,
						loader.GetUtcFromUnlocoTime(c.RecipientFromView.RelatedPortCodeForScheduling,
							scheduledCampaignItems.ScheduleSendTimeUTC)) == scheduledCampaignItems.StandardTimeZoneCode
					&& scheduledCampaignItems.ScheduleSendTimeUTC == c.G8_ScheduleTimeUtc);
		}
	}
}
