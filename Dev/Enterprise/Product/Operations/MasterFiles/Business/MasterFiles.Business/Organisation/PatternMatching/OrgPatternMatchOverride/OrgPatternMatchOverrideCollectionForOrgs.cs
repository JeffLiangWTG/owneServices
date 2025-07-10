using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPatternMatchOverrideCollectionForOrgs : ActiveBusinessObjectCollection<OrgPatternMatchOverride>
	{
		public OrgPatternMatchOverrideCollectionForOrgs(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
