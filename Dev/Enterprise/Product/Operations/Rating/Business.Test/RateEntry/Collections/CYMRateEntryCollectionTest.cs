using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class CYMRateEntryCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			var clientRate = Factory.New<ClientRate>();
			var testCYMEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.CYM);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.CYM, testCYMEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.ALL, testCYMEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testCYMEntry.TI_RX_NKCurrency);
		}
	}

	[TestedType(typeof(CYMRateEntryCollection))]
	public class CYMRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CYMRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
