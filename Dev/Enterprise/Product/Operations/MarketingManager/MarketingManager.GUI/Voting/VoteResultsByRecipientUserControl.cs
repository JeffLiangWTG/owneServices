using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class VoteResultsByRecipientUserControl : ResultsByRecipientUserControl
	{
		public VoteResultsByRecipientUserControl(GlbCompanyCampaign campaign)
			: base(campaign)
		{
			InitializeComponent();
		}
	}
}
