using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccGroupCommissionRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckACM_EndDate_Unique()
		{
			TestChecksUnique((rule) => rule.ACM_EndDateInfo);
		}

		public void TestCheckACM_GC_Unique()
		{
			TestChecksUnique((rule) => rule.ACM_GCInfo);
		}

		public void TestCheckACM_Product_Unique()
		{
			TestChecksUnique((rule) => rule.ACM_ProductInfo);
		}

		public void TestCheckACM_Service_Unique()
		{
			TestChecksUnique((rule) => rule.ACM_ServiceInfo);
		}

		public void TestCheckACM_StartDate_Unique()
		{
			TestChecksUnique((rule) => rule.ACM_StartDateInfo);
		}

		public void TestCheckACM_SubModule_Unique()
		{
			TestChecksUnique((rule) => rule.ACM_SubModuleInfo);
		}

		void TestChecksUnique(Func<AccGroupCommissionRule, ZPropertyInfo> propertyInfoGetter)
		{
			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var salesTeam = Factory.New<SalesTeam>();
				var rule_ANY_ANY_ANY = AddNewRule(salesTeam, CommissionRuleLookups.AnyProductsCode, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, ZDate.Empty, ZDate.Empty);
				var rule_SHP_ANY_ANY = AddNewRule(salesTeam, JobInvoicingConsumerTypes.Shipment.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, ZDate.Empty, ZDate.Empty);

				var rule_CON_ANY_ANYa = AddNewRule(salesTeam, JobInvoicingConsumerTypes.Consol.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, ZDate.Empty, ZDate.Empty);
				var rule_CON_ANY_ANYb = AddNewRule(salesTeam, JobInvoicingConsumerTypes.Consol.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, ZDate.Empty, ZDate.Empty);

				var rule_BRK_ANY_ANY_old = AddNewRule(salesTeam, JobInvoicingConsumerTypes.Brokerage.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, ZDate.Empty, new ZDate(2000, 1, 1));
				var rule_BRK_ANY_ANY_new = AddNewRule(salesTeam, JobInvoicingConsumerTypes.Brokerage.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, new ZDate(2000, 1, 2), ZDate.Empty);

				var rule_TRN_ANY_ANY_old = AddNewRule(salesTeam, JobInvoicingConsumerTypes.LocalCartage.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, new ZDate(1995, 1, 1), new ZDate(2000, 1, 1));
				var rule_TRN_ANY_ANY_new = AddNewRule(salesTeam, JobInvoicingConsumerTypes.LocalCartage.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, new ZDate(1999, 1, 1), ZDate.Empty);

				salesTeam.RunPreSaveValidation();

				const string expectedUniqueErrorMessage = "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates.";

				AssertNoError(propertyInfoGetter(rule_ANY_ANY_ANY), expectedUniqueErrorMessage);
				AssertNoError(propertyInfoGetter(rule_SHP_ANY_ANY), expectedUniqueErrorMessage);

				AssertHasError(propertyInfoGetter(rule_CON_ANY_ANYa), expectedUniqueErrorMessage);
				AssertHasError(propertyInfoGetter(rule_CON_ANY_ANYb), expectedUniqueErrorMessage);

				AssertNoError(propertyInfoGetter(rule_BRK_ANY_ANY_old), expectedUniqueErrorMessage);
				AssertNoError(propertyInfoGetter(rule_BRK_ANY_ANY_new), expectedUniqueErrorMessage);

				AssertHasError(propertyInfoGetter(rule_TRN_ANY_ANY_old), expectedUniqueErrorMessage);
				AssertHasError(propertyInfoGetter(rule_TRN_ANY_ANY_new), expectedUniqueErrorMessage);
			}
		}

		static AccGroupCommissionRule AddNewRule(SalesTeam salesTeam, ZString product, ZString service, ZString subModule, ZDate startDate, ZDate endDate)
		{
			var result = salesTeam.CommissionRules.AddNew();
			result.ACM_Product = product;
			result.ACM_Service = service;
			result.ACM_SubModule = subModule;
			result.ACM_StartDate = startDate;
			result.ACM_EndDate = endDate;

			return result;
		}
	}
}
