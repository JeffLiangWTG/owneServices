using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Provides access to tracking related order details
	/// </summary>
	public class TrackingOrderLine : OrderLine
	{
		public TrackingOrderLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public abstract new class Schema : OrderLine.Schema
		{
			public const string OrderNumber = "OrderNumber";
			public const string OrderPK = "OrderPK";
			public const string ShipmentNumber = "ShipmentNumber";
			public const string ShipmentPK = "ShipmentPK";
			public const string HouseBillNumber = "HouseBillNumber";
			public const string SupplierName = "SupplierName";
			public const string ProductNumber = "ProductNumber";
			public const string CustomAttribute1 = "CustomAttribute1";
			public const string Description = "Description";
			public const string ContainerQuantity = "ContainerQuantity";
			public const string Packs = "Packs";
		}

		public override void Delete()
		{
			throw new NotSupportedException("You cannot delete this because it is never edited in Tracking");
		}

		public void SetContainerNumber(ZString containerNumber)
		{
			ContainerNumber = containerNumber;
		}

		ZString ContainerNumber;

		#region Order Number

		public ZString OrderNumber
		{
			get { return Order != null ? Order.JD_OrderNumber : ZString.Empty; }
		}

		public ZPropertyInfo OrderNumberInfo
		{
			get { return GetZPropertyInfo(Schema.OrderNumber); }
		}

		public ZGuid OrderPK
		{
			get { return Order != null ? Order.PK : ZGuid.Empty; }
		}

		#endregion

		#region Shipment Number

		public ZString ShipmentNumber
		{
			get { return Shipment != null ? Shipment.JS_UniqueConsignRef : ZString.Empty; }
		}

		public ZGuid ShipmentPK
		{
			get { return Shipment != null ? Shipment.PK : ZGuid.Empty; }
		}

		CommonShipment Shipment
		{
			get { return Order != null ? Order.Shipment : null; }
		}

		public ZPropertyInfo ShipmentNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ShipmentNumber); }
		}

		#endregion

		#region House Bill

		public ZString HouseBillNumber
		{
			get { return Shipment != null ? Shipment.JS_HouseBill : ZString.Empty; }
		}

		public ZPropertyInfo HouseBillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.HouseBillNumber); }
		}

		#endregion

		#region Supplier Name

		public ZString SupplierName
		{
			get { return Order != null && Order.Supplier != null ? Order.Supplier.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo SupplierNameInfo
		{
			get { return GetZPropertyInfo(Schema.SupplierName); }
		}

		#endregion

		#region Product Number

		public ZString ProductNumber
		{
			get { return Product != null ? Product.OP_PartNum : ZString.Empty; }
		}

		public ZPropertyInfo ProductNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ProductNumber); }
		}

		#endregion

		#region Custom Attribute 1

		public ZString CustomAttribute1
		{
			get { return JO_CustomAttrib1; }
		}

		public ZPropertyInfo CustomAttribute1Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttribute1); }
		}

		#endregion

		#region Description

		public ZString Description
		{
			get { return JO_Description; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region Container Quantity

		public ZDecimal ContainerQuantity
		{
			get
			{
				if (fContainerQuantity == 0)
				{
					if (JO_ContainerNumber == ContainerNumber)
					{
						fContainerQuantity = JO_QtyInvoiced;
					}
					else
					{
						foreach (OrderLineDelivery delivery in Deliveries)
						{
							foreach (OrderLineDeliverContainer container in delivery.Containers)
							{
								if (container.J5_ContainerNum == ContainerNumber)
								{
									fContainerQuantity += container.J5_QuantityInvoiced;
								}
							}
						}
					}
				}

				return fContainerQuantity;
			}
		}
		ZDecimal fContainerQuantity;

		public ZPropertyInfo ContainerQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerQuantity); }
		}

		#endregion

		#region Packs

		public ZInt Packs
		{
			get
			{
				if (fPacks == 0)
				{
					foreach (OrderLineDelivery delivery in Deliveries)
					{
						foreach (OrderLineDeliverContainer container in delivery.Containers)
						{
							if (container.J5_ContainerNum == ContainerNumber)
							{
								fPacks += container.J5_PackCount;
							}
						}
					}
				}

				return fPacks;
			}
		}
		ZInt fPacks;

		public ZPropertyInfo PacksInfo
		{
			get { return GetZPropertyInfo(Schema.Packs); }
		}

		#endregion
	}
}
