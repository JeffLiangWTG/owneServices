using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class RegionOrgImpAddInfoLookups : EUOrgImpAddInfoLookups
{
	public RegionOrgImpAddInfoLookups(AutoEUOrgImpAddInfo parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList DefermentMethodList => Factory.GetCachedValue<PaymentPartyList>();
}
