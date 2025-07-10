using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Orders
{
	public class JobSupplierBookingLineConverter
	{
		public void ConvertDispatchedLooseCargoBookingLineToPackLine(ForwardingShipment shipment, JobSupplierBookingLine bookingLine)
		{
			if (IsLooseCargoDispatched(bookingLine.SupplierBooking) && bookingLine.JSL_DispatchedQuantity > 0m)
			{
				var packLine = shipment.OuterPackLines.AddNew();
				var orderLine = bookingLine.OrderLine;

				packLine.JL_JSL_BookingLine = bookingLine.PK;

				packLine.JL_Length = bookingLine.JSL_PackLength;
				packLine.JL_Width = bookingLine.JSL_PackWidth;
				packLine.JL_Height = bookingLine.JSL_PackHeight;
				packLine.JL_UnitOfDimension = bookingLine.JSL_PackUnitOfDimension;

				packLine.JL_PackageCount = bookingLine.JSL_DispatchedPackages;
				packLine.JL_ActualVolumeUQ = bookingLine.JSL_VolumeUnit;
				packLine.JL_ActualVolume = bookingLine.JSL_DispatchedVolume;
				packLine.JL_ActualWeight = bookingLine.JSL_DispatchedWeight;
				packLine.JL_ActualWeightUQ = bookingLine.JSL_GrossWeightUnit;
				packLine.JL_F3_NKPackType = bookingLine.JSL_F3_NKBookedPackagesUnit;
				packLine.JL_RH_NKCommodityCode = bookingLine.JSL_RH_NKCommodityCode;
				packLine.JL_MarksAndNumbers = bookingLine.JSL_MarksAndNumbers;
				packLine.JL_HarmonisedCode = bookingLine.JSL_HarmonisedCode;
				packLine.JL_Description = bookingLine.JSL_Description;

				packLine.JL_RN_NKOrigin = orderLine.JO_RN_NKCountryOfOrigin;
				packLine.JL_DetailedDescription = orderLine.JO_AdditionalInformation;

				packLine.JL_LinePrice = packLine.Shipment.CalculatePackLinePrice(orderLine.Order?.OrderCurrency, orderLine.JO_ItemPrice, bookingLine.JSL_DispatchedQuantity);

				var packProduct = packLine.Products.AddNew();
				packProduct.D2_JO = orderLine.PK;
				packProduct.D2_ProductQuantity = bookingLine.JSL_DispatchedQuantity;
				packProduct.D2_ProductUnitOfQty = orderLine.JO_F3_NKPackType;
			}
		}

		static bool IsLooseCargoDispatched(JobSupplierBooking booking)
		{
			return booking.JSB_LoadMode == SupplierBookingLoadMode.LooseCargo && SupplierBookingStatus.Shipped == booking.JSB_Status;
		}
	}
}
