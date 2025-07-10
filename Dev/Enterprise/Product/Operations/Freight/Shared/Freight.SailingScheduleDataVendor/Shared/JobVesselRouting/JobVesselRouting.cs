using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.SailingDataVendor.Business;

namespace Enterprise.Freight.SailingDataVendor.Shared
{
	public class JobVesselRouting : AutoJobVesselRouting
	{
		public JobVesselRouting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		public virtual JobVesselSchedule VesselSchedule
		{
			get { return Factory.Load<JobVesselSchedule>(E1_EV); }
		}

		#endregion
	}
}
