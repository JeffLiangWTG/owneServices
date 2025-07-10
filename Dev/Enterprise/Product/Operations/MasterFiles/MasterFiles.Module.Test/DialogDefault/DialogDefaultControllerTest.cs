using Enterprise.Core.DialogDefault;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(DialogDefaultController))]
	sealed class DialogDefaultControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public void TestCanDeletePermissions()
		{
			var defaults = Factory.NewWithValidTestData<StmDialogDefault>();

			defaults.SDD_Level = DialogDefaultLevel.Codes.User;

			Env.Security.CanCreateAndModifyGlobalDialogDefaults.IsAllowed = true;

			Assert("If you can modify global defaults, you can modify anything", Controller.GetCheckPointForDelete(defaults).IsAllowed);

			Env.Security.CanCreateAndModifyGlobalDialogDefaults.IsAllowed = false;

			Assert("You should only be allowed to modify your own defaults", !Controller.GetCheckPointForDelete(defaults).IsAllowed);

			defaults.SDD_Owner = EnvProxy.Instance.CurrentUser.PK;

			Assert("You can modify your own defaults", Controller.GetCheckPointForDelete(defaults).IsAllowed);

			defaults.SDD_Level = DialogDefaultLevel.Codes.Global;

			Assert("You can't global defaults without permission", !Controller.GetCheckPointForDelete(defaults).IsAllowed);
		}

		new DialogDefaultController Controller
		{
			get { return (DialogDefaultController)base.Controller; }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DialogDefault;
		}
	}
}
