using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.Common;
using CargoWise.Common.Cache;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class BookingEngineRateViewModel : RateViewModel, IHasTransportLegs
	{
		readonly bool displayTotalInOriginalCurrency;

		public BookingEngineRateViewModel()
		{
		}

		public BookingEngineRateViewModel(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BookingEngineRateViewModel(BusinessObjectFactory factory, IBookingRate rate)
			: this(factory)
		{
			var charge = BookingEngineChargeViewModel.New(factory, rate);

			var transportLegs = rate
				.TransportLegs?
					.Select(TransportLegViewModel.New)
					.ToArray()
				?? Array.Empty<TransportLegViewModel>();

			MainCharge = Argument.NotNull(charge, nameof(charge));
			Currency = charge.Currency;
			FreightCharges.Add(charge);
			displayTotalInOriginalCurrency = charge.LocalAmount == 0 && !string.IsNullOrEmpty(charge.LocalAmountError);

			var sortedLegs = transportLegs.OrderBy(x => x.LegOrder).ToArray();
			TransportLegs = new ObservableCollection<TransportLegViewModel>(sortedLegs);

			if (rate.AdditionalDetails != null)
			{
				var additionalDetails =
					rate.AdditionalDetails.Select(
						rateAdditionaldetail => new AdditionalDetailsViewModel(rateAdditionaldetail));

				AdditionalDetails = new ObservableCollection<AdditionalDetailsViewModel>(additionalDetails);
			}
			else
			{
				AdditionalDetails = new ObservableCollection<AdditionalDetailsViewModel>();
			}

			if (rate.CostBreakdownCharges != null)
			{
				var costBreakdownCharges = rate.CostBreakdownCharges.Select(c => CostBreakdownChargeViewModel.New(factory, c, displayTotalInOriginalCurrency));
				CostBreakdownCharges = new ObservableCollection<CostBreakdownChargeViewModel>(costBreakdownCharges);
			}
			else
			{
				CostBreakdownCharges = new ObservableCollection<CostBreakdownChargeViewModel>();
			}

			if (sortedLegs.Length > 0)
			{
				Origin = sortedLegs[0].PortOfLoadingCode;
				DepartureTime = sortedLegs[0].EstimatedDeparture;

				var lastLeg = sortedLegs.Last();
				Destination = lastLeg.PortOfDischargeCode;
				ArrivalTime = lastLeg.EstimatedArrival;
			}

			PopulateCarrier(rate.CarrierPrefix);

			Remarks = rate.Remarks;
			Rate = rate;
		}

		public static BookingEngineRateViewModel New(BusinessObjectFactory factory, IBookingRate rate)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(rate, nameof(rate));

			return new BookingEngineRateViewModel(factory, rate);
		}

		public IBookingRate Rate { get; private set; }

		public string ReferenceNumber { get; set; }

		public string Remarks { get; set; }

		public DateTime DepartureTime { get; set; }

		public DateTime ArrivalTime { get; set; }

		public string RawRateXml { get; set; }

		Bitmap airlineIconBitmap;
		public Bitmap AirlineIconBitmap
		{
			get => airlineIconBitmap;
			set
			{
				airlineIconBitmap = value;
				OnPropertyChanged(nameof(AirlineIconBitmap));
			}
		}

		protected override Money TotalPriceMoney
		{
			get
			{
				if (displayTotalInOriginalCurrency)
				{
					var refCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Currency);

					// recalc total in origin currency
					var totalPrice = Charges.Sum(x => x.Charges.Sum(c => c.Amount));
					if (refCurrency == null)
					{
						// in the rare case of provided currency is not in the system
						return new Money(ZArchitecture.Core.Utilities.Round(totalPrice, 2), null);
					}

					// return total in origin currency
					return new Money(totalPrice, refCurrency);
				}

				return base.TotalPriceMoney;
			}
		}

		public ObservableCollection<AdditionalDetailsViewModel> AdditionalDetails { get; set; }

		public bool ShowAdditionalDetails
		{
			get
			{
				return AdditionalDetails?.Count > 0;
			}
		}

		public ObservableCollection<CostBreakdownChargeViewModel> CostBreakdownCharges { get; set; }

		public bool ShowCostBreakdownCharges
		{
			get
			{
				return CostBreakdownCharges?.Count > 0;
			}
		}

		public override object Clone()
		{
			return MemberwiseClone();
		}

		public override IEnumerable<AutoRateInfo> GetAutoRateInfos() => Enumerable.Empty<AutoRateInfo>();

		protected override void SetCharges(IEnumerable<AutoRateInfo> autoRateInfoCollection, string logs)
		{
		}

		public override bool DisplayPrice => true;

		public ChargeViewModel MainCharge { get; protected set; }

		void PopulateCarrier(string carrierPrefix)
		{
			if (!string.IsNullOrWhiteSpace(carrierPrefix))
			{
				var refAirline = RefAirline.LoadFromAirlinePrefix(Factory, carrierPrefix);
				if (refAirline == null)
				{
					var voyageNumber = TransportLegs.FirstOrDefault()?.VoyageNumber;
					CarrierCode = voyageNumber?.Substring(0, Math.Min(2, voyageNumber.Length));
				}
				else
				{
					CarrierCode = refAirline.RM_TwoCharacterCode;
					CarrierName = refAirline.RM_AirlineName1;
					AirlineIconBitmap = GetImageSourceFromCacheOrAirline(refAirline);
				}
			}

			if (string.IsNullOrEmpty(CarrierCode))
			{
				CarrierCode = "??"; // Invalid value
			}

			if (string.IsNullOrEmpty(CarrierName))
			{
				CarrierName = Res.GetString("665befaa-29af-4cf9-8386-7537aaa683cb", "The rate has no carrier");
			}
		}

		static Bitmap GetImageSourceFromCacheOrAirline(RefAirline airline)
		{
			if (airline?.RM_AirlineLogo.IsEmpty ?? true)
			{
				return null;
			}

			var cacheKey = $"RateSelector.Airline.{airline.RM_EagleAddedAirlinePrefixOrAccountingCode}.Logo"; // Cache key

			// cache the image, to prevent loading it everytime, we keep it only for a short period of time, as long as it is needed
			return MemoryCache.Default.GetOrAdd(cacheKey, () => airline.RM_AirlineLogo.ToBitmapImage(), new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMinutes(10) });
		}
	}
}
