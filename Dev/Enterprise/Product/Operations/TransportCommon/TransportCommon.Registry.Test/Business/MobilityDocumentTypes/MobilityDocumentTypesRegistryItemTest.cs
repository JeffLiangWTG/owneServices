using Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(MobilityDocumentTypesRegistryItem))]
	internal class MobilityDocumentTypesRegistryItemTest : StronglyTypedRegistryItemTestCase<MobilityDocumentTypeCollection>
	{
		protected override StronglyTypedRegistryItem<MobilityDocumentTypeCollection, MobilityDocumentTypeCollection> GetNewRegistryItem()
		{
			return new MobilityDocumentTypesRegistryItem(string.Empty, null, new MobilityDocumentTypeCollection());
		}
	}
}
