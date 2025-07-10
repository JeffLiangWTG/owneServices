using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX301MessageSendingObject : LicensingMessageSendingObject, INX301Declaration
	{
		public NX301MessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}
		protected override ZString GetEM_MessageTypeCore() => MessageTypeList.Codes._301;

		protected override ITWMessageBuilder GetMessageBuilder() => new NX301MessageBuilder();

		protected override CodeDescriptionPairList GetTypeListCore() => new NX301TypeList();

		protected override IGoodsShipment GetGoodsShipmentCore() => new NX301GoodsShipment(Header);
	}
}
