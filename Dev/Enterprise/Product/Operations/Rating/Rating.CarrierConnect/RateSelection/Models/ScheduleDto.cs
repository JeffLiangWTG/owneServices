using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Model = WiseRates.Api.Model;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models;

public class ScheduleDto
{
	public ScheduleLegDto[] ScheduleLegs { get; set; } = [];

	public string RateId { get; set; }

	public ScheduleDto() { }

	public ScheduleDto(UrsRateEntry entry, Model.Schedule schedule)
	{
		ScheduleLegs = schedule.ScheduleDetails.Select(detail => new ScheduleLegDto(detail)).ToArray();
		RateId = entry.BookingInfo.UrsSchedule.ExternalPriceReference;
	}

	public override string ToString() => string.Join("|", RateId, string.Join(",", ScheduleLegs.AsEnumerable()));
}

public class ScheduleLegDto
{
	public ScheduleLegDto() { }

	public ScheduleLegDto(Model.ScheduleDetail scheduleDetail)
	{
		var factory = new BusinessObjectFactory();

		Departure = scheduleDetail.Origin;
		DepartureDate = scheduleDetail.DepartureDate;
		Arrival = scheduleDetail.Destination;
		ArrivalDate = scheduleDetail.ArrivalDate;
		Vessel = scheduleDetail.VesselName;
		VesselNumber = scheduleDetail.IMONumber;
		VoyageNumber = scheduleDetail.VoyageNumber;
		ServiceCode = scheduleDetail.ServiceCode;
		ServiceName = scheduleDetail.ServiceName;

		var country = RefCountry.LoadFromCountryCode(factory, scheduleDetail.FlagCode);
		if (country != null)
		{
			FlagName = country.RN_Desc;
		}
		FlagCode = scheduleDetail.FlagCode;
	}

	public string Departure { get; set; }

	public DateTime DepartureDate { get; set; }

	public string Arrival { get; set; }

	public DateTime ArrivalDate { get; set; }

	public string Vessel { get; set; }

	public string VesselNumber { get; set; }

	public string VoyageNumber { get; set; }

	public string ServiceCode { get; set; }

	public string ServiceName { get; set; }

	public string FlagName { get; set; }

	public string FlagCode { get; set; }

	public override string ToString() => string.Join("|",
		Departure,
		DepartureDate,
		Arrival,
		ArrivalDate,
		Vessel,
		VesselNumber,
		VoyageNumber,
		ServiceCode,
		ServiceName,
		FlagName,
		FlagCode);
}
