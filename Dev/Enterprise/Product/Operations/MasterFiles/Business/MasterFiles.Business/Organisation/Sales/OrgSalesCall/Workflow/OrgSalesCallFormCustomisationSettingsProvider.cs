using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			// Must match OrgSalesCall.GetTemplateSelectionCriteria
			// and OrgSalesCallWorkflowDescriptor.SubTypeInformation
			return new[]
			{
				OrgSalesCallSchema.OQ_TypeOfCall.Name,
				OrgSalesCallSchema.OQ_Status.Name,
				OrgSalesCallSchema.OQ_Category.Name
			};
		}
	}
}
