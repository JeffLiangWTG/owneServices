using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class MaxDocLineAmountValueTest : BaseDocLineAmountValueTest
	{
		public void TestQuotationLineList()
		{
			var docLineAmountValue = new MaxDocLineAmountValue(currency: "AUD", applyTo: "Note1", max: 10);
			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "Amount:10" });
				AssertProperties(docLineAmountValue, expectedProperties: new[] { "Type:MaxDocLineAmountValue", "Currency:AUD", "ApplyTo:Note1" });
				AssertQuotationLineList(docLineAmountValue, new[] { "Maximum|AUD|10.00|Note1" });
			});
		}

		public void TestAdd()
		{
			var docLineAmountValue1 = new MaxDocLineAmountValue(currency: "AUD", applyTo: "Note1", max: 10);
			var docLineAmountValue2 = new MaxDocLineAmountValue(currency: "AUD", applyTo: "Note1", max: 20);

			var result = docLineAmountValue1.Add(docLineAmountValue2);
			AssertQuotationLineList(result, new[] { "Maximum|AUD|20.00|Note1" });
		}
	}
}
