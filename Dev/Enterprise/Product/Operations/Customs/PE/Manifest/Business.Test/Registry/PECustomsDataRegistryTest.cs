using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	[TestedType(typeof(PECustomsDataRegistry))]
	sealed class PECustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<PECustomsDataRegistry>
	{
		public void TestEnablePEManifests()
		{
			TestRegistryItem(
				ItemSet.EnablePEManifests,
				"EnablePEManifests",
				PECustomsDataRegistry.Categories.Customs_Peru,
				"Enable Peru Manifest",
				"Enable Peru Manifest?",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}
	}
}
