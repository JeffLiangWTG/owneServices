using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class PercentageDocLineAmountValueTest : BaseDocLineAmountValueTest
	{
		public void TestQuotationLineList()
		{
			var docLineAmountValue = new PercentageDocLineAmountValue(currency: "AUD", applyTo: "Note1", unit: "KG", percent: 10);
			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "Percent:10" });
				AssertProperties(docLineAmountValue, expectedProperties: new[] { "Type:PercentageDocLineAmountValue", "Currency:AUD", "ApplyTo:Note1", "Unit:KG" });
				AssertQuotationLineList(docLineAmountValue, new[] { "Note1||10.00|KG" });
			});
		}

		public void TestAdd()
		{
			var docLineAmountValue1 = new PercentageDocLineAmountValue(currency: "AUD", applyTo: "Note1", unit: "KG", percent: 10);
			var docLineAmountValue2 = new PercentageDocLineAmountValue(currency: "AUD", applyTo: "Note1", unit: "KG", percent: 20);

			var result = docLineAmountValue1.Add(docLineAmountValue2);
			AssertQuotationLineList(result, new[] { "Note1||30.00|KG" });
		}
	}
}
