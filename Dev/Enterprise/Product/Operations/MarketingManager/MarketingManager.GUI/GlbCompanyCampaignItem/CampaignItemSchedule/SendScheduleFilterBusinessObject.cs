using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class SendScheduleFilterBusinessObject : FilterStripBusinessObject
	{
		/// <summary>
		/// Parameterless constructor for color scheme support.
		/// </summary>
		public SendScheduleFilterBusinessObject()
		{
		}

		public SendScheduleFilterBusinessObject(GlbCompanyCampaign campaign)
		{
			this.Campaign = campaign;
		}

		public readonly GlbCompanyCampaign Campaign;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			return filters;
		}

		protected override bool ShouldAddUserDefinedFiltersCore => false;
	}
}
