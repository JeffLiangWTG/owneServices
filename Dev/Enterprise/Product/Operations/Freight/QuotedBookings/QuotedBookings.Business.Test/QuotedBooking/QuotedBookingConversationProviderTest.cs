using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.Business;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingConversationProviderTest : TestCaseWithFactory
	{
		public void TestConversation()
		{
			Factory.Save();
			AssertEquals(bookingProvider.eConversation.PK, provider.eConversation.PK);
		}

		public void TestParentModule()
		{
			AssertEquals(bookingProvider.ParentModule, provider.ParentModule);
		}

		public void TestParentController()
		{
			AssertEquals(bookingProvider.ParentController, provider.ParentController);
		}

		public void TestAdditionalParticipants()
		{
			AssertSequencesEqual(bookingProvider.AdditionalParticipants, provider.AdditionalParticipants);
		}

		public void TestSendEmailNotificationsOnSave()
		{
			AssertEquals(bookingProvider.SendEmailNotificationsOnSave, provider.SendEmailNotificationsOnSave);
		}

		public void TestFromAddressOverride()
		{
			AssertEquals(bookingProvider.FromAddressOverride, provider.FromAddressOverride);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			provider = quotedBooking;
			bookingProvider = booking;
		}

		IConversationProvider bookingProvider;
		IConversationProvider provider;
	}
}
