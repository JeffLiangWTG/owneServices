using Enterprise.MasterFiles.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class CampaignSalesRelationControl : SalesRelationControl
	{
		public CampaignSalesRelationControl()
		{
			InitializeComponent();

			this.organizationNameColumn.Header = SalesRelationTree.OrganizationNameColumn;
			this.organizationCodeColumn.Header = SalesRelationTree.OrganizationCodeColumn;
			this.contactNameColumn.Header = SalesRelationTree.ContactNameColumn;
		}
	}
}
