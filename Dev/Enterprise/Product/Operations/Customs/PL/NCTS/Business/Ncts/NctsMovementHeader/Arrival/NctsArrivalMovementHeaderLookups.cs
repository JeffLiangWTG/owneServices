using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsArrivalMovementHeaderLookups : EU.NCTS.Business.NctsArrivalMovementHeaderLookups
{
	public NctsArrivalMovementHeaderLookups(NctsArrivalMovementHeader parent) : base(parent)
	{
	}

	public new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

	protected override CodeDescriptionPairList AuthorizationCodeListCore
	{
		get
		{
			return new CodeDescriptionPairList()
			{
				CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit, NctsArrivalAuthorizationCodeList.Descriptions.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit),
				CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure, NctsArrivalAuthorizationCodeList.Descriptions.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure),
			};
		}
	}

	public override ICollection AuthorizationRuleList
	{
		get
		{
			var parent = Parent;
			var result = new CusAuthorisationRuleCollection(Factory);
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationRuleCollection.FilterConstants.Country, "Property", parent.Header.CountryCode, false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationRuleCollection.FilterConstants.AuthorizationType, "Property", parent.AuthorizationCode));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationRuleCollection.FilterConstants.AuthorizationNumber, "Property", parent.AuthorizationNumber));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationRuleCollection.FilterConstants.AuthorizationHolder, "Property", parent.AuthorizationOwner));
			return result;
		}
	}
}
