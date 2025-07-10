using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCommissionRuleRateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckACT_CommissionType()
		{
			var rate = Factory.New<AccCommissionRuleRate>();

			rate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			AssertMandatoryValidationError(rate.ACT_CommissionTypeInfo, false);
			AssertListValidationInvalidCodeError(rate.ACT_CommissionTypeInfo, false);

			rate.ACT_CommissionType = "";
			AssertMandatoryValidationError(rate.ACT_CommissionTypeInfo, true);
			AssertListValidationInvalidCodeError(rate.ACT_CommissionTypeInfo, false);

			rate.ACT_CommissionType = "XXX";
			AssertMandatoryValidationError(rate.ACT_CommissionTypeInfo, false);
			AssertListValidationInvalidCodeError(rate.ACT_CommissionTypeInfo, true);
		}

		public void TestCheckACT_CommissionType_AllMustBeTheSame()
		{
			const string expectedSameCommissionTypeErrorMessage = "All Rates must have the same Commission Type.";

			var rule = Factory.New<AccCommissionRule>();
			var ruleRate1 = rule.Rates.AddNew();
			var ruleRate2 = rule.Rates.AddNew();

			ruleRate1.ACT_CommissionType = CommissionTypes.Codes.FIX;
			ruleRate2.ACT_CommissionType = CommissionTypes.Codes.PCT;
			ruleRate1.Validation.ValidateACT_CommissionType();
			ruleRate2.Validation.ValidateACT_CommissionType();
			AssertHasError(ruleRate1.ACT_CommissionTypeInfo, expectedSameCommissionTypeErrorMessage);
			AssertHasError(ruleRate2.ACT_CommissionTypeInfo, expectedSameCommissionTypeErrorMessage);

			ruleRate1.ACT_CommissionType = CommissionTypes.Codes.FIX;
			ruleRate2.ACT_CommissionType = CommissionTypes.Codes.FIX;
			ruleRate1.Validation.ValidateACT_CommissionType();
			ruleRate2.Validation.ValidateACT_CommissionType();
			AssertNoError(ruleRate1.ACT_CommissionTypeInfo, expectedSameCommissionTypeErrorMessage);
			AssertNoError(ruleRate2.ACT_CommissionTypeInfo, expectedSameCommissionTypeErrorMessage);

			var ruleOverride = Factory.New<AccCommissionRuleStaffOverride>();
			var ruleOverrideRate1 = ruleOverride.Rates.AddNew();
			var ruleOverrideRate2 = ruleOverride.Rates.AddNew();

			ruleOverrideRate1.ACT_CommissionType = CommissionTypes.Codes.FIX;
			ruleOverrideRate2.ACT_CommissionType = CommissionTypes.Codes.PCT;
			ruleOverrideRate1.Validation.ValidateACT_CommissionType();
			ruleOverrideRate2.Validation.ValidateACT_CommissionType();
			AssertHasError(ruleOverrideRate1.ACT_CommissionTypeInfo, expectedSameCommissionTypeErrorMessage);
			AssertHasError(ruleOverrideRate2.ACT_CommissionTypeInfo, expectedSameCommissionTypeErrorMessage);

			ruleOverrideRate1.ACT_CommissionType = CommissionTypes.Codes.FIX;
			ruleOverrideRate2.ACT_CommissionType = CommissionTypes.Codes.FIX;
			ruleOverrideRate1.Validation.ValidateACT_CommissionType();
			ruleOverrideRate2.Validation.ValidateACT_CommissionType();
			AssertNoError(ruleOverrideRate1.ACT_CommissionTypeInfo, expectedSameCommissionTypeErrorMessage);
			AssertNoError(ruleOverrideRate2.ACT_CommissionTypeInfo, expectedSameCommissionTypeErrorMessage);
		}

		public void TestCheckACT_CommissionPercentage()
		{
			var rate = Factory.New<AccCommissionRuleRate>();

			rate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			rate.ACT_CommissionPercentage = 0;
			AssertNoErrors(rate.ACT_CommissionPercentageInfo);

			rate.ACT_CommissionPercentage = 30;
			AssertNoErrors(rate.ACT_CommissionPercentageInfo);

			rate.ACT_CommissionPercentage = 100;
			AssertNoErrors(rate.ACT_CommissionPercentageInfo);

			rate.ACT_CommissionPercentage = 101;
			AssertHasError(rate.ACT_CommissionPercentageInfo, "Please enter a 'Commission Percentage' within the range 0 to 100.");

			rate.ACT_CommissionPercentage = -1;
			AssertHasError(rate.ACT_CommissionPercentageInfo, "Please enter a 'Commission Percentage' within the range 0 to 100.");
		}

		public void TestCheckACT_CommissionAmount()
		{
			var rate = Factory.New<AccCommissionRuleRate>();

			rate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			rate.ACT_CommissionAmount = 1;
			AssertNoErrors(rate.ACT_CommissionAmountInfo);

			rate.ACT_CommissionAmount = 10;
			AssertNoErrors(rate.ACT_CommissionAmountInfo);

			rate.ACT_CommissionAmount = 500;
			AssertNoErrors(rate.ACT_CommissionAmountInfo);

			rate.ACT_CommissionAmount = 0;
			AssertHasError(rate.ACT_CommissionAmountInfo, "Please enter a 'Commission Flag Fall' greater than 0.");

			rate.ACT_CommissionAmount = -1;
			AssertHasError(rate.ACT_CommissionAmountInfo, "Please enter a 'Commission Flag Fall' greater than 0.");

			rate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			rate.Validation.ValidateACT_CommissionAmount();
			AssertNoErrors(rate.ACT_CommissionAmountInfo);
		}

		public void TestCheckACT_RX_NKCommissionCurrency()
		{
			var rate = Factory.New<AccCommissionRuleRate>();

			rate.ACT_CommissionType = "";
			rate.ACT_RX_NKCommissionCurrency = "";
			rate.Validation.ValidateACT_RX_NKCommissionCurrency();
			AssertMandatoryValidationError(rate.ACT_RX_NKCommissionCurrencyInfo, false);
			AssertListValidationInvalidCodeError(rate.ACT_RX_NKCommissionCurrencyInfo, false);

			rate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			rate.ACT_RX_NKCommissionCurrency = "AUD";
			AssertMandatoryValidationError(rate.ACT_RX_NKCommissionCurrencyInfo, false);
			AssertListValidationInvalidCodeError(rate.ACT_RX_NKCommissionCurrencyInfo, false);

			rate.ACT_RX_NKCommissionCurrency = "";
			AssertMandatoryValidationError(rate.ACT_RX_NKCommissionCurrencyInfo, true);
			AssertListValidationInvalidCodeError(rate.ACT_RX_NKCommissionCurrencyInfo, false);

			rate.ACT_RX_NKCommissionCurrency = "XXX";
			AssertMandatoryValidationError(rate.ACT_RX_NKCommissionCurrencyInfo, false);
			AssertListValidationInvalidCodeError(rate.ACT_RX_NKCommissionCurrencyInfo, true);
		}

		public void TestCheckACT_CommissionPeriod()
		{
			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12).IsEnabled = true;
			commissionPeriods.AddNew("0-24", (NoResString)"First two years", 0, 24).IsEnabled = false;
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			var group = Factory.NewWithValidTestData<SalesTeam>();
			var rule = group.CommissionRules.AddNew();
			rule.FillWithValidTestData();
			var rate = rule.Rates.AddNew();
			rate.FillWithValidTestData();

			rate.ACT_CommissionPeriod = "";
			AssertMandatoryValidationError(rate.ACT_CommissionPeriodInfo, true);
			AssertListValidationInvalidCodeError(rate.ACT_CommissionPeriodInfo, false);

			rate.ACT_CommissionPeriod = "XXX";
			AssertMandatoryValidationError(rate.ACT_CommissionPeriodInfo, false);
			AssertListValidationInvalidCodeError(rate.ACT_CommissionPeriodInfo, true);

			rate.ACT_CommissionPeriod = "0-12";
			AssertMandatoryValidationError(rate.ACT_CommissionPeriodInfo, false);
			AssertListValidationInvalidCodeError(rate.ACT_CommissionPeriodInfo, false);

			rate.ACT_CommissionPeriod = "0-24";
			AssertMandatoryValidationError(rate.ACT_CommissionPeriodInfo, false);
			AssertListValidationInvalidCodeError(rate.ACT_CommissionPeriodInfo, true);

			Factory.Save();

			rate.Validation.ValidateACT_CommissionPeriod();
			AssertMandatoryValidationError(rate.ACT_CommissionPeriodInfo, false);
			AssertListValidationInvalidCodeError(rate.ACT_CommissionPeriodInfo, false);
			AssertHasWarning(rate.ACT_CommissionPeriodInfo, ListValidation.InactiveCodeMessage);
		}

		public void TestCheckACT_CommissionPeriod_NoOverlapping()
		{
			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12);
			commissionPeriods.AddNew("12-24", (NoResString)"Second year only", 12, 24);
			commissionPeriods.AddNew("0-24", (NoResString)"First two years", 0, 24);
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			var ruleA = Factory.New<AccCommissionRule>();
			var ruleARate_0_12 = AddNewRateWithPeriod(ruleA, "0-12");
			var ruleARate_12_24 = AddNewRateWithPeriod(ruleA, "12-24");

			var ruleB = Factory.New<AccCommissionRule>();
			var ruleBRate_0_12 = AddNewRateWithPeriod(ruleB, "0-12");
			var ruleBRate_0_24 = AddNewRateWithPeriod(ruleB, "0-24");

			var ruleOverrideA = Factory.New<AccCommissionRuleStaffOverride>();
			var ruleOverrideARate0_12 = AddNewRateWithPeriod(ruleOverrideA, "0-12");
			var ruleOverrideARate0_24 = AddNewRateWithPeriod(ruleOverrideA, "0-24");

			ruleA.RunPreSaveValidation();
			ruleB.RunPreSaveValidation();

			const string expectedErrorMessage = "Another rate has overlapping Entitlement Period.";

			AssertNoError(ruleARate_0_12.ACT_CommissionPeriodInfo, expectedErrorMessage);
			AssertNoError(ruleARate_12_24.ACT_CommissionPeriodInfo, expectedErrorMessage);

			AssertHasError(ruleBRate_0_12.ACT_CommissionPeriodInfo, expectedErrorMessage);
			AssertHasError(ruleBRate_0_24.ACT_CommissionPeriodInfo, expectedErrorMessage);
		}

		static AccCommissionRuleRate AddNewRateWithPeriod(ICommissionRuleRatesProvider ratesProvider, ZString commissionPeriod)
		{
			var result = ratesProvider.Rates.AddNew();
			result.ACT_CommissionPeriod = commissionPeriod;
			return result;
		}
	}
}
