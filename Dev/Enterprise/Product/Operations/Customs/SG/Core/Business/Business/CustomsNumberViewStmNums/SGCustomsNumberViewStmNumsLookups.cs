using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCustomsNumberViewStmNumsLookups : CustomsNumberViewStmNumsLookups
	{
		public SGCustomsNumberViewStmNumsLookups(CustomsNumberViewStmNums parent)
			: base(parent)
		{ }

		public override CodeDescriptionPairList TypeList => Factory.GetCachedValue<NumberRangeTypeList>();
	}
}
