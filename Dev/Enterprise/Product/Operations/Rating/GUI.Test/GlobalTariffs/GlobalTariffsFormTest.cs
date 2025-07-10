using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.Rating.GUI.Testing
{
	public class GlobalTariffsFormTest : RatingTestCase
	{
		public void TestValidateDropMode_GivenInvalidDropMode_ThenShouldShowDropModeErrorMessage()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costingRateEntry = costing.AddRateEntry(RateCategory.DST, "FCL", "AU", "SG", removeLines: true);
			var costingRateLine = costingRateEntry.AddRateLine("DDOC", CartageCalculator.Code, "KG");
			costingRateLine.Calculator.EquipmentType = LCLAIREquipmentNeeded.Premise;
			costingRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			Factory.Save();

			var companyTariff = Helper.NewCompanyTariff();
			var companyTariffRateEntry = companyTariff.AddRateEntry(RateCategory.DST, "FCL", "AU", "SG", removeLines: true);

			var companyTariffRateLine1 = companyTariffRateEntry.AddRateLine("DDOC", CartageCalculator.Code, "KG");
			companyTariffRateLine1.Calculator.EquipmentType = FCLEquipmentNeeded.WaitForUnpack;
			companyTariffRateLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			var companyTariffRateLine2 = companyTariffRateEntry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var companyTariffCalculator2 = (CompanyTariffOrCostBasedCalculator)companyTariffRateLine2.Calculator;
			companyTariffRateLine2.Calculator.EquipmentType = FCLEquipmentNeeded.SideLoader;

			Factory.Save();

			using (var form = new GlobalTariffsForm(companyTariff))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				// Show the calculators controls and SetCopyCaptionToPropertyHumanReadableNameForTest 
				form.BaseTabControl.SelectTabPage(RateCategory.DST);
				var destinationControl = form.BaseTabControl.FindTabPage(RateCategory.DST).RateLinesAndItemsControl;
				SetCopyCaptionToPropertyHumanReadableNameForTest(destinationControl);
				Application.DoEvents();
				destinationControl.RateLinesGrid_ForTest.ListManager.Position = 1;
				destinationControl.RateLinesGrid_ForTest.Select(1);
				SetCopyCaptionToPropertyHumanReadableNameForTest(destinationControl);
				Application.DoEvents();

				// Re-select the calculators controls to trigger CopyCaptionToPropertyHumanReadableName
				destinationControl.RateLinesGrid_ForTest.ListManager.Position = 0;
				destinationControl.RateLinesGrid_ForTest.Select(0);
				Application.DoEvents();
				destinationControl.RateLinesGrid_ForTest.ListManager.Position = 1;
				destinationControl.RateLinesGrid_ForTest.Select(1);
				Application.DoEvents();

				companyTariffRateLine2.Calculator.EquipmentType = "XXX";
				form.FireSaveButton();
				AssertHasError
				(
					"DropMode Error Message",
					companyTariffRateLine2.RateLineItems.Cast<RateLineItem>().Single(rateLineItem => rateLineItem.TM_Type == Calculator.Items.EquipmentType).TM_TextInfo,
					"Enter a valid Drop Mode."
				);
			}
		}

		static void SetCopyCaptionToPropertyHumanReadableNameForTest(Control control)
		{
			var extension = control.GetExtension<ZLabelCaptionRenderer>();
			if (extension != null)
			{
				extension.CopyCaptionToPropertyHumanReadableNameForTest = true;
			}

			foreach (Control childControl in control.Controls)
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(childControl);
			}
		}

		public void TestFiltersShouldBeEnabledInViewMode()
		{
			var costing = Helper.NewCompanyTariff();
			Factory.Save();

			using (var form = new GlobalTariffsForm(costing))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				var filterControl = (IReadOnlyToggleControl)form.rateEntryFilterStripControl;
				AssertEquals(false, filterControl.ReadOnly);
			}
		}
	}

	#region Form Basher

	[TestedType(typeof(GlobalTariffsForm))]
	public class CompanyTariffFormBasherTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (ZForm testForm = (ZForm)GetFormToBash())
			{
				AssertEquals("Company Tariff 1", testForm.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			Helper.IsMarkAsNeedingValidationSuspended = true;

			var tariff = Helper.NewFullyPopulatedCompanyTariff();

			Helper.IsMarkAsNeedingValidationSuspended = false;

			var tariffValidationSuspender = tariff.SuspendMarkingAsNeedingValidation();

			Factory.Save();

			tariffValidationSuspender.Dispose();

			return new GlobalTariffsForm(tariff) { ControllerID = ControllerIDs.GlobalRates };
		}

		#region Implementation

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}

	#endregion
}
