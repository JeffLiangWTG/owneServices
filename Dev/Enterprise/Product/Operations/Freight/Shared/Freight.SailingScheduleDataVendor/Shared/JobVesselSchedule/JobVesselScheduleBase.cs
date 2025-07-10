using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class JobVesselScheduleBase : AutoJobVesselSchedule
	{
		public JobVesselScheduleBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
