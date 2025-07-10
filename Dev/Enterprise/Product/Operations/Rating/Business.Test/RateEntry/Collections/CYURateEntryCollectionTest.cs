using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class CYURateEntryCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			var testQuote = Factory.New<Quote>();
			var testCYUEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.CYU);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.CYU, testCYUEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testCYUEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testCYUEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(CYURateEntryCollection))]
	public class CYURateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CYURateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
