using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCommissionAgreementRecipientRateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCAT_CommissionPercentage()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			var recipientRate = recipient.Rates.AddNew();

			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipientRate.CAT_CommissionPercentage = 0;
			AssertNoErrors(recipientRate.CAT_CommissionPercentageInfo);

			recipientRate.CAT_CommissionPercentage = 30;
			AssertNoErrors(recipientRate.CAT_CommissionPercentageInfo);

			recipientRate.CAT_CommissionPercentage = 100;
			AssertNoErrors(recipientRate.CAT_CommissionPercentageInfo);

			recipientRate.CAT_CommissionPercentage = 101;
			AssertHasError(recipientRate.CAT_CommissionPercentageInfo, "Please enter a 'Commission Percentage' within the range 0 to 100.");

			recipientRate.CAT_CommissionPercentage = -1;
			AssertHasError(recipientRate.CAT_CommissionPercentageInfo, "Please enter a 'Commission Percentage' within the range 0 to 100.");
		}

		public void TestCheckCAT_CommissionAmount()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			var recipientRate = recipient.Rates.AddNew();

			recipient.CAR_CommissionType = CommissionTypes.Codes.FIX;
			recipientRate.CAT_CommissionAmount = 1;
			AssertNoErrors(recipientRate.CAT_CommissionAmountInfo);

			recipientRate.CAT_CommissionAmount = 100;
			AssertNoErrors(recipientRate.CAT_CommissionAmountInfo);

			recipientRate.CAT_CommissionAmount = 500;
			AssertNoErrors(recipientRate.CAT_CommissionAmountInfo);

			recipientRate.CAT_CommissionAmount = 0;
			AssertHasError(recipientRate.CAT_CommissionAmountInfo, "Please enter a 'Commission Flag Fall' greater than 0.");

			recipientRate.CAT_CommissionAmount = -1;
			AssertHasError(recipientRate.CAT_CommissionAmountInfo, "Please enter a 'Commission Flag Fall' greater than 0.");

			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipientRate.Validation.ValidateCAT_CommissionAmount();
			AssertNoErrors(recipientRate.CAT_CommissionAmountInfo);
		}

		public void TestCheckCAT_RX_NKCommissionCurrency()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			var recipientRate = recipient.Rates.AddNew();

			recipient.CAR_CommissionType = "";
			recipientRate.CAT_RX_NKCommissionCurrency = "";
			recipientRate.Validation.ValidateCAT_RX_NKCommissionCurrency();
			AssertMandatoryValidationError(recipientRate.CAT_RX_NKCommissionCurrencyInfo, false);
			AssertListValidationInvalidCodeError(recipientRate.CAT_RX_NKCommissionCurrencyInfo, false);

			recipient.CAR_CommissionType = CommissionTypes.Codes.FIX;
			recipientRate.CAT_RX_NKCommissionCurrency = "AUD";
			AssertMandatoryValidationError(recipientRate.CAT_RX_NKCommissionCurrencyInfo, false);
			AssertListValidationInvalidCodeError(recipientRate.CAT_RX_NKCommissionCurrencyInfo, false);

			recipientRate.CAT_RX_NKCommissionCurrency = "";
			AssertMandatoryValidationError(recipientRate.CAT_RX_NKCommissionCurrencyInfo, true);
			AssertListValidationInvalidCodeError(recipientRate.CAT_RX_NKCommissionCurrencyInfo, false);

			recipientRate.CAT_RX_NKCommissionCurrency = "XXX";
			AssertMandatoryValidationError(recipientRate.CAT_RX_NKCommissionCurrencyInfo, false);
			AssertListValidationInvalidCodeError(recipientRate.CAT_RX_NKCommissionCurrencyInfo, true);
		}

		public void TestCheckCAT_CommissionPeriod()
		{
			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12).IsEnabled = true;
			commissionPeriods.AddNew("0-24", (NoResString)"First two years", 0, 24).IsEnabled = false;
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = GlbStaff.CurrentUser.GS_Code;
			var recipientRate = recipient.Rates.AddNew();

			recipientRate.CAT_CommissionPeriod = "";
			AssertListValidationInvalidCodeError(recipientRate.CAT_CommissionPeriodInfo, false);

			recipientRate.CAT_CommissionPeriod = "XXX";
			AssertListValidationInvalidCodeError(recipientRate.CAT_CommissionPeriodInfo, true);

			recipientRate.CAT_CommissionPeriod = "0-12";
			AssertListValidationInvalidCodeError(recipientRate.CAT_CommissionPeriodInfo, false);

			recipientRate.CAT_CommissionPeriod = "0-24";
			AssertListValidationInvalidCodeError(recipientRate.CAT_CommissionPeriodInfo, true);

			Factory.Save();

			recipientRate.Validation.ValidateCAT_CommissionPeriod();
			AssertListValidationInvalidCodeError(recipientRate.CAT_CommissionPeriodInfo, false);
			AssertHasWarning(recipientRate.CAT_CommissionPeriodInfo, ListValidation.InactiveCodeMessage);
		}

		public void TestCheckCAT_CommissionPeriod_NoOverlapping()
		{
			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12);
			commissionPeriods.AddNew("12-24", (NoResString)"Second year only", 12, 24);
			commissionPeriods.AddNew("0-24", (NoResString)"First two years", 0, 24);
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();
			var recipientA = agreement.Recipients.AddNew();
			var recipientARate_0_12 = recipientA.Rates.AddNew();
			recipientARate_0_12.CAT_CommissionPeriod = "0-12";
			var recipientARate_12_24 = recipientA.Rates.AddNew();
			recipientARate_12_24.CAT_CommissionPeriod = "12-24";
			var recipientARate_Custom = recipientA.Rates.AddNew();
			recipientARate_Custom.CAT_CommissionPeriod = "";

			var recipientB = agreement.Recipients.AddNew();
			var recipientBRate_0_12 = recipientB.Rates.AddNew();
			recipientBRate_0_12.CAT_CommissionPeriod = "0-12";
			var recipientBRate_0_24 = recipientB.Rates.AddNew();
			recipientBRate_0_24.CAT_CommissionPeriod = "0-24";

			recipientA.RunPreSaveValidation();
			recipientB.RunPreSaveValidation();

			const string expectedErrorMessage = "Another rate has overlapping Entitlement Period.";

			AssertNoError(recipientARate_0_12.CAT_CommissionPeriodInfo, expectedErrorMessage);
			AssertNoError(recipientARate_12_24.CAT_CommissionPeriodInfo, expectedErrorMessage);
			AssertNoError(recipientARate_Custom.CAT_CommissionPeriodInfo, expectedErrorMessage);

			AssertHasError(recipientBRate_0_12.CAT_CommissionPeriodInfo, expectedErrorMessage);
			AssertHasError(recipientBRate_0_24.CAT_CommissionPeriodInfo, expectedErrorMessage);
		}

		[TestDate(2000, 1, 1)]
		public void TestCheckCAT_CommissionStartDate()
		{
			var rate = Factory.New<OrgCommissionAgreementRecipientRate>();
			rate.CAT_CommissionEndDateOverride = new ZDate(2000, 1, 1);
			rate.CAT_CommissionStartDate = new ZDate(1999, 1, 1);
			AssertNoErrors(rate.CAT_CommissionStartDateInfo);

			rate.CAT_CommissionStartDate = ZDate.Empty;
			AssertNoErrors(rate.CAT_CommissionStartDateInfo);

			rate.CAT_CommissionStartDate = new ZDate(2001, 1, 1);
			AssertHasError(rate.CAT_CommissionStartDateInfo, "The 'Entitlement Start Date' must be before the 'Entitlement End Date'.");

			rate.CAT_CommissionEndDateOverride = ZDate.Empty;
			rate.Validation.ValidateCAT_CommissionStartDate();
			AssertNoErrors(rate.CAT_CommissionStartDateInfo);
		}

		[TestDate(2000, 1, 1)]
		public void TestCheckCAT_CommissionStartDate_WarningIfBeforeOpportunityEffectiveDate()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.CA0_EffectiveDate = new ZDate(2002, 1, 1);

			var recipient = agreement.Recipients.AddNew();
			var rate = recipient.Rates.AddNew();
			rate.CAT_CommissionStartDate = new ZDate(1999, 1, 1);
			AssertHasWarning(rate.CAT_CommissionStartDateInfo, "The agreement effective date of '01-Jan-02' will take precedence over this rate start date.");

			rate.CAT_CommissionStartDate = ZDate.Empty;
			AssertHasWarning(rate.CAT_CommissionStartDateInfo, "The agreement effective date of '01-Jan-02' will take precedence over this rate start date.");

			rate.CAT_CommissionStartDate = new ZDate(2003, 1, 1);
			AssertNoWarnings(rate.CAT_CommissionStartDateInfo);
		}

		[TestDate(2000, 1, 1)]
		public void TestCheckCAT_CommissionEndDate()
		{
			var rate = Factory.New<OrgCommissionAgreementRecipientRate>();

			rate.CAT_CommissionStartDateOverride = new ZDate(2000, 1, 1);
			rate.CAT_CommissionEndDate = new ZDate(2001, 1, 1);
			AssertNoErrors(rate.CAT_CommissionEndDateInfo);

			rate.CAT_CommissionEndDate = ZDate.Empty;
			AssertNoErrors(rate.CAT_CommissionEndDateInfo);

			rate.CAT_CommissionEndDate = new ZDate(1999, 1, 1);
			AssertHasError(rate.CAT_CommissionEndDateInfo, "The 'Entitlement End Date' must be after the 'Entitlement Start Date'.");

			rate.CAT_CommissionStartDateOverride = ZDate.Empty;
			rate.Validation.ValidateCAT_CommissionEndDate();
			AssertNoErrors(rate.CAT_CommissionEndDateInfo);
		}

		[TestDate(2000, 1, 1)]
		public void TestCheckCAT_CommissionEndDate_WarningIfAfterOpportunityExpiredDateOrRecipientEndDate()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.CA0_ExpiredDate = new ZDate(2002, 1, 1);

			var recipient = agreement.Recipients.AddNew();
			var rate = recipient.Rates.AddNew();
			rate.CAT_CommissionEndDate = new ZDate(2003, 1, 1);
			AssertHasWarning(rate.CAT_CommissionEndDateInfo, "The agreement expiry date of '01-Jan-02' will take precedence over this rate end date.");

			rate.CAT_CommissionEndDate = ZDate.Empty;
			AssertHasWarning(rate.CAT_CommissionEndDateInfo, "The agreement expiry date of '01-Jan-02' will take precedence over this rate end date.");

			rate.CAT_CommissionEndDate = new ZDate(2001, 1, 1);
			AssertNoWarnings(rate.CAT_CommissionEndDateInfo);

			recipient.CAR_EndDate = new ZDate(2000, 1, 1);
			rate.Validation.ValidateCAT_CommissionEndDate();
			AssertHasWarning(rate.CAT_CommissionEndDateInfo, "The entity entitlement end date of '01-Jan-00' will take precedence over this rate end date.");

			recipient.CAR_EndDate = new ZDate(2002, 1, 1);
			rate.Validation.ValidateCAT_CommissionEndDate();
			AssertNoWarnings(rate.CAT_CommissionEndDateInfo);
		}

		public void TestCheckNoOverlappingOverridenDates()
		{
			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12);
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_CMClientCommenced = new ZDateTime(2014, 1, 1);
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_IsCommissionRateOverriden = true;
			var rate1 = recipient.Rates.AddNew();
			var rate2 = recipient.Rates.AddNew();
			var rate3 = recipient.Rates.AddNew();

			rate1.CAT_CommissionStartDateOverride = new ZDate(2013, 1, 1);
			rate1.CAT_CommissionEndDateOverride = new ZDate(2014, 1, 1);

			rate2.CAT_CommissionStartDateOverride = new ZDate(2013, 5, 1);
			rate2.CAT_CommissionEndDateOverride = new ZDate(2015, 1, 1);

			rate3.CAT_CommissionPeriod = "0-12";

			rate1.Validation.ValidateAll();
			rate2.Validation.ValidateAll();
			rate3.Validation.ValidateAll();

			var expectedOverlappingErrorMessage = "Another rate has overlapping Start/End dates.";
			AssertHasError(rate1.CAT_CommissionStartDateInfo, expectedOverlappingErrorMessage);
			AssertHasError(rate1.CAT_CommissionEndDateInfo, expectedOverlappingErrorMessage);
			AssertHasError(rate2.CAT_CommissionStartDateInfo, expectedOverlappingErrorMessage);
			AssertHasError(rate2.CAT_CommissionEndDateInfo, expectedOverlappingErrorMessage);
			AssertNoError(rate3.CAT_CommissionStartDateInfo, expectedOverlappingErrorMessage);
			AssertNoError(rate3.CAT_CommissionEndDateInfo, expectedOverlappingErrorMessage);

			rate2.CAT_CommissionStartDateOverride = new ZDate(2014, 2, 1);
			rate1.Validation.ValidateAll();
			rate2.Validation.ValidateAll();
			rate3.Validation.ValidateAll();

			AssertNoError(rate1.CAT_CommissionStartDateInfo, expectedOverlappingErrorMessage);
			AssertNoError(rate1.CAT_CommissionEndDateInfo, expectedOverlappingErrorMessage);
			AssertNoError(rate2.CAT_CommissionStartDateInfo, expectedOverlappingErrorMessage);
			AssertNoError(rate2.CAT_CommissionEndDateInfo, expectedOverlappingErrorMessage);
			AssertNoError(rate3.CAT_CommissionStartDateInfo, expectedOverlappingErrorMessage);
			AssertNoError(rate3.CAT_CommissionEndDateInfo, expectedOverlappingErrorMessage);
		}
	}
}
