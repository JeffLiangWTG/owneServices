using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(IssueTypeMappingRegistryItem))]
	class IssueTypeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<IssueTypeMap>
	{
		protected override StronglyTypedRegistryItem<IssueTypeMap, IssueTypeMap> GetNewRegistryItem()
		{
			var map = ProcessMgmtTestHelper.GetDummyIssueTypeMap();

			return new IssueTypeMappingRegistryItem("Moo. Really, moo.", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.Default, map);
		}
	}
}
