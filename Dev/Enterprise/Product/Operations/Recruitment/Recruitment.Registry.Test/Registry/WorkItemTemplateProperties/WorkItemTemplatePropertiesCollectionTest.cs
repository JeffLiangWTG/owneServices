using CargoWise.EntityFramework;
using Enterprise.Recruitment.Registry;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Registry
{
	[TestedType(typeof(WorkItemTemplatePropertiesCollection))]
	sealed class WorkItemTemplatePropertiesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<WorkItemTemplatePropertiesCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override WorkItemTemplatePropertiesCollection GetCollectionToTest() => new WorkItemTemplatePropertiesCollection();
		protected override BusinessObject GetNewElementToAddToTheCollection() => new WorkItemTemplateProperties();
	}
}
