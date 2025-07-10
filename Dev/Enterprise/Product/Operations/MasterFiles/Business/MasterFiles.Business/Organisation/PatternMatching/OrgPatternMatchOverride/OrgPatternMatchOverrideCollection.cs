using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPatternMatchOverrideCollection : DependentBusinessObjectCollection<OrgPatternMatchOverride, OrgHeader>
	{
		public OrgPatternMatchOverrideCollection(OrgHeader parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}
	}
}
