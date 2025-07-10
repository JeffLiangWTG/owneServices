using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class FlatDocLineAmountValueTest : BaseDocLineAmountValueTest
	{
		public void TestQuotationLineList()
		{
			var docLineAmountValue = new FlatDocLineAmountValue(currency: "AUD", flat: 10, applyTo: "");
			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "Amount:10" });
				AssertProperties(docLineAmountValue, expectedProperties: new[] { "Type:FlatDocLineAmountValue", "Currency:AUD" });
				AssertQuotationLineList(docLineAmountValue, new[] { "|AUD|10.00|" });
			});
		}

		public void TestQuotationLineList_ApplyTo()
		{
			var docLineAmountValue = new FlatDocLineAmountValue(currency: "AUD", applyTo: "Note1", flat: 10);

			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "Amount:10" });
				AssertProperties(docLineAmountValue, expectedProperties: new[] { "Type:FlatDocLineAmountValue", "Currency:AUD", "ApplyTo:Note1" });
				AssertQuotationLineList(docLineAmountValue, new[] { "Note1|AUD|10.00|" });
			});
		}

		public void TestAdd()
		{
			var docLineAmountValue1 = new FlatDocLineAmountValue(currency: "AUD", flat: 10, applyTo: "");
			var docLineAmountValue2 = new FlatDocLineAmountValue(currency: "AUD", flat: 20, applyTo: "");

			var result = docLineAmountValue1.Add(docLineAmountValue2);
			AssertQuotationLineList(result, new[] { "|AUD|30.00|" });
		}
	}
}
