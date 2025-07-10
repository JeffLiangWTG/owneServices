using CargoWise.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Orders
{
	class ContainerLoadListLineConverter
	{
		public void ConvertLoadListLineToPackLine(ForwardingShipment shipment, ContainerLoadListLine loadListLine)
		{
			Argument.NotNull(shipment, nameof(shipment));
			Argument.NotNull(loadListLine, nameof(loadListLine));
			Argument.NotNull(loadListLine.SupplierBookingLine, nameof(loadListLine.SupplierBookingLine));

			var supplierBookingLine = loadListLine.SupplierBookingLine;

			var packLine = shipment.OuterPackLines.AddNew();
			loadListLine.CLL_JL_PackLine = packLine.PK;

			packLine.JL_Length = supplierBookingLine.JSL_PackLength;
			packLine.JL_Width = supplierBookingLine.JSL_PackWidth;
			packLine.JL_Height = supplierBookingLine.JSL_PackHeight;
			packLine.JL_UnitOfDimension = supplierBookingLine.JSL_PackUnitOfDimension;

			packLine.SetContainer(loadListLine.Container.Consol, loadListLine.Container);
			packLine.JL_PackageCount = loadListLine.CLL_Packages;
			packLine.JL_ActualVolumeUQ = loadListLine.CLL_VolumeUnit;
			packLine.JL_ActualVolume = loadListLine.CLL_Volume;
			packLine.JL_ActualWeight = loadListLine.CLL_Weight;
			packLine.JL_ActualWeightUQ = loadListLine.CLL_WeightUnit;
			packLine.JL_F3_NKPackType = loadListLine.CLL_F3_NKPackagesUnit;
			packLine.JL_RH_NKCommodityCode = loadListLine.CLL_RH_NKCommodityCode;
			packLine.JL_HarmonisedCode = loadListLine.CLL_HarmonizedCode;
			packLine.JL_RefNumber = loadListLine.CLL_ReferenceNumber;
			packLine.JL_MarksAndNumbers = loadListLine.CLL_MarksAndNumbers;
			packLine.JL_Description = loadListLine.CLL_Description;

			var orderLine = supplierBookingLine?.OrderLine;
			if (orderLine != null)
			{
				packLine.JL_RN_NKOrigin = orderLine.JO_RN_NKCountryOfOrigin;
				packLine.JL_DetailedDescription = orderLine.JO_AdditionalInformation;
				new OrderLineConverter().FillPackLine(packLine, orderLine, loadListLine.CLL_PackedQuantity);
			}
		}
	}
}
