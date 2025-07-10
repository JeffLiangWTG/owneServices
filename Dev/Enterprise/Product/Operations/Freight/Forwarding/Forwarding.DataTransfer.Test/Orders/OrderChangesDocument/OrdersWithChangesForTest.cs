using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrdersWithChangesForTest
	{
		public OrdersWithChangesForTest(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public Order NewOrder
		{
			get
			{
				if (newOrder == null)
				{
					newOrder = Factory.New<Order>();
					newOrder.JD_OrderNumber = "new";
					newOrder.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				}
				return newOrder;
			}
		}
		Order newOrder;

		public Order AmendedOrder
		{
			get
			{
				if (amendedOrder == null)
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					Order savedOrder = newFactory.New<Order>();
					savedOrder.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
					savedOrder.JD_OrderNumber = "modified";
					OrderLine amendedOrderLine = savedOrder.OrderLines.AddNew();
					amendedOrderLine.JO_CustomAttrib1 = "modified";

					newFactory.Save();
					amendedOrder = Factory.Load<Order>(savedOrder.PK);
					amendedOrder.JD_CustomAttrib1 = "modified";
				}
				return amendedOrder;
			}
		}
		Order amendedOrder;

		public OrderLine AmendedOrderLine
		{
			get { return AmendedOrder.OrderLines[0]; }
		}

		public Order UnchangedOrder
		{
			get
			{
				if (unchangedOrder == null)
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					Order savedOrder = newFactory.New<Order>();
					savedOrder.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
					savedOrder.JD_OrderNumber = "unchanged";

					newFactory.Save();
					unchangedOrder = Factory.Load<Order>(savedOrder.PK);
				}
				return unchangedOrder;
			}
		}
		Order unchangedOrder;

		public Order CancelledOrder
		{
			get
			{
				if (cancelledOrder == null)
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					Order savedOrder = newFactory.New<Order>();
					savedOrder.JD_OrderNumber = "cancelled";
					savedOrder.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

					newFactory.Save();
					cancelledOrder = Factory.Load<Order>(savedOrder.PK);
					cancelledOrder.JD_OrderStatus = Core.Constants.OrderStatus.Cancelled;
				}
				return cancelledOrder;
			}
		}
		Order cancelledOrder;

		public Order AttachedOrder
		{
			get
			{
				if (attachedOrder == null)
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();

					ForwardingShipment shipment = newFactory.NewWithValidTestData<ForwardingShipment>();

					Order savedOrder = newFactory.New<Order>();
					savedOrder.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
					savedOrder.JD_OrderNumber = "attached";
					savedOrder.JD_JS = shipment.PK;

					OrderLine attachedOrderLine = savedOrder.OrderLines.AddNew();
					attachedOrderLine.JO_CustomAttrib1 = "attached";

					newFactory.Save();

					attachedOrder = Factory.Load<Order>(savedOrder.PK);
				}

				return attachedOrder;
			}
		}
		Order attachedOrder;

		readonly BusinessObjectFactory Factory;
	}
}
