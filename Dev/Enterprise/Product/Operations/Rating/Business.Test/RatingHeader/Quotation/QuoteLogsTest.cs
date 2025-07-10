using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(QuoteLogs))]
	public class QuoteLogsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParent()
		{
			var quote = Factory.New<Quote>();
			var logs = new QuoteLogs(quote);
			AssertEquals(quote, logs.Parent);
		}

		public void TestQuoteLogs_DoNotLoadQuotedBookingLogs()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);
			var logParent = (IStmALogParent)quotedBooking;
			var quotedBookingLogEntry = logParent.Logs.AddNew(Events.Authorised);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(quotedBooking.Quote.PK, ZGuid.Empty, factory);
			var logs = new QuoteLogs((Quote)quotedBooking.Quote);
			Assert("should not contain quotedBooking logs", logs.GetAllLogs().Cast<StmALog>().All(log => log.PK != quotedBookingLogEntry.PK));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new QuoteLogs(Factory.New<Quote>());
		}
	}
}
