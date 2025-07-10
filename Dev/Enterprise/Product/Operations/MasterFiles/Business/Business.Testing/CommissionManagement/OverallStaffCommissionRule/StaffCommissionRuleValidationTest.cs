using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StaffCommissionRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckProduct()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.Product = CommissionRuleLookups.AnyProductsItem.Code;
			AssertMandatoryValidationError(rule.ProductInfo, false);
			AssertListValidationInvalidCodeError(rule.ProductInfo, false);

			rule.Product = "";
			AssertMandatoryValidationError(rule.ProductInfo, true);
			AssertListValidationInvalidCodeError(rule.ProductInfo, false);

			rule.Product = "XXX";
			AssertMandatoryValidationError(rule.ProductInfo, false);
			AssertListValidationInvalidCodeError(rule.ProductInfo, true);
		}

		public void TestCheckService()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.Service = CommissionRuleLookups.AnyServicesItem.Code;
			AssertMandatoryValidationError(rule.ServiceInfo, false);
			AssertListValidationInvalidCodeError(rule.ServiceInfo, false);

			rule.Service = "";
			AssertMandatoryValidationError(rule.ServiceInfo, true);
			AssertListValidationInvalidCodeError(rule.ServiceInfo, false);

			rule.Service = "XXX";
			AssertMandatoryValidationError(rule.ServiceInfo, false);
			AssertListValidationInvalidCodeError(rule.ServiceInfo, true);
		}

		public void TestCheckSubModule()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.SubModule = CommissionRuleLookups.AnySubModulesCode;
			AssertMandatoryValidationError(rule.SubModuleInfo, false);
			AssertListValidationInvalidCodeError(rule.SubModuleInfo, false);

			rule.SubModule = "";
			AssertMandatoryValidationError(rule.SubModuleInfo, true);
			AssertListValidationInvalidCodeError(rule.SubModuleInfo, false);

			rule.SubModule = "XXX";
			AssertMandatoryValidationError(rule.SubModuleInfo, false);
			AssertListValidationInvalidCodeError(rule.SubModuleInfo, true);
		}

		public void TestCheckMode()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.Mode = OrgCommissionAgreementItemLookups.AllModesCode;
			AssertMandatoryValidationError(rule.ModeInfo, false);
			AssertListValidationInvalidCodeError(rule.ModeInfo, false);

			rule.Mode = "";
			AssertMandatoryValidationError(rule.ModeInfo, false);
			AssertListValidationInvalidCodeError(rule.ModeInfo, false);

			rule.Mode = "XXX";
			AssertMandatoryValidationError(rule.ModeInfo, false);
			AssertListValidationInvalidCodeError(rule.ModeInfo, true);

			rule.Product = "SHP";
			rule.Mode = "";
			AssertMandatoryValidationError(rule.ModeInfo, true);
			AssertListValidationInvalidCodeError(rule.ModeInfo, false);
		}

		public void TestCheckOrigin()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.Origin = "AUSYD";
			AssertListValidationInvalidCodeError(rule.OriginInfo, false);

			rule.Origin = "";
			AssertListValidationInvalidCodeError(rule.OriginInfo, false);

			rule.Origin = "XXXXX";
			AssertListValidationInvalidCodeError(rule.OriginInfo, true);
		}

		public void TestCheckDestination()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.Destination = "AUSYD";
			AssertListValidationInvalidCodeError(rule.DestinationInfo, false);

			rule.Destination = "";
			AssertListValidationInvalidCodeError(rule.DestinationInfo, false);

			rule.Destination = "XXXXX";
			AssertListValidationInvalidCodeError(rule.DestinationInfo, true);
		}

		public void TestCheckCommissionBasis()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.CommissionBasis = CommissionBasisType.Codes.PRF;
			AssertMandatoryValidationError(rule.CommissionBasisInfo, false);
			AssertListValidationInvalidCodeError(rule.CommissionBasisInfo, false);

			rule.CommissionBasis = "";
			AssertMandatoryValidationError(rule.CommissionBasisInfo, true);
			AssertListValidationInvalidCodeError(rule.CommissionBasisInfo, false);

			rule.CommissionBasis = "XXX";
			AssertMandatoryValidationError(rule.CommissionBasisInfo, false);
			AssertListValidationInvalidCodeError(rule.CommissionBasisInfo, true);
		}

		public void TestCheckCommissionTriggerType()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			AssertMandatoryValidationError(rule.CommissionTriggerTypeInfo, false);
			AssertListValidationInvalidCodeError(rule.CommissionTriggerTypeInfo, false);

			rule.CommissionTriggerType = CommissionTriggerTypes.Codes.EarliestRevRecog;
			AssertMandatoryValidationError(rule.CommissionTriggerTypeInfo, false);
			AssertListValidationInvalidCodeError(rule.CommissionTriggerTypeInfo, false);

			rule.CommissionTriggerType = "";
			AssertMandatoryValidationError(rule.CommissionTriggerTypeInfo, true);
			AssertListValidationInvalidCodeError(rule.CommissionTriggerTypeInfo, false);

			rule.CommissionTriggerType = "XXX";
			AssertMandatoryValidationError(rule.CommissionTriggerTypeInfo, false);
			AssertListValidationInvalidCodeError(rule.CommissionTriggerTypeInfo, true);
		}

		[TestDate(2000, 1, 1)]
		public void TestCheckStartDate()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.EndDate = new ZDateTime(2000, 1, 1);
			rule.StartDate = new ZDateTime(1999, 1, 1);
			AssertNoErrors(rule.StartDateInfo);

			rule.StartDate = ZDateTime.Empty;
			AssertNoErrors(rule.StartDateInfo);

			rule.StartDate = new ZDateTime(2001, 1, 1);
			AssertHasError(rule.StartDateInfo, "The 'Start Date' must be before the 'End Date'.");

			rule.EndDate = ZDateTime.Empty;
			rule.Validation.ValidateStartDate();
			AssertNoErrors(rule.StartDateInfo);
		}

		[TestDate(2000, 1, 1)]
		public void TestCheckEndDate()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.StartDate = new ZDateTime(2000, 1, 1);
			rule.EndDate = new ZDateTime(2001, 1, 1);
			AssertNoErrors(rule.EndDateInfo);

			rule.EndDate = ZDateTime.Empty;
			AssertNoErrors(rule.EndDateInfo);

			rule.EndDate = new ZDateTime(1999, 1, 1);
			AssertHasError(rule.EndDateInfo, "The 'End Date' must be after the 'Start Date'.");

			rule.StartDate = ZDateTime.Empty;
			rule.Validation.ValidateEndDate();
			AssertNoErrors(rule.EndDateInfo);
		}

		public void TestCheckEndDate_Unique()
		{
			TestChecksUnique((rule) => rule.EndDateInfo);
		}

		public void TestCheckCompanyPk_Unique()
		{
			TestChecksUnique((rule) => rule.CompanyPkInfo);
		}

		public void TestCheckGroupPk_Unique()
		{
			TestChecksUnique((rule) => rule.GroupPkInfo);
		}

		public void TestCheckProduct_Unique()
		{
			TestChecksUnique((rule) => rule.ProductInfo);
		}

		public void TestCheckService_Unique()
		{
			TestChecksUnique((rule) => rule.ServiceInfo);
		}

		public void TestCheckStartDate_Unique()
		{
			TestChecksUnique((rule) => rule.StartDateInfo);
		}

		public void TestCheckSubModule_Unique()
		{
			TestChecksUnique((rule) => rule.SubModuleInfo);
		}

		public void TestCheckMode_Unique()
		{
			TestChecksUnique((rule) => rule.ModeInfo);
		}

		public void TestCheckOrigin_Unique()
		{
			TestChecksUnique((rule) => rule.OriginInfo);
		}

		public void TestCheckDestination_Unique()
		{
			TestChecksUnique((rule) => rule.DestinationInfo);
		}

		void TestChecksUnique(Func<OverallStaffCommissionRule, ZPropertyInfo> propertyInfoGetter)
		{
			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var otherCompanyA = Factory.NewWithValidTestData<GlbCompany>();
				var otherCompanyB = Factory.NewWithValidTestData<GlbCompany>();
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var groupXXX = Factory.NewWithValidTestData<SalesTeam>();
				groupXXX.GG_Code = "XXX";
				var groupYYY = Factory.NewWithValidTestData<SalesTeam>();
				groupYYY.GG_Code = "YYY";

				var rule_XXX_ANY_ANY_ANY = AddNewRule(staff, groupXXX, CommissionRuleLookups.AnyProductsCode, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", ZDateTime.Empty, ZDateTime.Empty);
				var rule_XXX_ANY_ANY_ANY_ANY_AUSYD = AddNewRule(staff, groupXXX, CommissionRuleLookups.AnyProductsCode, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "AUSYD", "", ZDateTime.Empty, ZDateTime.Empty);
				var rule_XXX_ANY_ANY_ANY_ANY_AUSYD_GBLON = AddNewRule(staff, groupXXX, CommissionRuleLookups.AnyProductsCode, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "AUSYD", "GBLON", ZDateTime.Empty, ZDateTime.Empty);
				var rule_XXX_SHP_ANY_ANY = AddNewRule(staff, groupXXX, JobInvoicingConsumerTypes.Shipment.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", ZDateTime.Empty, ZDateTime.Empty);
				var rule_XXX_SHP_ANY_ANY_AIR = AddNewRule(staff, groupXXX, JobInvoicingConsumerTypes.Shipment.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, "AIR", "", "", ZDateTime.Empty, ZDateTime.Empty);

				var rule_XXX_CON_ANY_ANYa = AddNewRule(staff, groupXXX, JobInvoicingConsumerTypes.Consol.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", ZDateTime.Empty, ZDateTime.Empty);
				var rule_XXX_CON_ANY_ANYb = AddNewRule(staff, groupXXX, JobInvoicingConsumerTypes.Consol.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", ZDateTime.Empty, ZDateTime.Empty);

				var rule_XXX_SHP_ANY_ANY_ROAa = AddNewRule(staff, groupXXX, JobInvoicingConsumerTypes.Shipment.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, "ROA", "", "", ZDateTime.Empty, ZDateTime.Empty);
				var rule_XXX_SHP_ANY_ANY_ROAb = AddNewRule(staff, groupXXX, JobInvoicingConsumerTypes.Shipment.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, "ROA", "", "", ZDateTime.Empty, ZDateTime.Empty);

				var rule_YYY_ANY_ANY_ANY = AddNewRule(staff, groupYYY, CommissionRuleLookups.AnyProductsCode, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", ZDateTime.Empty, ZDateTime.Empty);

				var rule_YYY_BRK_ANY_ANY_old = AddNewRule(staff, groupYYY, JobInvoicingConsumerTypes.Brokerage.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", ZDateTime.Empty, new ZDateTime(2000, 1, 1));
				var rule_YYY_BRK_ANY_ANY_new = AddNewRule(staff, groupYYY, JobInvoicingConsumerTypes.Brokerage.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", new ZDateTime(2000, 1, 2), ZDateTime.Empty);

				var rule_YYY_TRN_ANY_ANY_old = AddNewRule(staff, groupYYY, JobInvoicingConsumerTypes.LocalCartage.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", new ZDateTime(1995, 1, 1), new ZDateTime(2000, 1, 1));
				var rule_YYY_TRN_ANY_ANY_new = AddNewRule(staff, groupYYY, JobInvoicingConsumerTypes.LocalCartage.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", new ZDateTime(1999, 1, 1), ZDateTime.Empty);

				var rule_NULL_WKI_ANY_ANY_global = AddNewRule(staff, null, JobInvoicingConsumerTypes.WorkItem.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", ZDateTime.Empty, ZDateTime.Empty, null);
				var rule_NULL_WKI_ANY_ANY_companyA1 = AddNewRule(staff, null, JobInvoicingConsumerTypes.WorkItem.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", ZDateTime.Empty, ZDateTime.Empty, otherCompanyA);
				var rule_NULL_WKI_ANY_ANY_companyA2 = AddNewRule(staff, null, JobInvoicingConsumerTypes.WorkItem.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", ZDateTime.Empty, ZDateTime.Empty, otherCompanyA);
				var rule_NULL_WKI_ANY_ANY_companyB = AddNewRule(staff, null, JobInvoicingConsumerTypes.WorkItem.Code, CommissionRuleLookups.AnyServicesCode, CommissionRuleLookups.AnySubModulesCode, OrgCommissionAgreementItemLookups.AllModesCode, "", "", ZDateTime.Empty, ZDateTime.Empty, otherCompanyB);

				rule_XXX_CON_ANY_ANYa.Staff.OverallCommissionRulesView.LoadRules();
				foreach (OverallStaffCommissionRule rule in staff.OverallCommissionRules.ToArray())
				{
					rule.Validation.ValidateAll();
				}

				AssertNoError(propertyInfoGetter(rule_XXX_ANY_ANY_ANY), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (XXX).");

				AssertNoError(propertyInfoGetter(rule_XXX_ANY_ANY_ANY_ANY_AUSYD), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (XXX).");
				AssertNoError(propertyInfoGetter(rule_XXX_ANY_ANY_ANY_ANY_AUSYD_GBLON), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (XXX).");
				AssertNoError(propertyInfoGetter(rule_XXX_SHP_ANY_ANY), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (XXX).");
				AssertNoError(propertyInfoGetter(rule_XXX_SHP_ANY_ANY_AIR), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (XXX).");

				AssertHasError(propertyInfoGetter(rule_XXX_CON_ANY_ANYa), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (XXX).");
				AssertHasError(propertyInfoGetter(rule_XXX_CON_ANY_ANYb), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (XXX).");

				AssertHasError(propertyInfoGetter(rule_XXX_SHP_ANY_ANY_ROAa), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (XXX).");
				AssertHasError(propertyInfoGetter(rule_XXX_SHP_ANY_ANY_ROAb), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (XXX).");

				AssertNoError(propertyInfoGetter(rule_YYY_ANY_ANY_ANY), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (YYY).");

				AssertNoError(propertyInfoGetter(rule_YYY_BRK_ANY_ANY_old), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (YYY).");
				AssertNoError(propertyInfoGetter(rule_YYY_BRK_ANY_ANY_new), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (YYY).");

				AssertHasError(propertyInfoGetter(rule_YYY_TRN_ANY_ANY_old), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (YYY).");
				AssertHasError(propertyInfoGetter(rule_YYY_TRN_ANY_ANY_new), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates for Sales Team (YYY).");

				AssertNoError(propertyInfoGetter(rule_NULL_WKI_ANY_ANY_global), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates.");
				AssertHasError(propertyInfoGetter(rule_NULL_WKI_ANY_ANY_companyA1), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates.");
				AssertHasError(propertyInfoGetter(rule_NULL_WKI_ANY_ANY_companyA2), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates.");
				AssertNoError(propertyInfoGetter(rule_NULL_WKI_ANY_ANY_companyB), "Another rule with the same Product, Service and Sub-Module has overlapping Start/End Dates.");
			}
		}

		public void TestCheckHasAtLeastOneRate()
		{
			var staff = Factory.New<GlbStaff>();
			var rule = new OverallStaffCommissionRule(staff.CommissionRules.AddNew());

			rule.Validation.ValidateAll();
			AssertHasRowError(rule, "Please enter at least one commission rate.");

			rule.Rates.AddNew();
			rule.Validation.ValidateAll();
			AssertNoRowError(rule, "Please enter at least one commission rate.");
		}

		static OverallStaffCommissionRule AddNewRule(GlbStaff staff, SalesTeam salesTeam, ZString product, ZString service, ZString subModule, ZString mode, ZString origin, ZString destination, ZDateTime startDate, ZDateTime endDate, GlbCompany company = null)
		{
			var result = staff.OverallCommissionRules.AddNew();
			result.GroupPk = salesTeam != null ? salesTeam.PK : ZGuid.Empty;
			result.Product = product;
			result.Service = service;
			result.SubModule = subModule;
			result.Mode = mode;
			result.Origin = origin;
			result.Destination = destination;
			result.StartDate = startDate;
			result.EndDate = endDate;

			if (company != null)
			{
				result.CompanyPk = company.PK;
			}

			return result;
		}
	}
}
