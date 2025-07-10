using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort
{
	sealed class DocLineAmountTest : TestCaseWithFactory
	{
		public void TestFlat()
		{
			var docLineAmount = new DocLineAmount();
			docLineAmount.SetFlat(currency: "AUD", amount: 10);
			AssertQuotationLineList(docLineAmount, new[] { "|AUD|10.00|" });
		}

		public void TestUnit()
		{
			var docLineAmount = new DocLineAmount();
			docLineAmount.SetUnit(currency: "AUD", unit: "KG", amount: 10);
			AssertQuotationLineList(docLineAmount, new[] { "|AUD|10.00|KG" });
		}

		public void TestMin()
		{
			var docLineAmount = new DocLineAmount();
			docLineAmount.SetMin(currency: "AUD", applyTo: "Note1", amount: 10);
			AssertQuotationLineList(docLineAmount, new[] { "Note1|AUD|10.00|" });
		}

		public void TestMax()
		{
			var docLineAmount = new DocLineAmount();
			docLineAmount.SetMax(currency: "AUD", applyTo: "Note1", amount: 10);
			AssertQuotationLineList(docLineAmount, new[] { "Maximum|AUD|10.00|Note1" });
		}

		public void TestPercentage()
		{
			var docLineAmount = new DocLineAmount();
			docLineAmount.SetPercentage(currency: "AUD", applyTo: "Note1", unit: "KG", percent: 10);
			AssertQuotationLineList(docLineAmount, new[] { "Note1||10.00|KG" });
		}

		public void TestAdd()
		{
			var docLineAmount1 = new DocLineAmount();
			docLineAmount1.SetFlat(currency: "AUD", amount: 10);

			var docLineAmount2 = new DocLineAmount();
			docLineAmount2.SetUnit(currency: "AUD", unit: "KG", amount: 10);

			var docLineAmount3 = new DocLineAmount();
			docLineAmount3.SetMin(currency: "AUD", amount: 10);

			var docLineAmount4 = new DocLineAmount();
			docLineAmount4.SetMax(currency: "AUD", applyTo: "Test1", amount: 10);

			var docLineAmount5 = new DocLineAmount();
			docLineAmount5.SetPercentage(currency: "AUD", applyTo: "Note1", unit: "KG", percent: 10);

			var result = docLineAmount1 + docLineAmount2 + docLineAmount3 + docLineAmount4 + docLineAmount5;
			AssertQuotationLineList
			(
				result,
				new[]
				{
					"|AUD|10.00|",
					"|AUD|10.00|KG",
					"Minimum|AUD|10.00|",
					"Maximum|AUD|10.00|Test1",
					"Note1||10.00|KG",
				}
			);
		}

		void AssertQuotationLineList(DocLineAmount docLineAmount, string[] expectedQuotationLineList)
			=> AssertContainsExactElementsInAnyOrder
			(
				"QuotationLineList",
				expectedQuotationLineList,
				docLineAmount.GetQuotationLineList(DummyRateLine).Select(quotationLine => quotationLine.ToString())
			);

		RateLine DummyRateLine
		{
			get
			{
				if (rateLine == null)
				{
					var ratingHeader = Factory.New<ClientRate>();
					ratingHeader.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
					var rateEntry = ratingHeader.AddRateEntry("ORG");
					rateLine = rateEntry.RateLines.AddNew();
				}
				return rateLine;
			}
		}
		RateLine rateLine;
	}
}
