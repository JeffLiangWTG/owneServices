using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Rating.GUI.Testing
{
	abstract class BaseCombinedCalculatorControlTest<T> : CargoWise.EntityFramework.Testing.TestCaseWithFactory
		where T : BaseCombinedCalculatorControl, new()
	{
		public abstract string CalculatorCode { get; }

		public void TestUseInclusiveBreakCheckBoxVisibility()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			var ratingHeader = Factory.NewWithValidTestData<CompanyTariff>();
			ratingHeader.TH_GC = GlbCompany.CurrentCompany.PK;

			var entry = ratingHeader.AddRateEntry("AIR");
			var rateLine = entry.AddRateLine(chargeCode.AC_Code);
			rateLine.TL_RateCalculator = CombinedCalculator.Code;

			using (var control = new T())
			{
				control.SetDataBinding(ratingHeader, "");
				AssertEquals(true, control.UseInclusiveBreaksCheckBox.Visible);

				control.UseAccumulatedCheckbox.Checked = true;
				AssertEquals(false, control.UseInclusiveBreaksCheckBox.Visible);
			}
		}

		public void TestIsAccumulatedCheckBoxAndUseHigherChargeableLowerRateRuleStateChangedWhenBreaksPerIsNotEmpty()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			var ratingHeader = Factory.NewWithValidTestData<CompanyTariff>();
			ratingHeader.TH_GC = GlbCompany.CurrentCompany.PK;

			var entry = ratingHeader.AddRateEntry("AIR");
			var rateLine = entry.AddRateLine(chargeCode.AC_Code);
			rateLine.TL_RateCalculator = CalculatorCode;

			using (var form = new Form())
			using (var control = new T())
			{
				control.SetDataBinding(ratingHeader, "");
				control.ViewCalculatorForBinding = rateLine.ViewCalculatorForBinding;
				control.BindTo = "AIRRateEntriesForBinding.RateLines.ViewCalculator";

				form.Controls.Add(control);
				form.Show();

				Assert(!control.BreaksPerDropDown.ReadOnly);
				Assert(!control.UseAccumulatedCheckbox.ReadOnly);
				Assert(!control.HigherChargeableLowerRateCheckBox.ReadOnly);

				control.UseAccumulatedCheckbox.Checked = false;
				control.HigherChargeableLowerRateCheckBox.Checked = true;

				SwitchToFilter(control.BreaksPerDropDown, BaseCombinedCalculator.Items.BreaksPerContainer);
				AssertEquals(true, control.UseAccumulatedCheckbox.ReadOnly);
				AssertEquals(true, control.UseAccumulatedCheckbox.Checked);
				AssertEquals(true, control.HigherChargeableLowerRateCheckBox.ReadOnly);
				AssertEquals(false, control.HigherChargeableLowerRateCheckBox.Checked);

				SwitchToFilter(control.BreaksPerDropDown, string.Empty);
				AssertEquals(false, control.UseAccumulatedCheckbox.ReadOnly);
				AssertEquals(false, control.UseAccumulatedCheckbox.Checked);
				AssertEquals(false, control.HigherChargeableLowerRateCheckBox.ReadOnly);
				AssertEquals(true, control.HigherChargeableLowerRateCheckBox.Checked);

				SwitchToFilter(control.BreaksPerDropDown, BaseCombinedCalculator.Items.BreaksPerContainerTypeOrClass);
				AssertEquals(true, control.UseAccumulatedCheckbox.ReadOnly);
				AssertEquals(true, control.UseAccumulatedCheckbox.Checked);
				AssertEquals(true, control.HigherChargeableLowerRateCheckBox.ReadOnly);
				AssertEquals(false, control.HigherChargeableLowerRateCheckBox.Checked);
			}
		}

		void SwitchToFilter(ZDropEdit filterDescriptionDropEdit, string description)
		{
			filterDescriptionDropEdit.Focus();
			Application.DoEvents();
			filterDescriptionDropEdit.Text = description;
			filterDescriptionDropEdit.CommitBoundValue();
			TestKeyStrokeHelper.SendKeyToControl(filterDescriptionDropEdit, Keys.Tab);
			Application.DoEvents();
		}
	}
}
