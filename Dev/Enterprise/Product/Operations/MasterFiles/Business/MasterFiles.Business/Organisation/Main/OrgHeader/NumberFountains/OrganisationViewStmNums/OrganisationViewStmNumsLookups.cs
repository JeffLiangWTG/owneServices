using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationViewStmNumsLookups : ViewStmNumsLookups
	{
		public OrganisationViewStmNumsLookups(OrganisationViewStmNums parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList TypeList => Factory.GetCachedValue<OrgStmNumsTypeList>();
	}
}
