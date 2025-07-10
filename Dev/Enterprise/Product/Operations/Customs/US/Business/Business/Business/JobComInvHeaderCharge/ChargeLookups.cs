
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public ChargeLookups(Customs.Business.BaseJobComInvHeaderCharge charge)
			: base(charge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get { return Factory.GetCachedValue<USCustomsChargeTypeList>(); }
		}
	}
}
