using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using UniversalCommodity = Enterprise.UniversalDataBuss.DataObjects.Universal.Commodity;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalOrder = Enterprise.UniversalDataBuss.DataObjects.Universal.Order;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;
using UniversalOrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalPackageType = Enterprise.UniversalDataBuss.DataObjects.Universal.PackageType;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalUnitOfVolume = Enterprise.UniversalDataBuss.DataObjects.Universal.UnitOfVolume;
using UniversalUnitOfWeight = Enterprise.UniversalDataBuss.DataObjects.Universal.UnitOfWeight;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ContainerLoadListLineDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestBasicFieldMappings()
		{
			var supplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			var container = ContainerLoadListDataObjectHelper.BuildContainerForSupplierBooking(Factory, supplierBooking);
			var containerLinkManager = new ContainerLoadListContainerLinkManager();
			containerLinkManager.CollectContainerLink(container, new UniversalContainer { Link = 2 });
			var containerLoadList = Factory.BOFactory.NewWithValidTestData<CYContainerLoadList>();
			containerLoadList.CLH_JSB_Booking = supplierBooking.PK;
			var originalContainerLoadListLine = containerLoadList.LoadListLines.AddNew();
			originalContainerLoadListLine.CLL_JSL_BookingLine = supplierBooking.SupplierBookingLines[0].PK;
			originalContainerLoadListLine.CLL_JC_Container = container.PK;
			containerLoadList.CLH_LoadMode = Core.Constants.ContainerLoadListHeaderLoadMode.ContainerYard;
			Factory.SaveForTesting();

			var resultContainerLoadListLine = CommonCheck(containerLoadList, containerLinkManager, supplierBooking);
			AssertEquals(originalContainerLoadListLine.PK, resultContainerLoadListLine.PK);

			CheckContainerLoadLineResult(supplierBooking, container, resultContainerLoadListLine);
		}

		ContainerLoadListLine CommonCheck(CommonContainerLoadList containerLoadList, ContainerLoadListContainerLinkManager containerLinkManager, JobSupplierBooking supplierBooking, bool isEmptyContainerLink = false)
		{
			var order = Factory.BOFactory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ORD0002";
			order.JD_OrderNumberSplit = 1;
			var buyer = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "Buyer2";
			buyer.Addresses.AddNew().FillWithValidTestData();
			buyer.Contacts.AddNew().FillWithValidTestData();
			order.JD_OA_BuyerAddress = buyer.Addresses[0].PK;
			order.JD_OC_BuyerContact = buyer.Contacts[0].PK;
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 2;

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKXXX";

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var uShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			uShipment.DataContext = DataContextFactory.New();

			AssertError("There is no or more than one Packing Line.", uShipment, containerLoadList, containerLinkManager);
			uShipment.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { new UniversalPackingLine { } });
			FillPackLineWithTestData(uShipment, uShipment.PackingLineCollection[0]);

			AssertError("There is no supplier booking key.", uShipment, containerLoadList, containerLinkManager);
			var supplierBookingData = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			uShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { supplierBookingData });
			supplierBookingData.DataContext = DataContextFactory.New();
			supplierBookingData.DataContext.AddDataSource(DataContextType.JobSupplierBooking, "SBK_XXXX");

			AssertError("The supplier booking SBK_XXXX does not exist.", uShipment, containerLoadList, containerLinkManager);
			uShipment.SubShipmentCollection[0].DataContext.DataSourceCollection.First().Key = supplierBooking.JSB_BookingId;
			AssertError("There is no or more than one order in SBK001.", uShipment, containerLoadList, containerLinkManager);

			var orderData = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			supplierBookingData.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { orderData });
			AssertError("There is no order in SBK001.", uShipment, containerLoadList, containerLinkManager);

			orderData.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance);
			orderData.Order.OrderNumber = "ORDXXX";
			orderData.Order.OrderNumberSplit = 1;
			AssertError("There is no or more than one order line in SBK001.", uShipment, containerLoadList, containerLinkManager);

			orderData.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderData.Order.OrderLineCollection.Add(new UniversalOrderLine());
			orderData.Order.OrderLineCollection.Add(new UniversalOrderLine());
			AssertError("There is no or more than one order line in SBK001.", uShipment, containerLoadList, containerLinkManager);

			orderData.Order.OrderLineCollection.Clear();
			orderData.Order.OrderLineCollection.Add(new UniversalOrderLine());
			orderData.Order.OrderLineCollection[0].LineNumber = 1;
			orderData.Order.OrderLineCollection[0].SubLineNumber = 2;
			AssertError("The order line ORDXXX-1 [1-2] does not exist.", uShipment, containerLoadList, containerLinkManager);

			orderData.Order.OrderNumber = "ORD0002";
			orderData.Order.OrderNumberSplit = 1;
			orderData.Order.OrderLineCollection[0].LineNumber = 1;
			orderData.Order.OrderLineCollection[0].SubLineNumber = 2;
			orderData.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());
			orderData.OrganizationAddressCollection.Add(new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ConsigneeDocumentaryAddress)).GetDataObject(order.BuyerAddress));
			AssertError("The supplier booking line related to order line ORD0002-1 [1-2] does not exist on supplier booking SBK001.", uShipment, containerLoadList, containerLinkManager);

			uShipment.PackingLineCollection[0].PackingLineID = "JSL_XXXX";
			AssertError("The supplier booking line JSL_XXXX does not exist.", uShipment, containerLoadList, containerLinkManager);

			uShipment.PackingLineCollection[0].PackingLineID = "";
			var bookingline = supplierBooking.SupplierBookingLines[0];
			bookingline.JSL_JO_OrderLine = orderLine.PK;
			AssertError("Should provide container link for booking line JSL001.", uShipment, containerLoadList, containerLinkManager);

			uShipment.PackingLineCollection[0].PackingLineID = supplierBooking.SupplierBookingLines[0].JSL_BookingLineId;
			AssertError("Should provide container link for booking line JSL001.", uShipment, containerLoadList, containerLinkManager);

			uShipment.PackingLineCollection[0].ContainerLink = 1;
			AssertError("Could not find container by link 1 for booking line JSL001.", uShipment, containerLoadList, containerLinkManager);

			var container = containerLinkManager.GetContainer(2);
			container.JC_ContainerNum = "";
			uShipment.PackingLineCollection[0].ContainerLink = 2;
			AssertError("Allocated Container (2) must have container number.", uShipment, containerLoadList, containerLinkManager);

			container.JC_ContainerNum = "OOVQ3027364";
			return AssertError("", uShipment, containerLoadList, containerLinkManager);
		}

		static void CheckContainerLoadLineResult(JobSupplierBooking supplierBooking, ForwardingContainer container, ContainerLoadListLine resultContainerLoadListLine)
		{
			AssertEquals(1.2m, resultContainerLoadListLine.CLL_PackedQuantity);
			AssertEquals(4, resultContainerLoadListLine.CLL_Packages);
			AssertEquals(5.6m, resultContainerLoadListLine.CLL_Volume);
			AssertEquals(6.7m, resultContainerLoadListLine.CLL_Weight); 

			AssertEquals(3, resultContainerLoadListLine.CLL_LoadSequence);

			AssertEquals("M3", resultContainerLoadListLine.CLL_VolumeUnit);
			AssertEquals("T", resultContainerLoadListLine.CLL_WeightUnit);
			AssertEquals("HC0001", resultContainerLoadListLine.CLL_HarmonizedCode);
			AssertEquals("FGFG", resultContainerLoadListLine.CLL_RH_NKCommodityCode);
			AssertEquals("RN001", resultContainerLoadListLine.CLL_ReferenceNumber);
			AssertEquals(supplierBooking.SupplierBookingLines[0].PK, resultContainerLoadListLine.CLL_JSL_BookingLine);
			AssertEquals(container?.PK ?? ZGuid.Empty, resultContainerLoadListLine.CLL_JC_Container);
		}

		static void FillPackLineWithTestData(UniversalShipment shipmentData, UniversalPackingLine packingLineData)
		{
			shipmentData.PackingLineCollection.Clear();
			shipmentData.PackingLineCollection.Add(packingLineData);
			shipmentData.TotalNoOfPacksDecimal = 1.2m;
			packingLineData.ContainerPackingOrder = 3;
			packingLineData.PackQty = 4;
			packingLineData.PackType = new UniversalPackageType { Code = "PKG" };
			packingLineData.Volume = 5.6;
			packingLineData.VolumeUnit = new UniversalUnitOfVolume
			{
				Code = "M3",
				Description = "Cubic Meters"
			};
			packingLineData.Weight = 6.7;
			packingLineData.WeightUnit = new UniversalUnitOfWeight
			{
				Code = "T",
				Description = "Tonnes"
			};
			packingLineData.HarmonisedCode = "HC0001";
			packingLineData.Commodity = new UniversalCommodity
			{
				Code = "FGFG",
				Description = "Fudge Guts Fingers Gone"
			};
			packingLineData.ReferenceNumber = "RN001";
		}

		ContainerLoadListLine TryReadIntoBusinessObject(UniversalShipment uShipment, CommonContainerLoadList containerLoadList, ContainerLoadListContainerLinkManager containerLinkManager)
		{
			return new ContainerLoadListLineDataObjectReader(uShipment, containerLoadList, Logger, Factory, containerLinkManager).ReadIntoBusinessObject();
		}

		ContainerLoadListLine AssertError(string message, UniversalShipment uShipment, CommonContainerLoadList containerLoadList, ContainerLoadListContainerLinkManager containerLinkManager)
		{
			Logger.ClearLogs();
			var result = TryReadIntoBusinessObject(uShipment, containerLoadList, containerLinkManager);
			if (string.IsNullOrEmpty(message))
			{
				AssertNullOrEmpty(Logger.GetWarnings());
			}
			else
			{
				AssertContains(message, Logger.GetWarnings());
			}

			return result;
		}
	}
}
