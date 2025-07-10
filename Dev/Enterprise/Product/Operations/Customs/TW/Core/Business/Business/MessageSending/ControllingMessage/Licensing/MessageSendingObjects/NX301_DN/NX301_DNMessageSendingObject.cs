using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX301_DNMessageSendingObject : LicensingMessageSendingObject, INX301_DNDeclaration
	{
		public NX301_DNMessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ZString GetEM_MessageTypeCore() => MessageTypeList.Codes._31D;

		protected override IPackaging GetPackagingCore() => null;

		protected override ZDate GetAcceptanceDateTimeCore() => ZDate.Empty;

		protected override CodeDescriptionPairList GetTypeListCore()
		{
			var listKey = "TW_NX301_DN_" + BusinessType;
			return Factory.GetCachedValue(listKey, () =>
			{
				CodeDescriptionPairList result;
				switch (BusinessType)
				{
					case CPT_111_BusinessTypeList.Codes.Inspection:
						result = new NX301_DN_ATypeList();
						break;
					case CPT_111_BusinessTypeList.Codes.ExemptionFromInspection:
						result = new NX301_DN_CTypeList();
						break;
					default:
						result = new CodeDescriptionPairList();
						break;
				}
				return result;
			});
		}

		public new NX301_DNMessageSendingObjectLookups Lookups => (NX301_DNMessageSendingObjectLookups)base.Lookups;

		protected override LicensingMessageSendingObjectLookups GetNewLookups() => new NX301_DNMessageSendingObjectLookups(this);

		protected override ITWMessageBuilder GetMessageBuilder() => new NX301_DNMessageBuilder();
	}
}
