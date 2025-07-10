using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.VN.Manifest.Business.Testing
{
	[TestedType(typeof(VNCustomsDataRegistry))]
	sealed class VNCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<VNCustomsDataRegistry>
	{
		public void TestEnableVNManifest()
		{
			TestVNManifestRegistryItem(true);
		}

		public void TestDisableVNManifest()
		{
			TestVNManifestRegistryItem(false);
		}

		void TestVNManifestRegistryItem(bool defaultFeatureControlValue)
		{
			ResetRegistryCache();

			var featureControlMock = CustomsFeatureControlTestHelper.CreateVietnamManifestFeatureControlMock(defaultFeatureControlValue);
			using (ObjectFactory.Substitute(featureControlMock.Object))
			{
				TestGenericRegistryItem(
					ItemSet.EnableVNManifest,
					"EnableVNManifest",
					VNCustomsDataRegistry.Categories.Customs_Vietnam,
					"Enable Vietnam Manifest",
					"Enable Vietnam Manifest?",
					RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
					defaultFeatureControlValue);
			}
		}

		void ResetRegistryCache()
		{
			RegistryItemDictionary.Instance.PurgeAll();
		}
	}
}
