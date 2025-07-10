using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class AllocateWeightLookups : ZLookups
	{
		public AllocateWeightLookups(AllocateWeight parent) : base(parent)
		{
		}

		public new AllocateWeight Parent => (AllocateWeight)base.Parent;

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public CodeDescriptionPairList AllocateWeightMethodList => Factory.GetCachedValue<AllocateWeightMethodList>();
	}
}
