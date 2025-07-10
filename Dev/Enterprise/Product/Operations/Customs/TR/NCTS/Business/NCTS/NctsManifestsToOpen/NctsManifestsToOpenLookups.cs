using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsManifestsToOpenLookups : CusSupportingInfoLookups
	{
		public NctsManifestsToOpenLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection TRWarehouseList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, ZDateTime.Today);
	}
}
