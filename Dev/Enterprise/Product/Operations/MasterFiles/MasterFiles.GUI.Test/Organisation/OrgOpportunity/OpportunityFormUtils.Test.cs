using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class OpportunityFormUtilsTest : TestCaseWithFactory
	{
		public void TestShouldPromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective()
		{
			var opportunity = GetOpportunity();

			AssertEquals(false, opportunity.ShouldPromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective("NON", "NON"));
			AssertEquals("Original and previous agreement are not effective", false, opportunity.ShouldPromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective("NON", "EFF"));

			GetOrgCommissionAgreementsAndOrgHeader(opportunity);

			AssertEquals("Original and previous agreement are effective", true, opportunity.ShouldPromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective("NON", "EFF"));

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			opportunity.PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective("EFF");

			AssertEquals("Original and previous agreement are reversed", false, opportunity.ShouldPromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective("EFF", "NON"));
		}

		public void TestReverseAgreement()
		{
			var opportunity = GetOpportunity();
			var temp = GetOrgCommissionAgreementsAndOrgHeader(opportunity);
			var approvedAgreement = temp.Item1;
			var unapprovedAgreement = temp.Item2;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			opportunity.PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective("EFF");

			AssertEquals("EFF", opportunity.P8_Status);
			AssertEquals(true, approvedAgreement.HasDraft);
			AssertEquals(true, approvedAgreement.Draft.IsReversed);
			AssertEquals(true, unapprovedAgreement.IsReversed);

			var logs = approvedAgreement.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.Events.StatusChangeCode).Select(x => x.SL_Reference).ToArray();
			AssertArrayEqualsByElements(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeLogs, logs);

			logs = unapprovedAgreement.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.Events.StatusChangeCode).Select(x => x.SL_Reference).ToArray();
			AssertArrayEqualsByElements(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeLogs, logs);
		}

		public void TestPromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective()
		{
			var opportunity = GetOpportunity();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var actualResult = opportunity.PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective("NON");
			AssertEquals(false, actualResult);
			AssertEquals(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals(string.Format(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaptionText, "EFF", "NON"), UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			actualResult = opportunity.PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective("NON");
			AssertEquals(true, actualResult);
			AssertEquals(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals(string.Format(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaptionText, "EFF", "NON"), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestOpportunity_CommissionAgreements_RemoveDuplicatedCommissionDrafts()
		{
			var opportunity = GetOpportunity();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.CA0_Name = "#1";
			agreement.FillWithValidTestData();
			var draft = agreement.CreateDraft(true);
			draft.CA0_EffectiveDate = ZDate.Today;
			Factory.Save();
			agreement.CreateDraft(true);

			var duplicatedDrafts = opportunity.CommissionAgreements.Where(x => x.IsDraft && x.AgreementId == agreement.AgreementId).ToList();
			var duplicatedDraftsInDB = duplicatedDrafts.Where(x => x.IsInDatabase).ToList();
			var duplicatedDraftsNotInDB = duplicatedDrafts.Where(x => !x.IsInDatabase).ToList();

			AssertEquals(2, duplicatedDrafts.Count);
			AssertEquals(1, duplicatedDraftsInDB.Count);
			AssertEquals(1, duplicatedDraftsNotInDB.Count);

			opportunity.RemoveDuplicatedDrafts();
			duplicatedDrafts = opportunity.CommissionAgreements.Where(x => x.IsDraft && x.AgreementId == agreement.AgreementId).ToList();
			duplicatedDraftsInDB = duplicatedDrafts.Where(x => x.IsInDatabase).ToList();
			duplicatedDraftsNotInDB = duplicatedDrafts.Where(x => !x.IsInDatabase).ToList();

			AssertEquals(1, duplicatedDrafts.Count);
			AssertEquals(1, duplicatedDraftsInDB.Count);
			AssertEquals(0, duplicatedDraftsNotInDB.Count);
		}

		OrgOpportunity GetOpportunity(BusinessObjectFactory factory = null)
		{
			factory = factory ?? Factory;
			var opportunity = factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_Status = "EFF";
			opportunity.P8_OpportunityDescription = "This is a test opportunity.";
			opportunity.P8_Stage = "UDF";

			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OpportunityCommissionChangeUtils.GetOpportunityStatusCollection());

			factory.Save();

			return opportunity;
		}

		(OrgCommissionAgreement, OrgCommissionAgreement, OrgHeader) GetOrgCommissionAgreementsAndOrgHeader(OrgOpportunity opportunity)
		{
			var approvedAgreement = opportunity.ApprovedCommissionAgreements.AddNew();
			approvedAgreement.FillWithValidTestData();
			var unapprovedAgreement = opportunity.CommissionAgreements.AddNew();
			unapprovedAgreement.FillWithValidTestData();

			var organization = OrgHeader.New(Factory);
			organization.OH_IsSalesLead = true;
			organization.SalesOpportunities.Add(opportunity);

			return (approvedAgreement, unapprovedAgreement, organization);
		}
	}
}
