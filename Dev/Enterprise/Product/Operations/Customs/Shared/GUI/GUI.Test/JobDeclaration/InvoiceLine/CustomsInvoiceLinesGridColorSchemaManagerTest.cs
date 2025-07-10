using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CustomsInvoiceLinesGridColorSchemaManagerTest : TestCaseWithFactory
	{
		public void TestGetFilterBusinessObjectForGrid()
		{
			using (var grid = new ZGrid())
			{
				var gridColorSchemaManager = new CustomsInvoiceLinesGridColorSchemaManager(grid);
				AssertType<InvoiceLinesGridFilterStripBusinessObject>(gridColorSchemaManager.FilterBusinessObject);
			}
		}
	}
}
