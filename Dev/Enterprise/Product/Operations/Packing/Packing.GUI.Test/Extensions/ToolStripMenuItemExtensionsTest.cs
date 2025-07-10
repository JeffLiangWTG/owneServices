using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	sealed class ToolStripMenuItemExtensionsTest : TestCase
	{
		#region TestPerformClickEnableFirst

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
		public void TestPerformClickEnableFirst()
		{
			bool performClickEventFired = false;
			var item = new ToolStripMenuItem();
			item.Click += delegate
			{ performClickEventFired = true; };

			item.Enabled = false;
			AssertEquals("Precondition", false, item.Enabled);
			AssertEquals("Precondition", false, performClickEventFired);

			// demonstrates the problem we are fixing. if this behaviour changes we can delete this extension.
			item.PerformClick();
			AssertEquals(false, item.Enabled);
			AssertEquals(false, performClickEventFired);

			item.PerformClickEnableFirst();
			AssertEquals(true, item.Enabled);
			AssertEquals(true, performClickEventFired);
		}

		#endregion
	}
}
