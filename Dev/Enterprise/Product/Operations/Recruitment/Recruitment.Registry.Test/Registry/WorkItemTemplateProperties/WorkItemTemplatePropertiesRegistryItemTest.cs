using Enterprise.Integration;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Registry
{
	[TestedType(typeof(WorkItemTemplatePropertiesRegistryItem))]
	sealed class WorkItemTemplatePropertiesRegistryItemTest : StronglyTypedRegistryItemTestCase<WorkItemTemplatePropertiesCollection>
	{
		protected override StronglyTypedRegistryItem<WorkItemTemplatePropertiesCollection, WorkItemTemplatePropertiesCollection> GetNewRegistryItem()
			=> new WorkItemTemplatePropertiesRegistryItem("", null, null, null, RegistryStorageFlags.System);
	}
}
