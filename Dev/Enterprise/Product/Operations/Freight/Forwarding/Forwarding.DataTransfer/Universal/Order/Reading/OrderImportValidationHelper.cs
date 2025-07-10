using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	static class OrderImportValidationHelper
	{
		internal static void ValidateOrder(IXmlImportLogger logger, Order order)
		{
			ValidateOrderWeightUnit(logger, order);
			ValidateOrderVolumeUnit(logger, order);
			ValidateOrderPackTypeUnit(logger, order);
			ValidateExWorksDate(logger, order);
			ValidateCurrencyUnit(logger, order);
			ValidateShipmentWindowEnd(logger, order);
		}

		static void ValidateOrderWeightUnit(IXmlImportLogger logger, Order order)
		{
			if (order.JD_ActualWeight > 0 && order.JD_UnitOfWeight.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("f6ba19a8-8af7-40b3-4d07-50dc392e4c18", "Order Weight Unit is required when Actual Weight is greater than zero."));
			}
		}

		static void ValidateOrderVolumeUnit(IXmlImportLogger logger, Order order)
		{
			if (order.JD_ActualVolume > 0 && order.JD_UnitOfVolume.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("70b370d4-2a22-1c90-463b-89461f4c4e79", "Order Volume Unit is required when Actual Volume is greater than zero."));
			}
		}

		static void ValidateOrderPackTypeUnit(IXmlImportLogger logger, Order order)
		{
			if (order.JD_Packs > 0 && order.JD_F3_NKPackType.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("6aa9d99c-e056-0791-43a6-8582bc0048e2", "Order Pack Type is required when Packs is greater than zero."));
			}
		}

		static void ValidateExWorksDate(IXmlImportLogger logger, Order order)
		{
			if (order.JD_ExWorksRequiredBy > order.JD_DeliveryRequiredBy)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("0534fdb3-33e0-8d99-4262-a5ff1bfe31de", "Order Ex Works Date should be before the Required In Store Date."));
			}
		}

		static void ValidateCurrencyUnit(IXmlImportLogger logger, Order order)
		{
			if (order.JD_RX_NKOrderCurrency.IsEmpty && (order?.OrderLines?.Any(x => x.JO_ItemPrice > 0) ?? false))
			{
				logger.LogBoth(LogType.Warning, Res.GetString("daf6a432-3f63-cbaa-412a-159a5dd3e5ab", "Order Currency is required when an attached Order Line has an Item Price set."));
			}
		}

		static void ValidateShipmentWindowEnd(IXmlImportLogger logger, Order order)
		{
			if (!order.JD_ShipmentWindowStart.IsEmpty && !order.JD_ShipmentWindowEnd.IsEmpty & order.JD_ShipmentWindowStart > order.JD_ShipmentWindowEnd)
			{
				logger.LogBoth(LogType.Error, Res.GetString("b988b454-51d3-47b1-b0e5-6c7b900bccff", "Order Ship Window Start date by must be earlier than or equal to Ship Window End date."));
			}
		}

		internal static void ValidateOrderLine(IXmlImportLogger logger, OrderLine orderLine)
		{
			ValidateOrderLineWeightUnit(logger, orderLine);
			ValidateOrderLineVolumeUnit(logger, orderLine);
			ValidateOrderLineInnerPackTypeUnit(logger, orderLine);
			ValidateOrderLineOuterPackTypeUnit(logger, orderLine);
			ValidateOrderLineUnitOfDimension(logger, orderLine);
			ValidateOrderLineQuantityUnit(logger, orderLine);
			ValidateOrderLineQuantityReceived(logger, orderLine);
			ValidateOrderLineShipmentWindowEnd(logger, orderLine);
			ValidateOrderLineReference(logger, orderLine);
		}

		static void ValidateOrderLineWeightUnit(IXmlImportLogger logger, OrderLine orderLine)
		{
			if (orderLine.JO_ActualWeight > 0 && orderLine.JO_UnitOfWeight.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("67d89be3-b080-11a6-44ae-a263927bd5ce", "Order Line Weight Unit is required when Actual Weight is greater than zero."));
			}
		}

		static void ValidateOrderLineVolumeUnit(IXmlImportLogger logger, OrderLine orderLine)
		{
			if (orderLine.JO_ActualVolume > 0 && orderLine.JO_UnitOfVolume.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("6fd3d149-1459-c2a9-4dcc-f4afc0cb0566", "Order Line Volume Unit is required when Actual Volume is greater than zero."));
			}
		}

		static void ValidateOrderLineInnerPackTypeUnit(IXmlImportLogger logger, OrderLine orderLine)
		{
			if (orderLine.JO_InnerPacks > 0 && orderLine.JO_InnerPacksUQ.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("634b7b7e-c9e6-56a0-4582-3740877d0438", "Order Line Inner Package Type is required when Inner Packs are greater than zero."));
			}
		}

		static void ValidateOrderLineOuterPackTypeUnit(IXmlImportLogger logger, OrderLine orderLine)
		{
			if (orderLine.JO_OuterPacks > 0 && orderLine.JO_OuterPacksUQ.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("ce7ac928-5b9d-f18d-4198-e5d38e5c3c5e", "Order Line Outer Package Type is required when Outer Packs are greater than zero."));
			}
		}

		static void ValidateOrderLineUnitOfDimension(IXmlImportLogger logger, OrderLine orderLine)
		{
			if (orderLine.JO_OuterPackUnitOfDimension.IsEmpty
				&& (orderLine.JO_OuterPackLength > 0
				|| orderLine.JO_OuterPackWidth > 0
				|| orderLine.JO_OuterPackHeight > 0))
			{
				logger.LogBoth(LogType.Warning, Res.GetString("0b26e598-0428-20b5-410a-5db3031be39b", "Order Line Unit of Dimension is required when Width, Length or Height are greater than zero."));
			}
		}

		static void ValidateOrderLineQuantityUnit(IXmlImportLogger logger, OrderLine orderLine)
		{
			if (orderLine.JO_F3_NKPackType.IsEmpty
				&& (orderLine.JO_Quantity > 0
				|| orderLine.JO_QtyInvoiced > 0
				|| orderLine.JO_QtyReceived > 0))
			{
				logger.LogBoth(
					LogType.Warning,
					Res.GetString(
						"da8d4907-e08b-0390-4ca4-6010ad03e3d1",
						"Order Line Quantity Unit is required when Quantity Ordered, Quantity Invoiced, or Quantity Received are greater than zero."));
			}
		}

		static void ValidateOrderLineQuantityReceived(IXmlImportLogger logger, OrderLine orderLine)
		{
			if (orderLine.JO_QtyReceived > orderLine.JO_Quantity)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("9b9f67b5-df4d-e1b5-4d18-db081e142020", "Order Line Quantity Received is greater than Quantity Ordered."));
			}
		}

		static void ValidateOrderLineShipmentWindowEnd(IXmlImportLogger logger, OrderLine orderLine)
		{
			if (!orderLine.JO_ShipmentWindowStart.IsEmpty && !orderLine.JO_ShipmentWindowEnd.IsEmpty & orderLine.JO_ShipmentWindowStart > orderLine.JO_ShipmentWindowEnd)
			{
				logger.LogBoth(LogType.Error, Res.GetString("8c4198d7-e021-4084-ac3e-3dced77f61c9", "Order Line Ship Window Start date by must be earlier than or equal to Ship Window End date."));
			}
		}

		static void ValidateOrderLineReference(IXmlImportLogger logger, OrderLine orderLine)
		{
			var enableOrderLineReferenceMatching = OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.Value;

			var regex = enableOrderLineReferenceMatching
				? regexValidationPatternWhenEnableOrderLineReferenceMatching
				: regexValidationPatternWhenDisableOrderLineReferenceMatching;

			if (!string.IsNullOrEmpty(orderLine.JO_LineReference) && !regex.IsMatch(orderLine.JO_LineReference))
			{
				if (enableOrderLineReferenceMatching)
				{
					logger.LogBoth(LogType.Error, Res.GetString("1923b28d-2cf4-4da3-a126-d87acfbbb9a1",
						"Must be upper-case and alphanumeric: Space( ), Back-tick(`) and Comma(,) are not allowed."));
				}
				else
				{
					logger.LogBoth(LogType.Error, Res.GetString("1923b28d-2cf4-4da3-a126-d87acfbbb9a2",
						"Must be upper-case and alphanumeric: Space( ), Back-tick(`), Tilde(~) and Comma(,) are not allowed."));
				}
			}
		}

		static readonly Regex regexValidationPatternWhenEnableOrderLineReferenceMatching = new Regex(@"^[0-9A-Z@(){}_#\-\!\%\^&\*\""'<>=;:\+\.\/\$\?\|\\[\]~]+$");

		static readonly Regex regexValidationPatternWhenDisableOrderLineReferenceMatching = new Regex(@"^[0-9A-Z@(){}_#\-\!\%\^&\*\""'<>=;:\+\.\/\$\?\|\\[\]]+$");
	}
}
