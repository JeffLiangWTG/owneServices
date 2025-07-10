using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.eTail.Business
{
	public class HVLVBookingHeaderDocManagerInfo : DocManagerInfo
	{
		public HVLVBookingHeaderDocManagerInfo(HVLVBookingHeader bookingHeader)
			: base(bookingHeader, Constants.DocManagerCodes.HVLVBookingHeader)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>(base.GetRelatedObjects());
			var bookingHeader = (HVLVBookingHeader)BusinessEntity;
			result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(bookingHeader));
			return result.ToArray();
		}
	}
}
