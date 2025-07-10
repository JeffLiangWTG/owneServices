using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class DropDownImageButtonTest : ZControlBaseTestCase<DropDownImageButton>
	{
		public void TestShowContextMenuStripOnClick()
		{
			using (var form = new ZForm())
			using (var button = new DropDownImageButton())
			{
				button.ContextMenuStrip.Items.Add("Dummy");
				form.Controls.Add(button);
				form.Show();

				var contextMenuOpened = false;
				button.ContextMenuStrip.Opened += (sender, e) => contextMenuOpened = true;
				button.PerformClick();
				AssertEquals(true, contextMenuOpened);

				button.ContextMenuStrip.Close();
			}
		}
	}
}
