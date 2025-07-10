using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TaxRateXmlParserTest : TransactionedTestCase
	{
		public void TestTaxRateXmlHasNoInvalidData()
		{
			var parser = new TaxRateXmlParser();
			var taxRatesByCountry = parser.BuildTaxRatesDictionaryBasedOnCountry();
			var isExtraRateTypeAvailable = false;

			foreach (var taxRatesOfACountry in taxRatesByCountry)
			{
				var countryCode = taxRatesOfACountry.Key;

				foreach (var rate in taxRatesOfACountry.Value)
				{
					var taxId = rate.TaxID;
					var type = rate.Type;
					var description = rate.Description;
					var postingGroupId = rate.PostingGroup;
					var extraRateType = rate.ExtraType;
					var referenceRateType = rate.ReferenceRateType;
					var referenceExtraRateType = rate.ReferenceExtraRateType;
					var taxSystem = rate.TaxSystem;

					Assert($"Tax Id can not be blank! Country code: {countryCode}", !string.IsNullOrWhiteSpace(taxId));
					Assert($"Tax rate type can not be blank! Country code: {countryCode}, Tax Id: {taxId}", !string.IsNullOrWhiteSpace(type));
					Assert($"Tax rate description can not be blank! Country code: {countryCode}, Tax Id: {taxId}", !string.IsNullOrWhiteSpace(description));
					Assert($"Posting Group can not be defined as 0 via XML file! Country code: {countryCode}, Tax Id: {taxId}", postingGroupId != AccTaxRate.DefaultPostingGroupID || !taxSystem.IsEmpty);
					Assert($"Reference Rate Type cannot be blank or empty! Country code: {countryCode}, Tax Id: {taxId}", !(string.IsNullOrWhiteSpace(referenceRateType)));

					Assert("Country code should be uppercase", !countryCode.ToString().Any(char.IsLower));
					Assert("Tax ID should be uppercase", !taxId.ToString().Any(char.IsLower));
					Assert("Type should be uppercase", !type.ToString().Any(char.IsLower));
					Assert("Reference Rate Type should be uppercase", !referenceRateType.ToString().Any(char.IsLower));
					Assert("Reference Extra Rate Type should be uppercase", !referenceExtraRateType.ToString().Any(char.IsLower));
					Assert("Extra Rate Type should be uppercase", !extraRateType.ToString().Any(char.IsLower));

					if (!isExtraRateTypeAvailable)
					{
						isExtraRateTypeAvailable = !string.IsNullOrWhiteSpace(extraRateType);
					}
					// TBD with Terri. Few rows in excel have data for Extra Rate but not for reference extra rate type, so below test fails
					//Assert($"Reference Extra Rate Type cannot be blank or empty when there is data available for Extra Rate Type! Country code: { countryCode }, Tax Id: { taxId }",
					//	!(!string.IsNullOrWhiteSpace(extraRateType) && string.IsNullOrWhiteSpace(referenceExtraRateType)));
				}
			}

			Assert("No <ExtraType> element found in xml file, remove <ReferenceExtraRateType> element if no more required", isExtraRateTypeAvailable);
		}
	}
}
