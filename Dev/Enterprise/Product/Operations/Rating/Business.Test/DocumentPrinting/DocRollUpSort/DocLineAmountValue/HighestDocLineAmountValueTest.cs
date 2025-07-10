using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class HighestDocLineAmountValueTest : BaseDocLineAmountValueTest
	{
		public void TestValuesPropertiesText()
		{
			var docLineAmountValue = new HighestDocLineAmountValue
			(
				currency: "AUD",
				applyTo: "Note1",
				weightUnit: "KG",
				weightRate: 10,
				weightFlat: 11,
				volumeUnit: "M3",
				volumeRate: 12,
				volumeFlat: 13
			);

			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "WeightRate:10", "WeightFlat:11", "VolumeRate:12", "VolumeFlat:13" });
				AssertProperties(docLineAmountValue, expectedProperties: new[] { "Type:HighestDocLineAmountValue", "Currency:AUD", "ApplyTo:Note1", "WeightUnit:KG", "VolumeUnit:M3", "Amount:10-11-12-13" });
				AssertQuotationLineList(docLineAmountValue, new[] { "Note1|||", "|AUD|10.00|KG", "|AUD|11.00|", "|AUD|12.00|M3", "|AUD|13.00|" });
			});
		}

		public void TestAdd()
		{
			var docLineAmountValue1 = new HighestDocLineAmountValue(currency: "AUD", applyTo: "Note1", weightUnit: "KG", weightRate: 10, weightFlat: 11, volumeUnit: "M3", volumeRate: 12, volumeFlat: 13);
			var docLineAmountValue2 = new HighestDocLineAmountValue(currency: "AUD", applyTo: "Note1", weightUnit: "KG", weightRate: 20, weightFlat: 21, volumeUnit: "M3", volumeRate: 22, volumeFlat: 23);

			var result = docLineAmountValue1.Add(docLineAmountValue2);
			AssertQuotationLineList(result, new[] { "Note1|||", "|AUD|30.00|KG", "|AUD|32.00|", "|AUD|34.00|M3", "|AUD|36.00|"
 });
		}
	}
}
