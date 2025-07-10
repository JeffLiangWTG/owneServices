using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryLineFeeLookups : Customs.Business.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList RateOverrideReasonList => Factory.GetCachedValue<RateOverrideReasonList>();
	}
}
