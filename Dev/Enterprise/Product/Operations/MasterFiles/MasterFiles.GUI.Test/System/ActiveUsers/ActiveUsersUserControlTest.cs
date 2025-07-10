using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ActiveUsersUserControlTest : TestCaseWithFactory
	{
		public void TestExportToExcelMenuItem()
		{
			using (var control = new ActiveUsersUserControlForTest())
			{
				Assert("Should show the 'Export to Excel' menu item", control.ActiveUsersGrid_Exposed.ForceShowExportToExcelMenuItem);
			}
		}

		class ActiveUsersUserControlForTest : ActiveUsersUserControl
		{
			public ZDisplayGrid ActiveUsersGrid_Exposed => ActiveUsersGrid;
		}
	}
}
