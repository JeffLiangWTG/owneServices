using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgMatchApprovalCollection : BusinessObjectCollection<OrgMatchApproval>
	{
		public OrgMatchApprovalCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
