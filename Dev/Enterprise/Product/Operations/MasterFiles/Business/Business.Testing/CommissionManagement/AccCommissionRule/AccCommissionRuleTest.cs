using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCommissionRule))]
	sealed class AccCommissionRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var rule = Factory.New<AccCommissionRule>();
				AssertEquals("", rule.ACM_Service);
				AssertEquals("", rule.ACM_SubModule);
			}

			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(false))
			{
				var rule = Factory.New<AccCommissionRule>();
				AssertEquals(CommissionRuleLookups.AnyServicesCode, rule.ACM_Service);
				AssertEquals(CommissionRuleLookups.AnySubModulesCode, rule.ACM_SubModule);
			}
		}

		public void TestACM_GC()
		{
			var company = Factory.New<GlbCompany>();
			var salesTeam = Factory.New<SalesTeam>();
			salesTeam.GG_GC = ZGuid.Empty;
			var rule = Factory.New<AccCommissionRule>();
			rule.ACM_GG = ZGuid.Empty;

			rule.ACM_GC = company.PK;
			AssertEquals(company.PK, rule.ACM_GC);

			rule.ACM_GG = salesTeam.PK;
			AssertEquals("Company should be inherited from sales team", ZGuid.Empty, rule.ACM_GC);

			salesTeam.GG_GC = company.PK;
			AssertEquals("Company should be inherited from sales team", company.PK, rule.ACM_GC);

			AssertExceptionThrown(typeof(InvalidOperationException), "ACM_GC can not be set on the rule level while it is linked to a group. It is inherited from the linked group.", () =>
			{
				rule.ACM_GC = Factory.New<GlbCompany>().PK;
			});
		}

		public void TestACM_GC_ReadOnly()
		{
			var rule = Factory.New<AccCommissionRule>();

			rule.ACM_GG = ZGuid.Empty;
			AssertEquals(false, rule.ACM_GCInfo.ReadOnly);

			rule.ACM_GG = Factory.New<SalesTeam>().PK;
			AssertEquals(true, rule.ACM_GCInfo.ReadOnly);
		}

		public void TestPropertyMaxLength()
		{
			AssertEquals(OrgCommissionAgreement.Schema.CA0_CommissionBasisMaxLength, AccCommissionRule.Schema.ACM_CommissionBasisMaxLength);
			AssertEquals(OrgCommissionAgreement.Schema.CA0_CommissionTriggerTypeMaxLength, AccCommissionRule.Schema.ACM_CommissionTriggerTypeMaxLength);
			AssertEquals(OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, AccCommissionRule.Schema.ACM_ProductMaxLength);
			AssertEquals(OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, AccCommissionRule.Schema.ACM_ServiceMaxLength);
			AssertEquals(OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, AccCommissionRule.Schema.ACM_SubModuleMaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObjectForDeleteTest(factory);
			((AccCommissionRule)result).ACM_GS_NKStaff = factory.NewWithValidTestData<GlbStaff>().GS_Code;
			return result;
		}
	}
}
