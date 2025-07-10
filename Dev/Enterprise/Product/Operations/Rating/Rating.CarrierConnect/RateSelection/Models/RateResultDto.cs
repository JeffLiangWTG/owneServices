#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	[DebuggerDisplay("{Origin}-{Destination}-{TransportMode}-{ContainerMode}-{CarrierContractNumber}")]
	public sealed class RateResultDto
	{
		public RateResultDto() { }

		internal RateResultDto(RatingCriteria criteria, RateGroup rateGroup, AutoRateInfoCollection preprocessedCharges, ICollection<RateChargeDto> charges)
		{
			ResultId = Guid.NewGuid();
			Source = RateSource.CargoWise;
			SourcePKs = preprocessedCharges.Select(e => e.Line.ParentRateEntry.PK.ToGuid()).Distinct().ToArray();
			Origin = criteria.OriginCode;
			Destination = criteria.DestinationCode;
			TransportMode = criteria.FreightMode.ToTransportMode();
			ContainerMode = criteria.ContainerMode;
			ContainerTypes = charges
				.Select(charge => charge.ContainerType)
				.Where(str => !string.IsNullOrEmpty(str))
				.Distinct()
				.ToArray();

			UnmappedContainerTypes = rateGroup.RateEntries
				.OfType<UrsRateEntry>()
				.Where(entry => entry.UrsContainer.HasValue && !entry.UrsContainer.Value.IsMapped)
				.Select(entry => entry.UrsContainer?.Code ?? string.Empty)
				.Distinct()
				.ToArray();

			CarrierServiceLevel = rateGroup.CarrierServiceLevel;
			ServiceLevel = rateGroup.ServiceLevel;
			UniversalServiceLevel = rateGroup.UniversalServiceLevel;
			CarrierContractNumber = rateGroup.ContractNumber;

			// Organisations
			Carrier = rateGroup.Carrier;
			Consignee = rateGroup.Consignee;
			Consignor = rateGroup.Consignor;
			ControllingCustomer = rateGroup.ControllingCustomer;
			NamedAccounts = rateGroup.ControllingCustomer != null ? [rateGroup.ControllingCustomer.OrgCode] : [];
			ServiceProvider = rateGroup.ServiceProvider is null ? new () : new (rateGroup.ServiceProvider);

			var startDate = rateGroup.RateEntries.Max(x => x.TI_RateStartDate);
			var endDate = rateGroup.RateEntries.Min(x => x.TI_RateEndDate);
			StartDate = startDate.IsEmpty ? null : startDate.ToDateTime();
			EndDate = endDate.IsEmpty ? null : endDate.ToDateTime();

			Route = rateGroup.RateEntries[0] switch
			{
				UrsRateEntry entry => entry.Route.ToArray(),
				_ => new[] { criteria.OriginCode, new ZString(rateGroup.Via), criteria.DestinationCode }
						.Where(port => !port.IsEmpty)
						.Select(port => port.ToString())
						.ToArray(),
			};

			TransitTime = rateGroup.TransitTime;
			AircraftType = rateGroup.AircraftType;
			OriginPostCode = rateGroup.OriginPostCode;
			DestinationPostCode = rateGroup.DestinationPostCode;
			Frequency = rateGroup.Frequency;
			FrequencyUnit = rateGroup.FrequencyUnit;
			IsNonOperatedReefer = rateGroup.IsNonOperatedReefer;
			FirstLoad = rateGroup.FirstLoad;
			LastDischarge = rateGroup.LastDischarge;
			FirstRouteSetLoad = rateGroup.FirstRouteSetLoad;
			LastRouteSetDischarge = rateGroup.LastRouteSetDischarge;

			Charges = charges;

			if (rateGroup.RateEntries[0] is UrsRateEntry ursEntry)
			{
				if (TransportMode == Core.Constants.TransportModes.Air)
				{
					Source = RateSource.Cargoguide;
					PaymentTerm = ursEntry.TI_PaymentTerm;
					Deck = rateGroup.Deck;
					ProductCode = rateGroup.ProductCode;
					UniversalCommodityGroup = new (ursEntry);

					if (ContainerMode == Core.Constants.ContainerModes.ULD)
					{
						PayloadInfo = rateGroup.RateEntries
							.Cast<WiseEntry>()
							.Where(entry => entry.Container is not null)
							.DistinctBy(entry => entry.Container.PK)
							.ToDictionary(entry => entry.Container.RC_Code.ToString(), entry => new PayloadDto(entry));
					}
				}
				else if (TransportMode == Core.Constants.TransportModes.Sea)
				{
					Source = RateSource.CargoSphere;
					BookingInfo = rateGroup.RateEntries
						.OfType<UrsRateEntry>()
						.Where(entry => entry.Container is not null && entry.BookingInfo is not null)
						.DistinctBy(entry => entry.Container.PK)
						.ToDictionary(entry => entry.Container.RC_Code.ToString(), entry => new BookingInfoDto(entry.BookingInfo!, ursEntry.TradeService));

					if (ursEntry.BookingInfo?.Schedule is not null)
					{
						Schedule = new (ursEntry, ursEntry.BookingInfo?.Schedule);
					}

					Vessel = rateGroup.Vessel;
					Tradelane = rateGroup.Tradelane;
					ServiceString = rateGroup.ServiceString;
					CarrierQuoteNumber = ursEntry.CarrierQuoteNumber;
					NamedAccounts = ursEntry.NamedAccounts.ToArray();
				}

				PerContainerCommodity = rateGroup.RateEntries
					.OfType<UrsRateEntry>()
					.GroupBy(r => new { r.Container?.RC_Code, r.TI_RH_NKCommodityCode, r.ContainerQuality })
					.Select(g => new PerContainerCommodityDto
					{
						Container = g.Key.RC_Code,
						Commodity = g.Key.TI_RH_NKCommodityCode,
						CarrierCommodity = g.Where(r => r.CarrierSpecificCommodity is not null)
							.Select(r => new CarrierCommodityDto(r.CarrierSpecificCommodity!, TransportMode))
							.FirstOrDefault(),
						ContainerQuality = string.IsNullOrEmpty(g.Key.ContainerQuality)
							? null
							: g.Key.ContainerQuality,
						ChargeableFactor = TransportMode == Core.Constants.TransportModes.Air
							? g.FirstOrDefault(r => !r.ChargeableFactor.IsEmpty)?.ChargeableFactor
							: null,
						Remarks = TransportMode == Core.Constants.TransportModes.Air
							? g.FirstOrDefault(r => !r.Remarks.IsEmpty)?.Remarks
							: null,
						AddOn = TransportMode == Core.Constants.TransportModes.Sea
							? g.FirstOrDefault(r => !r.AddOn.IsEmpty)?.AddOn
							: null,
						RateType = TransportMode == Core.Constants.TransportModes.Sea
							? g.FirstOrDefault(r => !r.RateType.IsEmpty)?.RateType
							: null,
						RateType2 = TransportMode == Core.Constants.TransportModes.Sea
							? g.FirstOrDefault(r => !r.RateType2.IsEmpty)?.RateType2
							: null
					})
					.ToArray();
			}
		}

		public Guid ResultId { get; set; }

		public RateSource Source { get; init; }

		public Guid[] SourcePKs { get; init; } = [];

		public string Origin { get; init; } = null!;

		public string Destination { get; init; } = null!;

		public string TransportMode { get; init; } = null!;

		public string ContainerMode { get; init; } = null!;

		public string[] ContainerTypes { get; init; } = [];

		public string[] UnmappedContainerTypes { get; init; } = [];

		public string? UniversalServiceLevel { get; init; }

		public string? CarrierServiceLevel { get; init; }

		public string? ServiceLevel { get; init; }

		public string? CarrierContractNumber { get; init; }

		public OrganisationDto? Carrier { get; init; }

		public CarrierDto ServiceProvider { get; init; } = null!;

		public OrganisationDto? Consignee { get; init; }

		public OrganisationDto? Consignor { get; init; }

		public OrganisationDto? ControllingCustomer { get; init; }

		public string[]? NamedAccounts { get; init; }

		public string[] Route { get; init; } = [];

		public DateTime? StartDate { get; init; }

		public DateTime? EndDate { get; init; }

		public string? TransitTime { get; init; }

		public string? VoyageOrFlightNumber { get; init; }

		public DateTime? DepartureTime { get; init; }

		public DateTime? ArrivalTime { get; init; }

		public string? AircraftType { get; init; }

		public string? OriginPostCode { get; init; }

		public string? DestinationPostCode { get; init; }

		public ICollection<RateChargeDto> Charges { get; init; } = [];

		public int? Frequency { get; init; }

		public string? FrequencyUnit { get; init; }

		public string? IsNonOperatedReefer { get; init; }

		public CarrierCommodityDto? CarrierCommodity { get; init; }

		public string? FirstLoad { get; set; }

		public string? LastDischarge { get; set; }

		public string? FirstRouteSetLoad { get; set; }

		public string? LastRouteSetDischarge { get; set; }

		#region Cargoguide (AIR) Only Fields

		public string? PaymentTerm { get; init; }

		public string? Deck { get; init; }

		public string? ProductCode { get; init; }

		public CommodityDto? UniversalCommodityGroup { get; init; }

		#endregion

		#region CargoSphere (SEA) Only Fields

		public Dictionary<string, PayloadDto> PayloadInfo { get; init; } = [];

		public string? Vessel { get; init; }

		public string? IncoTerm { get; init; }

		public string? CoLoad { get; init; }

		public string? Tradelane { get; init; }

		public string? ServiceString { get; init; }

		public ScheduleDto? Schedule { get; init; }

		public string? CarrierQuoteNumber { get; init; }

		public Dictionary<string, BookingInfoDto> BookingInfo { get; init; } = [];

		#endregion

		#region Per Container/Commodity fields

		public PerContainerCommodityDto[] PerContainerCommodity { get; set; } = [];

		#endregion

		public enum RateSource
		{
			CargoWise,
			Cargoguide,
			CargoSphere
		}
	}
}
