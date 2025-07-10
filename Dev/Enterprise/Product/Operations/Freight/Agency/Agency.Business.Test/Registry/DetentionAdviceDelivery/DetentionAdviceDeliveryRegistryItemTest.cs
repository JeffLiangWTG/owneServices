using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(DetentionAdviceDeliveryRegistryItem))]
	internal class DetentionAdviceDeliveryRegistryItemTest : StronglyTypedRegistryItemTestCase<DetentionAdviceDelivery>
	{
		#region Implementation
		protected override StronglyTypedRegistryItem<DetentionAdviceDelivery, DetentionAdviceDelivery> GetNewRegistryItem()
		{
			return new DetentionAdviceDeliveryRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
		#endregion
	}
}
