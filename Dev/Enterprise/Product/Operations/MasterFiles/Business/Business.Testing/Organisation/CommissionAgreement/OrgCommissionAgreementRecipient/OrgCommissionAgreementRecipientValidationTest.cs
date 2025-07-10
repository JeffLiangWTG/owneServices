using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCommissionAgreementRecipientValidationTest : BusinessObjectValidationTestCase
	{
		#region Properties

		public void TestRecipientMandatory()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			var org = Factory.New<OrgHeader>();

			var agreement = Factory.New<OrgCommissionAgreement>();
			var recipient = agreement.Recipients.AddNew();

			recipient.Validation.ValidateCAR_GS_NKStaff();
			recipient.Validation.ValidateCAR_OH_Party();
			AssertMandatoryValidationError(recipient.CAR_GS_NKStaffInfo, true);
			AssertMandatoryValidationError(recipient.CAR_OH_PartyInfo, true);

			recipient.CAR_GS_NKStaff = "ADL";

			recipient.Validation.ValidateCAR_GS_NKStaff();
			recipient.Validation.ValidateCAR_OH_Party();
			AssertMandatoryValidationError(recipient.CAR_GS_NKStaffInfo, false);
			AssertMandatoryValidationError(recipient.CAR_OH_PartyInfo, false);

			recipient.CAR_GS_NKStaff = "";
			recipient.CAR_OH_Party = org.PK;

			recipient.Validation.ValidateCAR_GS_NKStaff();
			recipient.Validation.ValidateCAR_OH_Party();
			AssertMandatoryValidationError(recipient.CAR_GS_NKStaffInfo, false);
			AssertMandatoryValidationError(recipient.CAR_OH_PartyInfo, false);
		}

		public void TestCheckCAR_Share_WarningIfNoApplicableRuleExistsAndHasNoOverrideSecurityPermission()
		{
			const string expectedNotificationMessage = "This staff can not receive any commission as it does not have any applicable commission rules setup for this agreement. Please add an applicable commission rule for this staff, or ask a person with the appropriate security rights to manually enter this staff's commission rates for this agreement.";

			Env.Security.CommissionAgreementOverrideAny.IsAllowed = false;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			AssertEquals("Precondition", 0, staff.OverallCommissionRules.Count);

			var recipient = Factory.NewWithValidTestData<OrgCommissionAgreementRecipient>();
			recipient.CAR_GS_NKStaff = "ADL";
			recipient.CAR_Share = 0;
			recipient.Validation.ValidateCAR_Share();
			AssertHasWarning(recipient.CAR_ShareInfo, expectedNotificationMessage);

			Env.Security.CommissionAgreementOverrideAny.IsAllowed = true;
			recipient.Validation.ValidateCAR_Share();
			AssertNoWarning(recipient.CAR_ShareInfo, expectedNotificationMessage);

			Factory.Save();

			Env.Security.CommissionAgreementOverrideAny.IsAllowed = false;
			recipient.Validation.ValidateCAR_Share();
			AssertHasWarning(recipient.CAR_ShareInfo, expectedNotificationMessage);
		}

		public void TestCheckCAR_Share_WarningIfShareZeroAndNoCommissions()
		{
			const string expectedNotificationMessage = "No commissions have been created for this Wolf Pack member as part of this Commission Agreement therefore there is no benefit in having a zero Share value. Increase the Share value or delete the Wolf Pack member instead.";

			var parentAgreement = Factory.New<OrgCommissionAgreement>();
			var parentRecipient = parentAgreement.Recipients.AddNew();
			parentRecipient.CAR_GS_NKStaff = "TES";
			parentRecipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			var parentRate1 = parentRecipient.Rates.AddNew();
			parentRecipient.Rates.AddNew();

			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_CA0_ParentVersion = parentAgreement.PK;

			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "TES";
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;

			recipient.CAR_Share = 1;
			recipient.Validation.ValidateCAR_Share();
			AssertNoWarning(recipient.CAR_ShareInfo, expectedNotificationMessage);

			recipient.CAR_Share = 0;
			recipient.Validation.ValidateCAR_Share();
			AssertNoWarning(recipient.CAR_ShareInfo, expectedNotificationMessage);

			recipient.Rates.AddNew();
			recipient.Validation.ValidateCAR_Share();

			AssertHasWarning(recipient.CAR_ShareInfo, expectedNotificationMessage);

			var commissionLine = Factory.New<IAccCommissionLine>();
			commissionLine.CL0_CAT = parentRate1.PK;

			recipient.Validation.ValidateCAR_Share();
			AssertNoWarning(recipient.CAR_ShareInfo, expectedNotificationMessage);

			agreement.CA0_CA0_ParentVersion = ZGuid.Empty;

			recipient.Validation.ValidateCAR_Share();

			AssertHasWarning(recipient.CAR_ShareInfo, expectedNotificationMessage);
		}

		public void TestCheckCAR_CommissionType()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			var recipient = agreement.Recipients.AddNew();

			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 1;
			AssertMandatoryValidationError(recipient.CAR_CommissionTypeInfo, false);
			AssertListValidationInvalidCodeError(recipient.CAR_CommissionTypeInfo, false);

			recipient.CAR_CommissionType = "";
			recipient.CAR_Share = 1;
			AssertMandatoryValidationError(recipient.CAR_CommissionTypeInfo, true);
			AssertListValidationInvalidCodeError(recipient.CAR_CommissionTypeInfo, false);

			recipient.CAR_CommissionType = "XXX";
			recipient.CAR_Share = 1;
			AssertMandatoryValidationError(recipient.CAR_CommissionTypeInfo, false);
			AssertListValidationInvalidCodeError(recipient.CAR_CommissionTypeInfo, true);

			recipient.CAR_CommissionType = "";
			recipient.CAR_Share = 0;
			recipient.Validation.ValidateCAR_CommissionType();
			AssertMandatoryValidationError(recipient.CAR_CommissionTypeInfo, false);
			AssertListValidationInvalidCodeError(recipient.CAR_CommissionTypeInfo, false);
		}

		#endregion

		#region Row Notifications

		public void TestCheckNoDuplicateEntityPerType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = org.PK;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_GB_HomeBranch = branch.PK;

			var agreement = Factory.New<OrgCommissionAgreement>();
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			AssertEquals(org.PK, recipient1.CAR_OH_Party);

			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_OH_Party = org.PK;

			recipient1.Validation.ValidateAll();
			recipient2.Validation.ValidateAll();
			AssertNoRowError(recipient1, "The recipient has been duplicated and must be unique per commission type.");
			AssertNoRowError(recipient2, "The recipient has been duplicated and must be unique per commission type.");

			recipient2.CAR_GS_NKStaff = "ADL";
			recipient1.Validation.ValidateAll();
			recipient2.Validation.ValidateAll();
			AssertHasRowError(recipient1, "The recipient has been duplicated and must be unique per commission type.");
			AssertHasRowError(recipient2, "The recipient has been duplicated and must be unique per commission type.");

			recipient1.CAR_GS_NKStaff = "";
			recipient1.CAR_OH_Party = org.PK;
			recipient2.CAR_GS_NKStaff = "";
			recipient2.CAR_OH_Party = org.PK;
			recipient1.Validation.ValidateAll();
			recipient2.Validation.ValidateAll();
			AssertHasRowError(recipient1, "The recipient has been duplicated and must be unique per commission type.");
			AssertHasRowError(recipient2, "The recipient has been duplicated and must be unique per commission type.");
		}

		public void TestCheckHasAtLeastOneRate()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			var recipient = agreement.Recipients.AddNew();

			recipient.CAR_Share = 0;
			recipient.Validation.ValidateAll();
			AssertNoRowError(recipient, "Please enter at least one commission rate.");

			recipient.CAR_Share = 1;
			recipient.Validation.ValidateAll();
			AssertHasRowError(recipient, "Please enter at least one commission rate.");

			recipient.Rates.AddNew();
			recipient.Validation.ValidateAll();
			AssertNoRowError(recipient, "Please enter at least one commission rate.");
		}

		#endregion
	}
}
