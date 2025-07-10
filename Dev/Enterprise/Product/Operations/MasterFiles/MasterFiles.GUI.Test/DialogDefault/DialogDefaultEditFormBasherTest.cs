using System.Windows.Forms;
using Enterprise.Core.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DialogDefaultEditForm))]
	sealed class DialogDefaultEditFormBasherTest : ZFormBasherTest
	{
		public void TestFileNewMenuItemisDisabled()
		{
			using (var form = GetFormToBashCore())
			{
				AssertEquals("The fNewMenuItem should be disabled", false, form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileNewMenuItemName].Enabled);
			}
		}

		public void TestViewOnlyButtons()
		{
			using (var form = (DialogDefaultEditForm)GetFormToBashCore())
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.Delete;
				form.Show();
				AssertEquals(true, form.saveButton.Enabled);
				AssertEquals(true, form.exitButton.Enabled);
			}

			using (var form2 = (DialogDefaultEditForm)GetFormToBashCore())
			{
				form2.DisplayMode = ZArchitecture.Core.ODisplayMode.Browse;
				form2.Show();
				AssertEquals(false, form2.saveButton.Enabled);
				AssertEquals(true, form2.exitButton.Enabled);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new DialogDefaultEditForm(Factory.NewWithValidTestData<StmDialogDefault>());
		}

		#endregion

	}
}
