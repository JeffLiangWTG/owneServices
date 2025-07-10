using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectCategoryMapCollection))]
	class ProjectCategoryMapCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ProjectCategoryMapCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override ProjectCategoryMapCollection GetCollectionToTest()
		{
			return new ProjectCategoryMap().JiraClassificationMap;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProjectCategoryMapItem();
		}
	}
}
