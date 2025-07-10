using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ManifestToOpenPackLookups : CusTRPreviousDocumentItemLookups
	{
		public ManifestToOpenPackLookups(ManifestToOpenPack parent) : base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection WarehouseCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, ZDateTime.Today);
	}
}
