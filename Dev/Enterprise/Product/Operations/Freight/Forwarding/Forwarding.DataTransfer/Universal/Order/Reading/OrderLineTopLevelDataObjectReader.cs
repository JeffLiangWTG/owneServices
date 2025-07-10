using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderLineTopLevelDataObjectReader : ShipmentDataObjectReader<OrderLine>
	{
		public OrderLineTopLevelDataObjectReader(UniversalShipment orderLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(orderLineDataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.OrderManagerOrderLine;

		protected override IMatchingBusinessEntityFinder<OrderLine> GetCombinedReferenceMatcher()
		{
			return null;
		}

		bool EnableOrderLineReferenceMatching => OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.Value;

		protected override OrderLine GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var orderReferences = GetMatchingReferences();
			var matcher = new OrderMatcher(factory.BOFactory, orderReferences, logger);
			var bestMatchOrder = matcher.GetBestMatch();

			ZInt? orderLineNumber = null;
			ZInt? orderLineSubLineNumber = null;
			ZString? orderLineLineReference = null;

			if (dataObject.Order?.OrderLineCollection != null
				&& dataObject.Order.OrderLineCollection.Count == 1)
			{
				var orderLineData = dataObject.Order.OrderLineCollection[0];
				orderLineNumber = orderLineData.LineNumber;
				orderLineSubLineNumber = orderLineData.SubLineNumber;
				orderLineLineReference = orderLineData.LineReference;
			}

			if (bestMatchOrder != null)
			{
				if (EnableOrderLineReferenceMatching && !string.IsNullOrWhiteSpace(orderLineLineReference))
				{
					var orderLine = bestMatchOrder.OrderLines.FirstOrDefault(l => l.JO_LineReference == orderLineLineReference.Value);
					return orderLine;
				}

				if (orderLineNumber != null && orderLineSubLineNumber != null)
				{
					var orderLine = bestMatchOrder.OrderLines
						.FirstOrDefault(l => l.JO_LineNo == orderLineNumber
							&& l.JO_SubLineNo == orderLineSubLineNumber);
					return orderLine;
				}
			}

			return null;
		}

		OrderReferences GetMatchingReferences()
		{
			var orderReferences = new OrderReferences();

			orderReferences.OrderNumber = dataObject.Order != null ? dataObject.Order.OrderNumber.GetValueOrDefault() : ZString.Empty;
			orderReferences.OrderNumberSplit = dataObject.Order != null ? dataObject.Order.OrderNumberSplit : null;

			var masterWayBill = dataObject.AdditionalBillCollection != null
				&& dataObject.AdditionalBillCollection.Count == 1
					? dataObject.AdditionalBillCollection[0].BillNumber.GetValueOrDefault()
					: ZString.Empty;

			var wayBillNumber = dataObject.WayBillNumber.GetValueOrDefault();
			if (dataObject.TransportMode.GetCodeAsUpperCase() == Core.Constants.TransportModes.Air)
			{
				orderReferences.MAWBNumber = masterWayBill;
				orderReferences.HAWBNumber = wayBillNumber;
			}
			else
			{
				orderReferences.MBOLNumber = masterWayBill;
				orderReferences.HBOLNumber = wayBillNumber;
			}

			orderReferences.InvoiceNumber = dataObject.CommercialInfo != null
				&& dataObject.CommercialInfo.CommercialInvoiceCollection != null
				&& dataObject.CommercialInfo.CommercialInvoiceCollection.Count == 1
					? dataObject.CommercialInfo.CommercialInvoiceCollection[0].InvoiceNumber.GetValueOrDefault()
					: ZString.Empty;

			orderReferences.ShippersReference = dataObject.BookingConfirmationReference.GetValueOrDefault();
			orderReferences.OriginUNLOCO = dataObject.PortOfOrigin.GetUNLOCOAsUpperCase(factory.BOFactory);
			orderReferences.DestinationUNLOCO = dataObject.PortOfDestination.GetUNLOCOAsUpperCase(factory.BOFactory);

			return orderReferences;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(OrderLine orderLineBO)
		{
			if (orderLineBO == null)
			{
				return Res.GetString("5184dc8f-6d52-4200-b33e-de86cfb55771", "Cannot find a matched Order Line.");
			}

			if (OrderLineDataObjectReadingHelper.GetHelper(dataObject, orderLineBO, DataContextType, logger, factory)?
				.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(orderLineBO, false) is ZString reason && !reason.IsEmpty)
			{
				return reason;
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(orderLineBO);
		}

		protected override void PopulateBusinessObject(OrderLine targetBO)
		{
			OrderLineDataObjectReadingHelper.GetHelper(dataObject, targetBO, DataContextType, logger, factory)?.PopulateBusinessObject(targetBO, false);
		}
	}
}
