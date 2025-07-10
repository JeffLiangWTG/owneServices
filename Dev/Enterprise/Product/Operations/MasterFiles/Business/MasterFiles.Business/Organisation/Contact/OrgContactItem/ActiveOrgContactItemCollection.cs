using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveOrgContactItemCollection : ActiveBusinessObjectCollection<OrgContactItem>
	{
		public ActiveOrgContactItemCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ActiveOrgContactItemCollection(OrgContact parent) : base(parent)
		{
		}
	}
}
