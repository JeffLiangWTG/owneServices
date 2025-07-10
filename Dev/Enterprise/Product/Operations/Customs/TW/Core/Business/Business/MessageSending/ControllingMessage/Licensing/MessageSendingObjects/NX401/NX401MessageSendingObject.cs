using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;

namespace Enterprise.Customs.TW.Business
{
	public abstract class NX401MessageSendingObject : LicensingMessageSendingObject, INX401Declaration, IDeclarationAdditionalInformation
	{
		public NX401MessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		IDeclarationAdditionalInformation INX401Declaration.AdditionalInformation => this;

		ZString IDeclarationAdditionalInformation.StatementDescription => Header.TW1_PrePermitNumber;

		protected override ZString GetEM_MessageTypeCore() => MessageTypeList.Codes._401;

		protected override IConsignment GetConsignmentCore() => null;

		protected override ITWMessageBuilder GetMessageBuilder() => new NX401MessageBuilder();
	}
}
