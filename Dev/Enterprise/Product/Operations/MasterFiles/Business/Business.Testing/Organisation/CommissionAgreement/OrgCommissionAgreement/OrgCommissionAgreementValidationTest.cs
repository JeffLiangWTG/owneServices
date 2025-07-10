using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCommissionAgreementValidationTest : BusinessObjectValidationTestCase
	{
		#region Properties

		public void TestCheckCA0_CommissionStream()
		{
			var commissionStreams = new CodeDescriptionBoolCollection();
			commissionStreams.Add("AAA", (NoResString)"AAA Description", true);
			commissionStreams.Add("BBB", (NoResString)"BBB Description", false);
			OrganisationRegistry.Instance.CommissionAgreementStreams.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionStreams);

			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			agreement.CA0_CommissionStream = "";
			AssertListValidationInvalidCodeError(agreement.CA0_CommissionStreamInfo, false);
			AssertNoWarnings(agreement.CA0_CommissionStreamInfo);

			agreement.CA0_CommissionStream = "BBB";
			AssertListValidationInvalidCodeError(agreement.CA0_CommissionStreamInfo, true);

			Factory.Save();
			agreement.Validation.ValidateCA0_CommissionStream();
			AssertListValidationInvalidCodeError(agreement.CA0_CommissionStreamInfo, false);
			AssertHasWarning(agreement.CA0_CommissionStreamInfo, ListValidation.InactiveCodeMessage);

			var draft = agreement.CreateDraft();
			draft.Validation.ValidateCA0_CommissionStream();
			AssertListValidationInvalidCodeError(draft.CA0_CommissionStreamInfo, false);
			AssertHasWarning(draft.CA0_CommissionStreamInfo, ListValidation.InactiveCodeMessage);

			draft.CA0_CommissionStream = "AAA";
			Factory.Save();
			draft.CA0_CommissionStream = "BBB";
			AssertListValidationInvalidCodeError(draft.CA0_CommissionStreamInfo, true);
		}

		public void TestCheckCA0_CommissionTriggerType()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();

			agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			AssertMandatoryValidationError(agreement.CA0_CommissionTriggerTypeInfo, false);
			AssertListValidationInvalidCodeError(agreement.CA0_CommissionTriggerTypeInfo, false);

			agreement.CA0_CommissionTriggerType = "";
			AssertMandatoryValidationError(agreement.CA0_CommissionTriggerTypeInfo, true);
			AssertListValidationInvalidCodeError(agreement.CA0_CommissionTriggerTypeInfo, false);

			agreement.CA0_CommissionTriggerType = "XXX";
			AssertMandatoryValidationError(agreement.CA0_CommissionTriggerTypeInfo, false);
			AssertListValidationInvalidCodeError(agreement.CA0_CommissionTriggerTypeInfo, true);
		}

		public void TestCheckCA0_CommissionBasis()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();

			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			AssertMandatoryValidationError(agreement.CA0_CommissionBasisInfo, false);
			AssertListValidationInvalidCodeError(agreement.CA0_CommissionBasisInfo, false);

			agreement.CA0_CommissionBasis = "";
			AssertMandatoryValidationError(agreement.CA0_CommissionBasisInfo, true);
			AssertListValidationInvalidCodeError(agreement.CA0_CommissionBasisInfo, false);

			agreement.CA0_CommissionBasis = "XXX";
			AssertMandatoryValidationError(agreement.CA0_CommissionBasisInfo, false);
			AssertListValidationInvalidCodeError(agreement.CA0_CommissionBasisInfo, true);
		}

		public void TestCheckCA0_Name()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement1 = opportunity.CommissionAgreementsForEdit.AddNew();
			agreement1.CA0_Name = "";
			AssertMandatoryValidationError(agreement1.CA0_NameInfo, true);

			agreement1.CA0_Name = "#1";
			AssertMandatoryValidationError(agreement1.CA0_NameInfo, false);

			var agreement2 = opportunity.CommissionAgreementsForEdit.AddNew();
			agreement2.CA0_Name = "#1";
			AssertPropertyIsUniqueInCollectionValidationError(agreement2.CA0_NameInfo, true);

			agreement2.CA0_Name = "#2";
			AssertPropertyIsUniqueInCollectionValidationError(agreement2.CA0_NameInfo, false);
		}

		[TestDate(2002, 2, 2)]
		public void TestCheckCA0_EffectiveDate_DoNotCheckRange()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_EffectiveDate = new ZDate(1990, 1, 1);    // really old date

			AssertNoErrors(agreement.CA0_EffectiveDateInfo);
			AssertNoWarnings(agreement.CA0_EffectiveDateInfo);
		}

		#endregion

		#region Row Notifications

		public void TestCheckAtLeastOneChildItemIsIncluded()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.ProductItems.DeleteAll();
			agreement.Validation.ValidateAll();
			AssertHasRowError(agreement, "There must be at least one Product item that is included.");

			var item_noAAA = agreement.ProductItems.AddNew(false, "AAA");
			agreement.Validation.ValidateAll();
			AssertHasRowError(agreement, "There must be at least one Product item that is included.");

			var item_BBB = agreement.ProductItems.AddNew(true, "BBB");
			agreement.Validation.ValidateAll();
			AssertNoRowError(agreement, "There must be at least one Product item that is included.");
		}

		public void TestCheckAllRecipientShareValuesAreNotZero()
		{
			const string expectedNotification = @"Cannot set Share for all Wolf Pack members to zero. Select a course of action from the following suggestions:
- To revert to previous Opportunity and Commission Agreement settings, Close the Opportunity without saving.
- To save the current Unapproved Agreement, delete all Wolf Pack members whose shares are zero.
- To nullify an Approved Commission Agreement, set the Agreement to Reversed to reverse all entitlements and/or commissions.
- To cease an Approved Commission Agreement, set the Agreement to Disabled from a specified date to cease commission generation.";

			var agreement = Factory.New<OrgCommissionAgreement>();
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_Share = 0;

			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_Share = 1;

			agreement.Validation.ValidateAll();
			AssertNoRowError(agreement, expectedNotification);

			recipient2.CAR_Share = 0;

			agreement.Validation.ValidateAll();
			AssertHasRowError(agreement, expectedNotification);
		}

		public void TestCheckNoWolfPackMember()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.EffectiveDate = ZDate.Today.AddDays(1);

			using (OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				agreement.Validation.ValidateAll();
				AssertHasRowError(agreement, expectedWolfPackMemberRequiredNotification);
			}

			using (OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				agreement.RunFromApprovalItem = true;

				agreement.Validation.ValidateAll();
				AssertHasRowError(agreement, expectedWolfPackMemberRequiredNotification);

				agreement.RunFromApprovalItem = false;

				agreement.Validation.ValidateAll();
				AssertHasRowWarning(agreement, expectedWolfPackMemberRequiredNotification);
			}
		}

		public void TestCheckNoWolfPackMember_OnRecipientCountChange()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.EffectiveDate = ZDate.Today.AddDays(-1);
			agreement.ProductItems.AddNew(true, "BBB");

			agreement.Validation.ValidateAll();
			AssertHasRowWarning(agreement, expectedWolfPackMemberRequiredNotification);

			agreement.Recipients.AddNew();
			AssertHasRowWarning(agreement, expectedWolfPackMemberRequiredNotification);

			agreement.Recipients.Delete(agreement.Recipients[0]);
			AssertHasRowWarning(agreement, expectedWolfPackMemberRequiredNotification);

			agreement.Recipients.EnableValidationOnCountChange = true;
			agreement.Recipients.AddNew();

			AssertNoRowWarnings(expectedWolfPackMemberRequiredNotification, agreement);

			agreement.Recipients.Delete(agreement.Recipients[0]);

			AssertHasRowWarning(agreement, expectedWolfPackMemberRequiredNotification);
		}

		const string expectedWolfPackMemberRequiredNotification = @"Cannot approve agreement without a wolf pack member and a rate defined";

		#endregion
	}
}
