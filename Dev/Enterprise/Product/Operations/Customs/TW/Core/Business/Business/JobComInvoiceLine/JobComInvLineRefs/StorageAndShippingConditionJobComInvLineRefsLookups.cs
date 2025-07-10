using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class StorageAndShippingConditionJobComInvLineRefsLookups : Customs.Business.JobComInvLineRefsLookups
	{
		public StorageAndShippingConditionJobComInvLineRefsLookups(StorageAndShippingConditionJobComInvLineRefs parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList ReferenceTypeList => Factory.GetCachedValue<CPT_122_StorageShippingConditionList>();
	}
}
