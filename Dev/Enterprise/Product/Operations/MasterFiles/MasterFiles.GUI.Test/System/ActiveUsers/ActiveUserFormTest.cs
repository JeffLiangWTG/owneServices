using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ActiveUserForm))]
	sealed class ActiveUserFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ActiveUser user = new ActiveUser(Factory);
			user.AU_HeartbeatId = ZGuid.NewZGuid();
			user.AU_GS = GlbStaff.CurrentUser.PK;
			user.ActiveSemaphores.HasChanges = false;
			user.HasChanges = false;
			return new ActiveUserForm(user);
		}
	}
}
