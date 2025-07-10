using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Common;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
namespace Enterprise.Rating.CarrierConnect.Test;

public class AutoratedBookingWithQuoteTest : AutoratedJobCreatorTest
{
	public void TestCreateBookingWithQuote()
	{
		var effectiveOn = new ZDate(2020, 1, 1);
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var appliedCharge = JobTestHelpers.CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var dto = JobTestHelpers.CreateJobDto(abcOrg, effectiveOn, appliedCharge);

		var viewQuotedBooking = new AutoratedBookingWithQuoteCreator(dto).CreateJob() as ViewQuotedBooking;
		var bwq = viewQuotedBooking.QuotedBooking;

		var cusNumbers = bwq.Booking.Numbers.Cast<CusEntryNumber>();

		// Assert
		CombineAssertions(() =>
		{
			AssertNotNull(bwq);
			AssertEquals(dto.RateResult.TransportMode, bwq.TransportMode);
			AssertEquals(dto.RateResult.ContainerMode, bwq.ContainerMode);
			AssertEquals(dto.RateResult.Origin, bwq.LoadPort);
			AssertEquals(dto.RateResult.Destination, bwq.DischargePort);
			AssertEquals("", bwq.Commodity);
			AssertEquals(abcOrg.PK, bwq.OH_Carrier);
			AssertEquals(dto.RateResult.CarrierContractNumber, cusNumbers.First().CE_EntryNum);
			AssertEquals("CON", cusNumbers.First().CE_EntryType);
			AssertEquals("STD", bwq.CarrierServiceLevel);
			AssertEquals((ZDateTime)effectiveOn, bwq.ETD);
			AssertEquals(0, bwq.Booking.OuterPackLines.Count);

			AssertEquals("Weight should be: 2 * 5000 + 3 * 2000 = 16000", (ZDecimal)16000, bwq.Weight);
			AssertEquals("KG", bwq.WeightUnit);
			AssertEquals("Volume should be: 20 * 2 + 30 * 3 = 130", (ZDecimal)130, bwq.Volume);
			AssertEquals("M3", bwq.VolumeUnit);
			AssertEquals("Chargable should be: 5 * 2 + 1 * 3", (ZDecimal)13, bwq.Chargeable);

			AssertJobHasCharge((Job)bwq.Job, appliedCharge);

			AssertHasContainers(bwq, dto);
		});
	}

	public void TestCommodity_OnlyPopulatedWithOneDistinctCommodity()
	{
		var effectiveOn = new ZDate(2020, 1, 1);
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var container1Dto = JobTestHelpers.CreateContainerDto("20GP");
		var container2Dto = JobTestHelpers.CreateContainerDto("40GP", commodity: "ATC");
		var dto = JobTestHelpers.CreateJobDto(abcOrg, effectiveOn, container1: container1Dto, container2: container2Dto);

		var viewQuotedBooking = new AutoratedBookingWithQuoteCreator(dto).CreateJob() as ViewQuotedBooking;
		AssertEquals("there are two different commodities, don't populate bwq commodity", "", viewQuotedBooking.QuotedBooking.Commodity);

		container2Dto = JobTestHelpers.CreateContainerDto("40GP");
		dto = JobTestHelpers.CreateJobDto(abcOrg, effectiveOn, container1: container1Dto, container2: container2Dto);
		viewQuotedBooking = new AutoratedBookingWithQuoteCreator(dto).CreateJob() as ViewQuotedBooking;
		AssertEquals("there is one distinct commodity, populate bwq commodity", "GEN", viewQuotedBooking.QuotedBooking.Commodity);
	}

	void AssertHasContainers(QuotedBooking bwq, CreateJobDto dto)
	{
		foreach (var container in dto.RateQuery.JobInfo.Containers)
		{
			var matchingContainers = bwq.QuotedBookingContainers.Where(c => c.RefContainer.RC_Code == container.ContainerType).First();
			AssertEquals((ZShort)container.Count, matchingContainers.JC_ContainerCount);
		}
	}
}

