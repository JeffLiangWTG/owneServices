using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_07MessageSendingObjectLookups : LicensingMessageSendingObjectLookups
	{
		public NX201_07MessageSendingObjectLookups(NX201_07MessageSendingObject parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList GetActionListCore() => Factory.GetCachedValue<NX201_07ActionCodeList>();
	}
}
