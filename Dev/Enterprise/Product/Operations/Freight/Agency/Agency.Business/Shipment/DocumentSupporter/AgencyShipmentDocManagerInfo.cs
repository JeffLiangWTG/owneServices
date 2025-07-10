using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.Freight.Agency.Business
{
	sealed class AgencyShipmentDocManagerInfo : DocManagerInfo
	{
		public AgencyShipmentDocManagerInfo(AgencyShipment shipment)
			: base(shipment, Constants.DocManagerCodes.AgencyShipment) { }

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new List<BusinessObject>();

			var shipment = (AgencyShipment)BusinessEntity;
			if (shipment != null)
			{
				list.AddRange(GetTransactions(shipment.JS_UniqueConsignRef));
				list.AddRange(shipment.RealContainers);
				list.AddRange(shipment.BookedContainers);
				list.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(shipment));
			}

			return list.ToArray();
		}

		AccTransactionHeaderCollection GetTransactions(ZString uniqueRef)
		{
			return new InvoiceLoader(BusinessEntity.Factory).GetInvoicesForUniqueRef(uniqueRef);
		}
	}
}


