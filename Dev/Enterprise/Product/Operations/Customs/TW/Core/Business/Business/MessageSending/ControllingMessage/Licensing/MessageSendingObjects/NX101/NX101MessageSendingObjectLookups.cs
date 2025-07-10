using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX101MessageSendingObjectLookups : LicensingMessageSendingObjectLookups
	{
		public NX101MessageSendingObjectLookups(NX101MessageSendingObject parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList GetActionListCore() => Factory.GetCachedValue<NX101ActionCodeList>();
	}
}
