using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business.Test
{
	public class ChargeViewModelTest : RatingTestCase
	{
		public void TestCalculatedFormulaToolTip()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();
			consol.JK_ConsolMode = "LCL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = ChooserHelper.AddPerContainerCharge(apiCosting1, "FR1", 100m, mapCodeWithCW1ChargeCode: true);
			var frtCharge2 = ChooserHelper.AddPerContainerCharge(apiCosting1, "FR2", 30m, mapCodeWithCW1ChargeCode: true);
			var frtCharge3 = ChooserHelper.AddPerContainerCharge(apiCosting1, "FR3", 3m, mapCodeWithCW1ChargeCode: true);

			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var convertedEntries = converter.Convert(response, null);
			var results = ChooserHelper.GetCalculatedResultFromEntries(convertedEntries, consol.RatingAdapter.OperationalJobCode);
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");
			var chooserRateEntry = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, convertedEntries, services, results);

			var convertedLines = convertedEntries.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();
			SetPriceByValue(convertedLines.First(l => l.WiseCharge == frtCharge1), "Per Container");
			SetPriceByValue(convertedLines.First(l => l.WiseCharge == frtCharge3), "Base Price");

			var charge1 = new ChargeViewModel(frtCharge1, convertedLines.First(l => l.WiseCharge == frtCharge1), chooserRateEntry, true);
			var charge2 = new ChargeViewModel(frtCharge2, convertedLines.First(l => l.WiseCharge == frtCharge2), chooserRateEntry, true);
			var charge3 = new ChargeViewModel(frtCharge3, convertedLines.First(l => l.WiseCharge == frtCharge3), chooserRateEntry, true);

			AssertEquals("Per Container", charge1.CalculatedFormulaToolTip);
			AssertEquals("", charge2.CalculatedFormulaToolTip);
			AssertEquals("Base Price", charge3.CalculatedFormulaToolTip);

			void SetPriceByValue(WiseLine line, string priceByValue)
			{
				line.CustomFields = new[] {
					new CustomField()
					{
						Code = Rate.CustomFields.CargoSphere.PriceBy,
						Value = priceByValue
					}
				};
			}
		}

		public void TestCalculatedFormulaAndTotalPriceVisibility_CW1()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();
			var costing = Helper.NewCosting(carrier);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			var rateLine1 = rateEntry.AddRateLine("FR1", calculatorCode: UnitCalculator.Code, lineUnit: QuantityUnit.CN, currencyCode: "AUD");
			var rateLine2 = rateEntry.AddRateLine("FR2", calculatorCode: UnitCalculator.Code, lineUnit: QuantityUnit.CN, currencyCode: "AUD");

			var logger = new ElementaryLogger();
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");
			var chooserRateEntry = new ChooserRateEntry(Factory, logger: logger, rateEntry, services, null);

			var charge1 = new ChargeViewModel(rateLine1, chooserRateEntry);
			var charge2 = new ChargeViewModel(rateLine2, chooserRateEntry);

			Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = false;
			AssertEquals(false, charge1.CalculatedFormulaVisibility);
			AssertEquals(false, charge2.CalculatedFormulaVisibility);

			Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = true;
			AssertEquals(true, charge1.CalculatedFormulaVisibility);
			AssertEquals(true, charge2.CalculatedFormulaVisibility);

			Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = false;
			AssertEquals(false, new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, new[] { charge1, charge2 }).TotalPriceVisibility);

			Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = true;
			AssertEquals(true, new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, new[] { charge1, charge2 }).TotalPriceVisibility);
		}

		public void TestCalculatedFormulaAndTotalPriceVisibility_WiseRates()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = ChooserHelper.AddPerContainerCharge(apiCosting1, "FR1", 100m, mapCodeWithCW1ChargeCode: true);
			var frtCharge2 = ChooserHelper.AddPerContainerCharge(apiCosting1, "FR2", 30m, mapCodeWithCW1ChargeCode: true);

			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var convertedEntries = converter.Convert(response, null);
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");
			var chooserRateEntry = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, convertedEntries, services, null);
			var convertedLines = convertedEntries.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var charge1 = new ChargeViewModel(frtCharge1, convertedLines.First(l => l.WiseCharge == frtCharge1), chooserRateEntry, true);
			var charge2 = new ChargeViewModel(frtCharge2, convertedLines.First(l => l.WiseCharge == frtCharge2), chooserRateEntry, true);

			Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = false;
			AssertEquals(false, charge1.CalculatedFormulaVisibility);
			AssertEquals(false, charge2.CalculatedFormulaVisibility);

			Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = true;
			AssertEquals(true, charge1.CalculatedFormulaVisibility);
			AssertEquals(true, charge2.CalculatedFormulaVisibility);

			Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = false;
			AssertEquals(false, new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, new[] { charge1, charge2 }).TotalPriceVisibility);

			Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = true;
			AssertEquals(true, new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, new[] { charge1, charge2 }).TotalPriceVisibility);
		}

		public void TestLocalCurrencyChange()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();
			consol.JK_ConsolMode = "LCL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var charges = new List<Charge>
			{
				ChooserHelper.AddFlatCharge(apiCosting1, "CH1", 100m, mapCodeWithCW1ChargeCode: true),
			};

			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var convertedEntries = converter.Convert(response, null);
			var results = ChooserHelper.GetCalculatedResultFromEntries(convertedEntries, consol.RatingAdapter.OperationalJobCode);
			var services = new DummyRateChooserServices(Factory, "HKD"); // HKD as missing currency
			var chooserRateEntry = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, convertedEntries, services, results);

			var convertedLines = convertedEntries.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var viewModel1 = CreateChargeViewModel(consol, converter, apiCosting1, charges[0], convertedLines, chooserRateEntry);
			var viewModel2 = CreateChargeViewModel(consol, converter, apiCosting1, charges[0], convertedLines, chooserRateEntry, "HKD");
			var viewModel3 = CreateChargeViewModel(consol, converter, apiCosting1, charges[0], convertedLines, chooserRateEntry, "NZD");

			AssertEquals("Local Currency is the same", "$100.00", viewModel1.CalculatedAmountString);
			AssertEquals("Missing Currency HKD", string.Empty, viewModel2.CalculatedAmountString);
			AssertEquals("Different Local Currency", "$50.00", viewModel3.CalculatedAmountString);
		}

		ChargeViewModel CreateChargeViewModel(
			Freight.Forwarding.Business.ForwardingConsol consol,
			WiseRatesConverter converter,
			Rate apiCosting,
			Charge charge,
			WiseLine[] convertedLines,
			ChooserRateEntry chooserRateEntry,
			string currency = null)
		{
			if (!string.IsNullOrEmpty(currency))
			{
				apiCosting.Charges.ForEach(c => c.Currency = currency);
				var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting });
				var convertedEntries = converter.Convert(response, null);
				var results = ChooserHelper.GetCalculatedResultFromEntries(convertedEntries, consol.RatingAdapter.OperationalJobCode);
				chooserRateEntry = new ChooserRateEntry(Factory, serviceProvider: chooserRateEntry.ServiceProvider, apiCosting, convertedEntries, chooserRateEntry.ChooserServices, results);
				convertedLines = convertedEntries.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();
			}

			return new ChargeViewModel(charge, convertedLines.First(l => l.WiseCharge == charge), chooserRateEntry, true);
		}

		public void TestMemoryLeak()
		{
			AssertContainsExactElementsInAnyOrder
			(
				"Static members should not exist because it cause memory leak",
				System.Array.Empty<string>(),
				typeof(ChargeViewModel).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Select(staticMember => staticMember.Name)
			);
		}

		#region Helper

		protected RateChooserTestHelper ChooserHelper
		{
			get { return helper ?? (helper = new RateChooserTestHelper(Factory)); }
		}
		RateChooserTestHelper helper;

		#endregion
	}
}
