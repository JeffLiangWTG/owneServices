using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(RecipientSourceFallbackRegistryItem))]
	class RecipientSourceFallbackRegistryItemTest : StronglyTypedRegistryItemTestCase<RecipientSourceFallbackHeader>
	{
		protected override StronglyTypedRegistryItem<RecipientSourceFallbackHeader, RecipientSourceFallbackHeader> GetNewRegistryItem()
		{
			return new RecipientSourceFallbackRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
