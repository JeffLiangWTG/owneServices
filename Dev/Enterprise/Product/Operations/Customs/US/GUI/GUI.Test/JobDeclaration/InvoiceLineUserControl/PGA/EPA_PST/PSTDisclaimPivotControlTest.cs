using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class PSTDisclaimPivotControlTest : TestCaseWithFactory
	{
		public void TestPSTDisclaimPivotControlProgramDropEdit()
		{
			using (var control = new PSTDisclaimPivotControl())
			{
				control.Show();

				var pstDisclaimGroupBox = control.FindSingle<ZGroupBox>("PSTDisclaimGroupBox");
				var pstProgramDropEdit = pstDisclaimGroupBox.FindSingle<ZDropEdit>("PSTProgramDropEdit");

				Assert(pstProgramDropEdit.AllowDrop);
				AssertEquals("BindingMember", "CD_PSTDisclaimProgram", pstProgramDropEdit.GetBindingMember());
				AssertNull(pstProgramDropEdit.BindToList);
			}
		}
	}
}
