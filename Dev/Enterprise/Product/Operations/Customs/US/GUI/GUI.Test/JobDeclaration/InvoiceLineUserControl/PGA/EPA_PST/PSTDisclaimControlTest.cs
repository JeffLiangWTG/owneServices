using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class PSTDisclaimControlTest : TestCaseWithFactory
	{
		public void TestPSTDisclaimControlProgramDropEdit()
		{
			using (var control = new PSTDisclaimControl())
			{
				control.Show();

				var pstDisclaimGroupBox = control.FindSingle<ZGroupBox>("PSTDisclaimGroupBox");
				var pstProgramDropEdit = pstDisclaimGroupBox.FindSingle<ZDropEdit>("PSTProgramDropEdit");

				Assert(pstProgramDropEdit.AllowDrop);
				AssertEquals("BindingMember", "US_PSTDisclaimProgram", pstProgramDropEdit.GetBindingMember());
				AssertNull(pstProgramDropEdit.BindToList);
			}
		}
	}
}
