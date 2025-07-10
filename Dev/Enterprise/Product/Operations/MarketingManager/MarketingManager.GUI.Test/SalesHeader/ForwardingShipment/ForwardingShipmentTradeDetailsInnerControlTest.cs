using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class ForwardingShipmentTradeDetailsInnerControlTest : TestCaseWithFactory
	{
		public void TestTopSplitContainer()
		{
			using (var form = new ZForm())
			using (var control = new ForwardingShipmentTradeDetailsInnerControl())
			{
				form.Controls.Add(control);
				form.Show();

				Assert(control.topSplitContainer.IsSplitterFixed);
				AssertEquals(FixedPanel.Panel2, control.topSplitContainer.FixedPanel);
			}
		}
	}
}
