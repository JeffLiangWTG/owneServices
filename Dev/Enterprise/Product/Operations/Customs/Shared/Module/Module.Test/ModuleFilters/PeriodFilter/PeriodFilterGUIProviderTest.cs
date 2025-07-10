using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module.Testing
{
	sealed class PeriodFilterGUIProviderTest : TestCaseWithFactory
	{
		public void TestGetPeriodFilterControls()
		{
			using (ZFilterStrip strip = new ZFilterStrip())
			{
				ZBindingSource source = new ZBindingSource(strip, typeof(PeriodFilter));
				Control[] controls = PeriodFilterGUIProvider.GetPeriodFilterControls(strip, source);
				Assert("Should provide a ZPeriodUserControl", controls[0] is ZPeriodUserControl);
				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}
	}
}
