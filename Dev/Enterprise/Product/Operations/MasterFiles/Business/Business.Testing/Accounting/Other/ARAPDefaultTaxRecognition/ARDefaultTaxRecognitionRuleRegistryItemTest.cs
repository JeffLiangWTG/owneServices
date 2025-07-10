using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ARDefaultTaxRecognitionRuleRegistryItem))]
	sealed class ARDefaultTaxRecognitionRuleRegistryItemTest : StronglyTypedRegistryItemTestCase<ARAPDefaultTaxRecognitionRuleCollection>
	{
		protected override StronglyTypedRegistryItem<ARAPDefaultTaxRecognitionRuleCollection, ARAPDefaultTaxRecognitionRuleCollection> GetNewRegistryItem()
		{
			return new ARDefaultTaxRecognitionRuleRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
