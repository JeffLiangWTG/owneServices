#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Collections;
using Enterprise.Rating.Business;
using Urs.Api.Integration.Interfaces;
using WiseRates.Api.Model;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models;

public class BookingInfoDto
{
	public BookingInfoDto() { }

	public BookingInfoDto(BookingInfo bookingInfo, ITradeServiceDto tradeService)
	{
		SpotTerms = bookingInfo.BookingTerms?.Items.Select(term => new SpotTermDto(term)).ToList() ?? [];
		Deadlines = bookingInfo.Schedule?.ScheduleDetails.SelectMany(detail => detail.DateInfos).Select(deadline => new DeadlineDto(deadline)).ToList() ?? [];
		Penalties = tradeService.PriceInfo?.FreeTime?.Items.Select(PenaltyDto.TryCreate).WhereNotNull().Cast<PenaltyDto>().ToList() ?? [];
	}

	public List<SpotTermDto> SpotTerms { get; set; } = [];
	public List<DeadlineDto> Deadlines { get; set; } = [];
	public List<PenaltyDto> Penalties { get; set; } = [];

	public override string ToString() => string.Join("|",
		string.Join(",", SpotTerms),
		string.Join(",", Deadlines),
		string.Join(",", Penalties));
}

public class SpotTermDto
{
	public SpotTermDto() { }

	public SpotTermDto(BookingTermItem term)
	{
		Description = term.Name;
		Currency = term.Currency;
		Price = term.Fee;
	}

	public string Description { get; set; } = null!;
	public string Currency { get; set; } = null!;
	public decimal Price { get; set; }

	public override string ToString() => string.Join("|",
		Description,
		Currency,
		Price);
}

public class DeadlineDto
{
	public DeadlineDto() { }

	public DeadlineDto(ScheduleDateInfo deadline)
	{
		Code = deadline.Code;
		Description = deadline.Name;
		Type = deadline.Type;
		DateTime = deadline.Date;
	}

	public string Code { get; set; } = null!;
	public string Description { get; set; } = null!;
	public string Type { get; set; } = null!;
	public DateTime DateTime { get; set; }

	public override string ToString() => string.Join("|",
		Code,
		Description,
		Type,
		DateTime);
}

public class PenaltyDto
{
	public PenaltyDto() { }

	public static PenaltyDto? TryCreate(IBaseChargeDto charge)
	{
		var collection = charge.RateCollections.Items.First(); // FreeTime charges should only have one collection
		if (collection.PriceEntries.IsNullOrEmpty())
		{
			return null;
		}

		var process = UrsRatesParseHelper.MapShippingPhase(charge.ChargeDefinition.ShippingPhase.Code);
		var type = charge.ChargeDefinition.Code;
		var description = charge.ChargeDefinition.Description;

		var entries = collection.PriceEntries.ToList();
		var lastEntry = entries[entries.Count - 1];

		return new()
		{
			Tiers = entries
				.SelectSequencedPairs((cur, next) => new PenaltyTierDto
				{
					Process = process,
					Type = type,
					Description = description,
					StartDay = cur.BreakQuantity,
					EndDay = next.BreakQuantity - 1,
					Currency = collection.Currency,
					Price = cur.Price
				})
				.Append(new PenaltyTierDto
				{
					Process = process,
					Type = type,
					Description = description,
					StartDay = lastEntry.BreakQuantity,
					EndDay = null,
					Currency = collection.Currency,
					Price = lastEntry.Price
				})
				.ToArray()
		};
	}

	public PenaltyTierDto[] Tiers { get; set; } = [];

	public override string ToString() => string.Join(",", Tiers.AsEnumerable());
}

public class PenaltyTierDto
{
	public PenaltyTierDto() { }

	public string Process { get; set; } = null!;
	public string Type { set; get; } = null!;
	public string Description { get; set; } = null!;
	public decimal StartDay { get; set; }
	public decimal? EndDay { get; set; }
	public string Currency { get; set; } = null!;
	public decimal Price { get; set; }

	public override string ToString() => string.Join("|",
		Process,
		Type,
		Description,
		StartDay,
		EndDay,
		Currency,
		Price);
}
