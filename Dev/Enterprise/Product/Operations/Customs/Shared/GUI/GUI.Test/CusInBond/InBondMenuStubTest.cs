using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InBondMenuStubTest : TestCaseForAttachGUI
	{
		public void TestInBondMenuStub()
		{
			using (var menu = new InBondMenuStub("NAME", Environment.Env.Security.USInBondMessaging))
			{
				var menuItem = menu.MenuItems.FindByText("The menu is disabled because you don't have the appropriate security rights: " + Environment.Env.Security.USInBondMessaging.DisplayTextPathToSecurityRight);
				AssertNotNull("NAME", menuItem);
			}
		}
	}
}
