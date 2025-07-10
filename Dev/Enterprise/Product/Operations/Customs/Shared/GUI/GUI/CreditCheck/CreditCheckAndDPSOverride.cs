using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.GUI
{
	public sealed class CreditCheckAndDPSOverride : ICreditCheckAndDPSOverride
	{
		public void ProcessOverride(SecurityLoginEventArgsForDocumentApproval args, ICreditControlledDocumentDelivery bizObj, ZString caption)
		{
			using (var creditGuiManger = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>())
			{
				creditGuiManger.SetCaption(caption);
				creditGuiManger.Initialise(bizObj);
				bizObj.RaiseOnGetDocumentLogin(args);

				if (bizObj.IsDPSFreightMovementRestricted && (!args.IsAllowedToProceed || args.WasCancelled))
				{
					args.MessageToShowWhenNotAllowed = IsRestrictedDocumentMessageForComplianceWise(args) ?
						ResString.GetMultilingualString("28115840-4a5d-40b4-aced-6b3be9afe7d6", "Unable to submit message due to Job Compliance Risk cancellation.") :
						ResString.GetMultilingualString("41C9BE48-9AE8-4BBB-956E-167945090E3A", "Unable to submit message due to Denied Party Screening cancellation.");
				}
			}
		}

		bool IsRestrictedDocumentMessageForComplianceWise(SecurityLoginEventArgsForDocumentApproval docApprovalArgs)
		{
			if (docApprovalArgs.ParentBusinessObject is IBaseJobDeclaration parent && parent.JE_JS.IsValid)
			{
				return ComplianceRiskHelper.IsFreightEnabledComplianceWise;
			}

			return docApprovalArgs.ParentBusinessObject is IComplianceItemRiskStatusProvider complianceRiskProvider && complianceRiskProvider.IsEnabledComplianceWise;
		}
	}
}
