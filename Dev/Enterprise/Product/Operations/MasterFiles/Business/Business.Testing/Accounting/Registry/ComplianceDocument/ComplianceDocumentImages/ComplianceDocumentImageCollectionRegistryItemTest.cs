using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceDocumentImageCollectionRegistryItem))]
	sealed class ComplianceDocumentImageCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceDocumentImageCollection>
	{
		protected override StronglyTypedRegistryItem<ComplianceDocumentImageCollection, ComplianceDocumentImageCollection> GetNewRegistryItem()
		{
			return new ComplianceDocumentImageCollectionRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
