using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class EDIMenuStubTest : TestCaseForAttachGUI
	{
		public void TestDeclarationDeactivatedMenuItemClick()
		{
			using (var menu = new EDIMenuStub())
			{
				var dec = Factory.New<BaseJobDeclaration>();
				menu.Declaration = dec;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var menuItem = menu.MenuItems.FindByText("Declaration deactivated, to reactivate go to Actions->Make Active");
				AssertNotNull("menuItem", menuItem);
			}
		}
	}
}
