using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX603MessageSendingObject : LicensingMessageSendingObject, INX603Declaration
	{
		public NX603MessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ZString GetEM_MessageTypeCore() => MessageTypeList.Codes._603;

		protected override ITWMessageBuilder GetMessageBuilder() => new NX603MessageBuilder();

		protected override CodeDescriptionPairList GetTypeListCore() => new NX601_NX603TypeList();
	}
}
