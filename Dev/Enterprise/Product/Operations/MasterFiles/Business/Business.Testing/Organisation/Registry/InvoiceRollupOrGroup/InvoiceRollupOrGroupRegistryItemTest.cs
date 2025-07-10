using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InvoiceRollupOrGroupRegistryItem))]
	sealed class InvoiceRollupOrGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<InvoiceRollupOrGroupCollection>
	{
		protected override StronglyTypedRegistryItem<InvoiceRollupOrGroupCollection, InvoiceRollupOrGroupCollection> GetNewRegistryItem()
		{
			return new InvoiceRollupOrGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new InvoiceRollupOrGroupCollection());
		}
	}
}
