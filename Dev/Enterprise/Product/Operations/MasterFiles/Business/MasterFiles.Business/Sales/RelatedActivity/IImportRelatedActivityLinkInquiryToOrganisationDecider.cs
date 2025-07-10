namespace Enterprise.MasterFiles.Business
{
	[ImportRelatedActivityPromptUserDecider("Enterprise.MasterFiles.GUI.ImportRelatedActivityLinkInquiryToOrganisationPromptUserDecider, Enterprise.MasterFiles.GUI")]
	public interface IImportRelatedActivityLinkInquiryToOrganisationDecider : IImportRelatedActivityDecider
	{
		bool GetDecision(SalesEnquiry inquiry);
	}
}
