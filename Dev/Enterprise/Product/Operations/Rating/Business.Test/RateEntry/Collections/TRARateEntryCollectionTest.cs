using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class TRARateEntryCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			var testQuote = Factory.New<Quote>();
			var testTBCEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.TBC);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.TBC, testTBCEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testTBCEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testTBCEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(TBCRateEntryCollection))]
	public class TRARateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TBCRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
