using System;
using System.Collections.Generic;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.RatingTests.Testing;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class ContainerGroupViewModelTest : BaseRatingIntegrationTest
	{
		public void TestBasicProperties()
		{
			var commodity = Helper.NewCommodity("MLRN", null);

			using (var viewModel = new ContainerGroupViewModel(Helper.Containers["LD-7"], commodity, 5))
			{
				viewModel.GoodsWeight = 500m;
				viewModel.GoodsVolume = 6.9m;

				AssertEquals("Commodity code should match", "MLRN", viewModel.CommodityCode);
				AssertEquals("Container code should match", "LD-7", viewModel.ContainerCode);
				AssertEquals("Container count should match", 5, viewModel.ContainerCount);
				AssertEquals("Header should match", "5 x LD-7 (MLRN)", viewModel.Header);
				AssertEquals("Goods weight for bonding should match", "500 KG", viewModel.GoodsWeightForBonding);
				AssertEquals("Goods volume for bonding should match", "6.9 M3", viewModel.GoodsVolumeForBonding);
			}
		}

		public void TestCanApply_RateIsNotSelected_ReturnFalse()
		{
			using (var viewModel = CreateViewModel())
			{
				viewModel.SelectedRate = null;
				AssertEquals("Expected CanApply to be false when no rate is selected", false, viewModel.CanApply);
			}
		}

		public void TestCanApply_RateIsSelectedButNotValid_ReturnFalse()
		{
			using (var viewModel = CreateViewModel())
			{
				((CargoguideRateViewModel)viewModel.Rates[0]).CarrierErrorLevel = ErrorLevel.Error;
				viewModel.SelectedRate = viewModel.Rates[0];
				AssertEquals("Expected CanApply to be false when rate is selected but not valid.", false, viewModel.CanApply);
			}
		}

		public void TestCanApply_RateIsSelectedAndValid_ReturnTrue()
		{
			using (var viewModel = CreateViewModel())
			{
				viewModel.SelectedRate = viewModel.Rates[0];
				AssertEquals("The CanApply property should be true when a rate is selected and valid.", true, viewModel.CanApply);
			}
		}

		public void TestRateIsNotSelectedText()
		{
			using (var viewModel = CreateViewModel())
			{
				viewModel.IsLoadingRates = true;
				AssertEquals("Loading rates...", viewModel.RateIsNotSelectedText);

				viewModel.IsLoadingRates = false;
				AssertEquals("Please select a rate", viewModel.RateIsNotSelectedText);

				viewModel.SelectedRate = viewModel.Rates[0];
				AssertNullOrEmpty(viewModel.RateIsNotSelectedText);

				viewModel.SelectedRate = null;
				viewModel.Rates.Clear();
				AssertEquals("No rates found", viewModel.RateIsNotSelectedText);
			}
		}

		public void TestSelectedRate_Setter_ShouldNotifyPropertiesChanged()
		{
			using (var viewModel = CreateViewModel())
			{
				var propertyChanges = new List<string>();
				viewModel.PropertyChanged += (s, e) => propertyChanges.Add(e.PropertyName);

				viewModel.SelectedRate = viewModel.Rates[0];

				AssertCollectionContains("Expected property change for SelectedRate", nameof(viewModel.SelectedRate), propertyChanges);
				AssertCollectionContains("Expected property change for RateIsNotSelectedText", nameof(viewModel.RateIsNotSelectedText), propertyChanges);
			}
		}

		public void TestIsLoadingRates_Setter_ShouldNotifyPropertiesChanged()
		{
			using (var viewModel = CreateViewModel())
			{
				var propertyChanges = new List<string>();
				viewModel.PropertyChanged += (s, e) => propertyChanges.Add(e.PropertyName);

				viewModel.IsLoadingRates = true;

				AssertCollectionContains(
					"Property change for IsLoadingRates should be raised",
					"IsLoadingRates",
					propertyChanges
				);

				AssertCollectionContains(
					"Property change for RateIsNotSelectedText should be raised",
					"RateIsNotSelectedText",
					propertyChanges
				);
			}
		}

		ContainerGroupViewModel CreateViewModel()
		{
			var commodity = Helper.NewCommodity("MLRN", null);
			var viewModel = new ContainerGroupViewModel(Helper.Containers["LD-7"], commodity, 5);
			viewModel.Rates.Add(CreateValidRate());
			viewModel.Rates.Add(CreateValidRate());

			return viewModel;
		}

		protected CargoguideRateViewModel CreateValidRate(string carrier = "EMIRATES", string container = "LD-7", string commodity = null, string commodityGroup = null)
		{
			var rate = new CargoguideRateViewModel();
			rate.Origin = "UAIEV";
			rate.Destination = "AUSYD";
			rate.CarrierCode = carrier;
			rate.ContainerType = container;
			rate.CarrierErrorLevel = ErrorLevel.None;
			rate.CarrierServiceLevelErrorLevel = ErrorLevel.None;
			rate.CommodityGroupErrorLevel = ErrorLevel.None;
			rate.Commodities = commodity;
			rate.CommodityGroups = !string.IsNullOrEmpty(commodityGroup) ? new[] { commodityGroup } : Array.Empty<string>();

			return rate;
		}
	}
}
