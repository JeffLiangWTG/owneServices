using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusConditionMappingFixture
	{
		[Test]
		public void GetMapping()
		{
			var mapping = RefCusConditionMapping.Mapping1;
			var applicabilityMapping = mapping.RelatedTableNames["RefCusApplicabilities"];
			Assert.AreEqual(2, applicabilityMapping.RelatedFKColumnNames.Count);
			Assert.False(applicabilityMapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(2, applicabilityMapping.RelatedTableNames.Count);
			Assert.False(applicabilityMapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));

			mapping = RefCusConditionMapping.Mapping2;
			applicabilityMapping = mapping.RelatedTableNames["RefCusApplicabilities"];
			Assert.AreEqual(3, applicabilityMapping.RelatedFKColumnNames.Count);
			Assert.True(applicabilityMapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(3, applicabilityMapping.RelatedTableNames.Count);
			Assert.True(applicabilityMapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));

			Assert.AreEqual(4, mapping.RelatedFKColumnNames.Count);
			Assert.False(mapping.RelatedFKColumnNames.ContainsKey("RefCusConditionLanguages"));
			Assert.AreEqual(4, mapping.RelatedTableNames.Count);
			Assert.False(mapping.RelatedTableNames.ContainsKey("RefCusConditionLanguages"));

			mapping = RefCusConditionMapping.Mapping3;
			Assert.AreEqual(5, mapping.RelatedFKColumnNames.Count);
			Assert.True(mapping.RelatedFKColumnNames.ContainsKey("RefCusConditionLanguages"));
			Assert.AreEqual(5, mapping.RelatedTableNames.Count);
			Assert.True(mapping.RelatedTableNames.ContainsKey("RefCusConditionLanguages"));
		}
	}
}
