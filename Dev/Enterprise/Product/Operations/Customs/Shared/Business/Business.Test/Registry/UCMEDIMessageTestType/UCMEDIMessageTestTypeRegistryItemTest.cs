using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(UCMEDIMessageTestTypeRegistryItem))]
	sealed class UCMEDIMessageTestTypesRegistryItemTest : StronglyTypedRegistryItemTestCase<UCMEDIMessageTestTypeCollection>
	{
		protected override StronglyTypedRegistryItem<UCMEDIMessageTestTypeCollection, UCMEDIMessageTestTypeCollection> GetNewRegistryItem()
		{
			return new UCMEDIMessageTestTypeRegistryItem("", null, null, null);
		}
	}
}
