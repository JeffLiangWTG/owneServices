using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class MinDocLineAmountValueTest : BaseDocLineAmountValueTest
	{
		public void TestQuotationLineList()
		{
			var docLineAmountValue = new MinDocLineAmountValue(currency: "AUD", applyTo: "Note1", min: 10);
			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "Amount:10" });
				AssertProperties(docLineAmountValue, expectedProperties: new[] { "Type:MinDocLineAmountValue", "Currency:AUD", "ApplyTo:Note1" });
				AssertQuotationLineList(docLineAmountValue, new[] { "Note1|AUD|10.00|" });
			});
		}

		public void TestAdd()
		{
			var docLineAmountValue1 = new MinDocLineAmountValue(currency: "AUD", applyTo: "Note1", min: 10);
			var docLineAmountValue2 = new MinDocLineAmountValue(currency: "AUD", applyTo: "Note1", min: 20);

			var result = docLineAmountValue1.Add(docLineAmountValue2);
			AssertQuotationLineList(result, new[] { "Note1|AUD|10.00|" });
		}
	}
}
