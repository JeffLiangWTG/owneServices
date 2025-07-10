using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusProfileTypeMappingFixture
	{
		[Test]
		public void Mapping()
		{
			var mapping = RefCusProfileTypeMapping.Mapping;
			Assert.AreEqual("#TempRefCusProfileType", mapping.TableName);
			Assert.AreEqual(1, mapping.RelatedFKColumnNames.Count);
			Assert.True(mapping.RelatedFKColumnNames.ContainsKey("RefCusTariffType"));
			Assert.AreEqual("XXX_ZZI_TariffType", mapping.RelatedFKColumnNames["RefCusTariffType"]);
			Assert.AreEqual(1, mapping.RelatedTableNames.Count);
			Assert.True(mapping.RelatedTableNames.ContainsKey("RefCusTariffType"));
		}
	}
}
