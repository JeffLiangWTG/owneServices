using CargoWise.Types;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	public class QuotedBookingFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<QuotedBooking>
	{
		protected override QuotedBooking GetNewBusinessObjectForFilterCollection()
		{
			return CreateQuotedBooking();
		}

		public override (QuotedBooking filteredBO1, QuotedBooking filteredBO2, QuotedBooking filteredBO3, QuotedBooking filteredBO4, QuotedBooking filteredBO5, QuotedBooking filteredBO6) GetSixNewBusinessObjectsForFilterCollection()
		{
			return (CreateBookingOnly(), CreateQuotedBooking(), CreateBookingOnly(), CreateBookingOnly(), CreateBookingOnly(), CreateQuotedBooking());
		}

		protected QuotedBooking CreateBookingOnly()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			return quotedBooking;
		}

		protected QuotedBooking CreateQuotedBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			return quotedBooking;
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get
			{
				return ModuleIDs.QuotedBookings;
			}
		}
	}
}
