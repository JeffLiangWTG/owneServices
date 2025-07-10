using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class AMSDisclaimPivotControlTest : TestCaseWithFactory
	{
		public void TestAMSDisclaimPivotControlProgramDropEdit()
		{
			using (var control = new AMSDisclaimPivotControl())
			{
				control.Show();

				var amsDisclaimGroupBox = control.FindSingle<ZGroupBox>("AMSDisclaimGroupBox");
				var amsProgramDropEdit = amsDisclaimGroupBox.FindSingle<ZDropEdit>("AMSProgramDropEdit");

				Assert(amsProgramDropEdit.AllowDrop);
				AssertEquals("BindingMember", "CD_AMSDisclaimProgram", amsProgramDropEdit.GetBindingMember());
				AssertNull(amsProgramDropEdit.BindToList);
			}
		}
	}
}
