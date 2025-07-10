using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using LinkedOrder = Enterprise.Freight.Forwarding.Orders.Business.Order;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderNumberDataObjectWriter : DataObjectWriter<OrderItem, OrderNumber>
	{
		public OrderNumberDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override OrderNumber PopulateDataObject(OrderItem orderItemBO)
		{
			return new OrderNumber()
			{
				Sequence = orderItemBO.JT_Sequence,
				OrderReference = orderItemBO.JT_OrderReference,
			};
		}
	}

	class LinkedOrder_OrderNumberDataObjectWriter : DataObjectWriter<LinkedOrder, OrderNumber>
	{
		internal LinkedOrder_OrderNumberDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override OrderNumber PopulateDataObject(LinkedOrder order)
		{
			return new OrderNumber()
			{
				Sequence = NextSequenceNo,
				OrderReference = order.JD_OrderNumberAndSplit,
			};
		}

		ZShort NextSequenceNo
		{
			get { return SequenceNo++; }
		}

		public ZShort SequenceNo { get; set; }
	}
}
