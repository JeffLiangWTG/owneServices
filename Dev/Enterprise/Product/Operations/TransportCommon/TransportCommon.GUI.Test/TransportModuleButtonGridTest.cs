using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.TransportCommon.GUI.Testing
{
	public class TransportModuleButtonGridTest : TestCaseWithFactory
	{
		#region TestButtonVisibility

		public void TestButtonVisibility()
		{
			using (var buttonGrid = new TransportModuleButtonGridForTest())
			{
				AssertEquals(true, buttonGrid.AllowDoubleClickForTest);
				AssertEquals(false, buttonGrid.ShowAttachButton);
				AssertEquals(false, buttonGrid.ShowDetachButton);
				AssertEquals(false, buttonGrid.ShowEditButton);
				AssertEquals(false, buttonGrid.ShowNewButton);
				AssertEquals("Eventhough we hide all button controls panel has a default space which we need to remove",
					false, buttonGrid.ToolStripControlForTest.Visible);
				AssertEquals(1, buttonGrid.MainLayoutPanelForTest.RowCount);
			}
		}

		class TransportModuleButtonGridForTest : TransportModuleButtonGrid
		{
			public ToolStrip ToolStripControlForTest { get { return toolStrip; } }
			public KTableLayoutPanel MainLayoutPanelForTest { get { return mainLayoutPanel; } }
			public bool AllowDoubleClickForTest { get { return AllowDoubleClick; } }
		}

		#endregion
	}
}
