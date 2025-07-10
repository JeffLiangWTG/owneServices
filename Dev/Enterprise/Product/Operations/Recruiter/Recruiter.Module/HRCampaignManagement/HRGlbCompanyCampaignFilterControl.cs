using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Module
{
	public class HRGlbCompanyCampaignFilterControl : GlbCompanyCampaignFilterControl
	{
		public HRGlbCompanyCampaignFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
		}

		#region Overrides
		protected override void AddWorkflowCustomFieldsColumns()
		{
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, GridCollection, HRCampaignWorkflowDescriptor.WorkflowTypeCode);
		}

		protected override string CategoryLabel => OrganisationsDataRegistry.Instance.HRCampaignCategory1Label.Value;

		protected override string TypeLabel => OrganisationsDataRegistry.Instance.HRCampaignCategory2Label.Value;

		#endregion
	}
}
