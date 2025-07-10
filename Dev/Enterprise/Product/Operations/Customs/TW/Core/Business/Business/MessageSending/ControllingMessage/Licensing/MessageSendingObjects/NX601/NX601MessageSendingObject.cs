using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX601MessageSendingObject : LicensingMessageSendingObject, INX601Declaration
	{
		public NX601MessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ZString GetEM_MessageTypeCore() => MessageTypeList.Codes._601;

		protected override IApplication GetApplicationCore() => new NX601LicensingMessageApplication(Header, this);

		protected override ITWMessageBuilder GetMessageBuilder() => new NX601MessageBuilder();

		protected override CodeDescriptionPairList GetTypeListCore() => new NX601_NX603TypeList();
	}
}
