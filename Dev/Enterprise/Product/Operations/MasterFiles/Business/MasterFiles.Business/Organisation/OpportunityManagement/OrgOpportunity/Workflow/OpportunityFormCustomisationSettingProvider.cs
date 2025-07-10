using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OpportunityFormCustomisationSettingProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			// Must match OrgOpportunity.GetTemplateSelectionCriteria
			// and OpportunityWorkflowDescriptor.SubTypeInformation
			return new[]
			{
				OrgOpportunitySchema.P8_OpportunityType.Name,
				OrgOpportunitySchema.P8_Source.Name,
				OrgOpportunitySchema.P8_PackageType.Name
			};
		}
	}
}
