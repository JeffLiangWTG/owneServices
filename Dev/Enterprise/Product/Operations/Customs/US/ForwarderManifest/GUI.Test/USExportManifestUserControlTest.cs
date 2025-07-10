using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	class USExportManifestUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var userControl = new USExportManifestUserControl())
			{
				AssertNotNull(userControl.Controls.Find("DeparturePortUNLOCOCodeFindBox", true));
				AssertNotNull(userControl.Controls.Find("ScheduleDCodeFindBox", true));
				AssertNotNull(userControl.Controls.Find("IssuerSCACUserControl", true));
			}
		}
	}
}
