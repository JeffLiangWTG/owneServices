using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105CMCommodity_WineTest : NX5105Commodity_WineAbstractTest<NX5105CMCommodityWine>
	{
		[ExpectNoExceptions]
		public void TestAgeNumeric()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_AlcoholAge = 12;
			IWine wine = new NX5105CMCommodityWine(invoiceLine);
			NUnit.Framework.Assert.That(wine.AgeNumeric, NUnit.Framework.Is.EqualTo(12).Using(CustomComparers.TypeComparison), "Wine.AgeNumeric should be");
		}

		[ExpectNoExceptions]
		public void TestBottledDate()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_BottledDate = new ZDateTime(2020, 12, 15, 17, 43, 0);
			IWine wine = new NX5105CMCommodityWine(invoiceLine);
			NUnit.Framework.Assert.That(wine.BottledDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2020, 12, 15, 17, 43, 0)), "Wine.BottledDate should be");
		}

		[ExpectNoExceptions]
		public void TestCoverLotNumberAmount()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_AlteredLotNoAmt = 15m;
			IWine wine = new NX5105CMCommodityWine(invoiceLine);
			NUnit.Framework.Assert.That(wine.CoverLotNumberAmount, NUnit.Framework.Is.EqualTo(15m).Using(CustomComparers.TypeComparison), "Wine.CoverLotNumberAmount should be");
		}

		[ExpectNoExceptions]
		public void TestGeographicRegion()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_AlcoholCountryRegion = "TWTW";
			IWine wine = new NX5105CMCommodityWine(invoiceLine);
			NUnit.Framework.Assert.That(wine.GeographicRegion, NUnit.Framework.Is.EqualTo("TWTW").Using(CustomComparers.TypeComparison), "Wine.GeographicRegion should be");
		}

		[ExpectNoExceptions]
		public void TestOriginalNonLotNumberAmount()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_NoOriginalLotNoAmt = 25.5m;
			IWine wine = new NX5105CMCommodityWine(invoiceLine);
			NUnit.Framework.Assert.That(wine.OriginalNonLotNumberAmount, NUnit.Framework.Is.EqualTo(25.5m).Using(CustomComparers.TypeComparison), "Wine.OriginalNonLotNumberAmount should be");
		}

		[ExpectNoExceptions]
		public void TestProductBestBeforeDateTime()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_AlcoholEndOfShelfLife = new ZDateTime(2020, 12, 15, 17, 43, 0);
			IWine wine = new NX5105CMCommodityWine(invoiceLine);
			NUnit.Framework.Assert.That(wine.ProductBestBeforeDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2020, 12, 15, 17, 43, 0)), "Wine.ProductBestBeforeDateTime should be");
		}

		[ExpectNoExceptions]
		public void TestProductExpiryDateTime()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_ExpirationDate = new ZDateTime(2020, 12, 15, 17, 43, 0);
			IWine wine = new NX5105CMCommodityWine(invoiceLine);
			NUnit.Framework.Assert.That(wine.ProductExpiryDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2020, 12, 15, 17, 43, 0)), "Wine.ProductExpiryDateTime should be");
		}

		[ExpectNoExceptions]
		public void TestRemoveLotNumberAmount()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_RemovedLotNoAmt = 15m;
			IWine wine = new NX5105CMCommodityWine(invoiceLine);
			NUnit.Framework.Assert.That(wine.RemoveLotNumberAmount, NUnit.Framework.Is.EqualTo(15m).Using(CustomComparers.TypeComparison), "Wine.RemoveLotNumberAmount should be");
		}

		[ExpectNoExceptions]
		public void TestYearNumeric()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_AlcoholYear = 12;
			IWine wine = new NX5105CMCommodityWine(invoiceLine);
			NUnit.Framework.Assert.That(wine.YearNumeric, NUnit.Framework.Is.EqualTo(12).Using(CustomComparers.TypeComparison), "Wine.YearNumeric should be");
		}

		public override void TestCheckNotApplicableProperties()
		{
			Assert("No need to test.", true);
		}
	}
}
