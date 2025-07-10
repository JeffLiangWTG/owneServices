using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business;

sealed class NctsDepartureMovementHeaderPhase5Lookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Lookups, INctsDepartureMovementHeaderLookups
{
	public NctsDepartureMovementHeaderPhase5Lookups(NctsDepartureMovementHeader parent) : base(parent)
	{
	}

	public CodeDescriptionPairList GoodsShippingLocationList => Factory.GetCachedValue<GoodsShippingLocationList>();

	public CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue<NCTSMovementHeaderCustomsStatusList>();

	public override CodeDescriptionPairList DeclarationTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, ZDateTime.Today);

	public override CodeDescriptionPairList SpecificCircumstanceIndicatorList => Factory.GetCachedValue<SpesificCircumstanceIndicatorList>();

	public CodeDescriptionPairList GoodsShiptoCodeList => Factory.GetCachedValue<GoodsShiptoCodeList>();

	public ZZRefCusCodeListCombinedCollection TRWarehouseList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, ZDateTime.Today);

	RefVesselCollection INctsDepartureMovementHeaderLookups.Vessels => base.Vessels;
	ICollection INctsDepartureMovementHeaderLookups.TOLCarrierIDList => base.TOLCarrierIDList;
	CodeDescriptionPairList INctsDepartureMovementHeaderLookups.TransportAtBorderTypeOfIdList => base.TransportAtBorderTypeOfIdList;

	#region TankerStatusList
	public CodeDescriptionPairList TankerStatusList
	{
		get
		{
			return Factory.GetCachedValue("TankerStatusList", () =>
			{
				var tankerStatusList = new CodeDescriptionPairList();
				tankerStatusList.AddPair(TankerStatus.CodeNo, TankerStatus.Description.No.ToString());
				tankerStatusList.AddPair(TankerStatus.CodeYes, TankerStatus.Description.Yes.ToString());
				return tankerStatusList;
			});
		}
	}
	static class TankerStatus
	{
		public const string CodeNo = "0";
		public const string CodeYes = "1";

		public static class Description
		{
			public static readonly MultilingualString No = ResString.GetMultilingualString("7B80AF76-A7D5-4D06-A9B2-3F615EA275BC", "No");
			public static readonly MultilingualString Yes = ResString.GetMultilingualString("FDC2EF33-C571-4C22-B61E-58327CB136F5", "Yes");
		}
	}
	#endregion
}
