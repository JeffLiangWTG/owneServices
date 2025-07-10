using CargoWise.Schema;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ViewQuotedBooking))]
	public class ViewQuotedBookingSalesRelationActivityTest : SalesRelationActivityTestCase<ViewQuotedBooking>
	{
		public override void TestRelatedPivotsDeletedOnDeletion()
		{
			Assert("Can not be deleted", true);
		}

		protected override ITableSchema TableSchema
		{
			get
			{
				return ViewQuotedBookingSchema.Instance;
			}
		}

		protected override ViewQuotedBooking GetNewActivity()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			return ViewQuotedBooking.LoadOrCreate(quotedBooking);
		}
	}
}
