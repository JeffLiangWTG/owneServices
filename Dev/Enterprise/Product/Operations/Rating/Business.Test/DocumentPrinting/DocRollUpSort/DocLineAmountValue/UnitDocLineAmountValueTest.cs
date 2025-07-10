using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class UnitDocLineAmountValueTest : BaseDocLineAmountValueTest
	{
		public void TestQuotationLineList()
		{
			var docLineAmountValue = new UnitDocLineAmountValue(currency: "AUD", unit: "KG", rate: 10, applyTo: "");
			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "Amount:10" });
				AssertProperties(docLineAmountValue, expectedProperties: new[] { "Type:UnitDocLineAmountValue", "Currency:AUD", "Unit:KG" });
				AssertQuotationLineList(docLineAmountValue, new[] { "|AUD|10.00|KG" });
			});
		}

		public void TestQuotationLineList_ApplyTo()
		{
			var docLineAmountValue = new UnitDocLineAmountValue(currency: "AUD", applyTo: "Note1", unit: "KG", rate: 10);
			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "Amount:10" });
				AssertProperties(docLineAmountValue, expectedProperties: new[] { "Currency:AUD", "Type:UnitDocLineAmountValue", "ApplyTo:Note1", "Unit:KG" });
				AssertQuotationLineList(docLineAmountValue, new[] { "Note1|AUD|10.00|KG" });
			});
		}

		public void TestAdd()
		{
			var docLineAmountValue1 = new UnitDocLineAmountValue(currency: "AUD", unit: "KG", rate: 10, applyTo: "");
			var docLineAmountValue2 = new UnitDocLineAmountValue(currency: "AUD", unit: "KG", rate: 10, applyTo: "");

			var result = docLineAmountValue1.Add(docLineAmountValue2);
			AssertQuotationLineList(result, new[] { "|AUD|20.00|KG" });
		}
	}
}
