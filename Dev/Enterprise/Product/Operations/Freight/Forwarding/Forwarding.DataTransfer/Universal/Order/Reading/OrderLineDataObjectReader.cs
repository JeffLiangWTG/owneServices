using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderLineDataObjectReader : DataObjectReader<UniversalOrderLine, OrderLine>
	{
		public OrderLineDataObjectReader(UniversalOrderLine orderLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Order parent)
			: base(orderLineDataObject, logger, factory)
		{
			Parent = Argument.NotNull(parent, "Order parent");
			helper = new OrderLineDataObjectReadingHelper(dataObject, logger, factory, Parent, UseLineReferenceMatching);
		}

		readonly Order Parent;
		readonly OrderLineDataObjectReadingHelper helper;

		#region Match Existing

		bool UseLineReferenceMatching => OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.Value && dataObject.LineReference is ZString reference && reference != string.Empty;

		protected override OrderLine GetExistingBusinessObject() => Parent.OrderLines.OfType<OrderLine>().FirstOrDefault(orderLine =>
		{
			if (UseLineReferenceMatching)
			{
				return orderLine.JO_LineReference == dataObject.LineReference.Value;
			}
			else
			{
				var lineNo = dataObject.LineNumber.GetValueOrDefault();
				var subLineNo = dataObject.SubLineNumber ?? (ZInt)1;
				return orderLine.JO_LineNo == lineNo && orderLine.JO_SubLineNo == subLineNo;
			}
		});

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(OrderLine orderLineBO)
		{
			if (helper.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(orderLineBO, IsNewBO) is ZString reason && !reason.IsEmpty)
			{
				return reason;
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(orderLineBO);
		}

		public OrderLine TryGetExistingBusinessObject()
		{
			return GetExistingBusinessObject();
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(OrderLine orderLine)
		{
			helper.PopulateBusinessObject(orderLine, IsNewBO);
		}

		#endregion

	}
}
