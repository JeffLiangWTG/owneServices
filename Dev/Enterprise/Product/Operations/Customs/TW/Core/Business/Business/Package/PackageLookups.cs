using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class PackageLookups : CusDecHouseContainerPackLookups
	{
		public PackageLookups(Package parent) : base(parent)
		{
		}

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public CodeDescriptionPairList VolumeUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);

		public CodeDescriptionPairList DimensionUQList => Factory.GetCachedValue<DimensionUQList>();
	}
}
