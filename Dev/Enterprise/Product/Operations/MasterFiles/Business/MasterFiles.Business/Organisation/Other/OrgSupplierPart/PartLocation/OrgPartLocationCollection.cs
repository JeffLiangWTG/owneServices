using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartLocationCollection : DependentBusinessObjectCollection<OrgPartLocation, OrgSupplierPart>
	{
		public OrgPartLocationCollection(OrgSupplierPart parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}
	}
}
