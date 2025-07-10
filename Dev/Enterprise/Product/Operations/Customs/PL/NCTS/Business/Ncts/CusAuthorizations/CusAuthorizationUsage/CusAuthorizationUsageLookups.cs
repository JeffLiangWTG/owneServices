using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CusAuthorizationUsageLookups : EU.NCTS.Business.CusAuthorizationUsageLookups
{
	public CusAuthorizationUsageLookups(CusAuthorizationUsage parent) : base(parent)
	{
	}

	public override ICollection CodeList
	{
		get
		{
			var nctsHeader = Parent.NctsHeader;
			var isArrivalMovement = nctsHeader.IsArrivalMovement;

			CodeDescriptionPairList result;
			if (isArrivalMovement)
			{
				result = nctsHeader.ArrivalMovementHeader.Lookups.AuthorizationCodeList;
			}
			else
			{
				result = new CodeDescriptionPairList
				{
					CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, Customs.Business.CusAuthorizationHeaderTypeList.Descriptions.AuthorizedConsignorTransit),
					CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, Customs.Business.CusAuthorizationHeaderTypeList.Codes.SpecialSeals, Customs.Business.CusAuthorizationHeaderTypeList.Descriptions.SpecialSeals),
					CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCW1Code(Factory, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, Customs.Business.CusAuthorizationHeaderTypeList.Descriptions.TransitReducedDataset),
				};
			}
			return result;
		}
	}

	public CusAuthorisationRuleCollection AuthorisationRuleList
	{
		get
		{
			var parent = Parent;
			var result = new CusAuthorisationRuleCollection(Factory);
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationRuleCollection.FilterConstants.Country, "Property", parent.Header.CountryCode, false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationRuleCollection.FilterConstants.AuthorizationType, "Property", parent.AGC_Code));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationRuleCollection.FilterConstants.AuthorizationNumber, "Property", parent.AGC_Number));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationRuleCollection.FilterConstants.AuthorizationHolder, "Property", parent.AGC_OH_Owner));
			return result;
		}
	}

	new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;
}
