using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ContainerLoadListLineDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestBasicFieldMappingsForCY()
		{
			var containerLoadList = ContainerLoadListDataObjectHelper.BuildDataForTest(Factory, SupplierBookingLoadModeList.Codes.CY);
			Factory.SaveForTesting();

			var manager = new ContainerLoadListDataContextManager();
			var writer = (manager as IShipmentDataContextManager).GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerLoadList)));
			var dataObject = writer.GetDataObject(containerLoadList) as UniversalShipment;
			AssertEquals(1, dataObject.SubShipmentCollection.Count);
			AssertEquals(1, dataObject.SubShipmentCollection[0].PackingLineCollection.Count);

			var containerLoadListData = dataObject.SubShipmentCollection[0];
			var containerLoadListLine = containerLoadList.LoadListLines[0];
			AssertContainerLoadListLine(containerLoadListData, containerLoadListLine, true);
		}

		static void AssertContainerLoadListLine(UniversalShipment containerLoadListData, ContainerLoadListLine containerLoadListLine, bool hasAttachedContainer)
		{
			var packingLine = containerLoadListData.PackingLineCollection[0];
			AssertEquals(1, containerLoadListData.PackingLineCollection.Count);

			AssertEquals((ZDecimal)67.0m, containerLoadListData.TotalNoOfPacksDecimal);
			AssertEquals("PLT", containerLoadListData.TotalNoOfPacksPackageType.Code);
			AssertEquals("Pallet", containerLoadListData.TotalNoOfPacksPackageType.Description);
			AssertEquals(45, Convert.ToInt32(packingLine.PackQty.Value));
			AssertEquals(PkgUnit.Piece, packingLine.PackType.Code);
			AssertEquals((ZDecimal)12.3m, packingLine.Volume);
			AssertEquals(Volume.CubicMetres, packingLine.VolumeUnit.Code);
			AssertEquals((ZDecimal)34.5m, packingLine.Weight);
			AssertEquals(Weight.Kilograms, packingLine.WeightUnit.Code);
			AssertEquals("RNXX87", packingLine.ReferenceNumber);
			AssertEquals("HC0001", packingLine.HarmonisedCode);
			AssertEquals("GEN", packingLine.Commodity.Code);
			AssertEquals("JSL001", packingLine.PackingLineID);
			AssertEquals("Desc", packingLine.GoodsDescription);
			AssertEquals("Line Marks&Nos", packingLine.MarksAndNos);

			if (hasAttachedContainer)
			{
				AssertEquals(1, packingLine.ContainerLink);

				AssertNotNull(containerLoadListData.AddInfoCollection);
				AssertEquals(1, containerLoadListData.AddInfoCollection.Count);
				AssertEquals("S0001001", containerLoadListData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == "ShipmentID").Value);
			}
			else
			{
				AssertNull(packingLine.ContainerLink);

				AssertNotNull(containerLoadListData.AddInfoCollection);
				AssertEquals(1, containerLoadListData.AddInfoCollection.Count);
			}

			var supplierBookingData = containerLoadListData.SubShipmentCollection[0];
			AssertEquals(containerLoadListLine.SupplierBookingLine.SupplierBooking.JSB_BookingId, supplierBookingData.GetMatchingDataSource(DataContextType.JobSupplierBooking).Key);

			AssertEquals(containerLoadListLine.SupplierBookingLine.OrderLine.Order.JD_OrderNumber, supplierBookingData.SubShipmentCollection[0].Order.OrderNumber);
			AssertEquals(containerLoadListLine.SupplierBookingLine.OrderLine.Order.JD_OrderNumberSplit, supplierBookingData.SubShipmentCollection[0].Order.OrderNumberSplit);
			AssertEquals(containerLoadListLine.SupplierBookingLine.OrderLine.JO_LineNo, supplierBookingData.SubShipmentCollection[0].Order.OrderLineCollection[0].LineNumber);
			AssertEquals(containerLoadListLine.SupplierBookingLine.OrderLine.JO_SubLineNo, supplierBookingData.SubShipmentCollection[0].Order.OrderLineCollection[0].SubLineNumber);

			AssertEquals(containerLoadListLine.SupplierBookingLine.OrderLine.JO_F3_NKPackType, containerLoadListData.TotalNoOfPacksPackageType.Code);
			AssertEquals("Pallet", containerLoadListData.TotalNoOfPacksPackageType.Description);

			var product = supplierBookingData.SubShipmentCollection[0].Order.OrderLineCollection[0].Product;
			AssertNotNull(product);
			AssertEquals(containerLoadListLine.SupplierBookingLine.OrderLine.JO_Partno, product.Code);
			AssertEquals(containerLoadListLine.SupplierBookingLine.OrderLine.JO_Description, product.Description);

			var buyerAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressType(supplierBookingData.SubShipmentCollection[0], DocAddressType.BuyerDocumentaryAddress);
			AssertNotNull(buyerAddress);
			AssertEquals("Buyer", buyerAddress.OrganizationCode);
		}
	}
}
