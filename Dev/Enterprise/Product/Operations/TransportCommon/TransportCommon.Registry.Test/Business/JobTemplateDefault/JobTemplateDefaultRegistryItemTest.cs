using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(JobTemplateDefaultRegistryItem))]
	class JobTemplateDefaultRegistryItemTest : StronglyTypedRegistryItemTestCase<JobTemplateDefaultCollection>
	{
		protected override StronglyTypedRegistryItem<JobTemplateDefaultCollection, JobTemplateDefaultCollection> GetNewRegistryItem()
		{
			return new JobTemplateDefaultRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new JobTemplateDefaultCollection());
		}
	}
}
