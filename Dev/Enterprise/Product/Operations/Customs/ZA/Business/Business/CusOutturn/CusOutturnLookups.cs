using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CusOutturnLookups : Customs.Business.CusOutturnLookups
	{
		public CusOutturnLookups(CusOutturn parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PackConditionList => Factory.GetCachedValue<PackConditionList>();

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public CodeDescriptionPairList VolumeUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);

		public CodeDescriptionPairList CargoTypeList => Factory.GetCachedValue<CargoTypeList>();

		public CodeDescriptionPairList ExcessShortIndicatorList => Factory.GetCachedValue<ExcessShortIndicatorList>();

		public new CusOutturn Parent => (CusOutturn)base.Parent;
	}
}
