using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.NL.Business;
using static Enterprise.Customs.NL.NCTS.Business.NLNctsConstants;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CusGoodsLocation : EU.NCTS.Business.CusGoodsLocation, Integration.Customs.NL.INctsCusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override ZString CGL_Qualifier
	{
		get => base.CGL_Qualifier;
		set
		{
			var oldValue = base.CGL_Qualifier;
			base.CGL_Qualifier = value;
			if (oldValue != value && value == Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress)
			{
				Address.E2_RN_NKCountryCode = Constants.CountryCodes.Netherlands;
			}
		}
	}

	protected override void SetDefaultsForNew()
	{
		base.SetDefaultsForNew();
		if (!isParentEnRouteIncident)
		{
			CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
			CGL_Type = isDepartureWithACROrTRDAuthorization || isArrivalWithACEOrACTAuthorization ? Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace : Customs.Business.CusGoodsLocationTypeList.Codes.DesignatedLocation;
		}
	}

	bool isDepartureWithACROrTRDAuthorization => DepartureMovementHeader is NctsDepartureMovementHeader moveHeader && moveHeader.CusAuthorizationUsages.Any(x => x.AGC_Code.In<ZString>(Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset));

	bool isArrivalWithACEOrACTAuthorization => ArrivalMovementHeader is NctsArrivalMovementHeader arrivalHeader && arrivalHeader.Header.CusAuthorizationUsages.Any(x => x.AGC_Code.In<ZString>(Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));

	public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

	protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

	public void SetDefaultsFromAuthorizationIfNeeded(Customs.Business.CusAuthorisationHeader cusAuthorisationHeader)
	{
		if ((isDepartureWithACROrTRDAuthorization || isArrivalWithACEOrACTAuthorization)
			&& cusAuthorisationHeader != null
			&& cusAuthorisationHeader.CPH_Type.In<ZString>(Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit,
														   Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir)
			&& cusAuthorisationHeader.CusAuthorisationRules.Count(x => x.CPR_RuleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location) == 1)
		{
			var location = Factory.Load<CusAuthorisationRule>(cusAuthorisationHeader.CusAuthorisationRules.First(x => x.CPR_RuleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location).PK).GoodsLocation;
			if (CGL_AdditionalIdentifier.IsEmpty)
			{
				CGL_AdditionalIdentifier = location.CGL_AdditionalIdentifier;
			}
			if (Address.E2_Postcode.IsEmpty)
			{
				Address.E2_Postcode = location.Address.E2_Postcode;
			}
			if (Address.E2_RN_NKCountryCode.IsEmpty)
			{
				Address.E2_RN_NKCountryCode = location.Address.E2_RN_NKCountryCode;
			}
		}
	}

	bool isParentEnRouteIncident => CGL_ParentTableCode == GoodsLocationParentTableCodes.Incident;
}
