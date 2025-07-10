using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Quotations.Testing
{
	[TestedType(typeof(TrackingQuoteCollection))]
	public class TrackingQuoteCollectionTest : BusinessObjectCollectionTestCase
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCompanyFilterIsNotAppliedForWeb()
		{
			TrackingQuoteCollection collection = new TrackingQuoteCollection(Factory);
			collection.Load();
			int originalQuoteCount = collection.Count;

			GlbCompany company2 = Factory.New<GlbCompany>();
			GlbBranch branch2 = company2.Branches.AddNew();

			QuotedBooking quote1 = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			quote1.TryLoadOrCreateJob();
			var job1 = quote1.Job;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;

			QuotedBooking quote2 = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			quote2.TryLoadOrCreateJob();
			var job2 = quote2.Job;
			job2.JH_GC = company2.PK;
			job2.JH_GB = branch2.PK;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			collection = new TrackingQuoteCollection(Factory);
			collection.Load();

			AssertEquals("Collection.Count", originalQuoteCount + 2, collection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TrackingQuoteCollection(Factory);
		}
	}
}
