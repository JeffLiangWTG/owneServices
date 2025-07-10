using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveDocManagerInfo : DocManagerInfo
	{
		public WhsReceiveDocManagerInfo(WhsReceive receive)
			: base(receive, "WID")
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new List<BusinessObject>(base.GetRelatedObjects());

			var receive = (WhsReceive)BusinessEntity;
			if (receive != null)
			{
				list.AddRange(receive.GetRelatedParents());
				list.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(receive));
			}

			return list.ToArray();
		}
	}
}
