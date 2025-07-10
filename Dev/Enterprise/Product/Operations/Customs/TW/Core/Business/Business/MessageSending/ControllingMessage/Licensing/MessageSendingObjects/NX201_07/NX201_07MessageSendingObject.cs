using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_07MessageSendingObject : LicensingMessageSendingObject, INX201_07
	{
		public NX201_07MessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ZString GetEM_MessageTypeCore() => MessageTypeList.Codes._207;

		protected override ZBool ReasonDescription_Readonly => !IsReasonDescriptionRequired;

		public bool IsReasonDescriptionRequired => Factory.GetValue(ref isReasonDescriptionRequired, () =>
		{
			switch (Action)
			{
				case NX201_07ActionCodeList.Codes._1:
				case NX201_07ActionCodeList.Codes._50:
					return true;
				default:
					return false;
			}
		});
		CachedProperty<bool> isReasonDescriptionRequired;

		protected override ControllingMessageSendingObjectValidation GetNewValidation() => new NX201_07MessageSendingObjectValidation(this);

		public new NX201_07MessageSendingObjectValidation Validation => (NX201_07MessageSendingObjectValidation)base.Validation;

		public new NX201_07MessageSendingObjectLookups Lookups => (NX201_07MessageSendingObjectLookups)base.Lookups;

		protected override LicensingMessageSendingObjectLookups GetNewLookups() => new NX201_07MessageSendingObjectLookups(this);

		protected override CodeDescriptionPairList GetTypeListCore() => GetNX201TypeListCore(Factory, Header.TW1_BusinessType);

		protected override ITWMessageBuilder GetMessageBuilder() => new NX201_07MessageBuilder();

		protected override ZString DefaultAction => NX201_07ActionCodeList.Codes._5;

		protected override IAdditionalInformation GetAdditionalInformationCore() => IsReasonDescriptionRequired ? new LicensingMessageAdditionalInformation(ReasonDescription) : null;
	}
}
