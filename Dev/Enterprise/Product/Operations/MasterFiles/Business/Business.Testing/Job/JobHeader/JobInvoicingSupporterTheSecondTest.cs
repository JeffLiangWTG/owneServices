using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobInvoicingSupporterTheSecondTest : TestCaseWithFactory
	{
		public void TestJobRateSecurityTest()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var parent = Factory.New<DummyJobHeaderParent>();

				var jobHeader = new JobHeader.Loader(parent).TryLoadOrCreate();
				jobHeader.LocalChargesPK = setupResult.AllowedOrg.PK;
				Factory.Save();

				var jobInvoicingSupporter = new JobInvoicingSupporterWithJob(parent);

				AssertNull(jobHeader.DeniedRateSecurityCheckPoint);
				AssertEquals(Env.Security.None, jobInvoicingSupporter.JobInvoicingSecurity);
				AssertEquals(Env.Security.None, jobInvoicingSupporter.EditSecurityCheckpoint);
				AssertEquals(Env.Security.None, jobInvoicingSupporter.AuditSecurity);

				jobHeader.LocalChargesPK = setupResult.DeniedOrg.PK;
				Factory.Save();

				AssertEquals(setupResult.DeniedSecurity, jobHeader.DeniedRateSecurityCheckPoint);
				AssertEquals(setupResult.DeniedSecurity, jobInvoicingSupporter.JobInvoicingSecurity);
				AssertEquals(setupResult.DeniedSecurity, jobInvoicingSupporter.EditSecurityCheckpoint);
				AssertEquals(setupResult.DeniedSecurity, jobInvoicingSupporter.AuditSecurity);
			}
		}
	}
}
