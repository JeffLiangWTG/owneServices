using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaBillLookups : ManifestBase.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent) : base(parent)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public ZZRefCarrierCombinedCollection BillIssuers =>
			ZZRefCarrierCombinedCollection.GetCachedCollection(Parent.Factory,
			Core.Constants.CountryCodes.SouthAfrica,
			Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER,
			Parent.Header?.AMA_TransportMode ?? ZString.Empty);

		public CodeDescriptionPairList CustomsStatusList => GetCachedRefCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus);

		CodeDescriptionPairList GetCachedRefCusCodeList(ZString codeType)
		{
			return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, codeType);
		}
	}
}
