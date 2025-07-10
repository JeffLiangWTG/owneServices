using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class CSTRateEntryCollectionTest : RatingTestCase
	{
		public void TestSetDefaults()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			var testCSTEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.CST);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.CST, testCSTEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testCSTEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testCSTEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(CSTRateEntryCollection))]
	public class CSTRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CSTRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
