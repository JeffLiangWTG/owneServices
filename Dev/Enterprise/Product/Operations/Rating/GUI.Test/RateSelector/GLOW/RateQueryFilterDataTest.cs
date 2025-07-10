using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateQueryFilterDataTest : RatingTestCase
	{
		public void TestRateQueryContainsCorrectValues_Consol()
		{
			var consol = CreateForwardingConsol();
			var criteria = new RatingCriteria(consol.RatingAdapter, Factory);
			var query = new RateQueryFilterData("", criteria);
			var dto = query.RateQueryDto;

			AssertEquals(consol.JK_RL_NKLoadPort, dto.Origin);
			AssertEquals(consol.JK_RL_NKDischargePort, dto.Destination);
			AssertEquals(consol.AutoratingDate, dto.EffectiveDate);
		}

		public void TestRateQueryContainsCorrectValues_BookingWithQuote()
		{
			var quotedBooking = CreateBookingWithQuote();
			var criteria = new RatingCriteria(quotedBooking.GetFirstAdapter(), Factory);
			var query = new RateQueryFilterData("", criteria);
			var dto = query.RateQueryDto;

			AssertEquals(quotedBooking.LoadPort, dto.Origin);
			AssertEquals(quotedBooking.DischargePort, dto.Destination);
			AssertEquals(null, dto.EffectiveDate);
		}

		public void TestRateQueryContainsCorrectValues_QuickBooking()
		{
			var quotedBooking = CreateQuickBooking();
			var criteria = new RatingCriteria(quotedBooking.GetFirstAdapter(), Factory);
			var query = new RateQueryFilterData("", criteria);
			var dto = query.RateQueryDto;

			AssertEquals(quotedBooking.LoadPort, dto.Origin);
			AssertEquals(quotedBooking.DischargePort, dto.Destination);
			AssertEquals(null, dto.EffectiveDate);
		}

		public void TestRateQueryContainsCorrectValues_OneOffQuote()
		{
			var quotedBooking = CreateOneOffQuote();
			var criteria = new RatingCriteria(quotedBooking.GetFirstAdapter(), Factory);
			var query = new RateQueryFilterData("", criteria);
			var dto = query.RateQueryDto;

			AssertEquals(quotedBooking.Origin, dto.Origin);
			AssertEquals(quotedBooking.Destination, dto.Destination);
			AssertEquals(null, dto.EffectiveDate);
		}

		public void TestRateFilterContainsCorrectValues_MatchingLocations()
		{
			var consol = CreateForwardingConsol();
			var criteria = new RatingCriteria(consol.RatingAdapter, Factory);
			var query = new RateQueryFilterData("", criteria);
			var dto = query.RateFilterDto;

			AssertEquals(consol.JK_RL_NKLoadPort, dto.FirstLoad.First());
			AssertEquals(consol.JK_RL_NKDischargePort, dto.LastDischarge.First());
			AssertEquals(consol.JK_RL_NKLoadPort, dto.FirstRouteSetLoad.First());
			AssertEquals(consol.JK_RL_NKDischargePort, dto.LastRouteSetDischarge.First());
		}

		ForwardingConsol CreateForwardingConsol(string origin = "AUMEL", string destination = "USLAX")
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			consol.AutoratingDate = ZDate.BrettsBirthday;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1234";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			return consol;
		}

		QuotedBooking CreateBookingWithQuote() => CreateQuotedBookingWithContainer(QuotedBookingState.AcceptedBookingWithQuote);

		QuotedBooking CreateOneOffQuote()
		{
			var qb = CreateQuotedBooking(QuotedBookingState.QuoteOnly);
			var container = qb.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			return qb;
		}

		QuotedBooking CreateQuickBooking() => CreateQuotedBookingWithContainer(QuotedBookingState.BookingOnly);

		QuotedBooking CreateQuotedBookingWithContainer(QuotedBookingState state)
		{
			var qb = CreateQuotedBooking(state);
			var container = qb.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "TEST1234";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			return qb;
		}

		QuotedBooking CreateQuotedBooking(QuotedBookingState state)
		{
			var booking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "SEA", "FCL", "PPD", null, null, null, null, "AUMEL", "USLAX", 10m, 1m, state);
			booking.StartDate = ZDate.BrettsBirthday;
			booking.LoadPort = "AUSYD";
			booking.DischargePort = "NZAKL";
			return booking;
		}
	}
}
