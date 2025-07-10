using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class ManifestToOpenLookups : CusSupportingInfoLookups
	{
		public ManifestToOpenLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}
		public ZZRefCusCodeListCombinedCollection TRWarehouseList
		{
			get
			{
				var list = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, TRWarehouseCodeType, ZDateTime.Today);
				return list;
			}
		}
		const string TRWarehouseCodeType = "TRCWH";
		public override CodeDescriptionPairList ProcedureList => new ProcedureList();

		public override CodeDescriptionPairList SubTypeList => new SubTypeListForManifestToOpen();
	}
}
