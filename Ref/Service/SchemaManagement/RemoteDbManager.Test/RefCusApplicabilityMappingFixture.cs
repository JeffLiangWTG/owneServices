using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusApplicabilityMappingFixture
	{
		[Test]
		public void GetMapping()
		{
			var mapping = RefCusApplicabilityMapping.Mapping1;
			Assert.AreEqual(2, mapping.RelatedFKColumnNames.Count);
			Assert.False(mapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(2, mapping.RelatedTableNames.Count);
			Assert.False(mapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));

			mapping = RefCusApplicabilityMapping.Mapping2;
			Assert.AreEqual(3, mapping.RelatedFKColumnNames.Count);
			Assert.True(mapping.RelatedFKColumnNames.ContainsKey("RefCusTradeGroup1"));
			Assert.AreEqual(3, mapping.RelatedTableNames.Count);
			Assert.True(mapping.RelatedTableNames.ContainsKey("RefCusTradeGroup1"));
		}
	}
}
