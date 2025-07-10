using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgOpportunityFilterControl : ZFilterStripControl
	{
		public OrgOpportunityFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetProductTypeColumnCaption();
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, OpportunityWorkflowDescriptor.WorkflowTypeCode);
			}
		}

		void SetProductTypeColumnCaption()
		{
			foreach (var columnStyle in grid.ColumnStyles)
			{
				var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
				if (textBoxColumnStyle != null)
				{
					if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_PackageType.Name)
					{
						textBoxColumnStyle.Caption = OrganisationsDataRegistry.Instance.ProductTypeLabel.Value;
					}
					else if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_DiscountAmount.Name)
					{
						textBoxColumnStyle.Caption = OrganisationsDataRegistry.Instance.CurrentLabel.Value;
					}
					else if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_RentalMultiplier.Name)
					{
						textBoxColumnStyle.Caption = OrganisationsDataRegistry.Instance.PotentialLabel.Value;
					}
				}
			}
		}
	}
}
