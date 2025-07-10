using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	public class CustomsMessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestInterpretedMessageTextBoxCharacterCasing()
		{
			using (var userControl = new CustomsMessagesTabUserControl())
			{
				var interpretedMessageTextBox = userControl.FindSingle<ZTextBox>("InterpretedMessageTextBox");
				AssertEquals("UserControl.InterpretedMessageTextBox.CharacterCasing", CharacterCasing.Normal, interpretedMessageTextBox.CharacterCasing);
			}
		}

		public void TestOnReprocessMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(staff.PK.ToGuid(), EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var frm = new ZForm())
			using (var ctr = new CustomsMessagesTabUserControl())
			{
				frm.Controls.Add(ctr);
				frm.Show();
				Application.DoEvents();
				var grid = ctr.FindSingleOrDefault<Messaging.GUI.MessageZGrid>("MessagesGrid");
				var menu = grid.ContextMenu.MenuItems.FindByText("Reprocess Message");
				AssertNull("The menu only be visible when the user is IsCurrentUserLocalAdminForThisStaff or IsSupportUser.", menu);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var frm = new ZForm())
			using (var ctr = new CustomsMessagesTabUserControl())
			{
				frm.Controls.Add(ctr);
				frm.Show();
				Application.DoEvents();
				var grid = ctr.FindSingleOrDefault<Messaging.GUI.MessageZGrid>("MessagesGrid");
				var menu = grid.ContextMenu.MenuItems.FindByText("Reprocess Message");
				AssertNotNull("The menu should be visible when the user is support user.", menu);
				UnitTestUserNotification.Instance.ClearMessages();
				AssertNoExceptionThrown(() => menu.PerformClick());
				AssertEquals("A single response message is required to be selected for reprocess.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
