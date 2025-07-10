using System.Reflection;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public abstract class WhsDocketFormTestCase : WhsGuiTestCaseWithFactory
	{
		#region TestSendEmailMenuItem

		public void TestSendEmailMenuItem()
		{
			using (ZForm form = GetNewDocketForm())
			{
				form.Show();
				FieldInfo info = form.GetType().GetField("ActionsMenuItem", BindingFlags.Instance | BindingFlags.NonPublic);
				MenuItem actionsMenuItem = (MenuItem)info.GetValue(form);

				var menuItem = actionsMenuItem.MenuItems.FindByText(EmailSender.GetSendEmailMenuItemTextForTest());
				AssertNotNull(menuItem);
			}
		}

		protected abstract ZForm GetNewDocketForm();

		#endregion
	}
}
