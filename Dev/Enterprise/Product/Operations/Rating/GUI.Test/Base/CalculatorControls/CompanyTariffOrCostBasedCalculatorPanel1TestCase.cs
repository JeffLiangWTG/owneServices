using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Rating.GUI.Testing
{
	class CompanyTariffOrCostBasedCalculatorPanel1TestCase : TestCaseWithFactory
	{
		public void TestDecimalPlaces()
		{
			var helper = new TestHelper(Factory);
			var ratingHeader = helper.NewClientRate(helper.NewOrgHeader());
			var entry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var rateLine = entry.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CostBasedCode);

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "3" }
			};
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				Form.Controls.Add(CalculatorPanel);
				Form.Show();
				Application.DoEvents();
				CalculatorPanel.SetDataBinding(ratingHeader, "");
				CalculatorPanel.ViewCalculatorForBinding = rateLine.ViewCalculatorForBinding;
				CalculatorPanel.BindTo = "AIRRateEntriesForBinding.RateLines.ViewCalculator";

				var percentageCalcEdit = GUITestHelper.FindControl<ZCalcEdit>(form.Controls, "PercentageCalcEdit");
				AssertEquals("percentageCalcEdit.DecimalPlaces", 3, percentageCalcEdit.DecimalPlaces);
				AssertEquals("percentageCalcEdit.Decimals", 3, percentageCalcEdit.Decimals);

				var unitPercentageChangeCalcEdit = GUITestHelper.FindControl<ZCalcEdit>(form.Controls, "UnitPercentageChangeCalcEdit");
				AssertEquals("unitPercentageChangeCalcEdit.DecimalPlaces", 3, unitPercentageChangeCalcEdit.DecimalPlaces);
				AssertEquals("unitPercentageChangeCalcEdit.Decimals", 3, unitPercentageChangeCalcEdit.Decimals);
			}
		}

		public void TestDisplay_UnitPriceChange()
		{
			Form.Controls.Add(CalculatorPanel);
			Form.Show();
			Application.DoEvents();

			var unitPercentageChangeCalcEdit = GUITestHelper.FindControl<ZCalcEdit>(form.Controls, "UnitPercentageChangeCalcEdit");
			AssertNotNull("unitPercentageChangeCalcEdit should exists", unitPercentageChangeCalcEdit);
		}

		public void TestIncreaseDecreaseCharge_ReadOnlyControls()
		{
			Form.Controls.Add(CalculatorPanel);
			Form.Show();
			Application.DoEvents();
			RateUpdater.IncreaseDecreaseCharge = true;
			CalculatorPanel.SetDataBinding(RateUpdater, "");

			AssertEquals("PercentageCalcEdit writable", false, CalculatorPanel.PercentageCalcEdit.GetReadOnly());
			AssertEquals("BasePriceCalcEdit writable", false, CalculatorPanel.BasePriceCalcEdit.GetReadOnly());
			AssertEquals("UnitPriceCalcEdit writable", false, CalculatorPanel.UnitPriceCalcEdit.GetReadOnly());
			AssertEquals("MinimumCalcEdit writable", false, CalculatorPanel.MinimumCalcEdit.GetReadOnly());
			AssertEquals("CalculationOrderDropDown writable", false, CalculatorPanel.CalculationOrderDropDown.GetReadOnly());
		}

		#region Test Classes

		class TestCompanyTariffOrCostBasedCalculatorPanel1 : CompanyTariffOrCostBasedCalculatorPanel1
		{
			public new ZCalcEdit UnitPriceCalcEdit
			{
				get { return base.UnitPriceCalcEdit; }
			}

			public new ZDropEdit CalculationOrderDropDown
			{
				get { return base.CalculationOrderDropDown; }
			}

			public new ZCalcEdit PercentageCalcEdit
			{
				get { return base.PercentageCalcEdit; }
			}

			public new ZCalcEdit MinimumCalcEdit
			{
				get { return base.MinimumCalcEdit; }
			}

			public new ZCalcEdit BasePriceCalcEdit
			{
				get { return base.BasePriceCalcEdit; }
			}
		}

		#endregion

		#region Implementation

		BulkRateUpdater RateUpdater
		{
			get
			{
				if (rateUpdater == null)
				{
					rateUpdater = new BulkRateUpdater();
				}
				return rateUpdater;
			}
		}
		BulkRateUpdater rateUpdater;

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm();
				}
				return form;
			}
		}
		ZForm form;

		TestCompanyTariffOrCostBasedCalculatorPanel1 CalculatorPanel
		{
			get
			{
				if (calculatorPanel == null)
				{
					calculatorPanel = new TestCompanyTariffOrCostBasedCalculatorPanel1();
				}
				return calculatorPanel;
			}
		}
		TestCompanyTariffOrCostBasedCalculatorPanel1 calculatorPanel;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
