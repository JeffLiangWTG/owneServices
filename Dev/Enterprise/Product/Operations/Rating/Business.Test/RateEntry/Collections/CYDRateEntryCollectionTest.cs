using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class CYDRateEntryCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			var testQuote = Factory.New<Quote>();
			var testCYDEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.CYD);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.CYD, testCYDEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testCYDEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testCYDEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(CYDRateEntryCollection))]
	public class CYDRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CYDRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
