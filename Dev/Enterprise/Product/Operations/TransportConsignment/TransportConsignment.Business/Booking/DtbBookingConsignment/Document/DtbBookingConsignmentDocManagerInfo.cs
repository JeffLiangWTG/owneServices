using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentDocManagerInfo : DtbTransportDocManagerInfo<DtbBookingConsignment>
	{
		public DtbBookingConsignmentDocManagerInfo(DtbBookingConsignment consignment)
			: base(consignment, Constants.DocManagerCodes.DomesticTransportConsignment)
		{
		}

		#region GetRelatedObjects

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>(base.GetRelatedObjects());

			var booking = Consignment.ConsolidationSingleJob != null ? Consignment.ConsolidationSingleJob.Parent : null;
			if (booking != null)
			{
				result.Add(booking);
			}
			return result.ToArray();
		}

		#endregion

		#region Booking

		DtbBookingConsignment Consignment
		{
			get { return Transport; }
		}

		#endregion
	}
}
