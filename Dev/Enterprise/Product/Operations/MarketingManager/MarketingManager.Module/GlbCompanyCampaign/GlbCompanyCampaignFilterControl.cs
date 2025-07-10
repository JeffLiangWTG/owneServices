using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public partial class GlbCompanyCampaignFilterControl : ZFilterStripControl
	{
		public GlbCompanyCampaignFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetCategoryColumnCaptions();
				AddWorkflowCustomFieldsColumns();
			}
		}

		void SetCategoryColumnCaptions()
		{
			foreach (var columnStyle in grid.ColumnStyles)
			{
				var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
				if (textBoxColumnStyle != null)
				{
					if (textBoxColumnStyle.ColumnName == GlbCompanyCampaignSchema.G0_Category.Name)
					{
						textBoxColumnStyle.Caption = CategoryLabel;
					}
					else if (textBoxColumnStyle.ColumnName == GlbCompanyCampaignSchema.G0_Type.Name)
					{
						textBoxColumnStyle.Caption = TypeLabel;
					}
				}
			}
		}

		protected virtual void AddWorkflowCustomFieldsColumns()
		{
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, GridCollection, CRMCampaignWorkflowDescriptor.WorkflowTypeCode);
		}

		protected virtual string CategoryLabel => OrganisationsDataRegistry.Instance.CampaignCategory1Label.Value;

		protected virtual string TypeLabel => OrganisationsDataRegistry.Instance.CampaignCategory2Label.Value;
	}
}
