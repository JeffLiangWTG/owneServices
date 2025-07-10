using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(TrackingOrderValueObjectDataAdapter))]
	sealed class TrackingOrderValueObjectDataAdapterTest : ValueObjectDataAdapterTest<TrackingOrder, Xsd.WebOrder>
	{
		protected override ValueObjectDataAdapter<TrackingOrder, Xsd.WebOrder> GetNewBizObjXmlDataAdapter() => new TrackingOrderValueObjectDataAdapter();

		protected override string ExpectedRootCollectionElementName => "WebOrders";

		protected override string ExpectedRootElementName => "WebOrder";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyOrder = Factory.NewWithValidTestData<TrackingOrder>(TestBusinessObjectKind.NoData);
			emptyOrder.JD_OrderNumber = "test";
			var emptyOrderXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyOrder.xml", "EmptyOrder.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyOrder, emptyOrderXmlPath, ValidationKind.None, "Empty Order");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedOrderShipment = GetFullyPopulatedOrder();
			var fullOrderShipmentXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullOrderShipment.xml", "FullOrderShipment.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedOrderShipment, fullOrderShipmentXmlPath, ValidationKind.Xsd, "Populated Order shipment");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			var populatedOrderDeclaration = CreatePopulatedOrderWithDeclaration();
			var fullOrderDeclarationXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullOrderDeclaration.xml", "FullOrderDeclaration.xml");
			return new BusinessObjectAndExpectedOutputFileName[] { new BusinessObjectAndExpectedOutputFileName(populatedOrderDeclaration, fullOrderDeclarationXmlPath, ValidationKind.Xsd, "Populated Order with declaration") };
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
					{
						"Events",
						"OrderDetail",
						"Shipment",
						"OrderLines",
						"Notes",
						"DocumentLinks",
						"CustomValues"
					};
			}
		}

		protected override bool IsImportFromValueObjectSupported => false;

		protected override bool IsExportToCollectionSupported => false;

		protected override void SetUp()
		{
			base.SetUp();
			GlbDepartment.CurrentDepartment.GE_Sea = true;
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		TrackingOrder GetFullyPopulatedOrder()
		{
			var order = Factory.NewWithValidTestData<TrackingOrder>(TestBusinessObjectKind.NoData);

			var buyer = GetFullyPopulatedBuyer();
			var supplier = GetFullyPopulatedSupplier();

			var shipment = Factory.NewWithValidTestData<TrackingShipment>(TestBusinessObjectKind.NoData);
			shipment.JS_UniqueConsignRef = "S1234567890";
			PopulateShipmentDetails(shipment);
			order.JD_JS = shipment.PK;
			order.JD_OrderNumber = "9999999999";
			order.SupplierPK = supplier.PK;
			order.BuyerPK = buyer.PK;

			PopulateOrderDetails(order);
			PopulateMilestones(order);

			PopulateShipmentPlanning(order);

			PopulateOrderLine(order.OrderLines.AddNew());

			return order;
		}

		void PopulateShipmentDetails(TrackingShipment shipment)
		{
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_HouseBill = "Waybill";
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = shipment.JS_RL_NKOrigin;
			transport.JW_RL_NKDiscPort = shipment.JS_RL_NKDestination;
			transport.JW_VoyageFlight = "1";
			var vessel = LoadOrCreateTestVessel();

			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_ATD = new ZDateTime(1998, 04, 01);
			transport.JW_ETD = new ZDateTime(1998, 04, 01);
			transport.JW_ATA = new ZDateTime(1998, 05, 01);
			transport.JW_ETA = new ZDateTime(1998, 05, 01);

			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(1998, 02, 01);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2004, 02, 01);
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2003, 08, 01);

			shipment.Logs.AddNew(AutoEvents.CustomsCommenced, new ZDateTimeOffset(2003, 06, 01));
			shipment.Logs.AddNew(AutoEvents.CustomsCleared, new ZDateTimeOffset(2003, 07, 01));
		}

		RefVessel LoadOrCreateTestVessel()
		{
			var vessel = Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, "THE FLYING DUTCHMAN");
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_Code = "THE FLYING DUTCHMAN";
			}
			return vessel;
		}

		TrackingOrder CreatePopulatedOrderWithDeclaration()
		{
			var order = Factory.NewWithValidTestData<TrackingOrder>();

			order.BuyerPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			order.Buyer.OH_Code = "BUYER";
			order.Buyer.OH_FullName = "Buyer";
			order.Buyer.MainAddress.OA_Address1 = "Buyer";

			order.SupplierPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			order.Supplier.OH_Code = "SUPSUP";
			order.Supplier.OH_FullName = "Supplier";
			order.Supplier.MainAddress.OA_Address1 = "Supplier";

			order.JD_OrderNumber = "OrderNumber";
			order.JD_OrderNumberSplit = 2;
			order.JD_BookingConfRef = "BookingConfRef";
			order.JD_BookingConfDate = new ZDateTime(2006, 12, 25);
			order.JD_InvoiceNumber = "InvoiceNumber";
			order.JD_InvoiceDate = new ZDateTime(2011, 07, 13);
			order.JD_OrderStatus = Constants.OrderStatus.Confirmed;
			order.JD_ExWorksRequiredBy = new ZDateTime(2005, 1, 1);
			order.JD_DeliveryRequiredBy = new ZDateTime(2005, 2, 2);
			order.JD_OrderGoodsDescription = "OrderGoodsDescription";
			order.JD_OrderDate = new ZDateTime(2005, 3, 3);
			order.JD_RX_NKOrderCurrency = Constants.CurrencyCodes.China;
			order.JD_EstimatedExchangeRate = 1.2m;
			order.JD_IncoTerm = Constants.IncoTerms.CostAndFreight;
			order.JD_ContainerMode = Constants.ContainerModes.FCL;

			order.UpdateEventEstimate(AutoEvents.Arrival, new ZDateTimeOffset(2005, 1, 1));
			order.UpdateEvent(AutoEvents.Arrival, new ZDateTimeOffset(2005, 2, 1));
			order.UpdateEventEstimate(AutoEvents.DeliveryCartageAdvised, new ZDateTimeOffset(2005, 1, 2));
			order.UpdateEvent(AutoEvents.DeliveryCartageAdvised, new ZDateTimeOffset(2005, 2, 2));
			order.UpdateEventEstimate(AutoEvents.CustomsCommenced, new ZDateTimeOffset(2005, 1, 3));
			order.UpdateEvent(AutoEvents.CustomsCommenced, new ZDateTimeOffset(2005, 2, 3));
			order.UpdateEventEstimate(AutoEvents.CustomsCleared, new ZDateTimeOffset(2005, 1, 4));
			order.UpdateEvent(AutoEvents.CustomsCleared, new ZDateTimeOffset(2005, 2, 4));
			order.UpdateEventEstimate(AutoEvents.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(2005, 1, 5));
			order.UpdateEvent(AutoEvents.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(2005, 2, 5));
			order.UpdateEventEstimate(AutoEvents.Departure, new ZDateTimeOffset(2005, 1, 6));
			order.UpdateEvent(AutoEvents.Departure, new ZDateTimeOffset(2005, 2, 6));
			order.UpdateEventEstimate(AutoEvents.ExWorks, new ZDateTimeOffset(2005, 1, 7));
			order.UpdateEvent(AutoEvents.ExWorks, new ZDateTimeOffset(2005, 2, 7));
			order.UpdateEventEstimate(AutoEvents.GateIn, new ZDateTimeOffset(2005, 1, 8));
			order.UpdateEvent(AutoEvents.GateIn, new ZDateTimeOffset(2005, 2, 8));
			order.UpdateEventEstimate(AutoEvents.CargoAvailable, new ZDateTimeOffset(2005, 1, 9));
			order.UpdateEvent(AutoEvents.CargoAvailable, new ZDateTimeOffset(2005, 2, 9));
			order.JD_EstimateUserDate1 = new ZDateTime(2005, 1, 10);
			order.JD_ActualUserDate1 = new ZDateTime(2005, 2, 10);
			order.JD_EstimateUserDate2 = new ZDateTime(2005, 1, 11);
			order.JD_ActualUserDate2 = new ZDateTime(2005, 2, 11);
			order.JD_EstimateUserDate3 = new ZDateTime(2005, 1, 12);
			order.JD_ActualUserDate3 = new ZDateTime(2005, 2, 12);
			order.JD_EstimateUserDate4 = new ZDateTime(2005, 1, 13);
			order.JD_ActualUserDate4 = new ZDateTime(2005, 2, 13);

			order.JD_RL_NKGoodsAvailableAt = "AUMEL";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			order.JD_RL_NKPortOfLoading = "AUBNE";
			order.JD_RL_NKPortOfDischarge = "AUPER";
			order.JD_Waybill = "Waybill";
			order.JD_Packs = 3;
			order.JD_F3_NKPackType = Constants.PkgUnit.Coil;
			order.JD_ActualWeight = 2;
			order.JD_UnitOfWeight = Constants.Weight.ShortTons;
			order.JD_ActualVolume = 3;
			order.JD_UnitOfVolume = Constants.Volume.CubicMetres;
			order.JD_RV_NKDepartureVessel = "DepVessel";
			order.JD_DepartureVoyage = "DepVoyage";
			order.JD_RV_NKIntermediateVessel = "IntVessel";
			order.JD_IntermediateVoyage = "IntVoyage";
			order.JD_RV_NKArrivalVessel = "ArvVessel";
			order.JD_ArrivalVoyage = "ArvVoyage";

			order.JD_OH_SendingAgent = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			order.SendingAgent.OH_Code = "SNDAG";
			order.SendingAgent.OH_FullName = "SendingAgent";
			order.SendingAgent.MainAddress.OA_Address1 = "SendingAgent";

			order.JD_OH_ReceivingAgent = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			order.ReceivingAgent.OH_Code = "RCVAG";
			order.ReceivingAgent.OH_FullName = "ReceivingAgent";
			order.ReceivingAgent.MainAddress.OA_Address1 = "ReceivingAgent";

			var orderContainer = order.PlannedContainers.AddNew();
			orderContainer.J1_ContainerNumber = "C123";
			orderContainer.J1_ContainerCount = 1;
			orderContainer.J1_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "40FR").PK;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 2;
			orderLine.JO_SubLineNo = 3;
			orderLine.JO_LineSplitNumber = 4;
			orderLine.JO_Partno = "Partno";
			orderLine.JO_Description = "OrderLineDescription";
			orderLine.JO_Quantity = 5;
			orderLine.JO_InnerPacks = 2;
			orderLine.JO_OuterPacks = 4;
			orderLine.JO_ItemPrice = 2;
			orderLine.JO_LinePrice = 10;
			orderLine.JO_LineStatus = Constants.OrderStatus.Delivered;
			orderLine.JO_LineDropDate = new ZDateTime(2005, 9, 9);

			var delivery = orderLine.Deliveries.AddNew();
			delivery.J4_RL_NKDestinationPort = "AUADL";
			delivery.J4_OA_NKDeliveryPoint = order.Buyer.MainAddress.OA_Code;
			delivery.J4_Allocated = 5;

			var deliveryContainer = delivery.Containers.AddNew();
			deliveryContainer.J5_ContainerNum = "C123";
			deliveryContainer.J5_ContainerSeal = "Seal";
			deliveryContainer.J5_MasterBill = "MasterBill";
			deliveryContainer.J5_RL_NKLoadPort = "AUSYD";
			deliveryContainer.J5_RV_NKArrivalVessel = "ArrivalVessel";
			deliveryContainer.J5_Voyage = "Voyage";
			deliveryContainer.J5_ETA = new ZDateTime(2005, 3, 3);
			deliveryContainer.J5_ETD = new ZDateTime(2005, 4, 4);
			deliveryContainer.J5_PackCount = 7;
			deliveryContainer.J5_F3_NKPackType = Constants.PkgUnit.Box;
			deliveryContainer.J5_Volume = 5;
			deliveryContainer.J5_VolumeUQ = Constants.Volume.CubicFeet;
			deliveryContainer.J5_Weight = 6;
			deliveryContainer.J5_WeightUQ = Constants.Weight.ShortTons;

			((IBusinessObjectInternals)order).IsCopying = true;
			order.JD_TransportMode = Constants.TransportModes.Sea;
			((IBusinessObjectInternals)order).IsCopying = false;

			var cusDeclaration = Factory.New<BaseJobDeclaration>();
			cusDeclaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			cusDeclaration.JE_DeclarationReference = "ABCDE";
			cusDeclaration.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2005, 1, 5);
			cusDeclaration.JE_OH_Forwarder = order.JD_OH_ReceivingAgent;
			cusDeclaration.JE_RL_NKOrigin = order.JD_RL_NKGoodsAvailableAt;
			cusDeclaration.JE_RL_NKFinalDestination = order.JD_RL_NKGoodsDeliveredTo;
			cusDeclaration.JE_RL_NKPortOfLoading = order.JD_RL_NKPortOfLoading;
			cusDeclaration.JE_RL_NKPortOfArrival = order.JD_RL_NKPortOfDischarge;
			cusDeclaration.JE_HouseBill = order.JD_Waybill;

			order.JD_JE = cusDeclaration.PK;
			order.JD_OrderNumber = "Declaration";

			deliveryContainer.J5_RC_NKContainerType = string.Empty;

			return order;
		}

		void PopulateOrderDetails(TrackingOrder order)
		{
			order.JD_BookingConfRef = "BookingConfRef";
			order.JD_BookingConfDate = new ZDateTime(2006, 12, 25);
			order.JD_InvoiceNumber = "InvoiceNumber";
			order.JD_InvoiceDate = new ZDateTime(2011, 07, 13);
			order.JD_ExWorksRequiredBy = new ZDateTime(1998, 01, 02);
			order.JD_DeliveryRequiredBy = new ZDateTime(2004, 03, 04);
			order.JD_OrderGoodsDescription = "OrderGoodsDescription";
			order.JD_OrderStatus = Constants.OrderStatus.Open;

			var orderCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.Algeria);
			order.JD_RX_NKOrderCurrency = orderCurrency.RX_Code;
			order.JD_EstimatedExchangeRate = new ZDecimal(248);
			order.JD_RN_NKCountryOfSupply = Constants.CountryCodes.Australia;
		}

		void PopulateMilestones(TrackingOrder order)
		{
			order.UpdateEventEstimate(AutoEvents.ExWorks, new ZDateTimeOffset(1998, 01, 01));
			order.UpdateEvent(AutoEvents.ExWorks, new ZDateTimeOffset(2004, 01, 01));

			order.UpdateEventEstimate(AutoEvents.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(1998, 02, 01));
			order.UpdateEvent(AutoEvents.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(2004, 02, 01));

			order.UpdateEventEstimate(AutoEvents.GateIn, new ZDateTimeOffset(1998, 03, 01));
			order.UpdateEvent(AutoEvents.GateIn, new ZDateTimeOffset(2004, 03, 01));

			order.UpdateEventEstimate(AutoEvents.Departure, new ZDateTimeOffset(1998, 04, 01));
			order.UpdateEvent(AutoEvents.Departure, new ZDateTimeOffset(1998, 04, 01));

			order.UpdateEventEstimate(AutoEvents.Arrival, new ZDateTimeOffset(1998, 05, 01));
			order.UpdateEvent(AutoEvents.Arrival, new ZDateTimeOffset(1998, 05, 01));

			order.UpdateEventEstimate(AutoEvents.CustomsCommenced, new ZDateTimeOffset(1998, 06, 01));
			order.UpdateEvent(AutoEvents.CustomsCommenced, new ZDateTimeOffset(2003, 06, 01));

			order.UpdateEventEstimate(AutoEvents.CustomsCleared, new ZDateTimeOffset(1998, 07, 01));
			order.UpdateEvent(AutoEvents.CustomsCleared, new ZDateTimeOffset(2003, 07, 01));

			order.UpdateEventEstimate(AutoEvents.DeliveryCartageAdvised, new ZDateTimeOffset(1998, 08, 01));
			order.UpdateEvent(AutoEvents.DeliveryCartageAdvised, new ZDateTimeOffset(2003, 08, 01));

			order.JD_EstimateUserDate1 = new ZDateTime(1998, 09, 01);
			order.JD_EstimateUserDate2 = new ZDateTime(2005, 09, 01);
		}

		void PopulateOrderLine(OrderLine line)
		{
			line.JO_LineNo = 133;
			line.JO_SubLineNo = 133;
			line.JO_Partno = "Partno";
			line.JO_Description = "Description";
			line.JO_Quantity = new ZDecimal(76);
			line.JO_F3_NKPackType = Constants.PkgUnit.Unit;
			line.JO_InnerPacks = new ZDecimal(45);
			line.JO_OuterPacks = new ZDecimal(234);
			line.JO_LinePrice = new ZDecimal(148.20);
			line.JO_LineStatus = Constants.OrderStatus.PartDelivered;
			line.JO_LineDropDate = new ZDateTime(2004, 07, 08);
			line.JO_ContainerPackingOrder = 1;
			line.JO_ContainerNumber = "ContainerNo";
			line.JO_CommercialInvoiceNo = "123";
			line.JO_RN_NKCountryOfOrigin = "AU";
			line.UNDGs.AddNew().DI_DG = Substance.PK;

			var lineDelivery = line.Deliveries.AddNew();
			lineDelivery.J4_RL_NKDestinationPort = "MYPKG";
			lineDelivery.J4_OA_NKDeliveryPoint = line.Order.Buyer.MainAddress.OA_Code;
			lineDelivery.J4_Allocated = new ZDecimal(176);

			var delivery2 = line.Deliveries.AddNew();
			delivery2.J4_RL_NKDestinationPort = "AUADL";
			delivery2.J4_OA_NKDeliveryPoint = "ANAN";
			delivery2.J4_Allocated = 5;

			var deliveryContainer = delivery2.Containers.AddNew();
			deliveryContainer.J5_ContainerNum = "C123";
			deliveryContainer.J5_ContainerSeal = "Seal";
			deliveryContainer.J5_MasterBill = "MasterBill";
			deliveryContainer.J5_RL_NKLoadPort = "AUSYD";
			deliveryContainer.J5_RV_NKArrivalVessel = "ArrivalVessel";
			deliveryContainer.J5_Voyage = "Voyage";
			deliveryContainer.J5_ETA = new ZDateTime(2005, 3, 3);
			deliveryContainer.J5_ETD = new ZDateTime(2005, 4, 4);
			deliveryContainer.J5_PackCount = 7;
			deliveryContainer.J5_F3_NKPackType = Constants.PkgUnit.Box;
			deliveryContainer.J5_Volume = 5;
			deliveryContainer.J5_VolumeUQ = Constants.Volume.CubicFeet;
			deliveryContainer.J5_Weight = 6;
			deliveryContainer.J5_WeightUQ = Constants.Weight.ShortTons;
			deliveryContainer.J5_RC_NKContainerType = string.Empty;

			deliveryContainer = delivery2.Containers.AddNew();

			var container = lineDelivery.Containers.AddNew();
			container.J5_ContainerNum = "CONTAINERNUM";
			container.J5_ContainerSeal = "ContainerSeal";
			container.J5_MasterBill = "MasterBill";
			container.J5_RL_NKLoadPort = "AUSYD";
			container.J5_RV_NKArrivalVessel = "ArrivalVessel";
			container.J5_Voyage = "Voyage";
			container.J5_ETA = new ZDateTime(1998, 07, 08);
			container.J5_ETD = new ZDateTime(1998, 01, 02);
			container.J5_PackCount = 107;
			container.J5_F3_NKPackType = Constants.PkgUnit.Case;
			container.J5_Volume = new ZDecimal(14);
			container.J5_VolumeUQ = Constants.Volume.Litre;
			container.J5_Weight = new ZDecimal(227);
			container.J5_WeightUQ = Constants.Weight.Kilograms;
			container.J5_RC_NKContainerType = string.Empty;
		}

		UNDGSubstance Substance
		{
			get
			{
				if (substance == null)
				{
					substance = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.DG_UNNO, "2000"));
					AssertNotNull(substance);
				}
				return substance;
			}
		}
		UNDGSubstance substance;

		void PopulateShipmentPlanning(TrackingOrder order)
		{
			order.JD_RL_NKGoodsAvailableAt = "AUMEL";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "AUMEL";
			order.JD_RV_NKDepartureVessel = "DepartureVessel";
			order.JD_DepartureVoyage = "DepartureV";

			var cont = order.PlannedContainers.AddNew();
			cont.J1_ContainerNumber = "ContainerNum";
			cont.J1_ContainerCount = 1;
			cont.J1_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "40GP").PK;
		}

		OrgHeader GetFullyPopulatedBuyer()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.NoData);
			buyer.OH_FullName = "Buyer";
			buyer.OH_Code = "CMDQCZ46JXTI";
			return buyer;
		}

		OrgHeader GetFullyPopulatedSupplier()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.NoData);
			supplier.OH_FullName = "Supplier";
			supplier.OH_Code = "L1FZUT4BYOD8";
			return supplier;
		}
	}
}
