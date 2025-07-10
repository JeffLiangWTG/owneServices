using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX301_AXMessageSendingObject : LicensingMessageSendingObject, INX301_AXDeclaration
	{
		public NX301_AXMessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ZString GetEM_MessageTypeCore() => MessageTypeList.Codes._31A;

		protected override ZDate GetAcceptanceDateTimeCore() => ZDate.Empty;

		protected override IApplication GetApplicationCore() => new NX301_AXLicensingMessageApplication(Header, this);

		protected override CodeDescriptionPairList GetTypeListCore() => new NX301_AXTypeList();

		protected override ControllingMessageSendingObjectValidation GetNewValidation() => new NX301_AXMessageSendingObjectValidation(this);

		protected override ITWMessageBuilder GetMessageBuilder() => new NX301_AXMessageBuilder();

		public new NX301_AXMessageSendingObjectValidation Validation => (NX301_AXMessageSendingObjectValidation)base.Validation;
	}
}
