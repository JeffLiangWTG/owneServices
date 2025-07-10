using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface IShipmentReferenceDetail
	{
		ZString Qualifier { get; }
		ZString ReferenceIdentifier { get; }
	}

	public class ShipmentReferenceDetail : IShipmentReferenceDetail
	{
		public ZString Qualifier { get; set; }
		public ZString ReferenceIdentifier { get; set; }
	}
}
