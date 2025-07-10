using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusRulingFilterLookups : CommonFilterLookups
	{
		public ZZRefCusRulingFilterLookups(ZZRefCusRulingFilterStripBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		protected new ZZRefCusRulingFilterStripBusinessObject FilterBizObj => (ZZRefCusRulingFilterStripBusinessObject)base.FilterBizObj;

		public virtual CodeDescriptionPairList RulingTypeList => Factory.GetCachedValue<RefCusRulingTypeList>();

		public virtual OrgAddressCollection AppliesToAddressList
		{
			get { return fAppliesToAddressList ?? (fAppliesToAddressList = new OrgAddressCollection(Factory)); }
		}

		OrgAddressCollection fAppliesToAddressList;

		public virtual OrgHeaderCollection AppliesToOrgList => appliesToOrgList ?? (appliesToOrgList = new OrgHeaderCollection(Factory));
		OrgHeaderCollection appliesToOrgList;
	}
}
