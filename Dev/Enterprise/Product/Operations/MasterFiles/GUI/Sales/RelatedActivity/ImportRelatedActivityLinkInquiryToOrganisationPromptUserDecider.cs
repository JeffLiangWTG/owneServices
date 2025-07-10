using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class ImportRelatedActivityLinkInquiryToOrganisationPromptUserDecider : ImportRelatedActivityPromptUserDecider, IImportRelatedActivityLinkInquiryToOrganisationDecider
	{
		public ImportRelatedActivityLinkInquiryToOrganisationPromptUserDecider(KForm parentForm)
			: base(parentForm)
		{
		}

		public bool GetDecision(SalesEnquiry inquiry)
		{
			return
				EnsureSaved(inquiry)
				&& PopupOrgMatcher(inquiry) == DialogResult.OK;
		}

		bool EnsureSaved(SalesEnquiry inquiry)
		{
			bool result = true;
			if (inquiry.HasChanges)
			{
				result = false;
				var parentSalesEnquiryForm = ParentForm as SalesEnquiryForm;
				if (parentSalesEnquiryForm != null)
				{
					result = parentSalesEnquiryForm.PromptUserForSave() == DialogResult.OK;
				}
			}

			return result;
		}

		DialogResult PopupOrgMatcher(SalesEnquiry inquiry)
		{
			var findOrgForm = new SalesEnquiryFindOrgForm(inquiry, inquiry.CreateEnquiryOrgFinder(), true);
			return ZFormModaliser.ShowDialogAndDispose(findOrgForm);
		}
	}
}
