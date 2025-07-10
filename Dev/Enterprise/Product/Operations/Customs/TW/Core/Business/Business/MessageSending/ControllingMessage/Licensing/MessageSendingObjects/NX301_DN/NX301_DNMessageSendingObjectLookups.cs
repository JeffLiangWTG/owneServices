using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX301_DNMessageSendingObjectLookups : LicensingMessageSendingObjectLookups
	{
		public NX301_DNMessageSendingObjectLookups(NX301_DNMessageSendingObject parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList GetActionListCore() => Factory.GetCachedValue<NX301_DNActionCodeList>();
	}
}
