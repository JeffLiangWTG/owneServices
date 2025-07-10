using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class SlidingDocLineAmountValueTest : BaseDocLineAmountValueTest
	{
		public void TestValuesPropertiesText()
		{
			var docLineAmountValue = new SlidingDocLineAmountValue(currency: "AUD", operatorBreak: "<6", unit: "KG", rate: 10, flat: 15);
			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "Rate:10", "Amount:15" });
				AssertProperties(docLineAmountValue, expectedProperties: new[] { "Type:SlidingDocLineAmountValue", "Currency:AUD", "OperatorBreak:<6", "Unit:KG" });
				AssertQuotationLineList(docLineAmountValue, new[] { "<6|AUD|10.00|KG", "<6|AUD|15.00|" });
			});
		}

		public void TestAdd()
		{
			var docLineAmountValue1 = new SlidingDocLineAmountValue(currency: "AUD", operatorBreak: "<6", unit: "KG", rate: 10, flat: 15);
			var docLineAmountValue2 = new SlidingDocLineAmountValue(currency: "AUD", operatorBreak: "<6", unit: "KG", rate: 20, flat: 25);

			var result = docLineAmountValue1.Add(docLineAmountValue2);
			AssertQuotationLineList(result, new[] { "<6|AUD|30.00|KG", "<6|AUD|40.00|" });
		}
	}
}
