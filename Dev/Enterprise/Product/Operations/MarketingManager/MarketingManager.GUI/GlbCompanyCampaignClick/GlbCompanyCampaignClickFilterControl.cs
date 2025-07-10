using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class GlbCompanyCampaignClickFilterControl : ZFilterStripControl
	{
		public GlbCompanyCampaignClickFilterControl(IBusinessObjectCollection collection, GlbCompanyCampaignClickFilterBusinessObject filterBusinessObject, GlbCompanyCampaignItem campaignItem)
			: base(collection, filterBusinessObject)
		{
			CampaignItem = campaignItem;
			InitializeComponent();
		}
		readonly GlbCompanyCampaignItem CampaignItem;

		public void InitializeFilterStripsOnLoad()
		{
			base.OnLoad(EventArgs.Empty);

			if (CampaignItem != null && !CampaignItem.ContactName.IsEmpty)
			{
				ResetFilterStrips();

				FilterBusinessObject.FilterStrips[0].FilterDescription = ((ModuleTextFilter)((GlbCompanyCampaignClickFilterBusinessObject)FilterBusinessObject)["Email Address"]).Description;
				((ModuleTextFilter)FilterBusinessObject.FilterStrips[0].CurrentModuleFilter).SqlComparisonOperator = SQLComparisonOperator.Equal;
				((ModuleTextFilter)FilterBusinessObject.FilterStrips[0].CurrentModuleFilter).Property = CampaignItem.EmailAddress;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			InitializeFilterStripsOnLoad();
		}
	}
}
