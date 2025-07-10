using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentDocManagerInfo : DocManagerInfo
	{
		public HVLVConsignmentDocManagerInfo(HVLVConsignment consignment)
			: base(consignment, Constants.DocManagerCodes.HVLVConsignment)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>(base.GetRelatedObjects());
			var consignment = (HVLVConsignment)BusinessEntity;

			result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(consignment));
			return result.ToArray();
		}
	}
}
