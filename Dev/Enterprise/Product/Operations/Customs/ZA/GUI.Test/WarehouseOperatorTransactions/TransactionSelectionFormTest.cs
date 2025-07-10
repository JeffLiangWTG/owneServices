using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(TransactionSelectionForm))]
	class TransactionSelectionFormTest : ZFormBasherTest
	{
		public void TestGridColumns()
		{
			using (var form = GetFormToBash())
			{
				form.Show();

				var grid = form.FindSingleOrDefault<ZGrid>("TransactionsDisplayGrid");
				AssertNotNull("Find TransactionsDisplayGrid", grid);

				AssertEquals(8, grid.Columns.Count);
				AssertColumn(grid, "Select", "Select?", 60);
				AssertColumn(grid, "TransactionType", "Transaction Type", 75);
				AssertColumn(grid, "TransactionDate", "Transaction Date", 90);
				AssertColumn(grid, "ExportType", "Export Type", 75);
				AssertColumn(grid, "OwnerReference", "Owner Reference", 100);
				AssertColumn(grid, "ProductCode", "Product Code", 120);
				AssertColumn(grid, "BatchLineNo", "Batch Line No.", 85);
				AssertColumn(grid, "LineReference", "Line Reference", 100);
			}
		}

		void AssertColumn(ZGrid grid, string columnName, string expectedCaption, int expectedWidth)
		{
			var columnStyle = grid.GetColumnStyle(columnName);
			AssertEquals($"{columnName} Caption", expectedCaption, grid.GetColumnCaption(columnName));
			AssertEquals($"{columnName} Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(expectedWidth), columnStyle.Width);
		}

		protected override Form GetFormToBashCore() => new TransactionSelectionForm(new TransactionSelectionControllerForTest(Factory));

		class TransactionSelectionControllerForTest : TransactionSelectionController
		{
			public TransactionSelectionControllerForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override ZQuery TransactionFilter => new ZQuery();

			protected override void ProcessSelectedRecord(CusWHSOperatorTransaction tx)
			{
			}
		}
	}
}
