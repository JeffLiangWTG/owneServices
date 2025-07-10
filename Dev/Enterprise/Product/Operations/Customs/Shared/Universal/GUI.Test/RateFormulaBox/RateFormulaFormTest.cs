using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(RateFormulaForm))]
	class RateFormulaFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = GetFormToBashCore() as RateFormulaForm)
			{
				AssertEquals("New Rate Formula", form.FormHeading);
			}
		}

		public void TestFormulaOptions()
		{
			using (var form = GetFormToBashCore())
			{
				AssertNotNull("Free Rate Formula", form.Controls.Find("FreeRadioButton", true)[0] as ZRadioButton);
				AssertNotNull("FreeFormat Rate Formula", form.Controls.Find("FreeFormatRadioButton", true)[0] as ZRadioButton);
				AssertNotNull("PercentageOfCustomsValue Rate Formula", form.Controls.Find("PercentageOfCustomsValueRadioButton", true)[0] as ZRadioButton);
				AssertNotNull("RatePerUnit Rate Formula", form.Controls.Find("RatePerUnitRadioButton", true)[0] as ZRadioButton);
				AssertNotNull("PercentageOfCustomsValue + RatePerUnit", form.Controls.Find("PercentageOfCustomsValueAndRatePerUnitRadioButton", true)[0] as ZRadioButton);
				AssertNotNull("PercentageOfCustomsValue With a Minimum Of RatePerUnit", form.Controls.Find("PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton", true)[0] as ZRadioButton);
			}
		}

		public void TestFormulaTextBoxReadOnly()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var contorls = form.Controls;
				var formulaTextBox = contorls.Find("FormulaTextBox", true)[0] as ZTextBox;
				var freeRadioButton = contorls.Find("FreeRadioButton", true)[0] as ZRadioButton;
				var freeFormatRadioButton = contorls.Find("FreeFormatRadioButton", true)[0] as ZRadioButton;
				AssertEquals(false, freeRadioButton.Checked);
				AssertEquals(false, freeFormatRadioButton.Checked);
				AssertEquals("FormulaTextBox is ReadOnly as default", true, formulaTextBox.ReadOnly);
				freeFormatRadioButton.Select();
				AssertEquals(false, freeRadioButton.Checked);
				AssertEquals(true, freeFormatRadioButton.Checked);
				AssertEquals("FormulaTextBox is not ReadOnly when FreeFormat Rate Formula", false, formulaTextBox.ReadOnly);
				freeRadioButton.Select();
				AssertEquals(true, freeRadioButton.Checked);
				AssertEquals(false, freeFormatRadioButton.Checked);
				AssertEquals("FormulaTextBox is ReadOnly when Free Rate Formula", true, formulaTextBox.ReadOnly);
			}
		}

		public void TestPercentageOfCustomsValueCalcEditVisible()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var contorls = form.Controls;
				var percentageOfCustomsValueCalcEdit = contorls.Find("PercentageOfCustomsValueCalcEdit", true)[0] as ZCalcEdit;
				var freeRadioButton = contorls.Find("FreeRadioButton", true)[0] as ZRadioButton;
				var percentageOfCustomsValueRadioButton = contorls.Find("PercentageOfCustomsValueRadioButton", true)[0] as ZRadioButton;
				var percentageOfCustomsValueAndRatePerUnitRadioButton = contorls.Find("PercentageOfCustomsValueAndRatePerUnitRadioButton", true)[0] as ZRadioButton;
				var percentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton = contorls.Find("PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton", true)[0] as ZRadioButton;
				AssertEquals(false, freeRadioButton.Checked);
				AssertEquals(false, percentageOfCustomsValueRadioButton.Checked);
				AssertEquals("PercentageOfCustomsValueCalcEdit is not visible as default", false, percentageOfCustomsValueCalcEdit.Visible);
				freeRadioButton.Select();
				AssertEquals(true, freeRadioButton.Checked);
				AssertEquals("PercentageOfCustomsValueCalcEdit is not visible when FreeFormat Rate Formula", false, percentageOfCustomsValueCalcEdit.Visible);
				percentageOfCustomsValueRadioButton.Select();
				AssertEquals(true, percentageOfCustomsValueRadioButton.Checked);
				AssertEquals("PercentageOfCustomsValueCalcEdit is visible when percentofCustomsValue Rate Formula", true, percentageOfCustomsValueCalcEdit.Visible);
				percentageOfCustomsValueAndRatePerUnitRadioButton.Select();
				AssertEquals(true, percentageOfCustomsValueAndRatePerUnitRadioButton.Checked);
				AssertEquals("PercentageOfCustomsValueCalcEdit is visible when PercentageOfCustomsValue + RatePerUnit", true, percentageOfCustomsValueAndRatePerUnitRadioButton.Visible);
				percentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.Select();
				AssertEquals(true, percentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.Checked);
				AssertEquals("PercentageOfCustomsValueCalcEdit is visible when PercentageOfCustomsValue With a Minimum Of RatePerUnit", true, percentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.Visible);
			}
		}

		public void TestUnitDropEditVisible()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var contorls = form.Controls;
				var unitDropEdit = contorls.Find("UnitDropEdit", true)[0] as ZDropEdit;
				var freeRadioButton = contorls.Find("FreeRadioButton", true)[0] as ZRadioButton;
				var ratePerUnitRadioButton = contorls.Find("RatePerUnitRadioButton", true)[0] as ZRadioButton;
				var percentageOfCustomsValueAndRatePerUnitRadioButton = contorls.Find("PercentageOfCustomsValueAndRatePerUnitRadioButton", true)[0] as ZRadioButton;
				var percentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton = contorls.Find("PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton", true)[0] as ZRadioButton;
				AssertEquals(false, freeRadioButton.Checked);
				AssertEquals(false, ratePerUnitRadioButton.Checked);
				AssertEquals("UnitDropEdit is not visible as default", false, unitDropEdit.Visible);
				freeRadioButton.Select();
				AssertEquals(true, freeRadioButton.Checked);
				AssertEquals("UnitDropEdit is not visible when FreeFormat Rate Formula", false, unitDropEdit.Visible);
				ratePerUnitRadioButton.Select();
				AssertEquals(true, ratePerUnitRadioButton.Checked);
				AssertEquals("UnitDropEdit is visible when Rate Per Unit Formula", true, unitDropEdit.Visible);
				percentageOfCustomsValueAndRatePerUnitRadioButton.Select();
				AssertEquals(true, percentageOfCustomsValueAndRatePerUnitRadioButton.Checked);
				AssertEquals("UnitDropEdit is visible when PercentageOfCustomsValue + RatePerUnit", true, unitDropEdit.Visible);
				percentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.Select();
				AssertEquals(true, percentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.Checked);
				AssertEquals("UnitDropEdit is visible when PercentageOfCustomsValue With a Minimum Of RatePerUnit", true, unitDropEdit.Visible);
			}
		}

		public void TestRatePerUnitCalcEditVisible()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var contorls = form.Controls;
				var ratePerUnitCalcEdit = contorls.Find("RatePerUnitCalcEdit", true)[0] as ZCalcEdit;
				var freeRadioButton = contorls.Find("FreeRadioButton", true)[0] as ZRadioButton;
				var ratePerUnitRadioButton = contorls.Find("RatePerUnitRadioButton", true)[0] as ZRadioButton;
				AssertEquals(false, freeRadioButton.Checked);
				AssertEquals(false, ratePerUnitRadioButton.Checked);
				AssertEquals("RatePerUnitCalcEdit is not visible as default", false, ratePerUnitCalcEdit.Visible);
				freeRadioButton.Select();
				AssertEquals(true, freeRadioButton.Checked);
				AssertEquals("RatePerUnitCalcEdit is not visible when FreeFormat Rate Formula", false, ratePerUnitCalcEdit.Visible);
				ratePerUnitRadioButton.Select();
				AssertEquals(true, ratePerUnitRadioButton.Checked);
				AssertEquals("RatePerUnitCalcEdit is visible when Rate Per Unit Formula", true, ratePerUnitCalcEdit.Visible);
			}
		}

		public void TestFormula_Free()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var contorls = form.Controls;
				var freeRadioButton = contorls.Find("FreeRadioButton", true)[0] as ZRadioButton;
				var formulaTextBox = contorls.Find("FormulaTextBox", true)[0] as ZTextBox;
				freeRadioButton.Select();
				AssertEquals("0", formulaTextBox.Text);
			}
		}

		[RequiresSTA]
		public void TestOKButton()
		{
			using (var parentForm = new ZForm())
			{
				var findBox = new FindBoxForTest();
				parentForm.Controls.Add(findBox);
				var form = findBox.PopupForm;
				form.ShowModal(findBox, parentForm);
				var controls = ((ZForm)form).Controls;
				var freeRadioButton = controls.Find("FreeRadioButton", true)[0] as ZRadioButton;
				var okButton = controls.Find("OkButton", true)[0] as ZButton;
				freeRadioButton.Select();
				okButton.PerformClick();
				AssertEquals("RateFormula is selected", "0", findBox.Code);
			}
		}

		[RequiresSTA]
		public void TestOKButton_HasError()
		{
			using (var parentForm = new ZForm())
			{
				var findBox = new FindBoxForTest();
				parentForm.Controls.Add(findBox);
				var form = findBox.PopupForm;
				form.ShowModal(findBox, parentForm);
				var controls = ((ZForm)form).Controls;
				var percentageOfCustomsValueRadioButton = controls.Find("PercentageOfCustomsValueRadioButton", true)[0] as ZRadioButton;
				var okButton = controls.Find("OkButton", true)[0] as ZButton;
				percentageOfCustomsValueRadioButton.Select();
				findBox.BusinessEntity.PercentageOfCustomsValue = 1001;
				okButton.PerformClick();
				AssertEquals("LastMessage", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestCloseButton()
		{
			using (var parentForm = new ZForm())
			{
				var findBox = new FindBoxForTest();
				parentForm.Controls.Add(findBox);
				var form = findBox.PopupForm;
				form.ShowModal(findBox, parentForm);
				var controls = ((ZForm)form).Controls;
				var freeRadioButton = controls.Find("FreeRadioButton", true)[0] as ZRadioButton;
				var closeButton = controls.Find("closeButton", true)[0] as ZButton;
				freeRadioButton.Select();
				closeButton.PerformClick();
				AssertEquals("RateFormula not selected", "", findBox.Code);
			}
		}

		protected override Form GetFormToBashCore() => new RateFormulaForm(new RateFormulaEditHelper(null), 10);

		class FindBoxForTest : Control, IFindBox
		{
			public FindBoxForTest()
			{
				popupForm = new RateFormulaForm(BusinessEntity, 10);
				Code = "";
			}

			readonly IFindBoxPopup popupForm;

			public string Code { get; set; }

			public string Description { get; set; }

			public IFindBoxListProvider ListProvider => null;

			public IFindBoxPopup PopupForm => popupForm;

			public IFindBoxPopup PeekPopupForm() => popupForm;

			public RateFormulaEditHelper BusinessEntity => businessEntity ?? (businessEntity = new RateFormulaEditHelper(null));
			RateFormulaEditHelper businessEntity;
		}
	}
}
