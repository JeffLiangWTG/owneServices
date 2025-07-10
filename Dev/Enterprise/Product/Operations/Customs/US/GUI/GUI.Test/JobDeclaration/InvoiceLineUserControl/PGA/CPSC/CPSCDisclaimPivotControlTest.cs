using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class CPSCDisclaimPivotControlTest : TestCaseWithFactory
	{
		public void TestCPSCDisclaimPivotControl()
		{
			using (var control = new CPSCDisclaimPivotControl())
			{
				control.Show();

				var cpscDisclaimGroupBox = control.FindSingle<ZGroupBox>("CPSCDisclaimGroupBox");
				var cpscIntendedUseCodeDropEdit = cpscDisclaimGroupBox.FindSingle<ZDropEdit>("CPSCIntendedUseCodeDropEdit");
				var cpscIntendedUseDescriptionTextBox = cpscDisclaimGroupBox.FindSingle<ZTextBox>("US_IntendedUseDescriptionTextBox");

				AssertNotNull(cpscIntendedUseCodeDropEdit);
				Assert(cpscIntendedUseCodeDropEdit.AllowDrop);
				AssertEquals("BindingMember", "US_IntendedUseCode", cpscIntendedUseCodeDropEdit.GetBindingMember());

				AssertNotNull(cpscIntendedUseDescriptionTextBox);
				AssertEquals("BindingMember", "US_IntendedUseDescription", cpscIntendedUseDescriptionTextBox.GetBindingMember());
			}
		}
	}
}
