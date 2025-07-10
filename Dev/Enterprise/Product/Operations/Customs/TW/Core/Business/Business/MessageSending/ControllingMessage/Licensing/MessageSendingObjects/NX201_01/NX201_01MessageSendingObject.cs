using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_01MessageSendingObject : LicensingMessageSendingObject, INX201_01Declaration
	{
		public NX201_01MessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ZString GetEM_MessageTypeCore() => MessageTypeList.Codes._201;

		protected override IAdditionalInformation GetAdditionalInformationCore()
		{
			IAdditionalInformation result = null;
			switch (Action)
			{
				case NX201_01ActionCodeList.Codes._4:
					result = new LicensingMessageAdditionalInformation(Header.ProcessingNumber, ZString.Empty);
					break;
				case NX201_01ActionCodeList.Codes._5:
					result = new LicensingMessageAdditionalInformation(ReasonDescription);
					break;
				case NX201_01ActionCodeList.Codes._17:
					result = new LicensingMessageAdditionalInformation(ZString.Empty, Header.ProcessingNumber, ReasonDescription);
					break;
				case NX201_01ActionCodeList.Codes._52:
					result = new LicensingMessageAdditionalInformation(ReasonDescription);
					break;
			}
			return result;
		}

		public ZString AdditionalDeclarationID => Action == NX201_01ActionCodeList.Codes._52 ? Header.CustomsMessageIdentifier : ZString.Empty;

		protected override ZBool ReasonDescription_Readonly => base.ReasonDescription_Readonly && !ActionForReasonDescriptions.Contains(Action);

		public readonly HashSet<ZString> ActionForReasonDescriptions = new HashSet<ZString> { NX201_01ActionCodeList.Codes._5, NX201_01ActionCodeList.Codes._17, NX201_01ActionCodeList.Codes._52 };

		public readonly HashSet<ZString> ActionForProcessingNumbers = new HashSet<ZString> { NX201_01ActionCodeList.Codes._4, NX201_01ActionCodeList.Codes._17 };

		protected override ControllingMessageSendingObjectValidation GetNewValidation() => new NX201_01MessageSendingObjectValidation(this);

		public new NX201_01MessageSendingObjectValidation Validation => (NX201_01MessageSendingObjectValidation)base.Validation;

		public new NX201_01MessageSendingObjectLookups Lookups => (NX201_01MessageSendingObjectLookups)base.Lookups;

		protected override LicensingMessageSendingObjectLookups GetNewLookups() => new NX201_01MessageSendingObjectLookups(this);

		protected override CodeDescriptionPairList GetTypeListCore() => GetNX201TypeListCore(Factory, Header.TW1_BusinessType);

		protected override ITWMessageBuilder GetMessageBuilder() => new NX201_01MessageBuilder();

		protected override IGoodsShipment GetGoodsShipmentCore()
		{
			if (IsImport)
			{
				return new NX201_01ImportLicensingMessageGoodsShipment(Header);
			}
			else
			{
				return new NX201_01ExportLicensingMessageGoodsShipment(Header);
			}
		}
	}
}
