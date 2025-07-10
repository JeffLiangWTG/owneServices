using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ImportMessageSendingObject : NX401MessageSendingObject
	{
		public NX401ImportMessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override IGoodsShipment GetGoodsShipmentCore() => new NX401ImportLicensingMessageGoodsShipment(Header);
	}
}
