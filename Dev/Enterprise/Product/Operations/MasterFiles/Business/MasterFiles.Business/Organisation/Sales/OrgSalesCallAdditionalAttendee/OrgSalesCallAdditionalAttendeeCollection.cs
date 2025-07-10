
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgSalesCallAdditionalAttendeeCollection : DependentBusinessObjectCollection<OrgSalesCallAdditionalAttendee, OrgSalesCall>
	{
		protected OrgSalesCallAdditionalAttendeeCollection(OrgSalesCall parent) : base(parent)
		{
		}
	}
}
