using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(PermitNumbersSelectorForm))]
	sealed class PermitNumbersSelectorFormTest : ZFormBasherTest
	{
		public void TestGrid()
		{
			using (var form = new PermitNumbersSelectorForm(collection))
			{
				var grid = (ZGrid)form.Controls.Find("PermitNumbersGrid", true)[0];
				var gridColumnInfo1 = grid.ColumnStyles.OfType<ZGridColumnInfo>().First(x => x.ColumnName == "NeedPrint");
				Assert("Should be visible", gridColumnInfo1.IsVisible);
				var gridColumnInfo2 = grid.ColumnStyles.OfType<ZGridColumnInfo>().First(x => x.ColumnName == "PermitNumber");
				Assert("Should be visible", gridColumnInfo2.IsVisible);
			}
		}

		public void TestPrintButton()
		{
			using (var form = new PermitNumbersSelectorForm(collection))
			{
				form.Show();
				var printButton = (ZButton)form.Controls.Find("PrintButton", true)[0];
				printButton.PerformClick();
				AssertEquals(DialogResult.Yes, form.DialogResult);
			}
		}

		public void TestCancelPrintButton()
		{
			using (var form = new PermitNumbersSelectorForm(collection))
			{
				form.Show();
				var cancelPrintButton = (ZButton)form.Controls.Find("CancelPrintButton", true)[0];
				cancelPrintButton.PerformClick();
				AssertEquals(DialogResult.No, form.DialogResult);
			}
		}

		public void TestSelectAllButton()
		{
			foreach (PermitNumberToSelectFromForPrinting item in collection)
			{
				item.NeedPrint = false;
			}
			using (var form = new PermitNumbersSelectorForm(collection))
			{
				form.Show();
				var selectAllButton = (ZButton)form.Controls.Find("SelectAllButton", true)[0];
				selectAllButton.PerformClick();
				foreach (PermitNumberToSelectFromForPrinting permitNumber in collection)
				{
					Assert("Should be true", permitNumber.NeedPrint);
				}
			}
		}

		public void TestSelectNoneButton()
		{
			using (var form = new PermitNumbersSelectorForm(collection))
			{
				form.Show();
				var selectNoneButton = (ZButton)form.Controls.Find("SelectNoneButton", true)[0];
				selectNoneButton.PerformClick();
				foreach (PermitNumberToSelectFromForPrinting permitNumber in collection)
				{
					Assert("Should be false", !permitNumber.NeedPrint);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			collection = new PermitNumbersToSelectFromForPrintingCollection();
			var permitNumber = new PermitNumberToSelectFromForPrinting("123");
			collection.Add(permitNumber);
			permitNumber = new PermitNumberToSelectFromForPrinting("456");
			collection.Add(permitNumber);
			permitNumber = new PermitNumberToSelectFromForPrinting("789");
			collection.Add(permitNumber);
		}

		PermitNumbersToSelectFromForPrintingCollection collection;

		protected override Form GetFormToBashCore() => new PermitNumbersSelectorForm(collection);
	}
}
