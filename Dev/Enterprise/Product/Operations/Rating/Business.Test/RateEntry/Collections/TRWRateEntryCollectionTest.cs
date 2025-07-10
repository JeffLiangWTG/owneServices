using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class TRWRateEntryCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			var testQuote = Factory.New<Quote>();
			var testTRWEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.TRW);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.TRW, testTRWEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testTRWEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testTRWEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(TRWRateEntryCollection))]
	public class TRWRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TRWRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
