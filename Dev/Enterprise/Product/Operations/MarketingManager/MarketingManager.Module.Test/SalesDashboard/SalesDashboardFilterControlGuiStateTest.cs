using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.Module.Testing
{
	public class SalesDashboardFilterControlGuiStateTest : TestCaseWithFactory
	{
		public void TestMainSplitterPosition()
		{
			using (var control = new SalesDashboardFilterControl(new SalesDashboardActivityCollection(Factory), new SalesDashboardFilterBusinessObject(), false))
			{
				var guiState = new SalesDashboardFilterControlGuiState(control);

				guiState.MainSplitterPosition = 1;
				AssertEquals(1, guiState.MainSplitterPosition);
				guiState.MainSplitterPosition = 100;
				AssertEquals(100, guiState.MainSplitterPosition);

				guiState.PreviewSplitterDistance = 5;
				AssertEquals(5, guiState.PreviewSplitterDistance);
				guiState.PreviewSplitterDistance = 50;
				AssertEquals(50, guiState.PreviewSplitterDistance);

				guiState.SalesRelationSplitterDistance = 15;
				AssertEquals(15, guiState.SalesRelationSplitterDistance);
				guiState.SalesRelationSplitterDistance = 30;
				AssertEquals(30, guiState.SalesRelationSplitterDistance);

				guiState.TasksSplitterDistance = 20;
				AssertEquals(20, guiState.TasksSplitterDistance);
				guiState.TasksSplitterDistance = 120;
				AssertEquals(120, guiState.TasksSplitterDistance);
			}
		}
	}
}
