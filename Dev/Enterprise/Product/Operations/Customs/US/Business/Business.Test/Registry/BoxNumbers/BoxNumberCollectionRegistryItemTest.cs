using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BoxNumberCollectionRegistryItem))]
	sealed class BoxNumberCollectionRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<BoxNumberCollection>
	{
		protected override StronglyTypedRegistryItem<BoxNumberCollection, BoxNumberCollection> GetNewRegistryItem()
		{
			return new BoxNumberCollectionRegistryItem("", null, null, null, new BoxNumberCollection());
		}

		protected override BoxNumberCollection ValidValue
		{
			get
			{
				BoxNumberCollection collection = new BoxNumberCollection();
				BoxNumber boxNo = collection.AddNew();
				boxNo.TransportMode = BoxNoTransportModeList.Codes.ALL;
				boxNo.BoxNo = "123";
				return collection;
			}
		}
	}
}
