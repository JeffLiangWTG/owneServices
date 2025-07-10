using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class BillNumberWrapper : IAssociatedTransportDocument
	{
		public BillNumberWrapper(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, "Shipment cannot be null");
		}
		readonly ForwardingShipment shipment;

		public BillNumberWrapper(ZString billNo)
		{
			this.billNo = billNo;
		}
		readonly ZString billNo;

		public ZString BillNumber
		{
			get { return shipment != null ? shipment.JS_HouseBill : billNo; }
		}

		public ZString BillType
		{
			get { return BillTypeList.Codes.HWB; }
		}

		public IEnumerable<ZGuid> RelatedPackages
		{
			get { throw new System.NotImplementedException(); }
		}

		public IEnumerable<ZGuid> RelatedEquipment
		{
			get { throw new System.NotImplementedException(); }
		}

		public ZInt MessageSequence
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}

		public ZGuid PK
		{
			get { throw new System.NotImplementedException(); }
		}
	}
}
