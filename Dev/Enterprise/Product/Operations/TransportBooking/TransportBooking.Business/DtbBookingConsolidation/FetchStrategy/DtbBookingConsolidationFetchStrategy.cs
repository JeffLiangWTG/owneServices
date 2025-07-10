using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbBookingConsolidationFetchStrategy(DtbBookingConsolidation transportBooking)
			: base(transportBooking)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			if (columns.Any(c => c.ColumnName.Contains((NoResString)"Address")))
			{
				Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
			}
		}
	}
}
