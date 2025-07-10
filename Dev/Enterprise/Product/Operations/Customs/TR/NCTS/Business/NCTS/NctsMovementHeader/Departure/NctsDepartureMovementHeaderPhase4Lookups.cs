using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase4Lookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Lookups, INctsDepartureMovementHeaderLookups
	{
		public NctsDepartureMovementHeaderPhase4Lookups(EU.NCTS.Business.NctsDepartureMovementHeader parent) : base(parent)
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

		CodeDescriptionPairList INctsDepartureMovementHeaderLookups.TankerStatusList => new CodeDescriptionPairList();
	}
}
