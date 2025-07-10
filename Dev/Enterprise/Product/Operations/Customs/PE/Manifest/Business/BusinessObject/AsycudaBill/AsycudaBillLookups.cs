using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CargoNatureList => Factory.GetCachedValue<CargoNatureCodeList>();

		public CodeDescriptionPairList CargoConditionList => Factory.GetCachedValue<CargoConditionCodeList>();
	}
}
