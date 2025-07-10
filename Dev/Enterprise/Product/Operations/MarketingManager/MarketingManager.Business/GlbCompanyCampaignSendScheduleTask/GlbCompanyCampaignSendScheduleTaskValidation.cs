using CargoWise.EntityFramework;
using Enterprise.Scheduler.Business;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSendScheduleTaskValidation : StmScheduleTaskValidation
	{
		public GlbCompanyCampaignSendScheduleTaskValidation(GlbCompanyCampaignSendScheduleTask parent, GlbCompanyCampaignSendSettings campaignSendSettings)
			: base(parent)
		{
			this.campaignSendSettings = campaignSendSettings;
		}
		readonly GlbCompanyCampaignSendSettings campaignSendSettings;

		protected override void CheckS5_NextScheduledPrintRunTimeUtc()
		{
			base.CheckS5_NextScheduledPrintRunTimeUtc();
			if (campaignSendSettings != null && campaignSendSettings.IsBatchSchedule)
			{
				MandatoryValidation.CheckEntered(Parent.S5_NextScheduledPrintRunTimeUtcInfo);
			}
		}

		protected override void CheckS5_ParentID()
		{
			MandatoryValidation.CheckEntered(Parent.S5_ParentIDInfo);
		}
	}
}
