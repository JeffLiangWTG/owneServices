using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaPackLookups : ASYCUDA.Business.AsycudaPackLookups
	{
		public AsycudaPackLookups(AsycudaPack parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PackUQList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);
	}
}
