using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class BookingRatesViewModel : SortableRatesViewModel
	{
		public BookingRatesViewModel(IReadOnlyCollection<BookingEngineRateViewModel> rates) : base()
		{
			foreach (var rate in rates)
			{
				if (IsValidRate(rate, out var errorMessage))
				{
					Rates.Add(rate);
				}
				else
				{
					Logger.Log(LogType.Error, errorMessage);
				}
			}
			ReSort();

			SupportedSortOptions.Add(new SortOptionViewModel
			{
				PropertyName = nameof(BookingEngineRateViewModel.DepartureTime),
				DisplayName = Res.GetString("77ab93bb-190a-40fc-968d-a9f3aa0e2214", "Departure"),
			});
			SupportedSortOptions.Add(new SortOptionViewModel
			{
				PropertyName = nameof(BookingEngineRateViewModel.ArrivalTime),
				DisplayName = Res.GetString("1602b3cf-b629-43a6-a9c1-232b51d8dfea", "Arrival"),
			});
		}

		// Legacy WPF rates collection. In Winforms, use SortedRates
		public ObservableCollection<BookingEngineRateViewModel> Rates { get; } = new ObservableCollection<BookingEngineRateViewModel>();

		public ViewMode ViewMode
		{
			get { return viewMode; }
			set
			{
				viewMode = value;
				OnPropertyChanged(nameof(ViewMode));
				OnPropertyChanged(nameof(ShowSortOptions));
			}
		}

		public BookingEngineRateViewModel SelectedRate
		{
			get
			{
				return selectedRate;
			}
			set
			{
				selectedRate = value;
				OnPropertyChanged(nameof(SelectedRate));
			}
		}

		static bool IsValidRate(BookingEngineRateViewModel rate, out string errorMessage)
		{
			if ((rate.TransportLegs?.Count ?? 0) == 0)
			{
				errorMessage = (NoResString)"Invalid Rate, No Transport Leg: " + rate.ReferenceNumber; // Debug error message
				return false;
			}

			if (!rate.FreightCharges.Charges.Any())
			{
				errorMessage = (NoResString)"Invalid Rate, No Charge included: " + rate.ReferenceNumber; // Debug error message
				return false;
			}

			errorMessage = null;
			return true;
		}

		protected override object GetRatesSource() => Rates;

		public override bool ShowSortOptions => ViewMode == ViewMode.CardView;

		protected override object GetSortProperty(object o, SortOptionViewModel selectedSort)
		{
			var vm = (BookingEngineRateViewModel)o;
			switch (selectedSort.PropertyName)
			{
				case nameof(vm.TotalPriceAmount):
					// From the base constructor on SortableRatesViewModel
					return vm.TotalPriceAmount;
				case nameof(vm.DepartureTime):
					// From this constructor
					return vm.DepartureTime;
				case nameof(vm.ArrivalTime):
					// From this constructor
					return vm.ArrivalTime;
				default:
					return null; // unrecognised, don't sort
			}
		}

		BookingEngineRateViewModel selectedRate;
		ViewMode viewMode;
	}
}
