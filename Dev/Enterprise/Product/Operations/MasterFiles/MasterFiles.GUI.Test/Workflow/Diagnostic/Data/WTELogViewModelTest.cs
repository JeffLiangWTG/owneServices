using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.GUI.Workflow.Test
{
	[TestedType(typeof(WTELogViewModel))]
	sealed class WTELogViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTriggeredBranch_DifferentTriggerContextCodes()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "TST";
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "AAA";
			branch1.GB_GC = company.PK;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "ABB";
			branch2.GB_GC = company.PK;

			var user = Factory.New<GlbStaff>();
			user.GS_Code = "GOD";

			Factory.Save();

			var trigger = Factory.New<ProcessTask>();
			trigger.P9_GC = company.PK;
			trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			trigger.TriggerConditions.TriggerBranch = branch1.PK;

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.WorkflowTriggerEventCode;
				log.SL_GS_NKUser = user.GS_Code;
				log.SL_GB_NKBranch = branch1.GB_Code;
			}
			var wtelog = new WTELogViewModel(trigger, log);
			AssertEquals(log.SL_GB_NKBranch, wtelog.SL_GB_NKBranch);
			AssertEquals(branch1.GB_Code, wtelog.SL_TriggeredBranch);

			trigger.TriggerConditions.TriggerBranch = branch2.PK;
			AssertEquals(branch2.GB_Code, wtelog.SL_TriggeredBranch);

			trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Event;
			AssertEquals(branch1.GB_Code, wtelog.SL_TriggeredBranch);
		}

		public void TestTriggerFiredBranchDiffersFromLogBranchForDisplay()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "TST";
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "AAA";
			branch1.GB_GC = company.PK;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "ABB";
			branch2.GB_GC = company.PK;

			var user = Factory.New<GlbStaff>();
			user.GS_Code = "GOD";

			Factory.Save();

			var trigger = Factory.New<ProcessTask>();
			trigger.P9_GC = company.PK;

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.WorkflowTriggerEventCode;
				log.SL_GS_NKUser = user.GS_Code;
				log.SL_GB_NKBranch = branch2.GB_Code;
			}
			var wtelog = new WTELogViewModel(trigger, log);
			AssertEquals(log.SL_GB_NKBranch, wtelog.SL_GB_NKBranch);
			AssertEquals(branch1.GB_Code, wtelog.SL_TriggeredBranch);
		}

		public void TestCancelledLogsStillAppear()
		{
			var shipment = Factory.New<DummyWithWorkflow>();
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			Factory.Save();

			var log = shipment.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			log.Cancel();
			Factory.Save();

			var wteLog = trigger.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).FirstOrDefault();
			var viewModel = new WTELogViewModel(trigger, wteLog);
			AssertEquals("Find even cancelled Logs.", 1, viewModel.SourceLogs.Count);
		}

		public void TestSourceLogFromCompanySpecificObject()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var jobHeader = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			Factory.Save();

			//jobHeader is dependant on company, different companies can have different headers so company A may not see company B's jobHeader events on the job
			jobHeader.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var wteLog = trigger.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).FirstOrDefault();
			AssertNotNull("Precondition", wteLog);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			branch.GB_GC = company.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var viewModel = new WTELogViewModel(trigger, wteLog);
				AssertEquals("This user can access the branch/department of the source log", 1, viewModel.SourceLogs.Count);
				AssertEquals(ZString.Empty, viewModel.HiddenSourceLogDisplayText);
			}

			var security1 = new UserLoginController().GetSecurityForUser(staff.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
			var loginSecurity1 = Factory.New<GlbSecurity>();
			loginSecurity1.GU_GB = Env.CurrentBranch.PK;
			loginSecurity1.GU_GE = Env.CurrentDepartment.PK;
			loginSecurity1.GU_GS = staff.PK;
			loginSecurity1.GU_SecurityRight = security1.Login.Code;
			loginSecurity1.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			var sourceCompany = Env.CurrentCompany.Code;
			var sourceBranch = Env.CurrentBranch.Code;
			var sourceDepatment = Env.CurrentDepartment.Code;
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var viewModel = new WTELogViewModel(trigger, wteLog);
				AssertEquals("This user cannot login to the branch/department of the source log", 0, viewModel.SourceLogs.Count);
				AssertEquals($"This source log is available when logged into company {sourceCompany}, branch {sourceBranch} and department {sourceDepatment}", viewModel.HiddenSourceLogDisplayText);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var job = new BusinessObjectFactory().New<DummyWithWorkflow>();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			return new WTELogViewModel(trigger, null);
		}
	}
}
