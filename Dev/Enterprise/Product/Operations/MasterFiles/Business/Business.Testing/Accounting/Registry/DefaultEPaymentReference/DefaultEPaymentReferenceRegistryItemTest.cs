using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultEPaymentReferenceRegistryItem))]
	sealed class DefaultEPaymentReferenceRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultEPaymentReferenceCollection>
	{
		protected override StronglyTypedRegistryItem<DefaultEPaymentReferenceCollection, DefaultEPaymentReferenceCollection> GetNewRegistryItem()
		{
			return new DefaultEPaymentReferenceRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
