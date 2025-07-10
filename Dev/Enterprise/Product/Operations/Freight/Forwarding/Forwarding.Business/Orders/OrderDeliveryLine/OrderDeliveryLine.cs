using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderDeliveryLine : NonPersistentBusinessObject
	{
		public OrderDeliveryLine(JobShipmentPreplanning preAdvice)
			: base(preAdvice.Factory)
		{
			this.PreAdvice = preAdvice;
		}

		#region Properties

		#region Order Number

		[ReadOnly(true)]
		[List("Lookups.OrderNumbers")]
		[MaxLength(AutoJobOrderHeader.Schema.JD_OrderNumberMaxLength)]
		public ZString OrderNumber
		{
			get { return fOrderNumber; }
			set
			{
				if (fOrderNumber != value)
				{
					CheckMaximumLength(OrderNumberInfo, value);
					fOrderNumber = value;
					fOrder = null;
					fLine = null;
					Validation.ValidateOrderNumber();
					Validation.ValidateOrderNumberSplit();
					OrderNumberInfo.RefreshBinding();
					OrderNumberAndSplitInfo.RefreshBinding();
				}
			}
		}

		ZString fOrderNumber;

		public ZPropertyInfo OrderNumberInfo
		{
			get { return GetZPropertyInfo(nameof(OrderNumber)); }
		}

		#endregion

		#region Order Number Split

		public ZByte OrderNumberSplit
		{
			get { return orderNumberSplit; }
			set
			{
				if (orderNumberSplit != value)
				{
					orderNumberSplit = value;
					Validation.ValidateOrderNumberSplit();
					Validation.ValidateOrderNumber();
					OrderNumberSplitInfo.RefreshBinding();
					OrderNumberAndSplitInfo.RefreshBinding();
				}
			}
		}
		ZByte orderNumberSplit = 0;

		public ZPropertyInfo OrderNumberSplitInfo
		{
			get { return GetZPropertyInfo(nameof(OrderNumberSplit)); }
		}

		#endregion

		#region OrderNumberAndSplit

		public ZString OrderNumberAndSplit
		{
			get { return (OrderNumberSplit == 0) ? OrderNumber : (ZString)(OrderNumber + "-" + OrderNumberSplit); }
		}

		public ZPropertyInfo OrderNumberAndSplitInfo
		{
			get { return GetZPropertyInfo(nameof(OrderNumberAndSplit)); }
		}

		#endregion

		#region Order Line Developer

		[ReadOnly(true)]
		[List("Lookups.OrderLineNumbers")]
		[MaxLength(30)]
		public ZString OrderLineNumber
		{
			get { return fOrderLineNumber; }
			set
			{
				if (fOrderLineNumber != value)
				{
					CheckMaximumLength(OrderLineNumberInfo, value);
					fOrderLineNumber = value;
					fOrder = null;
					fLine = null;
					Validation.ValidateOrderLineNumber();
					OrderLineNumberInfo.RefreshBinding();
				}
			}
		}

		ZString fOrderLineNumber;

		public ZPropertyInfo OrderLineNumberInfo
		{
			get { return GetZPropertyInfo(nameof(OrderLineNumber)); }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public readonly JobShipmentPreplanning PreAdvice;

		#region Order

		public Order Order
		{
			get
			{
				if (fOrder == null)
				{
					ZQuery query = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber);
					query.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, OrderNumberSplit);
					query.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, PreAdvice.EF_OA_BuyerAddress);
					foreach (Order element in PreAdvice.Orders.Find(query))
					{
						fOrder = element;
						break;
					}
				}

				return fOrder;
			}
		}

		Order fOrder;

		#endregion

		#region Line

		public OrderLine Line
		{
			get
			{
				if (fLine == null && int.TryParse(OrderLineNumber, out int orderLineNum) && Order != null)
				{
					ZQuery query = new ZQuery(JobOrderLineSchema.JO_LineNo, orderLineNum);
					foreach (OrderLine element in Order.OrderLines.Find(query))
					{
						fLine = element;
						break;
					}
				}

				return fLine;
			}
		}

		OrderLine fLine;

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public OrderDeliveryLineValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual OrderDeliveryLineValidation GetNewValidation()
		{
			return new OrderDeliveryLineValidation(this);
		}

		#endregion

		#region Lookups

		public OrderDeliveryLineLookups Lookups
		{
			get { return new OrderDeliveryLineLookups(this); }
		}

		#endregion

		public void ReceiveAllIfNonReceived()
		{
			if (Line != null)
			{
				Line.ReceiveAllIfNonReceived();
			}
		}
	}
}
