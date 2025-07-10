using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderDocManagerInfo : DocManagerInfo
	{
		public WhsOrderDocManagerInfo(WhsOrder order, ZString docManagerCode)
			: base(order, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>();

			var order = (WhsOrder)BusinessEntity;
			result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(order));
			var cartageJob = (BusinessObject)order.CartageJob;
			if (cartageJob != null)
			{
				result.Add(cartageJob);
			}

			result.AddRange(order.GetRelatedParents());

			return result.ToArray();
		}
	}
}
