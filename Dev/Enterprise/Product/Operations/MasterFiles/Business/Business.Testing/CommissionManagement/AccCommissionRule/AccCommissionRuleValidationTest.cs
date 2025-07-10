using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCommissionRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckACM_Product()
		{
			var rule = Factory.New<AccCommissionRule>();

			rule.ACM_Product = "";
			AssertMandatoryValidationError(rule.ACM_ProductInfo, true);
			AssertListValidationInvalidCodeError(rule.ACM_ProductInfo, false);

			rule.ACM_Product = "XXX";
			AssertMandatoryValidationError(rule.ACM_ProductInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_ProductInfo, true);

			rule.ACM_Product = CommissionRuleLookups.AnyProductsCode;
			AssertMandatoryValidationError(rule.ACM_ProductInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_ProductInfo, false);
		}

		public void TestCheckACM_Service()
		{
			var rule = Factory.New<AccCommissionRule>();

			rule.ACM_Service = "";
			AssertMandatoryValidationError(rule.ACM_ServiceInfo, true);
			AssertListValidationInvalidCodeError(rule.ACM_ServiceInfo, false);

			rule.ACM_Service = "XXX";
			AssertMandatoryValidationError(rule.ACM_ServiceInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_ServiceInfo, true);

			rule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			AssertMandatoryValidationError(rule.ACM_ServiceInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_ServiceInfo, false);
		}

		public void TestCheckACM_SubModule()
		{
			var rule = Factory.New<AccCommissionRule>();

			rule.ACM_SubModule = "";
			AssertMandatoryValidationError(rule.ACM_SubModuleInfo, true);
			AssertListValidationInvalidCodeError(rule.ACM_SubModuleInfo, false);

			rule.ACM_SubModule = "XXX";
			AssertMandatoryValidationError(rule.ACM_SubModuleInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_SubModuleInfo, true);

			rule.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			AssertMandatoryValidationError(rule.ACM_SubModuleInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_SubModuleInfo, false);
		}

		public void TestCheckACM_CommissionBasis()
		{
			var rule = Factory.New<AccCommissionRule>();

			rule.ACM_CommissionBasis = "";
			AssertMandatoryValidationError(rule.ACM_CommissionBasisInfo, true);
			AssertListValidationInvalidCodeError(rule.ACM_CommissionBasisInfo, false);

			rule.ACM_CommissionBasis = "XXX";
			AssertMandatoryValidationError(rule.ACM_CommissionBasisInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_CommissionBasisInfo, true);

			rule.ACM_CommissionBasis = CommissionBasisType.Codes.PRF;
			AssertMandatoryValidationError(rule.ACM_CommissionBasisInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_CommissionBasisInfo, false);
		}

		public void TestCheckACM_CommissionTriggerType()
		{
			var rule = Factory.New<AccCommissionRule>();

			rule.ACM_CommissionTriggerType = "";
			AssertMandatoryValidationError(rule.ACM_CommissionTriggerTypeInfo, true);
			AssertListValidationInvalidCodeError(rule.ACM_CommissionTriggerTypeInfo, false);

			rule.ACM_CommissionTriggerType = "XXX";
			AssertMandatoryValidationError(rule.ACM_CommissionTriggerTypeInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_CommissionTriggerTypeInfo, true);

			rule.ACM_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			AssertMandatoryValidationError(rule.ACM_CommissionTriggerTypeInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_CommissionTriggerTypeInfo, false);

			rule.ACM_CommissionTriggerType = CommissionTriggerTypes.Codes.EarliestRevRecog;
			AssertMandatoryValidationError(rule.ACM_CommissionTriggerTypeInfo, false);
			AssertListValidationInvalidCodeError(rule.ACM_CommissionTriggerTypeInfo, false);
		}

		[TestDate(2000, 1, 1)]
		public void TestCheckACM_StartDate()
		{
			var rule = Factory.New<AccCommissionRule>();

			rule.ACM_EndDate = new ZDate(2000, 1, 1);
			rule.ACM_StartDate = new ZDate(1999, 1, 1);
			AssertNoErrors(rule.ACM_StartDateInfo);

			rule.ACM_StartDate = ZDate.Empty;
			AssertNoErrors(rule.ACM_StartDateInfo);

			rule.ACM_StartDate = new ZDate(2001, 1, 1);
			AssertHasError(rule.ACM_StartDateInfo, "The 'Start Date' must be before the 'End Date'.");

			rule.ACM_EndDate = ZDate.Empty;
			rule.Validation.ValidateACM_StartDate();
			AssertNoErrors(rule.ACM_StartDateInfo);
		}

		[TestDate(2000, 1, 1)]
		public void TestCheckACM_EndDate()
		{
			var rule = Factory.New<AccCommissionRule>();

			rule.ACM_StartDate = new ZDate(2000, 1, 1);
			rule.ACM_EndDate = new ZDate(2001, 1, 1);
			AssertNoErrors(rule.ACM_EndDateInfo);

			rule.ACM_EndDate = ZDate.Empty;
			AssertNoErrors(rule.ACM_EndDateInfo);

			rule.ACM_EndDate = new ZDate(1999, 1, 1);
			AssertHasError(rule.ACM_EndDateInfo, "The 'End Date' must be after the 'Start Date'.");

			rule.ACM_StartDate = ZDate.Empty;
			rule.Validation.ValidateACM_EndDate();
			AssertNoErrors(rule.ACM_EndDateInfo);
		}

		public void TestCheckHasAtLeastOneRate()
		{
			var rule = Factory.New<AccCommissionRule>();

			rule.Validation.ValidateAll();
			AssertHasRowError(rule, "Please enter at least one commission rate.");

			rule.Rates.AddNew();
			rule.Validation.ValidateAll();
			AssertNoRowError(rule, "Please enter at least one commission rate.");
		}
	}
}
