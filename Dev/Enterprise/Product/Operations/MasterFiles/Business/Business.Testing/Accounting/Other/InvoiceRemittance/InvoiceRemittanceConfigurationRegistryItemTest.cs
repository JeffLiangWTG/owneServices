using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InvoiceRemittanceConfigurationRegistryItem))]
	sealed class InvoiceRemittanceConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<InvoiceRemittanceConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<InvoiceRemittanceConfigurationCollection, InvoiceRemittanceConfigurationCollection> GetNewRegistryItem()
		{
			return new InvoiceRemittanceConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
