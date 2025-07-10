using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultEPaymentReasonRegistryItem))]
	sealed class DefaultEPaymentReasonRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultEPaymentReasonCollection>
	{
		protected override StronglyTypedRegistryItem<DefaultEPaymentReasonCollection, DefaultEPaymentReasonCollection> GetNewRegistryItem()
		{
			return new DefaultEPaymentReasonRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
