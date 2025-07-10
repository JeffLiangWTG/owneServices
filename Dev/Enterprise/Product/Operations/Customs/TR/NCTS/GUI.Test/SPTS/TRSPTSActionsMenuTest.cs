using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	class TRSPTSActionsMenuTest : TestCaseWithFactory
	{
		public void TestManualRegistrationNoEntry()
		{
			Env.Security.SPTSModifyRegistrationNumbers.IsAllowed = true;
			var header = Factory.NewWithValidTestData<SPTSHeader>();
			header.Factory.Save();

			using (var menu = new TRSPTSMenu(header))
			using (var form = new SPTSHeaderForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.ShowPopupMenu();
				var manualRegNoEntryMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("Manual Registration No Entry");
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals("Do you want to enter Registration No manually?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			using (var form = new SPTSHeaderForm(header))
			{
				var manualRegNoEntryMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("Manual Registration No Entry");
				header.RegistrationDate = ZDateTime.Now;
				header.RegistrationNumber = "TestRegno001";
				header.Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object formOrDialog)
				{
					var be = (ManualRegistrationNoEntry)((ZForm)formOrDialog).BusinessEntity;
					be.RegistrationNumber = "TestRegno222";
					be.RegistrationDate = ZDateTime.BrettsBirthday;
				});
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals(ZDateTime.BrettsBirthday, header.RegistrationDate);
				AssertEquals("TestRegno222", header.RegistrationNumber);

				var logEntry = header.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_Reference.StartsWith("Manual Change Registration No"));
				AssertNotNull(logEntry);
			}
		}

		public void TestModifyRegistrationNumbersSecurityPoint()
		{
			var header = Factory.NewWithValidTestData<SPTSHeader>();
			header.Factory.Save();

			using (var menu = new TRSPTSMenu(header))
			using (var form = new SPTSHeaderForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.ShowPopupMenu();
				Env.Security.SPTSModifyRegistrationNumbers.IsAllowed = true;
				var manualRegNoEntryMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("Manual Registration No Entry");
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals("Do you want to enter Registration No manually?", UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.SPTSModifyRegistrationNumbers.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> NCTS -> SPTS Simplified Procedure Transit System -> Manual Registration No Entry", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
