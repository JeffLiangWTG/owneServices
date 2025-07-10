using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class TRNRateEntryCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			var testQuote = Factory.New<Quote>();
			var testTRNEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.TRN);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.TRN, testTRNEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testTRNEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testTRNEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(TRNRateEntryCollection))]
	public class TRNRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TRNRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
