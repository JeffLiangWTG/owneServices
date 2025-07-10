using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class PACRateEntryCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			var testQuote = Factory.New<Quote>();
			var testPACEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.PAC);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.PAC, testPACEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testPACEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testPACEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(PACRateEntryCollection))]
	public class PACRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PACRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
