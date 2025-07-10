using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Rating.Business.RateCommodityFMCPairProvider;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(RateCommodityFMCSelectorForm))]
	internal class RateCommodityFMCSelectorFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
			=> new RateCommodityFMCSelectorForm(new RateCommodityFMCSelectorViewModel());

		public void TestGridColumns()
		{
			var companyTariffRateSelectorViewModel = new RateCommodityFMCSelectorViewModel();
			companyTariffRateSelectorViewModel.CompanyTariffs.Add
			(
				new RateCommodityFMCViewModel(commodity: "GEN", description: "Commodity Description", localCode: "Commodity local dode", fmcTariffId: "001", rateSource: RateSources.Code.CompanyTariff)
			);
			using (var form = new RateCommodityFMCSelectorForm(companyTariffRateSelectorViewModel))
			{
				var grid = form.FindSingle<ZGrid>("zGrid1");

				AssertContainsExactElementsInAnyOrder
				(
					"Grid columns",
					new[] { "Commodity", "LocalCode", "FMCTariffID", "Description", "RateSource" },
					grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName)
				);
				AssertEquals("ReadOnly", false, grid.ReadOnly);
			}
		}

		#region buttons

		public void TestYesButton()
		{
			var companyTariffRateSelectorViewModel = new RateCommodityFMCSelectorViewModel();
			var companyTariff1 = new RateCommodityFMCViewModel(commodity: "GEN", description: "GEN Commodity Description", localCode: "GEN Commodity local dode", fmcTariffId: "001", rateSource: RateSources.Code.CompanyTariff);
			var companyTariff2 = new RateCommodityFMCViewModel(commodity: "HAZ", description: "HAZ Commodity Description", localCode: "HAZ Commodity local dode", fmcTariffId: "002", rateSource: RateSources.Code.CompanyTariff);
			companyTariffRateSelectorViewModel.CompanyTariffs.AddRange(companyTariff1, companyTariff2);

			AssertButton
			(
				companyTariffRateSelectorViewModel,
				selectedRow: 1,
				buttonName: "yesButton",
				expectedTotalRows: 2,
				expectedPair: companyTariff2.CommodityPair,
				expectedIsCancel: false
			);
		}

		public void TestNoButton()
		{
			var companyTariffRateSelectorViewModel = new RateCommodityFMCSelectorViewModel();
			var companyTariff1 = new RateCommodityFMCViewModel(commodity: "GEN", description: "GEN Commodity Description", localCode: "GEN Commodity local dode", fmcTariffId: "001", rateSource: RateSources.Code.CompanyTariff);
			var companyTariff2 = new RateCommodityFMCViewModel(commodity: "HAZ", description: "HAZ Commodity Description", localCode: "HAZ Commodity local dode", fmcTariffId: "002", rateSource: RateSources.Code.CompanyTariff);
			companyTariffRateSelectorViewModel.CompanyTariffs.AddRange(companyTariff1, companyTariff2);

			AssertButton
			(
				companyTariffRateSelectorViewModel,
				selectedRow: 1,
				buttonName: "noButton",
				expectedTotalRows: 2,
				expectedPair: null,
				expectedIsCancel: false
			);
		}

		static void AssertButton(RateCommodityFMCSelectorViewModel viewModel, int selectedRow, string buttonName, int expectedTotalRows, CommodityFMCPair expectedPair, bool expectedIsCancel)
		{
			using (var form = new RateCommodityFMCSelectorForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var grid = form.FindSingle<ZGrid>("zGrid1");
				AssertEquals("Grid list", expectedTotalRows, grid.List.Count);

				grid.ListManager.Position = selectedRow;

				var button = form.FindSingle<ZButton>(buttonName);
				button.PerformClick();

				AssertEquals("Selected commodity", expectedPair?.CommodityCode, viewModel.SelectedPair?.Commodity);
				AssertEquals("Selected FMCTariffID", expectedPair?.FMCTariffID, viewModel.SelectedPair?.FMCTariffID);
			}
		}

		#endregion
	}
}
