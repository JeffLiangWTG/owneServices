using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingBookingOrderLink : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string OrderPK = "OrderPK";
			public const string OrderNumber = "OrderNumber";
			public const string OrderDate = "OrderDate";
			public const string OrderGoodsDescription = "OrderGoodsDescription";
		}

		#endregion

		#region Constructors

		public TrackingBookingOrderLink(TrackingBooking booking, ZGuid orderPK)
			: base(booking.Factory)
		{
			this.booking = booking;
			this.orderPK = orderPK;
			Validate();
		}

		#endregion

		#region Properties

		#region OrderPK

		public ZGuid OrderPK
		{
			get
			{
				return orderPK;
			}
			set
			{
				if (orderPK != value)
				{
					if (Booking != null)
					{
						if (LinkedOrder != null)
						{
							if (Booking.AttachedOrders.Contains(LinkedOrder))
							{
								Booking.AttachedOrders.RemoveFromRelationship(LinkedOrder);
							}
						}
					}
				}
				orderPK = value;
				ValidateOrderPKInfo();
				OrderPKInfo.RefreshBinding();
			}
		}
		ZGuid orderPK;

		public ZPropertyInfo OrderPKInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.OrderPK);
				result.HumanReadableName = (LinkedOrder == null ? new ZString(Res.GetString("5d8754e9-de71-4d5a-9a27-02e81572b8ed", "Order")) : LinkedOrder.HumanReadableName);
				return result;
			}
		}

		void ValidateOrderPKInfo()
		{
			OrderPKInfo.ClearAllNotifications();
			if (Booking != null)
			{
				IAttachOrders orderParent = Booking.Booking;
				StringCollectionX errorsList = new StringCollectionX();
				StringCollectionX warningsList = new StringCollectionX();
				if (LinkedOrder != null)
				{
					if (LinkedOrder.JD_IsCancelled)
					{
						errorsList.Add(Res.GetString("294909bf-e4e2-4c52-b01e-5713b8e3c4b1", "This Order cannot be chosen here as it is inactive."));
					}
					else if (LinkedOrder.JD_OrderStatus == Constants.OrderStatus.Cancelled)
					{
						errorsList.Add(Res.GetString("53358bc9-6c0a-42b0-9a72-46b49631cb68", "This Order cannot be chosen here as it has been canceled."));
					}
					else if (!LinkedOrder.JD_JS.IsEmpty && LinkedOrder.JD_JS != orderParent.PK)
					{
						errorsList.Add(Res.GetString("034fc7d7-d8b6-40ba-8cab-33fd1ccd133e", "This Order cannot be chosen here as it is already attached to a Shipment."));
					}
					else if (!LinkedOrder.JD_TransportMode.ToUpper().Equals(orderParent.TransportMode.ToUpper()))
					{
						errorsList.Add(Res.GetString("408d12fb-6c57-49cd-a5c0-b9cac606de3a", "This Order cannot be chosen here as the Transport Mode is different to that of the Shipment."));
					}
					else if (!LinkedOrder.JD_ContainerMode.ToUpper().Equals(orderParent.ContainerMode.ToUpper()))
					{
						warningsList.Add(Res.GetString("e424c613-f8c2-4b66-a0d3-e4669f167d02", "The Container Mode of this Order is different to that of the Shipment."));
					}
					if (errorsList.Count == 0)
					{
						if (!Booking.AvailableOrders.Contains(LinkedOrder))
						{
							if (LinkedOrder.Supplier == null && LinkedOrder.BuyerPK != Booking.ConsigneeOrganisationPK && WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.Value)
							{
								errorsList.Add(Res.GetString("3600d4e7-ff7c-45c7-96f0-6f85c036260a", "This Order cannot be chosen here as the Buyer does not match the Consignee"));
							}
							else if (LinkedOrder.JD_JS != orderParent.PK)
							{
								errorsList.Add(Res.GetString("92345496-48d0-4b7e-8f87-addf65b126a5", "This Order is not available for this Booking."));
							}
						}
					}
					ZString errors = errorsList.ToString().Trim();
					if (!errors.IsEmpty)
					{
						OrderPKInfo.AddError(errors);
					}
					ZString warnings = warningsList.ToString().Trim();
					if (!warnings.IsEmpty)
					{
						OrderPKInfo.AddWarning(warnings);
					}
					if (!OrderPKInfo.HasErrors() && orderParent != null)
					{
						if (!orderParent.AttachedOrders.Contains(LinkedOrder))
						{
							orderParent.AttachedOrders.Add(LinkedOrder);
						}
					}
				}
			}
		}

		#endregion

		#region OrderNumber

		public ZString OrderNumber
		{
			get
			{
				return (LinkedOrder == null ? ZString.Empty : LinkedOrder.JD_OrderNumber);
			}
		}

		public ZPropertyInfo OrderNumberInfo
		{
			get { return GetZPropertyInfo(Schema.OrderNumber); }
		}

		#endregion

		#region OrderDate

		public ZDateTime OrderDate
		{
			get
			{
				return (LinkedOrder == null ? ZDateTime.Empty : LinkedOrder.JD_OrderDate);
			}
		}

		public ZPropertyInfo OrderDateInfo
		{
			get { return GetZPropertyInfo(Schema.OrderDate); }
		}

		#endregion

		#region OrderGoodsDescription

		public ZString OrderGoodsDescription
		{
			get
			{
				return (LinkedOrder == null ? ZString.Empty : LinkedOrder.JD_OrderGoodsDescription);
			}
		}

		public ZPropertyInfo OrderGoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.OrderGoodsDescription); }
		}

		#endregion

		public Order LinkedOrder
		{
			get
			{
				return Factory.Load<Order>(OrderPK);
			}
		}

		public OrderCollection AvailableOrders
		{
			get
			{
				return Booking.AvailableOrders;
			}
		}

		public TrackingBooking Booking
		{
			get
			{
				return booking;
			}
		}

		#endregion

		#region Methods

		public void Validate()
		{
			ValidateOrderPKInfo();
		}

		#endregion

		#region Overrides

		public override bool HasChanges
		{
			get
			{
				return (LinkedOrder != null);
			}
			set
			{
				base.HasChanges = value;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrderPKInfo();
		}

		#endregion

		#region Implementation

		readonly TrackingBooking booking;

		#endregion
	}
}
