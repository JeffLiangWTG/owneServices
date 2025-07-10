using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.CarrierConnect;

public class AutoratedOneOffQuoteCreator(CreateJobDto dto) : AutoratedJobCreator(dto)
{
	public override BusinessObject CreateJob()
	{
		var booking = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
		booking.TransportMode = Dto.RateResult.TransportMode;
		booking.ContainerMode = Dto.RateResult.ContainerMode;
		booking.Origin = Dto.RateResult.Origin;
		booking.Destination = Dto.RateResult.Destination;
		booking.WeightUnit = Env.Registry.FreightWeightUnit;
		booking.Weight = ContainerTotalWeight;
		booking.VolumeUnit = Env.Registry.FreightVolumeUnit;
		booking.Volume = ContainerTotalVolume;
		booking.Chargeable = ContainerTotalChargeable;
		booking.StartDate = (ZDateTime)Dto.RateQuery.EffectiveDate;
		booking.EndDate = (ZDate)booking.StartDate.AddMonths(Env.Registry.Rating.QuoteValidityPeriod.Value);

		booking.Quote.CurrentOneOffQuote.Numbers.AddNewIfNotExist(
			CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, Dto.RateResult.CarrierContractNumber);

		booking.OH_Carrier = GetOrgHeader(CarrierOrgCode)!.PK;
		booking.CarrierServiceLevel = Dto.RateResult.CarrierServiceLevel ?? "STD";

		var commodities = Dto.RateQuery.JobInfo.Containers.Select(c => c.Commodity).Distinct().ToList();
		if (commodities.Count == 1)
		{
			booking.Commodity = commodities[0];
		}

		if (NewJobShouldHaveContainers)
		{
			AddContainers(booking);
		}

		AddChargesToBooking(booking);

		var quotedBooking = ViewQuotedBooking.LoadOrCreate(booking);
		return quotedBooking;
	}

	void AddContainers(QuotedBooking booking)
	{
		foreach (var containerDto in JobInfo.Containers)
		{
			var container = booking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = GetRefContainer(containerDto.ContainerType)!.PK;
			container.TC_ContainerCount = (ZShort)(containerDto.Count ?? 1);
		}
	}

	void AddChargesToBooking(QuotedBooking booking)
	{
		booking.TryLoadOrCreateJob();
		var job = (Job)booking.Job;
		var costCollection = job.Charges;

		AddCharges(costCollection);

		job.Dispose();
	}
}

