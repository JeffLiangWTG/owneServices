using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.CarrierConnect;

public class AutoratedBookingWithQuoteCreator(CreateJobDto dto) : AutoratedJobCreator(dto)
{
	public override BusinessObject CreateJob()
	{
		var booking = QuotedBooking.New(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
		booking.TransportMode = Dto.RateResult.TransportMode;
		booking.ContainerMode = Dto.RateResult.ContainerMode;
		booking.LoadPort = Dto.RateResult.Origin;
		booking.DischargePort = Dto.RateResult.Destination;
		booking.WeightUnit = Env.Registry.FreightWeightUnit;
		booking.Weight = ContainerTotalWeight;
		booking.VolumeUnit = Env.Registry.FreightVolumeUnit;
		booking.Volume = ContainerTotalVolume;
		booking.Chargeable = ContainerTotalChargeable;

		booking.ETD = (ZDateTime)Dto.RateQuery.EffectiveDate;

		booking.Booking.Numbers.AddNewIfNotExist(
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

		booking.Booking.OuterPackLines.RemoveAll();

		var quotedBooking = ViewQuotedBooking.LoadOrCreate(booking);
		return quotedBooking;
	}

	void AddContainers(QuotedBooking booking)
	{
		foreach (var containerDto in JobInfo.Containers)
		{
			var container = booking.QuotedBookingContainers.AddNew();
			container.JC_RC = GetRefContainer(containerDto.ContainerType)!.PK;
			container.JC_ContainerCount = (ZShort)(containerDto.Count ?? 1);
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


