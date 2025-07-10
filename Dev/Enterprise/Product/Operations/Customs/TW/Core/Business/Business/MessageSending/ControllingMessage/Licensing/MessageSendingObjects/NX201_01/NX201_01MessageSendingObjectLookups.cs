using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_01MessageSendingObjectLookups : LicensingMessageSendingObjectLookups
	{
		public NX201_01MessageSendingObjectLookups(NX201_01MessageSendingObject parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList GetActionListCore() => Factory.GetCachedValue<NX201_01ActionCodeList>();
	}
}
