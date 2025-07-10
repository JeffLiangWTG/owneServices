using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.QuotedBookings.Business.Testing
{
	public class QuotedBookingJobDatesProviderTest : TestCaseWithFactory
	{
		public void TestArrivalAndDepartureDateWhenQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_QuoteDate = new ZDate(2008, 12, 20);
			quote.TH_QuoteEndDate = new ZDate(2008, 12, 31);
			var qb = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			var rating = qb.GetFirstAdapter();
			AssertEquals(new ZDateTime(2008, 12, 20), rating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2008, 12, 31), rating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestArrivalAndDepartureDateWhenBooking()
		{
			var ship = QuotedBooking.CreateNewBooking(Factory);
			ship.JS_E_DEP = new ZDateTime(2008, 12, 15);
			ship.JS_E_ARV = new ZDateTime(2008, 12, 23);
			var qb = QuotedBooking.New(ZGuid.Empty, ship.PK, Factory);

			var rating = qb.GetFirstAdapter();
			AssertEquals(new ZDateTime(2008, 12, 15), rating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2008, 12, 23), rating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestArrivalAndDepartureDateWhenBookingWithQuote()
		{
			var ship = QuotedBooking.CreateNewBooking(Factory);
			ship.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2008, 12, 14);
			ship.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2008, 12, 24);
			ship.JS_E_DEP = new ZDateTime(2008, 12, 15);
			ship.JS_E_ARV = new ZDateTime(2008, 12, 23);

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_QuoteDate = new ZDate(2008, 12, 20);
			quote.TH_QuoteEndDate = new ZDate(2008, 12, 31);

			var qb = QuotedBooking.New(quote.PK, ship.PK, Factory);

			var rating = qb.GetFirstAdapter();
			AssertEquals("Date from Booking should have higher priority", new ZDateTime(2008, 12, 15), rating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals("Date from Booking should have higher priority", new ZDateTime(2008, 12, 23), rating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestJobOpenDateWhenQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var qb = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var adapter = qb.GetFirstAdapter();

			AssertEquals(false, adapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Now;
			var jobHeader = new JobHeader.Loader(quote).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, adapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		public void TestJobOpenDateWhenBooking()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var qb = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			var adapter = qb.GetFirstAdapter();

			AssertEquals(false, adapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Now;
			var jobHeader = new JobHeader.Loader(booking).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, adapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		#region Transit Time

		public void TestTransitTime_GivenCarrierWithTransitTime_ThenIJobDatesProviderForCarriersShouldReturnCarrierTransitTime()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.OH_Carrier = Carrier1.PK;
			quotedBooking.TransitTime = "10";

			AssertTransitTime(quotedBooking, Carrier1.PK, expectedJobDatesProviderTransitTime: "10", expectedJobDatesProviderForCarriersTransitTimes: new[] { $"CARRIER1-10" });
		}

		public void TestTransitTime_GivenPotentialCarrierWithTransitTime_ThenIJobDatesProviderForCarriersShouldReturnPotentialCarrierTransitTime()
			=> TestTransitTime(carrierPK: Carrier1.PK, quotedBookingTransitTime: "10", potentialCarrierPK: Carrier2.PK, potentialCarrierTransitTime: "20", expectedJobDatesProviderTransitTime: "10", expectedJobDatesProviderForCarriersTransitTimes: new[] { $"CARRIER1-10", $"CARRIER2-20" });

		public void TestTransitTime_GivenPotentialCarrierWithEmptyTransitTime_ThenIJobDatesProviderForCarriersShouldReturnCarrierTransitTime()
			=> TestTransitTime(carrierPK: Carrier1.PK, quotedBookingTransitTime: "10", potentialCarrierPK: Carrier2.PK, potentialCarrierTransitTime: "", expectedJobDatesProviderTransitTime: "10", expectedJobDatesProviderForCarriersTransitTimes: new[] { $"CARRIER1-10", $"CARRIER2-10" });

		public void TestTransitTime_GivenCarrierExistInPotentialCarrier_ThenIJobDatesProviderForCarriersShouldReturnCarrierTransitTime()
			=> TestTransitTime(carrierPK: Carrier1.PK, quotedBookingTransitTime: "10", potentialCarrierPK: Carrier1.PK, potentialCarrierTransitTime: "20", expectedJobDatesProviderTransitTime: "10", expectedJobDatesProviderForCarriersTransitTimes: new[] { $"CARRIER1-10" });

		void TestTransitTime(ZGuid carrierPK, string quotedBookingTransitTime, ZGuid potentialCarrierPK, string potentialCarrierTransitTime, string expectedJobDatesProviderTransitTime, string[] expectedJobDatesProviderForCarriersTransitTimes)
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			var possibleCarrier1 = quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = potentialCarrierPK;
			possibleCarrier1.TTC_TransitTime = potentialCarrierTransitTime;

			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.OH_Carrier = carrierPK;
			quotedBooking.TransitTime = quotedBookingTransitTime;

			AssertTransitTime(quotedBooking, Carrier1.PK, expectedJobDatesProviderTransitTime: expectedJobDatesProviderTransitTime, expectedJobDatesProviderForCarriersTransitTimes);
		}

		void AssertTransitTime(QuotedBooking quotedBooking, ZGuid carrierPK, string expectedJobDatesProviderTransitTime, string[] expectedJobDatesProviderForCarriersTransitTimes)
		{
			CombineAssertions(() =>
			{
				var rating = quotedBooking.GetFirstAdapter();
				AssertEquals("JobDatesProvider TransitTime", expectedJobDatesProviderTransitTime, rating.JobDatesProvider.TransitTime);

				var jobDatesProviderForCarriers = (IJobDatesProviderForCarriers)rating.JobDatesProvider;

				var actualTransitTimes = new List<string>();
				foreach (var transitTimeKeyValue in jobDatesProviderForCarriers.CarrierTransitTimes)
				{
					var carrier = Factory.Load<OrgHeader>(transitTimeKeyValue.Key);
					actualTransitTimes.Add($"{carrier.OH_Code}-{transitTimeKeyValue.Value}");
				}
				AssertContainsExactElementsInAnyOrder("JobDatesProviderForCarriers TransitTime", expectedJobDatesProviderForCarriersTransitTimes, actualTransitTimes);
			});
		}

		OrgHeader Carrier1 => carrier1 ?? (carrier1 = CreateOrgHeader("CARRIER1"));
		OrgHeader carrier1;

		OrgHeader Carrier2 => carrier2 ?? (carrier2 = CreateOrgHeader("CARRIER2"));
		OrgHeader carrier2;

		OrgHeader CreateOrgHeader(string code)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = code;
			return orgHeader;
		}

		public void TestTransitTime()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_QuoteDate = new ZDate(2008, 12, 20);
			quote.TH_QuoteEndDate = new ZDate(2008, 12, 31);
			var qb = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			var rating = qb.GetFirstAdapter();
			AssertEquals("Should not be calculated from quote dates", "", rating.JobDatesProvider.TransitTime);

			qb.TransitTime = "10";
			AssertEquals("10", rating.JobDatesProvider.TransitTime);

			qb.TransitTime = "SMD";
			AssertEquals("SMD", rating.JobDatesProvider.TransitTime);
		}

		#endregion

		public void TestPickupDeliveryDatesWithQuotedBooking()
		{
			var ship = QuotedBooking.CreateNewBooking(Factory);
			ship.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2008, 12, 14);
			ship.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2008, 12, 24);

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_QuoteDate = new ZDate(2008, 12, 20);
			quote.TH_QuoteEndDate = new ZDate(2008, 12, 31);

			var qb = QuotedBooking.New(quote.PK, ship.PK, Factory);

			var rating = qb.GetFirstAdapter();
			AssertEquals(new ZDateTime(2008, 12, 14), rating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.PickupDate));
			AssertEquals(new ZDateTime(2008, 12, 24), rating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DeliveryDate));
		}
	}
}
