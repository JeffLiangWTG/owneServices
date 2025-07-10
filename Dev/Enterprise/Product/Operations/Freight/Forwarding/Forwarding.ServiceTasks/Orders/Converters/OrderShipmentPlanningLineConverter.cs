using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Orders
{
	internal class OrderShipmentPlanningLineConverter
	{
		public void ConvertToPackLine(OrderShipmentPlanningLine planningLine, ForwardingShipment shipment)
		{
			if (planningLine.OPL_MarkForDelete)
			{
				planningLine.PackLine?.Delete();
				return;
			}

			if (planningLine.OPL_Quantity == 0m)
			{
				planningLine.PackLine?.Delete();
				return;
			}

			if (planningLine.OPL_JL_PackLine.IsEmpty)
			{
				using (shipment.OuterPackLines.TemporarilyDisableAutomaticPackingIntoContainer())
				{
					PopulateBookingLineToPackLine(planningLine, shipment.OuterPackLines.AddNew());
				}
			}

			PopulatePlanningInfoToPackLine(planningLine, planningLine.PackLine);
		}

		static void PopulateBookingLineToPackLine(OrderShipmentPlanningLine planningLine, ForwardingPackLine packLine)
		{
			var bookingLine = planningLine.SupplierBookingLine;
			var orderLine = bookingLine.OrderLine;

			planningLine.OPL_JL_PackLine = packLine.PK;

			packLine.JL_JSL_BookingLine = planningLine.OPL_JSL_BookingLine;
			packLine.JL_RN_NKOrigin = planningLine.OrderShipmentPlanning.Origin?.Country?.Code ?? ZString.Empty;

			packLine.JL_RH_NKCommodityCode = bookingLine.JSL_RH_NKCommodityCode;
			packLine.JL_MarksAndNumbers = bookingLine.JSL_MarksAndNumbers;
			packLine.JL_HarmonisedCode = bookingLine.JSL_HarmonisedCode;

			packLine.JL_DetailedDescription = orderLine.JO_AdditionalInformation;
			packLine.JL_Description = bookingLine.JSL_Description;
		}

		static void PopulatePlanningInfoToPackLine(OrderShipmentPlanningLine planningLine, ForwardingPackLine packLine)
		{
			var bookingLine = planningLine.SupplierBookingLine;
			var orderLine = bookingLine.OrderLine;

			packLine.JL_Length = bookingLine.JSL_PackLength;
			packLine.JL_Width = bookingLine.JSL_PackWidth;
			packLine.JL_Height = bookingLine.JSL_PackHeight;
			packLine.JL_UnitOfDimension = bookingLine.JSL_PackUnitOfDimension;

			packLine.JL_PackageCount = planningLine.OPL_Packages.ToZInt();
			packLine.JL_ActualVolumeUQ = planningLine.OPL_VolumeUnit;
			packLine.JL_ActualWeightUQ = planningLine.OPL_WeightUnit;
			packLine.JL_ActualVolume = planningLine.OPL_Volume;
			packLine.JL_ActualWeight = planningLine.OPL_Weight;
			packLine.JL_F3_NKPackType = planningLine.OPL_F3_NKPackagesUnit;

			if (packLine.Products.Count == 0)
			{
				var packProduct = packLine.Products.AddNew();
				packProduct.D2_JO = planningLine.SupplierBookingLine.OrderLine.PK;
				packProduct.D2_ProductQuantity = planningLine.OPL_Quantity;
				packProduct.D2_ProductUnitOfQty = planningLine?.SupplierBookingLine?.OrderLine?.JO_F3_NKPackType ?? ZString.Empty;
			}
			else
			{
				var matchedProduct = packLine.Products.FirstOrDefault(product => product.D2_JO == planningLine.SupplierBookingLine.OrderLine.PK);
				if (matchedProduct != null)
				{
					matchedProduct.D2_ProductQuantity = planningLine.OPL_Quantity;
					matchedProduct.D2_ProductUnitOfQty = planningLine?.SupplierBookingLine?.OrderLine?.JO_F3_NKPackType ?? ZString.Empty;
				}
			}

			packLine.JL_LinePrice = packLine.Shipment.CalculatePackLinePrice(orderLine.Order?.OrderCurrency, orderLine.JO_ItemPrice, planningLine.OPL_Quantity);
		}
	}
}
