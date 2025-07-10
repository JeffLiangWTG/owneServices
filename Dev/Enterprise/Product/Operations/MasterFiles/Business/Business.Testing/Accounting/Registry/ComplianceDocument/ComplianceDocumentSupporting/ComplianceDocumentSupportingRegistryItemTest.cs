using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceDocumentSupportingReasonRegistryItem))]
	sealed class ComplianceDocumentSupportingRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceDocumentSupportingReasonCollection>
	{
		protected override StronglyTypedRegistryItem<ComplianceDocumentSupportingReasonCollection, ComplianceDocumentSupportingReasonCollection> GetNewRegistryItem()
		{
			return new ComplianceDocumentSupportingReasonRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}
}
