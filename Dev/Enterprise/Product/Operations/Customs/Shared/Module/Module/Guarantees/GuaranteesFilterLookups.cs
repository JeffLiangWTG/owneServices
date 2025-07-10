using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class GuaranteesFilterLookups : CommonFilterLookups
	{
		public GuaranteesFilterLookups(FilterStripBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public virtual CodeDescriptionPairList GuaranteeTypeList => Header.Lookups.PermitTypes;

		BaseCusGuaranteeHeader Header => header ??= Factory.GetNull<BaseCusGuaranteeHeader>();
		BaseCusGuaranteeHeader header;
	}
}
