using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.Rating.GUI.RateSelection;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class BookingInfoViewModel : ViewModelWithNotificationBase, IHasTransportLegs
	{
		public static BookingInfoViewModel PopulateFromRate(Rate rate)
		{
			Argument.NotNull(rate, nameof(rate));

			var result = new BookingInfoViewModel();
			if (rate.BookingInfo == null)
			{
				result.BookingTerms = new ObservableCollection<BookingTermViewModel>();
				result.TransportLegs = new ObservableCollection<TransportLegViewModel>();
				result.Penalties = new ObservableCollection<PenaltyViewModel>();
				return result;
			}

			var bookingInfo = rate.BookingInfo;

			if (bookingInfo.Schedule != null)
			{
				result.DepartureTime = bookingInfo.Schedule.DepartureDate;
				result.ArrivalTime = bookingInfo.Schedule.ArrivalDate;
				result.Origin = bookingInfo.Schedule.ScheduleDetails.First().Origin;
				result.Destination = bookingInfo.Schedule.ScheduleDetails.Last().Destination;
				result.VesselName = bookingInfo.Schedule.VesselName;
				result.VoyageNumber = bookingInfo.Schedule.VoyageNumber;
				result.RateId = rate.ProviderRateId;

				result.TransportLegs = PopulateTransportLegs(bookingInfo.Schedule);
			}

			if (bookingInfo.Penalties != null)
			{
				result.Penalties =
					new ObservableCollection<PenaltyViewModel>
					(
						bookingInfo
						.Penalties
						.Select(item =>
							new PenaltyViewModel()
							{
								Direction = item.Direction,
								Type = item.Type,
								Name = item.Name,
								FreeTime = item.StartDay - 1,
								PerUnitRate = item.PerUnitRate,
								Currency = item.Currency
							})
					);
			}
			else
			{
				result.Penalties = new ObservableCollection<PenaltyViewModel>();
			}

			if (bookingInfo.BookingTerms != null)
			{
				result.BookingTerms =
					new ObservableCollection<BookingTermViewModel>
					(
						bookingInfo
						.BookingTerms
						.Items
						.Select(item =>
							new BookingTermViewModel()
							{
								Name = item.Name,
								Fee = item.Fee.ToString(),
								Currency = item.Currency
							})
					);
			}
			else
			{
				result.BookingTerms = new ObservableCollection<BookingTermViewModel>();
			}

			return result;
		}

		static ObservableCollection<TransportLegViewModel> PopulateTransportLegs(Schedule schedule)
		{
			Argument.NotNull(schedule, nameof(schedule));

			var result = new ObservableCollection<TransportLegViewModel>();

			if (schedule.ScheduleDetails == null)
			{
				return result;
			}

			var legOrder = 0;
			foreach (var item in schedule.ScheduleDetails)
			{
				legOrder++;
				var viewModel = new TransportLegViewModel()
				{
					LegOrder = legOrder,
					VoyageNumber = item.VoyageNumber,
					VesselName = item.VesselName,
					EstimatedDeparture = item.DepartureDate,
					EstimatedArrival = item.ArrivalDate,
					PortOfLoadingCode = item.Origin,
					PortOfDischargeCode = item.Destination,
					FlagCode = item.FlagCode,
					TradeLane = item.TradeLane,
					TransitTime = item.TransitTime,
					TransportMode = TransportMode.Sea,
					ShowDateInfos = false,
				};

				if (item.DateInfos?.Any() == true)
				{
					viewModel.DateInfos =
						new ObservableCollection<TransportLegDateInfoViewModel>
						(
							item
							.DateInfos
							.Select(info =>
								new TransportLegDateInfoViewModel()
								{
									Code = info.Code,
									Name = info.Name,
									Type = info.Type,
									Date = info.Date
								})
						);
					viewModel.ShowDateInfos = true;
				}

				result.Add(viewModel);
			}

			return result;
		}

		public string Origin { get; set; }
		public string Destination { get; set; }
		public DateTime DepartureTime { get; set; }
		public DateTime ArrivalTime { get; set; }
		public TimeSpan TransitTime => ArrivalTime - DepartureTime;
		public string VoyageNumber { get; set; }
		public string VesselName { get; set; }
		public string RateId { get; set; }

		public bool AllAreNullOrEmpty =>
			BookingTerms.IsNullOrEmpty() &&
			TransportLegs.IsNullOrEmpty() &&
			Penalties.IsNullOrEmpty();

		public bool IsSpotRate => TransportLegs != null;
		public bool ShowBookingTerms => IsSpotRate && !BookingTerms.IsNullOrEmpty();
		public bool ShowTransportLegs => IsSpotRate && !TransportLegs.IsNullOrEmpty();
		public bool ShowPenalties => !Penalties.IsNullOrEmpty();

		public ObservableCollection<BookingTermViewModel> BookingTerms { get; set; }
		public ObservableCollection<TransportLegViewModel> TransportLegs { get; set; }
		public ObservableCollection<PenaltyViewModel> Penalties { get; set; }
	}
}
