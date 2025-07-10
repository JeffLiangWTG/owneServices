using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusTariffNationalCodeMappingFixture
	{
		[Test]
		public void GetMapping()
		{
			var mapping = RefCusTariffNationalCodeMapping.Mapping1;

			var rateMapping = mapping.RelatedTableNames["RefCusRates"];
			var applicabilityMapping = rateMapping.RelatedTableNames["RefCusApplicabilities"];
			Assert.AreEqual(2, applicabilityMapping.RelatedFKColumnNames.Count);
			Assert.False(applicabilityMapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(2, applicabilityMapping.RelatedTableNames.Count);
			Assert.False(applicabilityMapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));

			var additionalCodeMapping = mapping.RelatedTableNames["RefCusTariffAdditionalCodes"];
			applicabilityMapping = additionalCodeMapping.RelatedTableNames["RefCusApplicabilities"];
			Assert.AreEqual(2, applicabilityMapping.RelatedFKColumnNames.Count);
			Assert.False(applicabilityMapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(2, applicabilityMapping.RelatedTableNames.Count);
			Assert.False(applicabilityMapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));

			mapping = RefCusTariffNationalCodeMapping.Mapping2;
			rateMapping = mapping.RelatedTableNames["RefCusRates"];
			applicabilityMapping = rateMapping.RelatedTableNames["RefCusApplicabilities"];
			Assert.AreEqual(3, applicabilityMapping.RelatedFKColumnNames.Count);
			Assert.True(applicabilityMapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(3, applicabilityMapping.RelatedTableNames.Count);
			Assert.True(applicabilityMapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));

			additionalCodeMapping = mapping.RelatedTableNames["RefCusTariffAdditionalCodes"];
			applicabilityMapping = additionalCodeMapping.RelatedTableNames["RefCusApplicabilities"];
			Assert.AreEqual(3, applicabilityMapping.RelatedFKColumnNames.Count);
			Assert.True(applicabilityMapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(3, applicabilityMapping.RelatedTableNames.Count);
			Assert.True(applicabilityMapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));

			mapping = RefCusTariffNationalCodeMapping.Mapping3;
			var tariffUOMMapping = mapping.RelatedTableNames["RefCusTariffUOMs"];
			Assert.AreEqual(2, tariffUOMMapping.RelatedFKColumnNames.Count);
			Assert.True(tariffUOMMapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(2, tariffUOMMapping.RelatedTableNames.Count);
			Assert.True(tariffUOMMapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));

		}
	}
}
