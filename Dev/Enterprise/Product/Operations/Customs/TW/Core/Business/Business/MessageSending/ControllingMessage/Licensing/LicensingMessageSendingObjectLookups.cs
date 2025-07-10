using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageSendingObjectLookups : ZLookups
	{
		public LicensingMessageSendingObjectLookups(LicensingMessageSendingObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ActionList => GetActionListCore();

		protected virtual CodeDescriptionPairList GetActionListCore() => Factory.GetCachedValue<NXCommonActionCodeList>();
	}
}
