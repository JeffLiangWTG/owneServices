using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectCategoryMapItem))]
	class ProjectCategoryMapItemTest : RegistryBusinessObjectTemplateTestCase<ProjectCategoryMapItem>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override ProjectCategoryMapItem GetBusinessObjectToClone()
		{
			return new ProjectCategoryMap().JiraClassificationMap.AddNew();
		}

		protected override ProjectCategoryMapItem GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
