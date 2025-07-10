using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ReferenceNumberFilterGUIProviderTest : TestCaseWithFactory
	{
		public void TestHandlesRefrenceNumbers()
		{
			using (ZFilterStrip strip = new ZFilterStrip())
			{
				ZBindingSource source = new ZBindingSource(strip, typeof(ReferenceNumberFilter));

				Control[] controls = ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(strip, source);
				Assert("Should provide some controls", controls.Length > 0);

				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}
	}
}
