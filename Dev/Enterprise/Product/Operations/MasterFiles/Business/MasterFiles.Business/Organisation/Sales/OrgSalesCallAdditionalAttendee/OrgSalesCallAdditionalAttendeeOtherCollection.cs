using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallAdditionalAttendeeOtherCollection : OrgSalesCallAdditionalAttendeeCollection
	{
		public OrgSalesCallAdditionalAttendeeOtherCollection(OrgSalesCall parent) : base(parent)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return new ZQuery(OrgSalesCallAdditionalAttendeeSchema.O6_AttendeeTableCode, ZString.Empty);
		}
	}
}
