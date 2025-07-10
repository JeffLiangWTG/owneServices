using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business;

public class RegionOrgImpAddInfo : EUOrgImpAddInfo, Integration.Customs.NL.IRegionOrgImpAddInfo
{
	public RegionOrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
	{
		SetDefaultValueForOtherDeferType();
	}

	public RegionOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo) : base(parentPropertyInfo)
	{
		SetDefaultValueForOtherDeferType();
	}

	protected override EUOrgImpAddInfoLookups GetNewLookups() => new RegionOrgImpAddInfoLookups(this);

	public new RegionOrgImpAddInfoLookups Lookups => (RegionOrgImpAddInfoLookups)base.Lookups;

	public void SetDefaultValueForOtherDeferType()
	{
		if (ZO_OtherDeferType.IsEmpty)
		{
			ZO_OtherDeferType = PaymentPartyList.Codes.Representative;
		}
	}
}
