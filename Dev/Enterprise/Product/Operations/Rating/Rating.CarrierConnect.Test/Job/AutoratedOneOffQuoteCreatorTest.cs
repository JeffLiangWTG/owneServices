using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.CarrierConnect.Test;

public class AutoratedOneOffQuoteCreatorTest : AutoratedJobCreatorTest
{
	public void TestCreateOneOffQuote()
	{
		var effectiveOn = new ZDate(2020, 1, 1);
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var appliedCharge = JobTestHelpers.CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var dto = JobTestHelpers.CreateJobDto(abcOrg, effectiveOn, appliedCharge);

		Env.Registry.Rating.QuoteValidityPeriod = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, 2);

		var viewQuotedBooking = new AutoratedOneOffQuoteCreator(dto).CreateJob() as ViewQuotedBooking;
		var ooq = viewQuotedBooking.QuotedBooking;

		var cusNumbers = ooq.Quote.CurrentOneOffQuote.Numbers.Cast<CusEntryNumber>();

		// Assert
		CombineAssertions(() =>
		{
			AssertNotNull(ooq);
			AssertEquals(dto.RateResult.TransportMode, ooq.TransportMode);
			AssertEquals(dto.RateResult.ContainerMode, ooq.ContainerMode);
			AssertEquals(dto.RateResult.Origin, ooq.Origin);
			AssertEquals(dto.RateResult.Destination, ooq.Destination);
			AssertEquals("", ooq.Commodity);
			AssertEquals(abcOrg.PK, ooq.OH_Carrier);
			AssertEquals(dto.RateResult.CarrierContractNumber,cusNumbers.First().CE_EntryNum);
			AssertEquals("CON", cusNumbers.First().CE_EntryType);
			AssertEquals("STD", ooq.CarrierServiceLevel);
			AssertEquals((ZDateTime)effectiveOn, ooq.StartDate);
			AssertEquals(effectiveOn.AddMonths(2), ooq.EndDate);

			AssertEquals("Weight should be: 2 * 5000 + 3 * 2000 = 16000", (ZDecimal)16000, ooq.Weight);
			AssertEquals("KG", ooq.WeightUnit);
			AssertEquals("Volume should be: 20 * 2 + 30 * 3 = 130", (ZDecimal)130, ooq.Volume);
			AssertEquals("M3", ooq.VolumeUnit);
			AssertEquals("Chargable should be: 5 * 2 + 1 * 3", (ZDecimal)13, ooq.Chargeable);

			AssertJobHasCharge((Job)ooq.Job, appliedCharge);

			AssertHasContainers(ooq, dto);
		});
	}

	public void TestCommodity_OnlyPopulatedWithOneDistinctCommodity()
	{
		var effectiveOn = new ZDate(2020, 1, 1);
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var container1Dto = JobTestHelpers.CreateContainerDto("20GP");
		var container2Dto = JobTestHelpers.CreateContainerDto("40GP", commodity: "ATC");
		var dto = JobTestHelpers.CreateJobDto(abcOrg, effectiveOn, container1: container1Dto, container2: container2Dto);

		var viewQuotedBooking = new AutoratedOneOffQuoteCreator(dto).CreateJob() as ViewQuotedBooking;
		AssertEquals("there are two different commodities, don't populate bwq commodity", "", viewQuotedBooking.QuotedBooking.Commodity);

		container2Dto = JobTestHelpers.CreateContainerDto("40GP");
		dto = JobTestHelpers.CreateJobDto(abcOrg, effectiveOn, container1: container1Dto, container2: container2Dto);
		viewQuotedBooking = new AutoratedOneOffQuoteCreator(dto).CreateJob() as ViewQuotedBooking;
		AssertEquals("there is one distinct commodity, populate bwq commodity", "GEN", viewQuotedBooking.QuotedBooking.Commodity);
	}

	void AssertHasContainers(QuotedBooking ooq, CreateJobDto dto)
	{
		foreach (var container in dto.RateQuery.JobInfo.Containers)
		{
			var matchingContainers = ooq.Quote.CurrentOneOffQuote.Containers.Where(c => c.RefContainer.RC_Code == container.ContainerType).First();
			AssertEquals((ZShort)container.Count, matchingContainers.TC_ContainerCount);
		}
	}
}

