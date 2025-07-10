using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NctsDepartureMovementHeaderLookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Lookups
{
	public NctsDepartureMovementHeaderLookups(NctsDepartureMovementHeader parent) : base(parent)
	{
	}
	protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

	protected override CodeDescriptionPairList NctsTransitStatusListCore => Factory.GetCachedValue<NCTSDepartureCustomsStatusList>();

	protected override CodeDescriptionPairList TypeOfSecurityListCore => Factory.GetCachedValue("NL.NCTS.NctsTypeOfSecurityList", () =>
	{
		var list = new CodeDescriptionPairList(base.TypeOfSecurityListCore);
		list.RemoveCode(EU.NCTS.Business.NctsTypeOfSecurityList.Codes.ENT);
		list.RemoveCode(EU.NCTS.Business.NctsTypeOfSecurityList.Codes.BTH);
		return list;
	});
}
