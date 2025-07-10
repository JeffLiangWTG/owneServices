using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs._CustomsTemplate_.GUI.Testing
{
	class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}
	}
}
