using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class StaffViewStmNumsLookups : ViewStmNumsLookups
	{
		public StaffViewStmNumsLookups(AutoViewStmNums parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList TypeList => Factory.GetCachedValue<StaffStmNumsTypeList>();
	}
}
