using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SalesEnquiryFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			// Must match SalesEnquiry.GetTemplateSelectionCriteria
			// and SalesEnquiryWorkflowDescriptor.SubTypeInformation
			return new[]
			{
				OrgColdCallRegisterSchema.O1_EnquiryType.Name,
				OrgColdCallRegisterSchema.O1_LeadSource.Name,
				OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead.Name
			};
		}
	}
}
