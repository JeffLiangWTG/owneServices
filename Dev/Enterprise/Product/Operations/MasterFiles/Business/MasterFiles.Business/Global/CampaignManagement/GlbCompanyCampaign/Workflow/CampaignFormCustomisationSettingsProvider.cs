using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CampaignFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			// Must match GlbCompanyCampaign.GetTemplateSelectionCriteria
			// and CRM/HRCampaignWorkflowDescriptor.SubTypeInformation
			return new[]
			{
				GlbCompanyCampaignSchema.G0_Category.Name,
				GlbCompanyCampaignSchema.G0_Type.Name
			};
		}
	}
}
