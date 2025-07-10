using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class JobCommonTradeDetailsInnerControlTest : TestCaseWithFactory
	{
		public void TestTopSplitContainer()
		{
			using (var form = new ZForm())
			using (var control = new JobCommonTradeDetailsInnerControl("TEST"))
			{
				form.Controls.Add(control);
				form.Show();

				Assert(control.topSplitContainer.IsSplitterFixed);
				AssertEquals(FixedPanel.Panel2, control.topSplitContainer.FixedPanel);
			}
		}
	}
}
