using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ARTermsRegistryItem))]
	sealed class ARTermsRegistryItemTest : StronglyTypedRegistryItemTestCase<ARTermsCollection>
	{
		protected override StronglyTypedRegistryItem<ARTermsCollection, ARTermsCollection> GetNewRegistryItem()
		{
			return new ARTermsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ARTermsCollection());
		}
	}
}
