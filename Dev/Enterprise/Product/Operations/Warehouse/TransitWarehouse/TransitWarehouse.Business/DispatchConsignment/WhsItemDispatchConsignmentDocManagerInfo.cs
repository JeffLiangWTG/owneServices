using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.Warehouse.Transit.Business
{
	internal class WhsItemDispatchConsignmentDocManagerInfo : DocManagerInfo
	{
		public WhsItemDispatchConsignmentDocManagerInfo(BusinessObject parent, string docManagerCode) : base(parent, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>();

			var dcn = (WhsItemDispatchConsignment)BusinessEntity;
			result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(dcn));

			return result.ToArray();
		}
	}
}
