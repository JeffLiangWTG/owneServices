using System.Windows.Forms;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	public class USAMSFilterStripControlTest : ZFilterStripControlTest
	{
		public void TestColumn_LatestAMSDispositionCodeAndDesc()
		{
			using (var module = new USAMSModule())
			using (var form = new ZForm())
			{
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
				AssertColumn(grid, CusInBondHeader.Schema.BH_LatestDispositionCode);
				AssertColumn(grid, CusInBondHeader.Schema.BH_LatestDispositionCodeDescription);
			}
		}
	}
}
