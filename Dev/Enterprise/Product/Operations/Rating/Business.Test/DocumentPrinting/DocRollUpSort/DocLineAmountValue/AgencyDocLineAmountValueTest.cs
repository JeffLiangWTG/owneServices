using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class AgencyDocLineAmountValueTest : BaseDocLineAmountValueTest
	{
		public void TestValuesPropertiesText()
		{
			var docLineAmountValue = new AgencyDocLineAmountValue
			(
				currency: "AUD",
				description: "description1",
				agencyRate: 10,
				agencyRateDescription: "Agency1",
				additionalRate: 11,
				additionalRateDescription: "Additional1",
				costPerAdditionalRate: 12,
				costPerAdditionalRateDescription: "CostPerAdditional1",
				unit: "KG"
			);

			CombineAssertions(() =>
			{
				AssertValues(docLineAmountValue, expectedValues: new[] { "AgencyRate:10", "AdditionalRate:11", "CostPerAdditionalRate:12" });
				AssertProperties
				(
					docLineAmountValue,
					expectedProperties: new[]
					{
						"Type:AgencyDocLineAmountValue",
						"Currency:AUD",
						"Description:description1",
						"AgencyRateDescription:Agency1",
						"AdditionalRateDescription:Additional1",
						"CostPerAdditionalRateDescription:CostPerAdditional1",
						"Unit:KG"
					}
				);
				AssertQuotationLineList
				(
					docLineAmountValue,
					new[]
					{
						"description1|||",
						"Agency1|AUD|10.00|",
						"Additional1|AUD|11.00|",
						"CostPerAdditional1|AUD|12.00|KG"
					}
				);
			});
		}

		public void TestAdd()
		{
			var docLineAmountValue1 = new AgencyDocLineAmountValue(currency: "AUD", description: "description1", agencyRate: 10, agencyRateDescription: "Agency1", 11, additionalRateDescription: "Additional1", costPerAdditionalRate: 12, costPerAdditionalRateDescription: "CostPerAdditional1", unit: "KG");
			var docLineAmountValue2 = new AgencyDocLineAmountValue(currency: "AUD", description: "description1", agencyRate: 20, agencyRateDescription: "Agency1", 21, additionalRateDescription: "Additional1", costPerAdditionalRate: 22, costPerAdditionalRateDescription: "CostPerAdditional1", unit: "KG");

			var result = docLineAmountValue1.Add(docLineAmountValue2);
			AssertQuotationLineList
			(
				result,
				new[]
				{
					"description1|||",
					"Agency1|AUD|30.00|",
					"Additional1|AUD|32.00|",
					"CostPerAdditional1|AUD|34.00|KG"
				}
			);
		}
	}
}
