using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityProfileRegistryItem))]
	sealed class OrgSecurityProfileRegistryItemTest : StronglyTypedRegistryItemTestCase<OrgSecurityProfileCollection>
	{
		protected override StronglyTypedRegistryItem<OrgSecurityProfileCollection, OrgSecurityProfileCollection> GetNewRegistryItem()
		{
			return new OrgSecurityProfileRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new OrgSecurityProfileCollection());
		}
	}
}
