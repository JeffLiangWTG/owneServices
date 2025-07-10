using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class FirstPlusAdditionalDocLineAmountValueTest : BaseDocLineAmountValueTest
	{
		public void TestValuesPropertiesText()
		{
			var docLineAmountValue = new FirstPlusAdditionalDocLineAmountValue(currency: "AUD", unit: "KG", first: 10, additional: 11);

			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "First:10", "Additional:11" });
				AssertProperties(docLineAmountValue, expectedProperties: new[] { "Type:FirstPlusAdditionalDocLineAmountValue", "Currency:AUD", "Unit:KG" });
				AssertQuotationLineList(docLineAmountValue, new[] { "First KG|AUD|10.00|", "Additional|AUD|11.00|KG" });
			});
		}

		public void TestAdd()
		{
			var docLineAmountValue1 = new FirstPlusAdditionalDocLineAmountValue(currency: "AUD", unit: "KG", first: 10, additional: 11);
			var docLineAmountValue2 = new FirstPlusAdditionalDocLineAmountValue(currency: "AUD", unit: "KG", first: 20, additional: 21);

			var result = docLineAmountValue1.Add(docLineAmountValue2);
			AssertQuotationLineList
			(
				result,
				new[]
				{
					"First KG|AUD|30.00|",
					"Additional|AUD|32.00|KG"
				}
			);
		}
	}
}
