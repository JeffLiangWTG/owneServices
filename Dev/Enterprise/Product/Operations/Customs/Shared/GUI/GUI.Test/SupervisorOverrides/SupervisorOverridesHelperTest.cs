using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class SupervisorOverridesHelperTest : TestCaseWithFactory
	{
		public void TestIsSupervisorApproved()
		{
			bool oldIsController = GlbStaff.CurrentUser.GS_IsController;
			bool oldAllowMessageErrors = Env.Security.AllowMessageErrors.IsAllowed;
			try
			{
				GlbStaff.CurrentUser.GS_IsController = false;
				Env.Security.AllowMessageErrors.IsAllowed = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				SupervisorOverrides.AddMessageLogForTesting("1", "blah blah", false);
				Assert(SupervisorOverridesHelper.IsSupervisorApproved(SupervisorOverrides, Declaration.Logs));
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				Assert(!SupervisorOverridesHelper.IsSupervisorApproved(SupervisorOverrides, Declaration.Logs));
				SupervisorOverrides.ClearMessageLogs();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				Assert(SupervisorOverridesHelper.IsSupervisorApproved(SupervisorOverrides, Declaration.Logs));
				Assert(Declaration.Logs.Find(x => x.SL_SE_NKEvent.Equals("ATH") && x.SL_Reference.StartsWith("blah blah")).Any());
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				Assert(!SupervisorOverridesHelper.IsSupervisorApproved(SupervisorOverrides, Declaration.Logs));
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldIsController;
				Env.Security.AllowMessageErrors.IsAllowed = oldAllowMessageErrors;
			}
		}

		public void TestSupervisorDialogPopsup()
		{
			AddSecurityRight(Env.Security.SupervisorOverrides.Code, false);
			bool oldIsController = GlbStaff.CurrentUser.GS_IsController;
			bool oldIsOperational = GlbStaff.CurrentUser.GS_IsOperational;
			try
			{
				GlbStaff.CurrentUser.GS_IsController = false;
				GlbStaff.CurrentUser.GS_IsOperational = true;
				SupervisorOverrides.AddMessageLogForTesting("2", "Ola~", false);
				Assert(!SupervisorOverridesHelper.IsSupervisorApproved(SupervisorOverrides, Declaration.Logs));
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("SupervisorOverridesForm", ZFormModaliser.LastFormShownDialogForTest.GetType().Name);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldIsController;
				GlbStaff.CurrentUser.GS_IsOperational = oldIsOperational;
			}
		}

		public void TestAuthorisedActionLogs()
		{
			AddSecurityRight(Env.Security.SupervisorOverrides.Code, false);
			bool oldIsController = GlbStaff.CurrentUser.GS_IsController;
			bool oldIsOperational = GlbStaff.CurrentUser.GS_IsOperational;
			try
			{
				GlbStaff.CurrentUser.GS_IsController = false;
				GlbStaff.CurrentUser.GS_IsOperational = true;
				SupervisorOverrides.AddMessageLogForTesting("2", "Ola~", true);
				Assert(SupervisorOverridesHelper.IsSupervisorApproved(SupervisorOverrides, Declaration.Logs));
				Assert(Declaration.Logs.Find(x => x.SL_SE_NKEvent.Equals("ATH") && x.SL_Reference.StartsWith("Ola~")).Any());
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldIsController;
				GlbStaff.CurrentUser.GS_IsOperational = oldIsOperational;
			}
		}

		SupervisorOverridesForTesting supervisorOverrides;
		SupervisorOverridesForTesting SupervisorOverrides
		{
			get
			{
				if (supervisorOverrides == null)
				{
					var mock = new Mock<SupervisorOverridesForTesting>(new object[] { Declaration, SupervisorOverridesContext.Unknown });
					supervisorOverrides = mock.Object;
				}
				return supervisorOverrides;
			}
		}

		BaseJobDeclaration declaration;
		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}

				return declaration;
			}
		}

		void AddSecurityRight(ZString context, bool allowed)
		{
			var se = Factory.New<GlbSecurity>();
			se.GU_SecurityRight = context;
			se.GU_SecurityItemIsAllowed = allowed;
			se.GU_GS = GlbStaff.CurrentUser.PK;
			se.GU_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
		}
	}
}
