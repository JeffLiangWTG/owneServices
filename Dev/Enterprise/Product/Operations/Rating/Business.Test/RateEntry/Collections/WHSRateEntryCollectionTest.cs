using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class WHSRateEntryCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			var testQuote = Factory.New<Quote>();
			var testWHSEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.WHS);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.WHS, testWHSEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testWHSEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testWHSEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(WHSRateEntryCollection))]
	public class WHSRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WHSRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
