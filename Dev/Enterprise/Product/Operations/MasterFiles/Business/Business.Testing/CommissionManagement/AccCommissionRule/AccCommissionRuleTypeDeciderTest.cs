using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCommissionRuleTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new AccCommissionRuleTypeDecider();
			AssertEquals(typeof(AccCommissionRule), typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForLoad()
		{
			var salesTeam = Factory.NewWithValidTestData<SalesTeam>();
			var staff = salesTeam.Staff.AddNew();
			staff.GS_Code = "ADL";
			Factory.Save();

			var groupRule = Factory.NewWithValidTestData<AccCommissionRule>();
			groupRule.ACM_GG = salesTeam.PK;
			groupRule.ACM_GS_NKStaff = ZString.Empty;

			var staffGroupOverrideRule = Factory.NewWithValidTestData<AccCommissionRule>();
			staffGroupOverrideRule.ACM_GS_NKStaff = staff.GS_Code;
			staffGroupOverrideRule.ACM_GG = salesTeam.PK;

			var staffGlobalOverrideRule = Factory.NewWithValidTestData<AccCommissionRule>();
			staffGlobalOverrideRule.ACM_GS_NKStaff = staff.GS_Code;
			staffGlobalOverrideRule.ACM_GG = ZGuid.Empty;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			AssertType(typeof(AccGroupCommissionRule), otherFactory.Load<AccCommissionRule>(groupRule.PK));
			AssertType(typeof(AccStaffCommissionRule), otherFactory.Load<AccCommissionRule>(staffGroupOverrideRule.PK));
			AssertType(typeof(AccStaffCommissionRule), otherFactory.Load<AccCommissionRule>(staffGlobalOverrideRule.PK));
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new AccCommissionRuleTypeDecider();
			AssertEquals(typeof(AccCommissionRule), typeDecider.GetTypeForNew());
		}
	}
}
