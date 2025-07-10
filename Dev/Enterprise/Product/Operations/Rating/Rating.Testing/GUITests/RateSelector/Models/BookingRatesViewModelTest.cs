using System;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class BookingRatesViewModelTest : RatingTestCase
	{
		public void TestIgnoreInvalidRates()
		{
			var rates = new[]
			{
				new BookingEngineRateViewModel()
				{
					ReferenceNumber = "123456"
				},
				new BookingEngineRateViewModel()
				{
					TransportLegs = new ObservableCollection<TransportLegViewModel>()
					{
						new TransportLegViewModel()
					}
				},
				new BookingEngineRateViewModel()
				{
					ReferenceNumber = "987654",
					TransportLegs = new ObservableCollection<TransportLegViewModel>()
					{
						new TransportLegViewModel()
					}
				}
			};
			rates[1].FreightCharges.Add(new BookingEngineChargeViewModel());

			using (var viewModel = new BookingRatesViewModel(rates))
			{
				AssertEquals("Expected only 1 valid rate", 1, viewModel.Rates.Count);
				AssertNotNull("viewModel.Logger", viewModel.Logger);
				AssertEquals("Expected 2 log entries for invalid rates", 2, viewModel.Logger.Logs.Count());
				AssertEquals("First log message didn't match", "Invalid Rate, No Transport Leg: 123456", viewModel.Logger.Logs.ElementAt(0).Message);
				AssertEquals("Second log message didn't match", "Invalid Rate, No Charge included: 987654", viewModel.Logger.Logs.ElementAt(1).Message);
			}
		}

		public void TestPropertyChangesCalledWhenSettingSelectedRate()
		{
			using (var viewModel = new BookingRatesViewModel(Array.Empty<BookingEngineRateViewModel>()))
			{
				var propertyName = string.Empty;

				viewModel.PropertyChanged += (sender, args) => propertyName = args.PropertyName;
				viewModel.SelectedRate = null;

				AssertEquals(nameof(BookingRatesViewModel.SelectedRate), propertyName);
				AssertNotNull(nameof(viewModel.Logger), viewModel.Logger);
			}
		}

		public void TestPropertyChangesCalledWhenSettingViewMode()
		{
			using (var viewModel = new BookingRatesViewModel(Array.Empty<BookingEngineRateViewModel>()))
			{
				var count = 0;
				viewModel.PropertyChanged += (sender, args) => count++;
				viewModel.ViewMode = ViewMode.CardView;

				AssertEquals("Expected property change count to be 2", 2, count);
				AssertNotNull(nameof(viewModel.Logger), viewModel.Logger);
			}
		}

		public void TestSupportedSortOptions()
		{
			using (var viewModel = new BookingRatesViewModel(Array.Empty<BookingEngineRateViewModel>()))
			{
				var actual = viewModel.SupportedSortOptions.Select(x => x.PropertyName).ToArray();
				var expected = new[] { "TotalPriceAmount", "ArrivalTime", "DepartureTime" };

				AssertContainsExactElementsInAnyOrder(
					"Supported sort options should match the expected property names",
					expected,
					actual
				);
			}
		}

		public void TestShowSortOptions()
		{
			using (var viewModel = new BookingRatesViewModel(Array.Empty<BookingEngineRateViewModel>()))
			{
				viewModel.ViewMode = ViewMode.CardView;
				AssertEquals("ShowSortOptions should be true when ViewMode is CardView", true, viewModel.ShowSortOptions);

				viewModel.ViewMode = ViewMode.Warnings;
				AssertEquals("ShowSortOptions should be false when ViewMode is Warnings", false, viewModel.ShowSortOptions);
			}
		}
	}
}
