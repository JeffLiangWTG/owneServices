using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GUI.PlugIn.Testing
{
	public class BaseShipmentAndBrokerageCommonTest : TestCaseWithFactory
	{
		public virtual void TestIsSupervisorApproved()
		{
			bool oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
			bool oldIsController = GlbStaff.CurrentUser.GS_IsController;
			bool oldSupervisorOverridens = Env.Security.SupervisorOverrides.IsAllowed;
			bool oldAllowMessageErrors = Env.Security.AllowMessageErrors.IsAllowed;
			var oldMergeByDefaultAllowed = Env.Security.MergeByDefault.IsAllowed;
			try
			{
				Env.Security.SupervisorOverrides.IsAllowed = false;
				Env.Security.AllowMessageErrors.IsAllowed = false;
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
				GlbStaff.CurrentUser.GS_IsController = false;
				Env.Security.MergeByDefault.IsAllowed = false;
				BaseJobDeclaration decl = Factory.New<BaseJobDeclaration>();
				GlbStaff staff = Factory.New<GlbStaff>();
				staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				staff.GS_IsActive = true;
				GlbSecurity se = Factory.New<GlbSecurity>();
				se.GU_SecurityRight = Env.Security.MergeByDefault.Code;
				se.GU_SecurityItemIsAllowed = true;
				se.GU_GS = staff.PK;
				Factory.Save();
				var common = new BaseShipmentAndBrokerageCommon(decl);
				Assert(common.IsSupervisorApproved() == ContinueWithSave.Yes);
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				decl.JE_MergeBy = "ZZZ";
				common = new BaseShipmentAndBrokerageCommon(decl);
				Assert(common.IsSupervisorApproved() == ContinueWithSave.No);
			}
			finally
			{
				Env.Security.MergeByDefault.IsAllowed = oldMergeByDefaultAllowed;
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
				GlbStaff.CurrentUser.GS_IsController = oldIsController;
				Env.Security.SupervisorOverrides.IsAllowed = oldSupervisorOverridens;
				Env.Security.AllowMessageErrors.IsAllowed = oldAllowMessageErrors;
			}
		}
	}
}
