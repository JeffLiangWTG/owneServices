using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class USOrgAdditionalCustomsDefaultsControlTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			using (var control = new USOrgAdditionalCustomsDefaultsControl())
			{
				AssertEquals("USOrgAdditionalCustomsDefaultsControl", control.Name);
				control.ControlsVisibility(true);
				Assert(control.FirstSaleDropEdit.Visible);
				Assert(control.ReconIndicatorDropEdit.Visible);
				Assert(control.NAFTAReconIndicatorCheckBox.Visible);
				var aesUltimateConsigneeTypeDropEdit = control.Controls.Cast<System.Windows.Forms.Control>().FirstOrDefault(c => c.Name == "AESUltimateConsigneeTypeDropEdit");
				AssertNotNull(aesUltimateConsigneeTypeDropEdit);
				Assert(aesUltimateConsigneeTypeDropEdit.Visible);
				control.ControlsVisibility(false);
				Assert(!control.FirstSaleDropEdit.Visible);
				Assert(!control.ReconIndicatorDropEdit.Visible);
				Assert(!control.NAFTAReconIndicatorCheckBox.Visible);
				Assert(aesUltimateConsigneeTypeDropEdit.Visible);
				var entryTypeDropEdit = control.Controls.Cast<System.Windows.Forms.Control>().FirstOrDefault(c => c.Name == "EntryTypeDropEdit");
				AssertNotNull(entryTypeDropEdit);
				Assert(entryTypeDropEdit.Visible);
			}
		}
	}
}
