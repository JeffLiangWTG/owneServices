using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	class RateFormulaBoxTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestPopupForm()
		{
			var tariffView = Factory.New<TariffView>();
			var rateView = tariffView.FilteredRates.AddNew();
			AssertEquals("PreCondition", rateView.PK, tariffView.FilteredRates[0].PK);
			using (var form = new ZForm(tariffView))
			{
				var grid = new ZGrid();
				grid.BindTo = "FilteredRates";
				var columnStyleInfo = new RateFormulaBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "ZZ2_RateFormula";
				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();
				var rateFormulaBox = (RateFormulaBox)((RateFormulaBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				rateFormulaBox.CodeBox.Text = "rate";
				rateFormulaBox.SelectFromPopupForm();
				var rateFormulaFormFieldInfo = typeof(RateFormulaBox).GetField("rateFormulaForm", BindingFlags.Instance | BindingFlags.NonPublic);
				var rateFormulaForm = rateFormulaFormFieldInfo.GetValue(rateFormulaBox) as RateFormulaForm;
				var okButton = rateFormulaForm.Controls.Find("OkButton", true)[0] as ZButton;
				var freeRadioButton = rateFormulaForm.Controls.Find("FreeRadioButton", true)[0] as ZRadioButton;
				var freeFormatRadioButton = rateFormulaForm.Controls.Find("FreeFormatRadioButton", true)[0] as ZRadioButton;
				var formulaTextBox = rateFormulaForm.Controls.Find("FormulaTextBox", true)[0] as ZTextBox;
				AssertEquals(false, freeRadioButton.Checked);
				AssertEquals(true, freeFormatRadioButton.Checked);
				AssertEquals("RATE", formulaTextBox.Text);
				AssertEquals(rateFormulaBox.CodeBox.MaxLength, formulaTextBox.MaxLength);
				freeRadioButton.Select();
				okButton.PerformClick();
				AssertEquals(true, freeRadioButton.Checked);
				AssertEquals(false, freeFormatRadioButton.Checked);
				AssertEquals("0", rateFormulaBox.CodeBox.Text);
			}
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestGetList()
		{
			using (var findBox = new RateFormulaBox())
			{
				AssertNull(findBox.List);
			}
		}
	}
}
