using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ImportedOrderLineDelivery : ImportedObject
	{
		public ImportedOrderLineDelivery(ImportedOrderLine orderLine, OrderLineDelivery orderLineDelivery)
		{
			this.orderLine = orderLine;
			Populate(orderLineDelivery);
		}

		public ImportedOrder Order
		{
			get { return OrderLine.Order; }
		}

		public ImportedOrderLine OrderLine
		{
			get { return orderLine; }
		}
		readonly ImportedOrderLine orderLine;

		#region Populate

		void Populate(OrderLineDelivery orderLineDelivery)
		{
			IsEmpty = orderLineDelivery.IsNull;
			J4_RL_NKDestinationPort = new ImportedProperty(orderLineDelivery.J4_RL_NKDestinationPortInfo, Res.GetString("e9c4e294-f501-4aae-b5ff-1fecfcdeb552", "Dest. Port"));
			J4_OA_NKDeliveryPoint = new ImportedProperty(orderLineDelivery.J4_OA_NKDeliveryPointInfo, Res.GetString("886eff7c-2d9e-4275-83cd-bdb88e45ad6e", "Deliver Point"));
			J4_Allocated = new ImportedProperty(orderLineDelivery.J4_AllocatedInfo, Res.GetString("343b3a56-6e68-4442-88e7-d7102e68794f", "Qty Delivered"));
			OrderNumberAndSplitAndLineNoAndSplitAndSubLine = Order.JD_OrderNumberAndSplit + ":" + OrderLine.JO_LineNoAndSplitAndSubLine;
		}

		#endregion

		#region Properties

		public ZBool IsEmpty { get; private set; }
		public ZString OrderNumberAndSplitAndLineNoAndSplitAndSubLine { get; private set; }

		#endregion

		#region AmendedProperties

		public ImportedProperty J4_RL_NKDestinationPort { get; private set; }
		public ImportedProperty J4_OA_NKDeliveryPoint { get; private set; }
		public ImportedProperty J4_Allocated { get; private set; }

		#endregion

		#region GetAllImportedProperties / AmendedPropertiesToShowAlways

		public override ImportedProperty[] GetAllImportedProperties()
		{
			return new ImportedProperty[]
			{
				J4_RL_NKDestinationPort,
				J4_OA_NKDeliveryPoint,
				J4_Allocated,
			};
		}

		public override List<string> PropertiesToShowAlways
		{
			get
			{
				if (propertiesToShowAlways == null)
				{
					propertiesToShowAlways = new List<string>();
					if (!IsEmpty)
					{
						propertiesToShowAlways.Add(J4_RL_NKDestinationPort.Name);
						propertiesToShowAlways.Add(J4_OA_NKDeliveryPoint.Name);
						propertiesToShowAlways.Add(J4_Allocated.Name);
					}
				}
				return propertiesToShowAlways;
			}
		}
		List<string> propertiesToShowAlways;

		public override List<string> OtherPropertiesToShow
		{
			get { return new List<string>(); }
		}

		#endregion
	}
}

