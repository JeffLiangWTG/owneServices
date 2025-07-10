using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DistanceCalculationRegistry))]
	sealed class DistanceCalculationRegistryTest : RegistryItemSetTestCaseWithFactory<DistanceCalculationRegistry>
	{
		public void TestDistanceCalculationService()
		{
			DistanceCalculationProviderConfigurationRegistryItem item = ItemSet.DistanceCalculationProviderConfigurationItem;
			AssertEquals("Name", "DistanceCalculationProviderConfigurationItem", item.Name);
			AssertEquals("Category", DistanceCalculationRegistry.Categories.Freight_DistanceCalculationService, item.Category);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Default value type", typeof(DistanceCalculationProviderConfiguration), item.DefaultValue.GetType());
		}

		public void TestDistanceCalculationServiceURL()
		{
			TestRegistryItem(ItemSet.DistanceCalculationServiceURL,
						"DistanceCalculationServiceURL",
						DistanceCalculationRegistry.Categories.Freight_DistanceCalculationService,
						"Distance Calculation Service URL",
						"The URL of the Distance Calculation Service.",
						RegistryStorageFlags.System,
						TextEditorType.TextBox,
						RegistryOptions.IsOnlyForDevelopers,
						"https://webservices-ausyd.cargowise.net/DistanceCalculation/DistanceCalculationService.svc");
		}
	}
}
