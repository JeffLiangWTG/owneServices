using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class UNPRateEntryCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			var testQuote = Factory.New<Quote>();
			var testUNPEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.UNP);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.UNP, testUNPEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testUNPEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testUNPEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(UNPRateEntryCollection))]
	public class UNPRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new UNPRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
