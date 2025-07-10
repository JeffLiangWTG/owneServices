using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(JiraCustomFieldMappingRegistryItem))]
	class JiraCustomFieldRegistryItemTest : StronglyTypedRegistryItemTestCase<JiraCustomFieldMap>
	{
		protected override StronglyTypedRegistryItem<JiraCustomFieldMap, JiraCustomFieldMap> GetNewRegistryItem()
		{
			var map = ProcessMgmtTestHelper.GetDummyJiraCustomFieldMap();

			return new JiraCustomFieldMappingRegistryItem(
				"Boring test item name",
				(NoResString)"",
				(NoResString)"",
				(NoResString)"",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				map);
		}
	}
}
