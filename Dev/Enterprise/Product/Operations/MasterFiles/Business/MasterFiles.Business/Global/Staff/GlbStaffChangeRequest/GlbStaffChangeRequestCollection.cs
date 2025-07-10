using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffChangeRequestCollection : ActiveBusinessObjectCollection<GlbStaffChangeRequest>
	{
		public GlbStaffChangeRequestCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
