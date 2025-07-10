using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(JiraCustomFieldMapCollection))]
	class JiraCustomFieldMapCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<JiraCustomFieldMapCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override JiraCustomFieldMapCollection GetCollectionToTest()
		{
			return new JiraCustomFieldMap().JiraClassificationMap;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new JiraCustomFieldMapItem();
		}
	}
}
