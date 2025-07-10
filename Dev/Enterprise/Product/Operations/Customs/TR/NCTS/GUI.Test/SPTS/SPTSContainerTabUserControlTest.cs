using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	class SPTSContainerTabUserControlTest : TestCaseWithFactory
	{
		public void TestInitializeGrid()
		{
			using (var form = new ZForm())
			using (var control = new SPTSContainerTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var containersGrid = control.FindSingle<ZGrid>("SPTSHeaderContainersGrid");

				CombineAssertions("Test for Existence of The Grid", () =>
				{
					AssertEquals("BC_ContainerNum", ((ZGridColumnInfo)containersGrid.ColumnStyles[0]).ColumnName);
				});
			}
		}
	}
}
