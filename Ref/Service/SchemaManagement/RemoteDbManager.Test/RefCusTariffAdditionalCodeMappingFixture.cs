using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusTariffAdditionalCodeMappingFixture
	{
		[Test]
		public void GetMapping()
		{
			var mapping = RefCusTariffAdditionalCodeMapping.Mapping1;
			var applicabilityMapping = mapping.RelatedTableNames["RefCusApplicabilities"];
			Assert.AreEqual(2, applicabilityMapping.RelatedFKColumnNames.Count);
			Assert.False(applicabilityMapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(2, applicabilityMapping.RelatedTableNames.Count);
			Assert.False(applicabilityMapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));

			mapping = RefCusTariffAdditionalCodeMapping.Mapping2;
			applicabilityMapping = mapping.RelatedTableNames["RefCusApplicabilities"];
			Assert.AreEqual(3, applicabilityMapping.RelatedFKColumnNames.Count);
			Assert.True(applicabilityMapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(3, applicabilityMapping.RelatedTableNames.Count);
			Assert.True(applicabilityMapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));
		}
	}
}
