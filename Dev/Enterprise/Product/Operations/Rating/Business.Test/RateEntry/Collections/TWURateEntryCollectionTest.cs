using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class TWURateEntryCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			var testQuote = Factory.New<Quote>();
			var testTWUEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.TWU);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.TWU, testTWUEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testTWUEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testTWUEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(TWURateEntryCollection))]
	public class TWURateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TWURateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
