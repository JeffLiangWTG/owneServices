using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccWithholding))]
	sealed class TestAccWithholding : EnterpriseBusinessObjectTestCase
	{
		public void TestHumenReadableNameCore()
		{
			var withholdingTaxID = Factory.NewWithValidTestData<AccWithholding>();
			withholdingTaxID.AW_Code = "WTH";
			withholdingTaxID.AW_Description = "Testing, One, Two, Three";

			AssertEquals("Withholding Tax ID - WTH - Testing, One, Two, Three", withholdingTaxID.HumanReadableName);
		}

		public void TestAW_Code_ReadOnly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "User1";

			Factory.Save();

			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			var departmentPK = EnvProxy.Instance.CurrentDepartment.PK;

			var withholdingTaxID = Factory.NewWithValidTestData<AccWithholding>();
			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, branchPK, departmentPK))
			{
				Assert("AW_Code is readonly for regular user", withholdingTaxID.AW_CodeInfo.ReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("AW_Code is not readonly for support user", !withholdingTaxID.AW_CodeInfo.ReadOnly);
			}
		}

		public void TestAW_Rate_ReadOnly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "User1";

			Factory.Save();

			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			var departmentPK = EnvProxy.Instance.CurrentDepartment.PK;

			var withholdingTaxID = Factory.NewWithValidTestData<AccWithholding>();
			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, branchPK, departmentPK))
			{
				Assert("AW_Rate is readonly for regular user", withholdingTaxID.AW_RateInfo.ReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("AW_Rate is not readonly for support user", !withholdingTaxID.AW_RateInfo.ReadOnly);
			}
		}

		public void TestNoStmAlogs()
		{
			var withholdingTaxID = Factory.NewWithValidTestData<AccWithholding>();
			withholdingTaxID.AW_Code = "WTH";
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, withholdingTaxID.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				withholdingTaxID.AW_Code = "UUU";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				withholdingTaxID.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}
	}
}
