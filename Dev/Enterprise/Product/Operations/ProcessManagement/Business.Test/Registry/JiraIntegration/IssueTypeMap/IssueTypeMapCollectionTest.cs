using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(IssueTypeMapCollection))]
	class IssueTypeMapCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<IssueTypeMapCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override IssueTypeMapCollection GetCollectionToTest()
		{
			return new IssueTypeMap().JiraClassificationMap;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IssueTypeMapItem();
		}
	}
}
