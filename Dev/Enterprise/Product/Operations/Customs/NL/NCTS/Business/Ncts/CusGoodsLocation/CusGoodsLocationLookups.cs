using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CusGoodsLocationLookups : EU.NCTS.Business.CusGoodsLocationLookups
{
	public CusGoodsLocationLookups(EU.NCTS.Business.CusGoodsLocation parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList QualifierList
	{
		get
		{
			var isParentIncidentPhase5Arrival = Parent.IsParentIncidentPhase5Arrival;
			return Factory.GetCachedValue("NL.NCTS.CusGoodsLocationLookups.QualifierList_" + isParentIncidentPhase5Arrival, () =>
			{
				var qualifierList = new CodeDescriptionPairList();

				if (isParentIncidentPhase5Arrival)
				{
					qualifierList = base.QualifierList;
				}
				else
				{
					qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.PostcodeAddress, CusGoodsLocationQualifierList.Descriptions.PostcodeAddress);
				}
				return qualifierList;
			});
		}
	}

	public override CodeDescriptionPairList TypeList
	{
		get
		{
			var isParentDepartureMovementHeader = Parent.DepartureMovementHeader != null;
			return Factory.GetCachedValue("NL.NCTS.CusGoodsLocationLookups.TypeList_" + isParentDepartureMovementHeader, () =>
			{
				var typeList = new CodeDescriptionPairList();

				if (isParentDepartureMovementHeader)
				{
					typeList.AddPair(CusGoodsLocationTypeList.Codes.DesignatedLocation, CusGoodsLocationTypeList.Descriptions.DesignatedLocation);
					typeList.AddPair(CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationTypeList.Descriptions.AuthorizedPlace);
				}
				else
				{
					typeList = base.TypeList;
				}

				return typeList;
			});
		}
	}

	new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;
}
