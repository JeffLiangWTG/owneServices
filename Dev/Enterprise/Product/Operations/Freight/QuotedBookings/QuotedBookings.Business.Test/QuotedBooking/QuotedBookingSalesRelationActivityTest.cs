using CargoWise.Schema;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBooking))]
	public class QuotedBookingSalesRelationActivityTest : SalesRelationActivityTestCase<QuotedBooking>
	{
		protected override ITableSchema TableSchema
		{
			get
			{
				return null;
			}
		}

		public override void TestSystemCreateTime()
		{
			Assert("Can not set SystemCreateTime for a view.", true);
		}

		public override void TestSystemLastEditTime()
		{
			Assert("Can not set SystemLastEditTime for a view.", true);
		}

		protected override QuotedBooking GetNewActivity()
		{
			return QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
		}
	}
}
