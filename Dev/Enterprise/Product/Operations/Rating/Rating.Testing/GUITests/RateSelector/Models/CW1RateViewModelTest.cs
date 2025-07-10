using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Moq;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class CW1RateViewModelTest : RateViewModelTest
	{
		public void TestCalculate_PopulateChargeCollections()
		{
			var costing = Helper.NewCosting(null);

			// Valid charge
			var freightEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "UAIEV");
			freightEntry.RateLines.RemoveAndDeleteAll();
			var frtLine = freightEntry.AddFlatRateLine("FRT", 100, "UAH");

			// A charge which can't be calculated but still should be displayed
			var originEntry = costing.AddRateEntry("ORG", "AIR", "AUSYD", "UAIEV");
			originEntry.RateLines.RemoveAndDeleteAll();
			var bafLine = originEntry.AddRateLine("BAF", "CMB", "KG", "UAH");
			var bafCalculator = bafLine.GetCalculator<CombinedCalculator>();
			bafCalculator["-100"] = (ZDecimal)10m;
			bafCalculator["+100"] = (ZDecimal)9m;
			var restrictedItem = bafCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 1000, 0);
			restrictedItem.TM_CallForPricing = true;
			restrictedItem.TM_Text = "Call for Price";

			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.AIR, 1666, 9, null);
			criteria.IsManualCostSelectMode = true;

			var viewModel = new CW1RateViewModel(GetContext(), criteria, ZGuid.Empty, ZString.Empty, new[] { frtLine, bafLine }, DisposableAction.NoAction);
			viewModel.Calculate(criteria, criteria.Creditors?.ChargeCodeGroups);

			var freightCharges = viewModel.FreightCharges.Charges.Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}|{c.ChargeCodeError ?? "null"}").ToArray();
			var expectedFreightCharges = new[]
			{
				"FRT|100|UAH|null"
			};
			AssertContainsExactElementsInAnyOrder("Freight charges should match the expected values.", expectedFreightCharges, freightCharges);

			var otherCharges = viewModel.OtherCharges.Charges.Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}|{c.ChargeCodeError}").ToArray();
			var expectedOtherCharges = new[]
			{
				"BAF|0|UAH|Calculation failed due to Greater than 1000 Kilogram(s) Call for Price"
			};
			AssertContainsExactElementsInAnyOrder("Other charges should match the expected values.", expectedOtherCharges, otherCharges);
		}

		public void TestCalculate_CSTCalculator_ShouldNotThrowException()
		{
			var standardCosting = Helper.NewCosting(null);
			standardCosting.AddRateEntryWithUnitRateLine("AIR", "LSE", "AUSYD", "UAIEV", "FRT", 555, "KG");

			var costing = Helper.NewCosting(TransportProvider1);
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10;

			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.LSE, 1000, 1, null);
			criteria.Carrier = TransportProvider1;
			criteria.IsManualCostSelectMode = true;

			var viewModel = new CW1RateViewModel(GetContext(), criteria, ZGuid.Empty, ZString.Empty, new[] { line }, DisposableAction.NoAction);
			AssertNoExceptionThrown(() => viewModel.Calculate(criteria, criteria.Creditors?.ChargeCodeGroups));
		}

		public void TestFieldMapping()
		{
			var rate = (CW1RateViewModel)GetInstance();

			AssertNotNull(rate);
			AssertEquals("SMD", rate.TransitTime);
			AssertEquals("STD", rate.CarrierServiceLevel);
			AssertEquals("ABCD123456", rate.ContractNumber);
			AssertEquals("AUSYD", rate.Origin);
			AssertEquals("UAIEV", rate.Destination);
			AssertEquals("GEN", rate.Commodities);
		}

		RateSelectorContext GetContext()
		{
			return new RateSelectorContext
			{
				Factory = Factory,
				Filters = GetValidFilters(),
				Logger = new MemoryLogger(),
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory),
				DialogService = new Mock<IDialogService>().Object
			};
		}

		protected override RateViewModel GetInstance()
		{
			var costing = Helper.NewCosting(null);

			var freightEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "UAIEV");
			freightEntry.TI_TransitTime = "SMD";
			freightEntry.TI_PL_NKCarrierServiceLevel = "STD";
			freightEntry.TI_ContractNumber = "ABCD123456";

			var frtLine = freightEntry.AddFlatRateLine("FRT", 100, "UAH");

			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.AIR, 66, 9, null);
			var viewModel = new CW1RateViewModel(GetContext(), criteria, ZGuid.Empty, ZString.Empty, new[] { frtLine }, DisposableAction.NoAction);
			return viewModel;
		}
	}
}
