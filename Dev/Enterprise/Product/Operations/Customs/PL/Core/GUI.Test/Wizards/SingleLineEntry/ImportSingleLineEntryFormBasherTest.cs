using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ImportSingleLineEntryForm))]
sealed class ImportSingleLineEntryFormBasherTest : ZFormBasherTest
{
	public void TestGoodsOriginControl_Visibility()
	{
		using (var form = (ImportSingleLineEntryForm)GetFormToBashCore())
		{
			form.Show();
			var control = form.GoodsOriginCodeFindBox;
			AssertEquals(true, control.Visible);
		}
	}

	public void TestPreviousDocumentControl_Visibility()
	{
		using (var form = (ImportSingleLineEntryForm)GetFormToBashCore())
		{
			form.Show();
			var control = form.PreviousDocumentDropEdit;
			AssertEquals(true, control.Visible);
		}
	}

	public void TestControls()
	{
		CombineAssertions(() =>
		{
			using (var form = (ImportSingleLineEntryForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals("PreviousDocumentNumberTextBox", true, form.PreviousDocumentNumberTextBox.Visible);
				AssertEquals("GoodsOriginCodeFindBox", true, form.GoodsOriginCodeFindBox.Visible);
				AssertEquals("PreviousDocumentDropEdit", true, form.PreviousDocumentDropEdit.Visible);
				AssertEquals("CPCFindBox", true, form.Controls.Find("CPCFindBox", true).First().Visible);
				AssertEquals("InvoiceNumberTextBox", true, form.Controls.Find("InvoiceNumberTextBox", true).First().Visible);
				AssertEquals("PriceCalcEdit", true, form.Controls.Find("PriceCalcEdit", true).First().Visible);
				AssertEquals("NetWeightCalcEdit", true, form.Controls.Find("NetWeightCalcEdit", true).First().Visible);
				AssertEquals("TariffFindBox", true, form.Controls.Find("TariffFindBox", true).First().Visible);
			}
		});
	}

	protected override Form GetFormToBashCore() => new ImportSingleLineEntryForm(new ImportSingleLineEntryManager(Factory.New<JobDeclaration>()));
}
