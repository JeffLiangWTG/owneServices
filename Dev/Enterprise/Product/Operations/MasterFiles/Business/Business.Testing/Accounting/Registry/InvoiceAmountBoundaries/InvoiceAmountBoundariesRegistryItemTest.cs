using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(InvoiceAmountBoundariesRegistryItem))]
sealed class InvoiceAmountBoundariesRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<InvoiceAmountBoundaryCollection>
{
	protected override StronglyTypedRegistryItem<InvoiceAmountBoundaryCollection, InvoiceAmountBoundaryCollection> GetNewRegistryItem()
	{
		return new InvoiceAmountBoundariesRegistryItem("", null, null, null, RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForSupport, new InvoiceAmountBoundaryCollection());
	}
}
