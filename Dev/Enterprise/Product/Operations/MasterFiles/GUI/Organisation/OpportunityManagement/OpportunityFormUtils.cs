using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public static class OpportunityFormUtils
	{
		public static bool PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(this OrgOpportunity opportunity, ZString newStatus)
		{
			var originalStatus = (ZString)opportunity.P8_StatusInfo.OriginalValue;

			var dialogResult = Globals.Message.Show(
				Res.GetString("62d0257a-d823-4ad4-b6ff-1ed565b6fb7b", @"The opportunity's current status of '{0}' indicates it is applicable for new commission agreements.
By changing the status to '{1}', this will no longer be allowed. Existing commission agreements on this opportunity will also be marked for reversal.

Are you sure you want to continue?", originalStatus, newStatus),
				Res.GetString("f9a42ffe-a520-4658-8bdc-f8f131c8c370", "Opportunity Commission Change"),
				MessageBoxButtons.YesNo,
				DialogResult.No);

			if (dialogResult == DialogResult.Yes)
			{
				opportunity.ReverseAgreement();
				return true;
			}

			return false;
		}

		public static bool ShouldPromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(this OrgOpportunity opportunity, ZString newStatus, ZString previousStatus)
		{
			if (previousStatus.EqualsIgnoringCase(newStatus))
			{
				return false;
			}

			var originalStatus = (ZString)opportunity.P8_StatusInfo.OriginalValue;
			var originalIsEffective = OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetEffectiveAgreementFromCode(originalStatus);
			var previousIsEffective = OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetEffectiveAgreementFromCode(previousStatus);
			if (!originalIsEffective || !previousIsEffective)
			{
				return false;
			}

			var newIsEffective = OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetEffectiveAgreementFromCode(newStatus);
			if (newIsEffective)
			{
				return false;
			}

			var nonReversedAgreements = opportunity.CommissionAgreementsForEdit.Where(x => !x.IsReversed).ToList();

			return nonReversedAgreements.Count != 0 && !nonReversedAgreements.All(x => x.IsFirstDraft);
		}

		static void ReverseAgreement(this OrgOpportunity opportunity)
		{
			var nonReversedAgreements = opportunity.CommissionAgreementsForEdit.Where(x => !x.IsReversed).ToList();
			foreach (var agreement in nonReversedAgreements)
			{
				agreement.Reverse();
				agreement.MainVersion.Logs.AddNew(ZArchitecture.Business.AutoEvents.StatusChange, "Opportunity status changed from effective to non-effective.");
			}
		}

		public static void RemoveDuplicatedDrafts(this OrgOpportunity opportunity)
		{
			if (opportunity is null)
			{
				return;
			}

			var approvedAgreementIDs = opportunity.ApprovedCommissionAgreements.Select(x => x.AgreementId).Distinct();

			foreach (var approvedAgreementID in approvedAgreementIDs)
			{
				var duplicatedDrafts = opportunity.CommissionAgreements.Where(x => x.IsDraft && x.AgreementId == approvedAgreementID).ToList();
				var duplicatedDraftsInDB = duplicatedDrafts.Where(x => x.IsInDatabase).ToList();
				var duplicatedDraftsNotInDB = duplicatedDrafts.Where(x => !x.IsInDatabase).ToList();
				if (duplicatedDrafts.Count > 1 && duplicatedDraftsInDB.Count > 0 && duplicatedDraftsNotInDB.Count > 0)
				{
					duplicatedDraftsNotInDB.ForEach(x => x.DisapproveDraft());
				}
			}
		}
	}
}
