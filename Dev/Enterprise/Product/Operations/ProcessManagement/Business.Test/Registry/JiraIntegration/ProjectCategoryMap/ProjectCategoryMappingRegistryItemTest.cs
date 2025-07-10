using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectCategoryMappingRegistryItem))]
	class ProjectCategoryMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<ProjectCategoryMap>
	{
		protected override StronglyTypedRegistryItem<ProjectCategoryMap, ProjectCategoryMap> GetNewRegistryItem()
		{
			var map = ProcessMgmtTestHelper.GetDummyProjectCategoryMap();

			return new ProjectCategoryMappingRegistryItem("Moo. Really, moo.", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.Default, map);
		}
	}
}
