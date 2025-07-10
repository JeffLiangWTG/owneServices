using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(UnacceptedQuotesCollection))]
	public class UnacceptedQuotesCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new UnacceptedQuotesCollection(Factory, GlbCompany.CurrentCompany, Factory.LoadTop1<OrgHeader>(new ZQuery()).PK);
		}

		public void TestUnacceptedQuotesFilter()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var quote1 = Factory.New<Quote>();
			quote1.TH_OH = org.PK;
			quote1.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			var quote2 = Factory.New<Quote>();
			quote2.TH_OH = org.PK;
			quote2.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			var quote3 = Factory.New<Quote>();
			quote3.TH_OH = org.PK;
			quote3.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			var quote4 = Factory.New<Quote>();
			quote4.TH_OH = org.PK;
			quote4.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			var quote5 = Factory.New<Quote>();
			quote5.TH_OH = org.PK;
			quote5.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			quote5.TH_IsCancelled = true;

			var quote6 = Factory.New<Quote>();
			quote6.TH_OH = ZGuid.NewZGuid();
			quote6.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			var quote7 = Factory.New<Quote>();
			quote7.TH_OH = org.PK;
			quote7.TH_Accepted = ZDateTime.Now;
			quote7.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			var quote8 = Factory.New<Quote>();
			quote8.TH_OH = org.PK;
			quote8.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			quote8.TH_QuoteEndDate = ZDate.Today.AddDays(-2);

			Factory.Save();

			var unacceptedQuotes = new UnacceptedQuotesCollection(Factory, GlbCompany.CurrentCompany, org.PK);
			unacceptedQuotes.Load();

			AssertEquals(4, unacceptedQuotes.Count);
			AssertEquals("Expired quote not included", false, unacceptedQuotes.Contains(quote8));
			AssertEquals("Cancelled quote not included", false, unacceptedQuotes.Contains(quote5));
			AssertEquals("Quote for different org not included", false, unacceptedQuotes.Contains(quote6));
			AssertEquals("Accepted quote not included", false, unacceptedQuotes.Contains(quote7));
		}
	}
}
