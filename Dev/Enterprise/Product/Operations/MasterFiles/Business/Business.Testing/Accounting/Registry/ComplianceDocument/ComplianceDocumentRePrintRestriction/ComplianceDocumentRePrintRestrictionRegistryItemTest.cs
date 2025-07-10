using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceDocumentRePrintRestrictionRegistryItem))]
	sealed class ComplianceDocumentRePrintRestrictionRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceDocumentRePrintRestrictionCollection>
	{
		protected override StronglyTypedRegistryItem<ComplianceDocumentRePrintRestrictionCollection, ComplianceDocumentRePrintRestrictionCollection> GetNewRegistryItem()
		{
			return new ComplianceDocumentRePrintRestrictionRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
