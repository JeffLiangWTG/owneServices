using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class SeaShipmentBookingRequestBuilderTest : TestCaseWithFactory
	{
		#region Populate DoorPickup/Delivery Related Properties

		public void TestDoorPickupAndDelivery_WithNoConsol()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			Assert(!bookingRequest.IsDoorPickup);
			Assert(!bookingRequest.IsDoorDelivery);
		}

		public void TestDoorPickupAndDelivery_WithConsol()
		{
			var shipment = CreateShipment(false);
			var consol = shipment.Consols[0];
			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;

			var builder = CreateDocDataObjectBuilder(shipment);
			var bookingRequest1 = builder.Build();
			Assert(bookingRequest1.IsDoorPickup);
			Assert(bookingRequest1.IsDoorDelivery);

			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CFS;

			var bookingRequest2 = builder.Build();
			Assert(!bookingRequest2.IsDoorPickup);
			Assert(bookingRequest2.IsDoorDelivery);

			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CY;

			var bookingRequest3 = builder.Build();
			Assert(bookingRequest3.IsDoorPickup);
			Assert(!bookingRequest3.IsDoorDelivery);
		}

		public void TestEstCargoPickupDateTime_ShouldBeEmpty_WhenShipmentIsNotDoorPickup()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(ZDateTime.Empty, bookingRequest.EstCargoPickupDateTime);
		}

		public void TestEstCargoPickupDateTime_ShouldBeShipmentPickuprequiredBy_WhenShipmentIsDoorPickup()
		{
			var shipment = CreateShipment(false);
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorPickup = true;
			AssertEquals(shipment.DocsAndCartage.JP_PickupRequiredBy, bookingRequest.EstCargoPickupDateTime);
		}

		public void TestPaymentTermsPopulationAndValidations()
		{
			var messageError = "Either Prepaid or Collect payment type must be selected for 'Payment Terms'.";

			var shipment = CreateShipment(true, Core.Constants.TransportModes.Sea);
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			CombineAssertions(() =>
			{
				Assert("Not IsPrepaid", !bookingRequest.PaymentTerms.IsPrepaid);
				Assert("Not IsCollect", !bookingRequest.PaymentTerms.IsCollect);

				AssertHasMessageError(((OptionalCharge)bookingRequest.PaymentTerms).IsPrepaidInfo, messageError);
				AssertHasMessageError(((OptionalCharge)bookingRequest.PaymentTerms).IsCollectInfo, messageError);
			});

			shipment = CreateShipment(false, Core.Constants.TransportModes.Sea);
			shipment.Consols[0].JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			shipment.Consols[0].JK_RL_NKLoadPort = "AUSYD";
			shipment.Consols[0].JK_RL_NKDischargePort = "AUBNE";
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			CombineAssertions(() =>
			{
				Assert("IsPrepaid", bookingRequest.PaymentTerms.IsPrepaid);
				Assert("Not IsCollect", !bookingRequest.PaymentTerms.IsCollect);

				AssertNoMessageError(((OptionalCharge)bookingRequest.PaymentTerms).IsPrepaidInfo, messageError);
				AssertNoMessageError(((OptionalCharge)bookingRequest.PaymentTerms).IsCollectInfo, messageError);
			});

			shipment = CreateShipment(false, Core.Constants.TransportModes.Sea);
			shipment.Consols[0].JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			CombineAssertions(() =>
			{
				Assert("Not IsPrepaid", !bookingRequest.PaymentTerms.IsPrepaid);
				Assert("IsCollect", bookingRequest.PaymentTerms.IsCollect);

				AssertNoMessageError(((OptionalCharge)bookingRequest.PaymentTerms).IsPrepaidInfo, messageError);
				AssertNoMessageError(((OptionalCharge)bookingRequest.PaymentTerms).IsCollectInfo, messageError);
			});

			var newFirstSeaConsol = CreateConsol(Core.Constants.TransportModes.Sea);
			newFirstSeaConsol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			newFirstSeaConsol.JK_RL_NKLoadPort = "BEANR";
			newFirstSeaConsol.JK_RL_NKDischargePort = "AUSYD";
			shipment.Consols.Add(newFirstSeaConsol);
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			CombineAssertions(() =>
			{
				Assert("IsPrepaid", bookingRequest.PaymentTerms.IsPrepaid);
				Assert("Not IsCollect", !bookingRequest.PaymentTerms.IsCollect);

				AssertNoMessageError(((OptionalCharge)bookingRequest.PaymentTerms).IsPrepaidInfo, messageError);
				AssertNoMessageError(((OptionalCharge)bookingRequest.PaymentTerms).IsCollectInfo, messageError);
			});
		}

		public void TestIsConsolAttached()
		{
			var shipment = CreateShipment(true, Core.Constants.TransportModes.Sea);
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			Assert("Not IsConsolAttached", !bookingRequest.IsConsolAttached);

			shipment = CreateShipment(false, Core.Constants.TransportModes.Sea);
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			Assert("IsConsolAttached", bookingRequest.IsConsolAttached);
		}

		public void TestReleaseType()
		{
			var shipment = CreateShipment(true, Core.Constants.TransportModes.Sea);
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("Not ReleaseType", null, bookingRequest.ReleaseType);

			shipment = CreateShipment(false, Core.Constants.TransportModes.Sea);
			shipment.Consols[0].JK_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.Consols[0].JK_RL_NKLoadPort = "AUSYD";
			shipment.Consols[0].JK_RL_NKDischargePort = "AUBNE";
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("ReleaseType", Core.Constants.ShipmentReleaseTypes.SeaWaybill, bookingRequest.ReleaseType.Code);

			var newFirstSeaConsol = CreateConsol(Core.Constants.TransportModes.Sea);
			newFirstSeaConsol.JK_ReleaseType = Core.Constants.ShipmentReleaseTypes.ExpressBofL;
			newFirstSeaConsol.JK_RL_NKLoadPort = "BEANR";
			newFirstSeaConsol.JK_RL_NKDischargePort = "AUSYD";
			shipment.Consols.Add(newFirstSeaConsol);
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("ReleaseType", Core.Constants.ShipmentReleaseTypes.ExpressBofL, bookingRequest.ReleaseType.Code);
		}

		#region PickupFrom

		public void TestPickupFrom_WithConsol()
		{
			var shipment = CreateShipment(false);
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertNotNull(bookingRequest.PickupFrom);
			AssertEquals(string.Empty, bookingRequest.PickupFrom.AddressFormatted);
			AssertNotNull(bookingRequest.PickupFrom.Unloco);

			bookingRequest.IsDoorPickup = true;
			var consol = shipment.Consols[0];
			AssertionHelper.AssertAddressData(consol.PackDepotAddress, bookingRequest.PickupFrom);
			AssertEquals(consol.PackDepotAddress.ClosestPort, bookingRequest.PickupFrom.Unloco.Code);
		}

		public void TestPickupFrom_ShouldBeEmpty_WhenIsNotDoorPickup()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNotNull(bookingRequest.PickupFrom);
			AssertEquals(string.Empty, bookingRequest.PickupFrom.AddressFormatted);
			AssertNotNull(bookingRequest.PickupFrom.Unloco);
		}

		public void TestPickupFrom_ShouldBeCFSAddress_WhenIsDoorPickupAndPickupCFSAddressIsnotEmpty()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorPickup = true;
			AssertionHelper.AssertAddressData(shipment.ExportReceivingDepot, bookingRequest.PickupFrom);
			AssertEquals(shipment.ExportReceivingDepot.ClosestPort, bookingRequest.PickupFrom.Unloco.Code);
		}

		public void TestPickupFrom_ShouldBePickupFromAddress_WhenIsDoorPickupAndPickupCFSAddressIsEmpty()
		{
			var shipment = CreateShipment();
			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorPickup = true;
			AssertionHelper.AssertAddressData(shipment.ConsignorPickupAddress, bookingRequest.PickupFrom);
			AssertEquals("AUSYD", bookingRequest.PickupFrom.Unloco.Code);
		}

		public void TestPickupFrom_ShouldBeEmpty_WhenIsDoorPickupAndPickupCFSAddressIsEmptyAndPickupFromAddressIsEmpty()
		{
			var shipment = CreateShipment();
			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorPickup = true;
			AssertNotNull(bookingRequest.PickupFrom);
			AssertEquals(string.Empty, bookingRequest.PickupFrom.AddressFormatted);
			AssertNotNull(bookingRequest.PickupFrom.Unloco);
		}

		#endregion

		#region DeliverTo

		public void TestDeliverTo_WithConsol()
		{
			var shipment = CreateShipment(false);
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertNotNull(bookingRequest.DeliverTo);
			AssertEquals(string.Empty, bookingRequest.DeliverTo.AddressFormatted);
			AssertNotNull(bookingRequest.DeliverTo.Unloco);

			bookingRequest.IsDoorDelivery = true;
			var consol = shipment.Consols[0];
			AssertionHelper.AssertAddressData(consol.UnpackDepotAddress, bookingRequest.DeliverTo);
			AssertEquals(consol.UnpackDepotAddress.ClosestPort, bookingRequest.DeliverTo.Unloco.Code);
		}

		public void TestDeliverTo_WithConsol_IsDoorDelivery()
		{
			var shipment = CreateShipment(false);
			var consol = shipment.Consols[0];
			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertionHelper.AssertAddressData(consol.UnpackDepotAddress, bookingRequest.DeliverTo);
			AssertEquals(consol.UnpackDepotAddress.ClosestPort, bookingRequest.DeliverTo.Unloco.Code);
		}

		public void TestDeliverTo_ShouldBeEmpty_WhenIsNotDoorDelivery()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNotNull(bookingRequest.DeliverTo);
			AssertEquals(string.Empty, bookingRequest.DeliverTo.AddressFormatted);
			AssertNotNull(bookingRequest.DeliverTo.Unloco);
		}

		public void TestDeliverTo_ShouldBeCFSAddress_WhenIsDoorDeliveryAndDeliveryCFSAddressIsnotEmpty()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorDelivery = true;
			AssertionHelper.AssertAddressData(shipment.ImportReleaseDepot, bookingRequest.DeliverTo);
			AssertEquals(shipment.ImportReleaseDepot.ClosestPort, bookingRequest.DeliverTo.Unloco.Code);
		}

		public void TestDeliverTo_ShouldBeConsigneeDeliveryAddress_WhenIsDoorDeliveryAndDeliveryCFSAddressIsEmpty()
		{
			var shipment = CreateShipment();
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorDelivery = true;
			AssertionHelper.AssertAddressData(shipment.ConsigneeDeliveryAddress, bookingRequest.DeliverTo);
			AssertEquals("SGSIN", bookingRequest.DeliverTo.Unloco.Code);
		}

		public void TestDeliverTo_ShouldBeEmpty_WhenIsDoorDeliveryAndDeliveryCFSAddressIsEmptyAndConsigneeDeliveryAddressIsEmpty()
		{
			var shipment = CreateShipment();
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorDelivery = true;
			AssertNotNull(bookingRequest.DeliverTo);
			AssertEquals(string.Empty, bookingRequest.DeliverTo.AddressFormatted);
			AssertNotNull(bookingRequest.DeliverTo.Unloco);
		}

		#endregion

		#region PlaceOfReceipt

		public void TestPlaceOfReceipt_ShouldBeEmpty_WhenIsNotDoorPickup()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNullOrEmpty(bookingRequest.PlaceOfReceipt?.Code);
		}

		public void TestPlaceOfReceipt_ShouldBePickupCFS_WhenIsDoorPickupAndDeliveryPickupCFSIsnotEmpty()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorPickup = true;
			AssertEquals(shipment.ExportReceivingDepot.HeaderClosestPort.Code, bookingRequest.PlaceOfReceipt.Code);
		}

		public void TestPlaceOfReceipt_ShouldBePickupFrom_WhenIsDoorPickupAndPickupCFSAddressIsEmptyAndPickupFromIsNotEmpty()
		{
			var shipment = CreateShipment();
			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorPickup = true;
			AssertEquals(shipment.ConsignorPickupAddress.Organisation?.ClosestPort.Code, bookingRequest.PlaceOfReceipt.Code);
		}

		public void TestPlaceOfReceipt_ShouldBeShipmentOrigin_WhenIsDoorPickupAndPickupCFSAddressIsEmptyAndPickupFromIsEmpty()
		{
			var shipment = CreateShipment();
			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorPickup = true;
			AssertEquals(shipment.Origin.Code, bookingRequest.PlaceOfReceipt.Code);
		}

		#endregion

		#region PlaceOfDelivery

		public void TestPlaceOfDelivery_ShouldBeEmpty_WhenIsNotDoorDelivery()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNullOrEmpty(bookingRequest.PlaceOfReceipt?.Code);
		}

		public void TestPlaceOfDelivery_ShouldBeDeliveryCFS_WhenIsDoorDeliveryrAndDeliveryPickupCFSIsnotEmpty()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorDelivery = true;
			AssertEquals(shipment.ImportReleaseDepot.HeaderClosestPort.Code, bookingRequest.PlaceOfDelivery.Code);
		}

		public void TestPlaceOfDelivery_ShouldBeDeliverTo_WhenIsDoorDeliveryAndDeliveryCFSAddressIsEmptyAndDeliverToIsNotEmpty()
		{
			var shipment = CreateShipment();
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorDelivery = true;
			AssertEquals(shipment.ConsigneeDeliveryAddress.Organisation?.ClosestPort.Code, bookingRequest.PlaceOfDelivery.Code);
		}

		public void TestPlaceOfDelivery_ShouldBeShipmentDestination_WhenIsDoorDeliveryAndDeliveryCFSAddressIsEmptyAndDeliverToIsEmpty()
		{
			var shipment = CreateShipment();
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorDelivery = true;
			AssertEquals(shipment.Destination.Code, bookingRequest.PlaceOfDelivery.Code);
		}

		#endregion

		#endregion

		#region Populate Other Properties

		public void TestTransport()
		{
			var shipment = CreateShipment(false);
			var transport = shipment.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "VesselData";
			transport.JW_VoyageFlight = "100";
			transport.JW_ETD = new ZDateTime(2021, 01, 01, 00, 00, 00);
			transport.JW_ETA = new ZDateTime(2021, 01, 04, 00, 00, 00);

			var bookingRequest1 = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(transport.JW_Vessel, bookingRequest1.VesselName);
			AssertNullOrEmpty(bookingRequest1.LloydsIMO);
			AssertEquals(transport.JW_VoyageFlight, bookingRequest1.VoyageNumber);
			AssertEquals(transport.JW_ETD, bookingRequest1.ETD);
			AssertEquals(transport.JW_ETA, bookingRequest1.ETA);
			AssertEquals(transport.JW_RL_NKLoadPort, bookingRequest1.PortOfLoading.Code);
			AssertEquals(transport.JW_RL_NKDiscPort, bookingRequest1.PortOfDischarge.Code);
			AssertEquals(transport.JW_TransportMode, bookingRequest1.LegTransportMode);
			AssertEquals(transport.JW_LegOrder, bookingRequest1.LegOrder);
			AssertEquals(transport.JW_TransportType, bookingRequest1.LegType);

			var consol = shipment.Consols[0];
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var transport1 = consol.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "CNSHA";
			transport1.JW_Vessel = "VesselData1";
			transport1.JW_VoyageFlight = "101";
			transport1.JW_ETD = new ZDateTime(2021, 01, 01, 00, 00, 00);
			transport1.JW_ETA = new ZDateTime(2021, 01, 02, 00, 00, 00);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "CNSHA";
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_Vessel = "VesselData2";
			transport2.JW_VoyageFlight = "102";
			transport2.JW_ETD = new ZDateTime(2021, 01, 03, 00, 00, 00);
			transport2.JW_ETA = new ZDateTime(2021, 01, 04, 00, 00, 00);

			var bookingRequest2 = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(transport1.JW_Vessel, bookingRequest2.VesselName);
			AssertNullOrEmpty(bookingRequest2.LloydsIMO);
			AssertEquals(transport1.JW_VoyageFlight, bookingRequest2.VoyageNumber);
			AssertEquals(transport1.JW_ETD, bookingRequest2.ETD);
			AssertEquals(transport1.JW_ETA, bookingRequest2.ETA);
			AssertEquals(transport1.JW_RL_NKLoadPort, bookingRequest2.PortOfLoading.Code);
			AssertEquals(transport1.JW_RL_NKDiscPort, bookingRequest2.PortOfDischarge.Code);
			AssertEquals(transport1.JW_TransportMode, bookingRequest2.LegTransportMode);
			AssertEquals(transport1.JW_LegOrder, bookingRequest2.LegOrder);
			AssertEquals(transport1.JW_TransportType, bookingRequest2.LegType);

			var newFirstSeaConsol = CreateConsol(Core.Constants.TransportModes.Sea);
			newFirstSeaConsol.JK_RL_NKLoadPort = "NLRTM";
			newFirstSeaConsol.JK_RL_NKDischargePort = "AUSYD";
			var transport3 = newFirstSeaConsol.Transports.AddNew();
			transport3.JW_LegOrder = 1;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport3.JW_RL_NKLoadPort = "NLRTM";
			transport3.JW_RL_NKDiscPort = "AUMEL";
			transport3.JW_Vessel = "VesselData3";
			transport3.JW_VoyageFlight = "102";
			transport3.JW_ETD = new ZDateTime(2021, 01, 03, 00, 00, 00);
			transport3.JW_ETA = new ZDateTime(2021, 01, 04, 00, 00, 00);

			var transport4 = newFirstSeaConsol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport4.JW_RL_NKLoadPort = "AUMEL";
			transport4.JW_RL_NKDiscPort = "AUSYD";
			transport4.JW_Vessel = "VesselData4";
			transport4.JW_VoyageFlight = "102";
			transport4.JW_ETD = new ZDateTime(2021, 01, 03, 00, 00, 00);
			transport4.JW_ETA = new ZDateTime(2021, 01, 04, 00, 00, 00);
			shipment.Consols.Add(newFirstSeaConsol);

			var bookingRequest3 = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(transport3.JW_Vessel, bookingRequest3.VesselName);
			AssertNullOrEmpty(bookingRequest3.LloydsIMO);
			AssertEquals(transport3.JW_VoyageFlight, bookingRequest3.VoyageNumber);
			AssertEquals(transport3.JW_ETD, bookingRequest3.ETD);
			AssertEquals(transport3.JW_ETA, bookingRequest3.ETA);
			AssertEquals(transport3.JW_RL_NKLoadPort, bookingRequest3.PortOfLoading.Code);
			AssertEquals(transport3.JW_RL_NKDiscPort, bookingRequest3.PortOfDischarge.Code);
			AssertEquals(transport3.JW_TransportMode, bookingRequest3.LegTransportMode);
			AssertEquals(transport3.JW_LegOrder, bookingRequest3.LegOrder);
			AssertEquals(transport3.JW_TransportType, bookingRequest3.LegType);
		}

		public void TestCurrentUserAddress()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertionHelper.AssertCurrentUserAddressData(bookingRequest.CurrentUser);
		}

		public void TestShipperAddress_WithConsol()
		{
			var shipment = CreateShipment(false, Core.Constants.TransportModes.Sea);
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			var consol = shipment.Consols[0];
			AssertionHelper.AssertAddressData(consol.SendingForwarderAddress, bookingRequest.Shipper);
			AssertEquals(consol.SendingForwarderWithContact.OrgContact.OC_ContactName, bookingRequest.Shipper.Contact);
			AssertNotNull(bookingRequest.Shipper.Unloco);
			AssertEquals(consol.SendingForwarderAddress.ClosestPort, bookingRequest.Shipper.Unloco.Code);
		}

		public void TestShipperAddress_ShouldBeLoginBranch()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertionHelper.AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, bookingRequest.Shipper);
			AssertNotNull(bookingRequest.Shipper.Unloco);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.MainAddress.ClosestPort, bookingRequest.Shipper.Unloco.Code);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			var context = new CommonContext(Factory);
			AssertionHelper.AssertAddressData(AddressBuilder.Create(context, GlbBranch.CurrentBranch?.OrgProxy?.MainAddress), bookingRequest.Shipper);
			AssertEquals(string.Empty, bookingRequest.Shipper.Unloco.Code);
		}

		public void TestConsigneeAddress()
		{
			var shipment = CreateShipment(false);
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.MainAddress.CompanyName = "DeliveryAgent";
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			Factory.Save();

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertionHelper.AssertAddressData(shipment.Consols[0].ReceivingForwarderAddress, bookingRequest.Consignee);
			AssertEquals(shipment.Consols[0].ReceivingForwarderWithContact.OrgContact.OC_ContactName, bookingRequest.Consignee.Contact);
			AssertEquals("ReceivingAgent", bookingRequest.Consignee.CompanyName);

			var transport = shipment.Consols[0].Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUBNE";

			var consol = CreateConsol(Core.Constants.TransportModes.Sea, "Receiving Agent xx");
			var transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "AUMEL";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "BEANR";
			shipment.Consols.Add(consol);

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertionHelper.AssertAddressData(shipment.Consols[1].ReceivingForwarderAddress, bookingRequest.Consignee);
			AssertEquals("Receiving Agent xx", bookingRequest.Consignee.CompanyName);

			shipment.Consols.RemoveAndDeleteAll();
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertAddressData(shipment.DeliveryAgent.MainAddress, bookingRequest.Consignee);
			AssertEquals("DeliveryAgent", bookingRequest.Consignee.CompanyName);
		}

		public void TestCarrierAddress_WithConsol()
		{
			var shipment = CreateShipment(false);
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			var consol = shipment.Consols[0];
			AssertionHelper.AssertAddressData(consol.CreditorAddress, bookingRequest.Recipient);
			AssertNotNull(bookingRequest.Recipient.Unloco);
			AssertEquals(consol.CreditorAddress.ClosestPort, bookingRequest.Recipient.Unloco.Code);
		}

		public void TestCarrierAddress_ShouldBeShipmentPlannedCarrier()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertionHelper.AssertAddressData(shipment.BookedShippingLineAddress, bookingRequest.Recipient);
			AssertNotNull(bookingRequest.Recipient.Unloco);
			AssertEquals(shipment.BookedShippingLineAddress.ClosestPort, bookingRequest.Recipient.Unloco.Code);
		}

		public void TestCarrierBookingReference_ShouldBeShipmentCommaSeparatedCarrierBookingReferenceNumbers()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(shipment.GetCarrierBookingReference(), bookingRequest.BookingReference);
		}

		public void TestCarrierBookingOffice_ShouldBePlannedCarrierUnloco()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNotNull(bookingRequest.CarrierBookingOffice);
			AssertEquals(shipment.BookedShippingLineAddress.ClosestPort, bookingRequest.CarrierBookingOffice.Code);
		}

		public void TestCarrierBookingOffice_WithConsol()
		{
			var shipment = CreateShipment(false);
			var consol = shipment.Consols[0];
			consol.JK_RL_NKCarrierBookingOffice = "AUSYD";
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNotNull(bookingRequest.CarrierBookingOffice);
			AssertEquals(consol.CarrierBookingOffice.Code, bookingRequest.CarrierBookingOffice.Code);
		}

		public void TestCarrierBookingOfficeValidation_Build()
		{
			var warning = "Carrier booking office does not match to UNLOCO or country code of Place of Receipt/Port of Loading/Origin.\r\nPlease provide a valid Carrier Booking Office to avoid booking rejection by carrier.";

			var shipment = CreateShipment(false);
			shipment.JS_RL_NKOrigin = "BEANR";
			var consol = shipment.Consols[0];
			consol.JK_RL_NKCarrierBookingOffice = "AUSYD";
			consol.JK_RL_NKLoadPort = "CNSHA";
			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;

			var bookingRequestData = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals("BEANR", bookingRequestData.Origin.Code);
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("CNSHA", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", bookingRequestData.PortOfLoading.Code);
			AssertHasWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			consol.JK_RL_NKLoadPort = "AUMEL";
			bookingRequestData = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals("BEANR", bookingRequestData.Origin.Code);
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUMEL", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			shipment.JS_RL_NKOrigin = "AUBNE";
			bookingRequestData = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals("AUBNE", bookingRequestData.Origin.Code);
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUMEL", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			consol.JK_RL_NKCarrierBookingOffice = string.Empty;
			bookingRequestData = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals("AUBNE", bookingRequestData.Origin.Code);
			AssertEquals("", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUMEL", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);
		}

		public void TestCarrierBookingOfficeValidation_OnValueChanged()
		{
			var warning = "Carrier booking office does not match to UNLOCO or country code of Place of Receipt/Port of Loading/Origin.\r\nPlease provide a valid Carrier Booking Office to avoid booking rejection by carrier.";

			var shipment = CreateShipment(false);
			shipment.JS_RL_NKOrigin = "BEANR";
			var consol = shipment.Consols[0];
			consol.JK_RL_NKCarrierBookingOffice = "AUSYD";
			consol.JK_RL_NKLoadPort = "CNSHA";
			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;

			var bookingRequestData = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals("BEANR", bookingRequestData.Origin.Code);
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("CNSHA", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", bookingRequestData.PortOfLoading.Code);
			AssertHasWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			bookingRequestData.PortOfLoading.Code = "AUMEL";
			AssertEquals("BEANR", bookingRequestData.Origin.Code);
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("CNSHA", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			bookingRequestData.Origin.Code = "AUBNE";
			AssertEquals("AUBNE", bookingRequestData.Origin.Code);
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("CNSHA", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			bookingRequestData.PlaceOfReceipt.Code = "AUBNE";
			AssertEquals("AUBNE", bookingRequestData.Origin.Code);
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUBNE", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			bookingRequestData.CarrierBookingOffice.Code = string.Empty;
			AssertEquals("AUBNE", bookingRequestData.Origin.Code);
			AssertEquals("", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUBNE", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);
		}

		public void TestCarrierBookingOfficeMandatory()
		{
			var carrierBookingOfficeMandatoryMessage_NoConsol = "The Carrier Booking Office is mandatory.\r\nPlease provide a valid UNLOCO on selected address of Shipment > Additional Detail > Planned Carrier.";
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertNoMessageError(bookingRequest.CarrierBookingOffice.CodeInfo, carrierBookingOfficeMandatoryMessage_NoConsol);

			shipment.BookedShippingLineAddress.ClosestPort = string.Empty;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertHasMessageError(bookingRequest.CarrierBookingOffice.CodeInfo, carrierBookingOfficeMandatoryMessage_NoConsol);

			var carrierBookingOfficeMandatoryMessage_Consol = "The Carrier Booking Office is mandatory.\r\nPlease provide it on Consol > Details > Docs > Carrier Booking Office.";
			shipment = CreateShipment(false);
			shipment.Consols[0].JK_RL_NKCarrierBookingOffice = "AUSYD";
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertNoMessageError(bookingRequest.CarrierBookingOffice.CodeInfo, carrierBookingOfficeMandatoryMessage_Consol);

			shipment.Consols[0].JK_RL_NKCarrierBookingOffice = string.Empty;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertHasMessageError(bookingRequest.CarrierBookingOffice.CodeInfo, carrierBookingOfficeMandatoryMessage_Consol);
		}

		public void TestOperationalPort_and_FreightPayableAt_WithConsol()
		{
			var shipment = CreateShipment(false);
			shipment.Consols[0].JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("OperationalPort", "AUSYD", bookingRequest.OperationalPort.Code);
			AssertEquals("FreighPayableAt PPD", "CNNJI", bookingRequest.FreightPayableAt.Code);

			AssertNoMessageError(bookingRequest.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(bookingRequest.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			shipment.Consols[0].JK_OA_SendingForwarderAddress = ZGuid.Empty;
			shipment.Consols[0].JK_OA_SendingForwarderAddress = ZGuid.Empty;

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("OperationalPort", "AUSYD", bookingRequest.OperationalPort.Code);
			AssertEquals("FreighPayableAt PPD", "AUSYD", bookingRequest.FreightPayableAt.Code);

			AssertNoMessageError(bookingRequest.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(bookingRequest.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			shipment.Consols[0].JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("OperationalPort", "AUSYD", bookingRequest.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", "BEANR", bookingRequest.FreightPayableAt.Code);

			AssertNoMessageError(bookingRequest.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(bookingRequest.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			shipment.Consols[0].JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			shipment.Consols[0].JK_OC_ReceivingForwarderContact = ZGuid.Empty;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("OperationalPort", "AUSYD", bookingRequest.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", "SGSIN", bookingRequest.FreightPayableAt.Code);

			AssertNoMessageError(bookingRequest.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(bookingRequest.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			shipment.Consols[0].JK_RL_NKLoadPort = string.Empty;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("OperationalPort", "AUSYD", bookingRequest.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", "SGSIN", bookingRequest.FreightPayableAt.Code);

			shipment.JS_RL_NKLoadPort = "ABCDE";
			shipment.Consols[0].JK_RL_NKDischargePort = "ABCDE";
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("OperationalPort", "ABCDE", bookingRequest.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", "ABCDE", bookingRequest.FreightPayableAt.Code);

			AssertHasMessageError(bookingRequest.OperationalPort.CodeInfo, "You have not entered a valid un loco.");
			AssertHasMessageError(bookingRequest.FreightPayableAt.CodeInfo, "You have not entered a valid un loco.");

			shipment.JS_RL_NKLoadPort = string.Empty;
			shipment.Consols[0].JK_RL_NKDischargePort = string.Empty;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("OperationalPort", string.Empty, bookingRequest.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", string.Empty, bookingRequest.FreightPayableAt.Code);

			AssertHasMessageError(bookingRequest.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertHasMessageError(bookingRequest.FreightPayableAt.CodeInfo, "Freight Payable At is required.");
		}

		public void TestOperationalPort_and_FreightPayableAt_NoConsol()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("OperationalPort", "AUSYD", bookingRequest.OperationalPort.Code);
			AssertEquals("FreighPayableAt PPD", string.Empty, bookingRequest.FreightPayableAt.Code);

			AssertNoMessageError(bookingRequest.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertHasMessageError(bookingRequest.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			shipment.JS_RL_NKLoadPort = string.Empty;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals("OperationalPort", string.Empty, bookingRequest.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", string.Empty, bookingRequest.FreightPayableAt.Code);

			AssertHasMessageError(bookingRequest.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertHasMessageError(bookingRequest.FreightPayableAt.CodeInfo, "Freight Payable At is required.");
		}

		public void TestOrigin_ShouldBeShipmentOrigin()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(shipment.Origin.Code, bookingRequest.Origin.Code);
		}

		public void TestDestination_ShouldBeShipmentDestination()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(shipment.Destination.Code, bookingRequest.Destination.Code);
		}

		public void TestMode_ShouldBeShipmentContainerMode()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(shipment.JS_PackingMode, bookingRequest.ContainerMode.Code);
		}

		public void TestEarliestDeparture_ShouldBeShipmentETD()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(shipment.JS_E_DEP, bookingRequest.EarliestDepartureDate);
		}

		public void TestLatestDelivery_ShouldBeShipmentETA()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(shipment.JS_E_ARV, bookingRequest.LatestDeliveryDate);
		}

		public void TestShipperReference_ShouldBeShipmentHouseBill()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(shipment.JS_UniqueConsignRef, bookingRequest.ShipperReference);
		}

		public void TestCarrierContractNumber_ShouldBeShipmentCommaSeparatedCarrierContractNumbers()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(shipment.GetCarrierContractNumber(), bookingRequest.CarrierContractNumbersFormatted);
		}

		public void TestAdditionalTerm_ShouldBeShipmentAdditionalTerm()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(shipment.JS_AdditionalTerms, bookingRequest.AdditionalTerms);
		}

		public void TestGoodsHandlingInstruction_ShouldBeLineSeparatedgoodHandlingInstructionsOfShipment()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(shipment.GetGoodsHandlingInstructions(), bookingRequest.GoodsHandlingInstructions);
		}

		public void TestPopulateIsRequiredSendAttachment()
		{
			var shipment = CreateShipment();

			var refShippingLineForShipment = Factory.New<RefShippingLine>();
			shipment.BookedShippingLine.OH_RSL_ShippingLine = refShippingLineForShipment.PK;

			AssertEquals(0, shipment.Consols.Count);

			AssertNull(refShippingLineForShipment.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage));
			Assert(!CreateDocDataObjectBuilder(shipment).Build().IsRequiredSendAttachment);

			var messagingRequirementForShipment = refShippingLineForShipment.ShippingLineMessagingRequirements.AddNew();
			messagingRequirementForShipment.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage;
			messagingRequirementForShipment.RSR_IsBookingRequest = true;

			Assert(refShippingLineForShipment.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsBookingRequest);
			Assert(CreateDocDataObjectBuilder(shipment).Build().IsRequiredSendAttachment);

			messagingRequirementForShipment.RSR_IsBookingRequest = false;

			Assert(!refShippingLineForShipment.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsBookingRequest);
			Assert(!CreateDocDataObjectBuilder(shipment).Build().IsRequiredSendAttachment);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "OrgHeader";
			orgHeader.OH_RL_NKClosestPort = "CNSHA";
			orgHeader.MainAddress.Address1 = "Unit 1";
			orgHeader.MainAddress.Address2 = "4 What Lane";
			orgHeader.MainAddress.City = "Shanghai";
			orgHeader.MainAddress.Postcode = "5022";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "CN";

			var refShippingLineForConsol = Factory.New<RefShippingLine>();
			refShippingLineForConsol.RSL_IsNVO = true;
			orgHeader.OH_RSL_ShippingLine = refShippingLineForConsol.PK;

			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;

			AssertNotNull(shipment.Consols.OfType<ForwardingConsol>().FirstOrDefault(c => c.IsCoLoad && c.CreditorIsNVOCC));

			AssertNull(refShippingLineForConsol.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage));
			Assert(!CreateDocDataObjectBuilder(shipment).Build().IsRequiredSendAttachment);

			var messagingRequirementForConsol = refShippingLineForConsol.ShippingLineMessagingRequirements.AddNew();
			messagingRequirementForConsol.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage;
			messagingRequirementForConsol.RSR_IsBookingRequest = true;

			Assert(refShippingLineForConsol.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsBookingRequest);
			Assert(CreateDocDataObjectBuilder(shipment).Build().IsRequiredSendAttachment);

			messagingRequirementForConsol.RSR_IsBookingRequest = false;

			Assert(!refShippingLineForConsol.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsBookingRequest);
			Assert(!CreateDocDataObjectBuilder(shipment).Build().IsRequiredSendAttachment);
		}

		#endregion

		#region Populate Goods and Equipment Details

		#region Goods Description

		public void TestGoodsdescription_ShouldBeDetailsPackLineDescription_WhenDetailsPackLineDescriptionIsNotEmpty()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = "Books";

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(packLine.JL_DetailedDescription, bookingRequest.GoodsAndEquipmentDetails.First().GoodsDescription);
		}

		public void TestGoodsdescription_ShouldBePackLineGoodsDescription_WhenDetailsPackLineDescriptionIsEmptyAndPackLineGoodsDescriptionIsNotEmpty()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = string.Empty;
			packLine.JL_Description = "Notebooks";

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(packLine.JL_Description, bookingRequest.GoodsAndEquipmentDetails.First().GoodsDescription);
		}

		public void TestGoodsdescription_ShouldBeShipmentDetailedGoodsDescription_WhenDetailsPackLineDescriptionIsEmptyAndPackLineGoodsDescriptionIsEmptyAndShipmentDetailedGoodsDescriptionIsNotEmpty()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = string.Empty;
			packLine.JL_Description = string.Empty;
			shipment.DetailedGoodsDescriptionNoteText = "Bundle of CSH4 77*56* 84";

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(shipment.DetailedGoodsDescriptionNoteText, bookingRequest.GoodsAndEquipmentDetails.First().GoodsDescription);
		}

		public void TestGoodsdescription_ShouldBeShipmentDescription_WhenDetailsPackLineDescriptionIsEmptyAndPackLineGoodsDescriptionIsEmptyAndShipmentDetailedGoodsDescriptionIsEmpty()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = string.Empty;
			packLine.JL_Description = string.Empty;
			shipment.DetailedGoodsDescriptionNoteText = string.Empty;
			shipment.JS_GoodsDescription = "Rapunzel doll";

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(shipment.JS_GoodsDescription, bookingRequest.GoodsAndEquipmentDetails.First().GoodsDescription);
		}

		#endregion

		#region Marks and Numbers

		public void TestMarksAndNumbers_ShouldBePackLineMarksAndNumbers_WhenPackLineMarksAndNumbersIsNotEmpty()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_MarksAndNumbers = "1234   27733";

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(packLine.JL_MarksAndNumbers, bookingRequest.GoodsAndEquipmentDetails.First().MarksAndNumbersOnPackages);
		}

		public void TestMarksAndNumbers_ShouldBeShipmentMarksAndNumbers_WhenPackLineMarksAndNumbersIsEmpty()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_MarksAndNumbers = string.Empty;
			shipment.JS_MarksAndNumbers = "27847473737227";

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(shipment.JS_MarksAndNumbers, bookingRequest.GoodsAndEquipmentDetails.First().MarksAndNumbersOnPackages);
		}

		public void TestMarksAndNumbers_ShouldBeTrimmedPackLineMarksAndNumbers_WhenPackLineMarksAndNumbersSizeIsMoreThanFieldCapacity()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_MarksAndNumbers =
				@"27847473737227322222222222222222222222222222222222222222222
				2222222222222222222222222222222222222222222222222222222222222
				2222222222222222222222222222222222222222222222222222222222222
				2222222222222222222222222222222222222222222222222222222222222";

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(packLine.JL_MarksAndNumbers.SubstringSafe(0, SeaShipmentBookingRequestBuilder.PackLineMarksAndNumbersFieldCapacity), bookingRequest.GoodsAndEquipmentDetails.First().MarksAndNumbersOnPackages);
		}

		#endregion

		#region Packs

		public void TestPacks_ShouldBePackLinePacksPackType()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals($"{packLine.JL_PackageCount} {packLine.PackType.F3_Code}", bookingRequest.GoodsAndEquipmentDetails.First().Packs);
		}

		#endregion

		#region Weight and Volume

		public void TestWeight_ShouldBePackLineWeight_WhenItIsKGs()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeight = 12;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(packLine.JL_ActualWeight, bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.Value);
			AssertEquals(packLine.JL_ActualWeightUQ, bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.Unit.Code);
		}

		public void TestWeight_ShouldBePackLineWeight_WhenItIsLBs()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeight = 12;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(packLine.JL_ActualWeight, bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.Value);
			AssertEquals(packLine.JL_ActualWeightUQ, bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.Unit.Code);
		}

		public void TestWeight_ShouldBeConvertedToPoundsOfPackLineWeight_WhenItIsImperialAndAllOtherPackagesAreImperial()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeight = 12;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;

			var secondPackLine = shipment.OuterPackLines.AddNew();
			secondPackLine.JL_ActualWeight = 18;
			secondPackLine.JL_ActualWeightUQ = Core.Constants.Weight.Ounces;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(2, bookingRequest.GoodsAndEquipmentDetails.Count);

			AssertEquals(packLine.JL_ActualWeight, bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.Value);
			AssertEquals(packLine.JL_ActualWeightUQ, bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.Unit.Code);

			AssertEquals(Core.Constants.Weight.Convert(secondPackLine.JL_ActualWeight, secondPackLine.JL_ActualWeightUQ, Core.Constants.Weight.Pounds), bookingRequest.GoodsAndEquipmentDetails.Last().CargoWeight.Value);
			AssertEquals(Core.Constants.Weight.Pounds, bookingRequest.GoodsAndEquipmentDetails.Last().CargoWeight.Unit.Code);
		}

		public void TestWeight_ShouldBeConvertedToKGsOfPackLineWeight_WhenItIsNotImperial()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeight = 12;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Hectograms;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(Core.Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, Core.Constants.Weight.Kilograms), bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.Value);
			AssertEquals(Core.Constants.Weight.Kilograms, bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.Unit.Code);
		}

		public void TestWeight_ShouldBeConvertedToKGOfPackLineWeight_WhenOneOfPackLinesWeightsIsNotImperial()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeight = 12;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Ounces;

			var secondPackLine = shipment.OuterPackLines.AddNew();
			secondPackLine.JL_ActualWeight = 18;
			secondPackLine.JL_ActualWeightUQ = Core.Constants.Weight.Hectograms;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(2, bookingRequest.GoodsAndEquipmentDetails.Count);

			AssertEquals(Core.Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, Core.Constants.Weight.Kilograms), bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.Value);
			AssertEquals(Core.Constants.Weight.Kilograms, bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.Unit.Code);

			AssertEquals(Core.Constants.Weight.Convert(secondPackLine.JL_ActualWeight, secondPackLine.JL_ActualWeightUQ, Core.Constants.Weight.Kilograms), bookingRequest.GoodsAndEquipmentDetails.Last().CargoWeight.Value);
			AssertEquals(Core.Constants.Weight.Kilograms, bookingRequest.GoodsAndEquipmentDetails.Last().CargoWeight.Unit.Code);
		}

		public void TestVolume_ShouldBePackLineVolume_WhenItIsCubicMetres()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolume = 12;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(packLine.JL_ActualVolume, bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.Value);
			AssertEquals(packLine.JL_ActualVolumeUQ, bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.Unit.Code);
		}

		public void TestVolume_ShouldBePackLineVolume_WhenItIsCubicFeet()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolume = 12;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertEquals(packLine.JL_ActualVolume, bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.Value);
			AssertEquals(packLine.JL_ActualVolumeUQ, bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.Unit.Code);
		}

		public void TestVolume_ShouldBeConvertedToCubicFeetOfPackLineVolume_WhenItIsImperialAndAllOtherPackagesAreImperial()
		{
			var shipment = CreateShipment();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolume = 65;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicInches;

			var secondPackLine = shipment.OuterPackLines.AddNew();
			secondPackLine.JL_ActualVolume = 4;
			secondPackLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(2, bookingRequest.GoodsAndEquipmentDetails.Count);

			AssertEquals(Core.Constants.Volume.Convert(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ, Core.Constants.Volume.CubicFeet), bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.Value);
			AssertEquals(Core.Constants.Volume.CubicFeet, bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.Unit.Code);

			AssertEquals(secondPackLine.JL_ActualVolume, bookingRequest.GoodsAndEquipmentDetails.Last().CargoVolume.Value);
			AssertEquals(secondPackLine.JL_ActualVolumeUQ, bookingRequest.GoodsAndEquipmentDetails.Last().CargoVolume.Unit.Code);
		}

		public void TestVolume_ShouldBeConvertedToCubicMetresOfPackLineVolume_WhenItIsNotImperial()
		{
			var shipment = CreateShipment();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolume = 65;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);

			AssertEquals(Core.Constants.Volume.Convert(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ, Core.Constants.Volume.CubicMetres), bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.Value);
			AssertEquals(Core.Constants.Volume.CubicMetres, bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.Unit.Code);
		}

		public void TestVolume_ShouldBeConvertedToCubicFeetOfPackLineVolume_WhenOneOfPackLinesVolumesIsNotImperial()
		{
			var shipment = CreateShipment();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolume = 65;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;

			var secondPackLine = shipment.OuterPackLines.AddNew();
			secondPackLine.JL_ActualVolume = 4;
			secondPackLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(2, bookingRequest.GoodsAndEquipmentDetails.Count);

			AssertEquals(Core.Constants.Volume.Convert(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ, Core.Constants.Volume.CubicMetres), bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.Value);
			AssertEquals(Core.Constants.Volume.CubicMetres, bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.Unit.Code);

			AssertEquals(Core.Constants.Volume.Convert(secondPackLine.JL_ActualVolume, secondPackLine.JL_ActualVolumeUQ, Core.Constants.Volume.CubicMetres), bookingRequest.GoodsAndEquipmentDetails.Last().CargoVolume.Value);
			AssertEquals(Core.Constants.Volume.CubicMetres, bookingRequest.GoodsAndEquipmentDetails.Last().CargoVolume.Unit.Code);
		}

		#endregion

		#region DangerousGoods

		public void TestDangerousGoods_ShouldBeUNDGsOfPackLine()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();

			var undg1 = packLine.UNDGs.AddNew();
			undg1.DI_TechnicalName = "WHATEVER";
			undg1.DI_IMOClass = "CLAS";
			undg1.DI_IsCombustible = true;
			undg1.DI_DGFlashPoint = 522.2m;

			var undg2 = packLine.UNDGs.AddNew();
			undg2.DI_TechnicalName = "WHATEVER";
			undg2.DI_IMOClass = "6.1";
			undg2.DI_IsCombustible = true;
			undg2.DI_DGFlashPoint = 541.2m;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(2, bookingRequest.GoodsAndEquipmentDetails.First().DangerousGoods.Count);

			AssertEquals(undg1.DI_DGFlashPoint, bookingRequest.GoodsAndEquipmentDetails.First().DangerousGoods.First().FlashPoint.Value);
			AssertEquals(undg1.DI_IMOClass, bookingRequest.GoodsAndEquipmentDetails.First().DangerousGoods.First().IMOClass);
			AssertEquals(undg2.DI_DGFlashPoint, bookingRequest.GoodsAndEquipmentDetails.First().DangerousGoods.Last().FlashPoint.Value);
			AssertEquals(undg2.DI_IMOClass, bookingRequest.GoodsAndEquipmentDetails.First().DangerousGoods.Last().IMOClass);
		}

		#endregion

		#region Harmonized Code

		public void TestHarmonizedCodes_ShouldHaveNCMPrefix_WhenCountryIsBrazil()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			var harmonizedCode = packLine.HarmonisedCodes.AddNew();
			harmonizedCode.JLH_Code = "123456";
			harmonizedCode.JLH_RN_NKCountry = Core.Constants.CountryCodes.Brazil;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals($"NCM ({Core.Constants.CountryCodes.Brazil}): {harmonizedCode.JLH_Code}", bookingRequest.GoodsAndEquipmentDetails.First().HarmonizedCodes);
		}

		public void TestHarmonizedCodes_ShouldHaveHSPrefix_WhenCountryIsNotBrazil()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			var harmonizedCode = packLine.HarmonisedCodes.AddNew();
			harmonizedCode.JLH_Code = "123456";
			harmonizedCode.JLH_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals($"HS ({Core.Constants.CountryCodes.Australia}): {harmonizedCode.JLH_Code}", bookingRequest.GoodsAndEquipmentDetails.First().HarmonizedCodes);
		}

		public void TestHarmonizedCodes_ShouldHaveHSPrefix_WhenCountryIsEmpty()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			var harmonizedCode = packLine.HarmonisedCodes.AddNew();
			harmonizedCode.JLH_Code = "123456";
			harmonizedCode.JLH_RN_NKCountry = ZString.Empty;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals($"HS: {harmonizedCode.JLH_Code}", bookingRequest.GoodsAndEquipmentDetails.First().HarmonizedCodes);
		}

		public void TestHarmonizedCodes_ShouldHaveHCPrefix_WhenItIsGeneric()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_RN_NKOrigin = "US";
			packLine.JL_HarmonisedCode = "12345678";

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals($"HC: {packLine.JL_HarmonisedCode}", bookingRequest.GoodsAndEquipmentDetails.First().HarmonizedCodes);
		}

		public void TestHarmonizedCodes_ShouldNotHavePrefix_WhenCodeIsEmpty()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			var harmonizedCode = packLine.HarmonisedCodes.AddNew();
			harmonizedCode.JLH_Code = ZString.Empty;
			harmonizedCode.JLH_RN_NKCountry = Core.Constants.CountryCodes.Brazil;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals(ZString.Empty, bookingRequest.GoodsAndEquipmentDetails.First().HarmonizedCodes);
		}

		public void TestHarmonizedCodes_ShouldBeCommaSeparated_WhenThereAreMoreThanOneHarmonizedCode()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();

			packLine.JL_RN_NKOrigin = "US";
			packLine.JL_HarmonisedCode = "12345678";

			var harmonizedCode1 = packLine.HarmonisedCodes.AddNew();
			harmonizedCode1.JLH_Code = "123456";
			harmonizedCode1.JLH_RN_NKCountry = Core.Constants.CountryCodes.Brazil;

			var harmonizedCode2 = packLine.HarmonisedCodes.AddNew();
			harmonizedCode2.JLH_Code = "456321";
			harmonizedCode2.JLH_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var harmonizedCode3 = packLine.HarmonisedCodes.AddNew();
			harmonizedCode3.JLH_Code = "456321";
			harmonizedCode3.JLH_RN_NKCountry = ZString.Empty;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertEquals($"HC: {packLine.JL_HarmonisedCode}, NCM ({harmonizedCode1.JLH_RN_NKCountry}): {harmonizedCode1.JLH_Code}, HS ({harmonizedCode2.JLH_RN_NKCountry}): {harmonizedCode2.JLH_Code}, HS: {harmonizedCode3.JLH_Code}", bookingRequest.GoodsAndEquipmentDetails.First().HarmonizedCodes);
		}

		#endregion

		#region Total Packs

		public void TestTotalWeight_ShouldBeSumOfPackLinesPackageCount()
		{
			var shipment = CreateShipment();

			var packLine = shipment.OuterPackLines.AddNew();
			var firstPack = 12;
			packLine.JL_PackageCount = firstPack;

			var secondPackLine = shipment.OuterPackLines.AddNew();
			var secondPack = 14;
			secondPackLine.JL_PackageCount = secondPack;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(firstPack + secondPack, bookingRequest.TotalPacks);
		}

		#endregion

		#region TotalWeight and TotalVolume

		public void TestTotalWeight_ShouldBeSumOfPackLinesWeight_WhenTheyAreKGs()
		{
			AssertTotalWeightHelper(
				firstPackWeight: 12,
				firstPackUnit: Core.Constants.Weight.Kilograms,
				secondPackWeight: 14,
				secondPackunit: Core.Constants.Weight.Kilograms,
				expectedTotalWeight: new ZDecimal(26),
				expectedTotalUnit: Core.Constants.Weight.Kilograms);
		}

		public void TestTotalWeight_ShouldBeSumOfPackLinesWeight_WhenTheyAreLbs()
		{
			AssertTotalWeightHelper(
				firstPackWeight: 12,
				firstPackUnit: Core.Constants.Weight.Pounds,
				secondPackWeight: 14,
				secondPackunit: Core.Constants.Weight.Pounds,
				expectedTotalWeight: new ZDecimal(26),
				expectedTotalUnit: Core.Constants.Weight.Pounds);
		}

		public void TestTotalWeight_ShouldBeConvertedToPoundsOfPackLinesWeight_WhenAllPackagesAreImperial()
		{
			var convertedToPoundsFirst = Utilities.Round(Core.Constants.Weight.Convert(12, Core.Constants.Weight.OuncesTroy, Core.Constants.Weight.Pounds), 3);
			var convertedToPoundsSecond = Utilities.Round(Core.Constants.Weight.Convert(14, Core.Constants.Weight.PoundsTroy, Core.Constants.Weight.Pounds), 3);

			AssertTotalWeightHelper(
				firstPackWeight: 12,
				firstPackUnit: Core.Constants.Weight.OuncesTroy,
				secondPackWeight: 14,
				secondPackunit: Core.Constants.Weight.PoundsTroy,
				expectedTotalWeight: convertedToPoundsFirst + convertedToPoundsSecond,
				expectedTotalUnit: Core.Constants.Weight.Pounds);
		}

		public void TestTotalWeight_ShouldBeConvertedToKGsOfPackLineWeight_WhenTheyAreNotImperial()
		{
			var convertedToPoundsFirst = Utilities.Round(Core.Constants.Weight.Convert(12, Core.Constants.Weight.Hectograms, Core.Constants.Weight.Kilograms), 3);
			var convertedToPoundsSecond = Utilities.Round(Core.Constants.Weight.Convert(14, Core.Constants.Weight.Milligrams, Core.Constants.Weight.Kilograms), 3);

			AssertTotalWeightHelper(
				firstPackWeight: 12,
				firstPackUnit: Core.Constants.Weight.Hectograms,
				secondPackWeight: 14,
				secondPackunit: Core.Constants.Weight.Milligrams,
				expectedTotalWeight: convertedToPoundsFirst + convertedToPoundsSecond,
				expectedTotalUnit: Core.Constants.Weight.Kilograms);
		}

		public void TestTotalWeight_ShouldBeConvertedToKGOfPackLinesWeight_WhenOneOfPackLinesWeightsIsNotImperial()
		{
			var convertedToPoundsFirst = Utilities.Round(Core.Constants.Weight.Convert(12, Core.Constants.Weight.Hectograms, Core.Constants.Weight.Kilograms), 3);
			var convertedToPoundsSecond = Utilities.Round(Core.Constants.Weight.Convert(14, Core.Constants.Weight.Ounces, Core.Constants.Weight.Kilograms), 3);

			AssertTotalWeightHelper(
				firstPackWeight: 12,
				firstPackUnit: Core.Constants.Weight.Hectograms,
				secondPackWeight: 14,
				secondPackunit: Core.Constants.Weight.Ounces,
				expectedTotalWeight: convertedToPoundsFirst + convertedToPoundsSecond,
				expectedTotalUnit: Core.Constants.Weight.Kilograms);
		}

		public void TestTotalVolume_ShouldBeSumOfPackLinesVolume_WhenAllOfThemAreCubicMetres()
		{
			AssertTotalVolumeHelper(
				firstPackVolume: 12,
				firstPackUnit: Core.Constants.Volume.CubicMetres,
				secondPackVolume: 14,
				secondPackunit: Core.Constants.Volume.CubicMetres,
				expectedTotalVolume: new ZDecimal(26),
				expectedTotalUnit: Core.Constants.Volume.CubicMetres);
		}

		public void TestTotalVolume_ShouldBeSumOfPackLineVolumes_WhenAllOfThemAreCubicFeet()
		{
			AssertTotalVolumeHelper(
				firstPackVolume: 12,
				firstPackUnit: Core.Constants.Volume.CubicFeet,
				secondPackVolume: 14,
				secondPackunit: Core.Constants.Volume.CubicFeet,
				expectedTotalVolume: new ZDecimal(26),
				expectedTotalUnit: Core.Constants.Volume.CubicFeet);
		}

		public void TestTotalVolume_ShouldBeConvertedToCubicFeetOfPackLinesVolume_WhenAllOtherPackagesVolumesAreImperial()
		{
			var convertedToCubicFeetFirst = Utilities.Round(Core.Constants.Volume.Convert(12, Core.Constants.Volume.CubicYards, Core.Constants.Volume.CubicFeet), 3);
			var convertedToCubicFeetSecond = Utilities.Round(Core.Constants.Volume.Convert(14, Core.Constants.Volume.CubicInches, Core.Constants.Volume.CubicFeet), 3);
			AssertTotalVolumeHelper(
				firstPackVolume: 12,
				firstPackUnit: Core.Constants.Volume.CubicYards,
				secondPackVolume: 14,
				secondPackunit: Core.Constants.Volume.CubicInches,
				expectedTotalVolume: convertedToCubicFeetFirst + convertedToCubicFeetSecond,
				expectedTotalUnit: Core.Constants.Volume.CubicFeet);
		}

		public void TestTotalVolume_ShouldBeConvertedToCubicMetresOfPackLinesVolume_WhenTheyAreNotImperial()
		{
			var convertedToCubicFeetFirst = Utilities.Round(Core.Constants.Volume.Convert(12, Core.Constants.Volume.CubicDecimetres, Core.Constants.Volume.CubicMetres), 3);
			var convertedToCubicFeetSecond = Utilities.Round(Core.Constants.Volume.Convert(14, Core.Constants.Volume.CubicCentimeters, Core.Constants.Volume.CubicMetres), 3);
			AssertTotalVolumeHelper(
				firstPackVolume: 12,
				firstPackUnit: Core.Constants.Volume.CubicDecimetres,
				secondPackVolume: 14,
				secondPackunit: Core.Constants.Volume.CubicCentimeters,
				expectedTotalVolume: convertedToCubicFeetFirst + convertedToCubicFeetSecond,
				expectedTotalUnit: Core.Constants.Volume.CubicMetres);
		}

		public void TestTotalVolume_ShouldBeConvertedToCubicFeetOfPackLineVolume_WhenOneOfPackLinesVolumesIsNotImperial()
		{
			var convertedToCubicFeetFirst = Utilities.Round(Core.Constants.Volume.Convert(12, Core.Constants.Volume.CubicInches, Core.Constants.Volume.CubicMetres), 3);
			var convertedToCubicFeetSecond = Utilities.Round(Core.Constants.Volume.Convert(14, Core.Constants.Volume.CubicCentimeters, Core.Constants.Volume.CubicMetres), 3);
			AssertTotalVolumeHelper(
				firstPackVolume: 12,
				firstPackUnit: Core.Constants.Volume.CubicInches,
				secondPackVolume: 14,
				secondPackunit: Core.Constants.Volume.CubicCentimeters,
				expectedTotalVolume: convertedToCubicFeetFirst + convertedToCubicFeetSecond,
				expectedTotalUnit: Core.Constants.Volume.CubicMetres);
		}

		#endregion

		#endregion

		#region Ports Validation

		public void TestPortsInvalidCodeValidation()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			var messageError = "You have not entered a valid un loco.";
			CombineAssertions(() =>
			{
				AssertionHelper.AssertInvalidCodeValidation("PortOfLoading", bookingRequest.PortOfLoading, messageError);
				AssertionHelper.AssertInvalidCodeValidation("PortOfDischarge", bookingRequest.PortOfDischarge, messageError);
				AssertionHelper.AssertInvalidCodeValidation("Origin", bookingRequest.Origin, messageError);
				AssertionHelper.AssertInvalidCodeValidation("Destination", bookingRequest.Destination, messageError);
				AssertionHelper.AssertInvalidCodeValidation("PlaceOfReceipt", bookingRequest.PlaceOfReceipt, messageError);
				AssertionHelper.AssertInvalidCodeValidation("PlaceOfDelivery", bookingRequest.PlaceOfDelivery, messageError);
			});
		}

		public void TestPortsValidation()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			bookingRequest.IsDoorPickup = true;
			bookingRequest.IsDoorDelivery = true;

			AssertionHelper.AssertAsciiCharactersValidation(bookingRequest.PortOfLoading.Code, bookingRequest.PortOfLoading.CodeInfo);
			AssertionHelper.AssertAsciiCharactersValidation(bookingRequest.PortOfDischarge.Code, bookingRequest.PortOfDischarge.CodeInfo);
			AssertionHelper.AssertAsciiCharactersValidation(bookingRequest.Origin.Code, bookingRequest.Origin.CodeInfo);
			AssertionHelper.AssertMessageErrorIfEmpty(bookingRequest.Origin.CodeInfo, "Origin is required.");

			AssertionHelper.AssertAsciiCharactersValidation(bookingRequest.Destination.Code, bookingRequest.Destination.CodeInfo);
			AssertionHelper.AssertMessageErrorIfEmpty(bookingRequest.Destination.CodeInfo, "Destination is required.");

			AssertionHelper.AssertAsciiCharactersValidation(bookingRequest.PlaceOfReceipt.Code, bookingRequest.PlaceOfReceipt.CodeInfo);
			AssertionHelper.AssertMessageErrorIfEmpty(bookingRequest.PlaceOfReceipt.CodeInfo, "Place of Receipt is required.");

			AssertionHelper.AssertAsciiCharactersValidation(bookingRequest.PlaceOfDelivery.Code, bookingRequest.PlaceOfDelivery.CodeInfo);
			AssertionHelper.AssertMessageErrorIfEmpty(bookingRequest.PlaceOfDelivery.CodeInfo, "Place of Delivery is required.");
		}

		public void TestPlaceOfReceipt_ShouldRaiseError_WhenPlaceOfReceiptIsEmptyAndIsDoorPickup()
		{
			var shipment = CreateShipment();
			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			shipment.JS_RL_NKOrigin = ZString.Empty;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNullOrEmpty(bookingRequest.PlaceOfReceipt?.Code);

			bookingRequest.IsDoorPickup = true;
			AssertionHelper.AssertMessageErrorIfEmpty(bookingRequest.PlaceOfReceipt.CodeInfo, "Place of Receipt is required.");
		}

		public void TestPlaceOfDelivery_ShouldRaiseError_WhenPlaceOfDeliveryIsEmptyAndIsDoorDelivery()
		{
			var shipment = CreateShipment();
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			shipment.JS_RL_NKDestination = ZString.Empty;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNullOrEmpty(bookingRequest.PlaceOfDelivery?.Code);

			bookingRequest.IsDoorDelivery = true;
			AssertionHelper.AssertMessageErrorIfEmpty(bookingRequest.PlaceOfDelivery.CodeInfo, "Place of Delivery is required.");
		}

		#endregion

		#region Addresses Validation

		public void TestPartiesCompanyNameMaxLengthValidation()
		{
			var shipment = CreateShipment();

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			CombineAssertions(() =>
			{
				AssertionHelper.AssertAddressCompanyNameLength("Shipper", bookingRequest.Shipper);
				AssertionHelper.AssertAddressCompanyNameLength("PickupFrom", bookingRequest.PickupFrom);
				AssertionHelper.AssertAddressCompanyNameLength("DeliverTo", bookingRequest.DeliverTo);
			});
		}

		public void TestPartiesContactNameIsValid()
		{
			var shipment = CreateShipment();

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			CombineAssertions(() =>
			{
				AssertionHelper.AssertAddressContactNameValid("Shipper", bookingRequest.Shipper);
				AssertionHelper.AssertAddressContactNameValid("PickupFrom", bookingRequest.PickupFrom);
				AssertionHelper.AssertAddressContactNameValid("DeliverTo", bookingRequest.DeliverTo);
			});
		}

		#endregion

		#region Goods and Equipment Details Validation

		public void TestGoodsDescription_ShouldRaiseError_WhenEmpty()
		{
			var shipment = CreateShipment();
			shipment.OuterPackLines.AddNew();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertionHelper.AssertMessageErrorIfEmpty(bookingRequest.GoodsAndEquipmentDetails.First().GoodsDescriptionInfo, "Goods Description is required");
		}

		public void TestPacks_ShouldRaiseError_WhenEmpty()
		{
			var shipment = CreateShipment();
			shipment.OuterPackLines.AddNew();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertionHelper.AssertMessageErrorIfEmpty(bookingRequest.GoodsAndEquipmentDetails.First().PacksInfo, "Pack/Pack type is required");
		}

		public void TestPacks_ShouldRaiseError_WhenPacksNumberIsZero()
		{
			var errorMessage = "Package count is required";
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 0;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertHasMessageError(bookingRequest.GoodsAndEquipmentDetails.First().PacksInfo, errorMessage);

			packLine.JL_PackageCount = 65;

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNoMessageError(bookingRequest.GoodsAndEquipmentDetails.First().PacksInfo, errorMessage);
		}

		public void TestPacks_ShouldRaiseError_WhenPackTypeIsEmpty()
		{
			var errorMessage = "Package type is required";
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;
			packLine.JL_F3_NKPackType = ZString.Empty;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertHasMessageError(bookingRequest.GoodsAndEquipmentDetails.First().PacksInfo, errorMessage);

			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNoMessageError(bookingRequest.GoodsAndEquipmentDetails.First().PacksInfo, errorMessage);
		}

		public void TestCargoWeight_ShouldRaiseError_WhenEmpty()
		{
			var shipment = CreateShipment();
			shipment.OuterPackLines.AddNew();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertMessageErrorIfZero(bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.ValueInfo, "Weight is required");
		}

		public void TestCargoWeight_ShouldRaiseError_WhenWeightIsZero()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_ActualWeight = 0;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertMessageErrorIfZero(bookingRequest.GoodsAndEquipmentDetails.First().CargoWeight.ValueInfo, "Weight is required");
		}

		public void TestPackingLinesValidation_DangerousGoods_Substance()
		{
			var errorMessage = "DG Class, UNDG and Proper Shipping Name are required for dangerous goods.\r\nPlease enter Shipment > Packing > Pack Lines > Dangerous Goods > DG Substance.";

			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_ActualWeight = 0;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "";
			contact.OC_Phone = "";

			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_PSN = "CLASPSN";

			var undg = packLine.UNDGs.AddNew();
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DG = undgSubstance.PK;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			DangerousGood GetDangerousGood()
			{
				return bookingRequest.GoodsAndEquipmentDetails.Single()
					.DangerousGoods.Single();
			}
			var dangerousGood = GetDangerousGood();

			Assert(!dangerousGood.Validator().Any());

			undg.DI_IMOClass = ZString.Empty;

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			dangerousGood = GetDangerousGood();
			Assert(!dangerousGood.Validator().Any());

			undgSubstance.DG_PSN = ZString.Empty;

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undgSubstance.DG_PSN = "CLASPSN";
			undgSubstance.DG_Class = ZString.Empty;

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_Code = ZString.Empty;

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undg.DI_DG = ZGuid.Empty;
			undg.DI_IMOClass = "CLAS";

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));
		}

		public void TestCargoVolume_ShouldRaiseError_WhenEmpty()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolume = 12;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertMessageErrorIfZero(bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.ValueInfo, "Volume is required");
		}

		public void TestCargoVolume_ShouldRaiseError_WhenVolumeIsZero()
		{
			var shipment = CreateShipment();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_ActualWeight = 27;
			packLine.JL_ActualVolume = 0;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(1, bookingRequest.GoodsAndEquipmentDetails.Count);
			AssertMessageErrorIfZero(bookingRequest.GoodsAndEquipmentDetails.First().CargoVolume.ValueInfo, "Volume is required");
		}

		static void AssertMessageErrorIfZero(ZPropertyInfo info, string expectedErrorMessage)
		{
			var originalValue = info.Value;
			info.Value = new ZDecimal(12);
			AssertNoMessageError(info, expectedErrorMessage);

			info.Value = ZDecimal.Zero;
			AssertHasMessageError(info, expectedErrorMessage);

			info.Value = originalValue;
		}

		#endregion

		#region Other Properties Validation

		public void TestCarrierBookingReferenceValidation()
		{
			var warning = @"Carrier Booking Request Number is populated during booking confirmation.
If you need to send Carrier Booking Number at the time of a Booking Request, please ensure this is the number pre-assigned by the carrier in advance.";

			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters
			{
				LogProvider = Factory.New<DummyEnterpriseBusinessObject>()
			};
			parameters.LogProvider?.Logs.RemoveAndDeleteAll();

			var bookingRequest = CreateDocDataObjectBuilder(shipment, parameters).Build();

			bookingRequest.BookingReference = string.Empty;
			AssertNoWarning(bookingRequest.BookingReferenceInfo, warning);

			bookingRequest.BookingReference = null;
			AssertNoWarning(bookingRequest.BookingReferenceInfo, warning);

			bookingRequest.BookingReference = "123456";
			AssertHasWarning(bookingRequest.BookingReferenceInfo, warning);

			bookingRequest.BookingReference = "123456, 789";
			AssertHasWarning(bookingRequest.BookingReferenceInfo, warning);

			CreateEventLog(parameters.LogProvider, Events.MessageSent, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "Booking Request"));
			bookingRequest = CreateDocDataObjectBuilder(shipment, parameters).Build();

			bookingRequest.BookingReference = "123456";
			AssertNoWarning(bookingRequest.BookingReferenceInfo, warning);

			CreateEventLog(parameters.LogProvider, Events.StatusUpdated, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "Booking Request"));
			bookingRequest = CreateDocDataObjectBuilder(shipment, parameters).Build();

			bookingRequest.BookingReference = "123456";
			AssertHasWarning(bookingRequest.BookingReferenceInfo, warning);

			bookingRequest.BookingReference = string.Empty;
			AssertNoWarning(bookingRequest.BookingReferenceInfo, warning);
		}

		void CreateEventLog(IStmALogProvider logProvider, Event @event, params KeyValuePair<string, string>[] parameters)
		{
			logProvider?.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, parameters);

			Thread.Sleep(1);
			Factory.Save();
		}

		public void TestShipperReferenceValidation()
		{
			var errorMessage = "Shipper reference is required.";

			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			bookingRequest.ShipperReference = string.Empty;
			AssertHasMessageError(bookingRequest.ShipperReferenceInfo, errorMessage);

			bookingRequest.ShipperReference = null;
			AssertHasMessageError(bookingRequest.ShipperReferenceInfo, errorMessage);

			bookingRequest.ShipperReference = "123456";
			AssertNoMessageError(bookingRequest.ShipperReferenceInfo, errorMessage);
		}

		public void TestModeValidation()
		{
			var errorMessage = "Mode (Container Mode) is required.";

			var shipment = CreateShipment();

			shipment.JS_PackingMode = ZString.Empty;
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertHasMessageError(bookingRequest.ContainerMode.CodeInfo, errorMessage);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNoMessageError(bookingRequest.ContainerMode.CodeInfo, errorMessage);
		}

		public void TestEarliestDepartureDateValidation()
		{
			var errorMessage = "Earliest Departure Date (ETD) is required.";

			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			bookingRequest.EarliestDepartureDate = ZDateTime.Empty;
			AssertHasMessageError(bookingRequest.EarliestDepartureDateInfo, errorMessage);

			bookingRequest.EarliestDepartureDate = ZDateTime.Today;
			AssertNoMessageError(bookingRequest.EarliestDepartureDateInfo, errorMessage);
		}

		public void TestLatestDeliveryDateValidation()
		{
			var errorMessage = "Latest Delivery Date (ETA) is required.";

			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			bookingRequest.LatestDeliveryDate = ZDateTime.Empty;
			AssertHasMessageError(bookingRequest.LatestDeliveryDateInfo, errorMessage);

			bookingRequest.LatestDeliveryDate = ZDateTime.Today;
			AssertNoMessageError(bookingRequest.LatestDeliveryDateInfo, errorMessage);
		}

		public void TestETAAndETDMandatory()
		{
			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			bookingRequest.ETD = ZDateTime.Empty;
			bookingRequest.ETA = ZDateTime.Empty;
			AssertHasMessageError(bookingRequest.ETDInfo, "ETD is required.");
			AssertHasMessageError(bookingRequest.ETAInfo, "ETA is required.");

			bookingRequest.ETD = ZDateTime.Today;
			bookingRequest.ETA = ZDateTime.Today;
			AssertNoMessageError(bookingRequest.ETDInfo, "ETD is required.");
			AssertNoMessageError(bookingRequest.ETAInfo, "ETA is required.");
		}

		public void TestEstCargoPickupDateTimeValidation()
		{
			var errorMessage = "Est. Cargo Pickup Date Time (Pickup > Pickup Required By) is required.";

			var shipment = CreateShipment();
			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			bookingRequest.EstCargoPickupDateTime = ZDateTime.Empty;
			AssertNoMessageError(bookingRequest.EarliestDepartureDateInfo, errorMessage);

			bookingRequest.IsDoorPickup = true;
			bookingRequest.EstCargoPickupDateTime = ZDateTime.Empty;
			AssertHasMessageError(bookingRequest.EstCargoPickupDateTimeInfo, errorMessage);

			bookingRequest.EstCargoPickupDateTime = ZDateTime.Today;
			AssertNoMessageError(bookingRequest.EarliestDepartureDateInfo, errorMessage);
		}

		public void TestTotalPackValidation()
		{
			var errorMessage = "Total Packs cannot be zero.";

			var shipment = CreateShipment();

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertHasMessageError(bookingRequest.TotalPacksInfo, errorMessage);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 65;
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNoMessageError(bookingRequest.TotalPacksInfo, errorMessage);
		}

		public void TestTotalWeightValidation()
		{
			var errorMessage = "Total cargo weight cannot be zero.";

			var shipment = CreateShipment();

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertHasMessageError(bookingRequest.TotalCargoWeight.ValueInfo, errorMessage);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeight = 65;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Hectograms;

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNoMessageError(bookingRequest.TotalCargoWeight.ValueInfo, errorMessage);
		}

		public void TestTotalVolumeValidation()
		{
			var errorMessage = "Total cargo volume cannot be zero.";

			var shipment = CreateShipment();

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertHasMessageError(bookingRequest.TotalCargoVolume.ValueInfo, errorMessage);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolume = 65;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;

			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNoMessageError(bookingRequest.TotalCargoVolume.ValueInfo, errorMessage);
		}

		#endregion

		#region TestValidationOnMessageCanOnlyBeSentFromConsolOrShipment

		public void TestConsolBRMessageHasBeenSentAndNoWithdrawalAcceptedOrResetToOriginalValidation()
		{
			var errorMessage = "A Booking Request already been sent from Consol. A Booking Request can only be sent from Consol or Shipment!";

			var shipment = CreateShipment();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertNoMessageError(bookingRequest.ErrorPlaceHolderInfo, errorMessage);

			CreateEvent(consol, Events.MessageSent);
			CreateEvent(consol, Events.MessageAccepted);
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertHasMessageError("Consol has sent the BR message", bookingRequest.ErrorPlaceHolderInfo, errorMessage);

			CreateEvent(consol, Events.MessageWithdrawCancelRequest);
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertHasMessageError("Consol has sent the MWR message but no MWA received", bookingRequest.ErrorPlaceHolderInfo, errorMessage);

			CreateEvent(consol, Events.MessageWithdrawCancelAccepted);
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertNoMessageError("Consol received MWA", bookingRequest.ErrorPlaceHolderInfo, errorMessage);

			CreateEvent(consol, Events.MessageSent);
			CreateEvent(consol, Events.StatusUpdated);
			bookingRequest = CreateDocDataObjectBuilder(shipment).Build();
			AssertNoMessageError("Reset to original", bookingRequest.ErrorPlaceHolderInfo, errorMessage);
		}

		void CreateEvent(ForwardingConsol consol, Event @event)
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ConsolDocumentNames.BookingRequest));

			consol.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters.ToArray());

			Factory.Save();
			Thread.Sleep(1);
		}

		public static void AssertAddressData(OrgAddress orgAddress, IAddress addressDataObject)
		{
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", orgAddress.CompanyName, addressDataObject.CompanyName);
				AssertEquals("AddressLine1", orgAddress.Address1, addressDataObject.AddressLine1);
				AssertEquals("AddressLine2", orgAddress.Address2, addressDataObject.AddressLine2);
				AssertEquals("AdditionalAddressInformation", orgAddress.UnrestrictedAdditionalAddressInformation, addressDataObject.AdditionalAddressInformation);
				AssertEquals("City", orgAddress.City, addressDataObject.City);
				AssertEquals("State", orgAddress.StateCode, addressDataObject.State);
				AssertEquals("Postcode", orgAddress.Postcode, addressDataObject.Postcode);
				AssertEquals("Country.Code", orgAddress.OA_RN_NKCountryCode, addressDataObject.Country.Code);
			});
		}

		#endregion

		#region Implementation

		SeaShipmentBookingRequestBuilder CreateDocDataObjectBuilder(ForwardingShipment shipment)
		{
			var parameters = new DummyDocDataObjectParameters()
			{
				LogProvider = Factory.New<DummyEnterpriseBusinessObject>()
			};
			return new SeaShipmentBookingRequestBuilder(shipment, parameters);
		}

		SeaShipmentBookingRequestBuilder CreateDocDataObjectBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters)
		{
			return new SeaShipmentBookingRequestBuilder(shipment, parameters);
		}

		ForwardingShipment CreateShipment(bool isStandAloneShipment = true, string transportMode = "Air")
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_UniqueConsignRef = "S00001000";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "SGSIN";

			shipment.JS_E_DEP = ZDateTime.UtcToday.AddDays(2);
			shipment.JS_E_ARV = ZDateTime.UtcToday.AddDays(4);

			shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.UtcToday.AddDays(6);
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			shipment.JS_AdditionalTerms = "Follow the white rabbit :)";

			var note = "simple note";
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, note);

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "MAERSK";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.Address1 = "Unit 13";
			consignor.MainAddress.Address2 = "4 Lost Lane";
			consignor.MainAddress.City = "Sydney";
			consignor.MainAddress.Postcode = "2000";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "MR Consignee";
			consignee.OH_RL_NKClosestPort = "SGSIN";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var pickupCfs = Factory.New<OrgHeader>();
			pickupCfs.OH_FullName = "PickupFromCo";
			pickupCfs.OH_RL_NKClosestPort = "AUSYD";
			pickupCfs.MainAddress.Address1 = "Unit 13";
			pickupCfs.MainAddress.Address2 = "4 Lost Lane";
			pickupCfs.MainAddress.City = "Sydney";
			pickupCfs.MainAddress.Postcode = "2000";
			pickupCfs.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.JS_OA_ExportReceivingDepot = pickupCfs.MainAddress.PK;

			var pickupFrom = Factory.New<OrgHeader>();
			pickupFrom.OH_FullName = "PickupFromCo";
			pickupFrom.OH_RL_NKClosestPort = "AUSYD";
			pickupFrom.MainAddress.Address1 = "Unit 13";
			pickupFrom.MainAddress.Address2 = "4 Lost Lane";
			pickupFrom.MainAddress.City = "Sydney";
			pickupFrom.MainAddress.Postcode = "2000";
			pickupFrom.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorPickupAddress.E2_OA_Address = pickupFrom.MainAddress.PK;

			var deliverToCFS = Factory.New<OrgHeader>();
			deliverToCFS.OH_FullName = "DeliverCFSCo";
			deliverToCFS.OH_RL_NKClosestPort = "SGSIN";
			deliverToCFS.MainAddress.Address1 = "Unit 1";
			deliverToCFS.MainAddress.Address2 = "4 What Lane";
			deliverToCFS.MainAddress.City = "Auckland";
			deliverToCFS.MainAddress.Postcode = "5022";
			deliverToCFS.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.JS_OA_ImportReleaseDepot = deliverToCFS.MainAddress.PK;

			var deliverTo = Factory.New<OrgHeader>();
			deliverTo.OH_FullName = "DeliverToCo";
			deliverTo.OH_RL_NKClosestPort = "SGSIN";
			deliverTo.MainAddress.Address1 = "Unit 1";
			deliverTo.MainAddress.Address2 = "4 What Lane";
			deliverTo.MainAddress.City = "Auckland";
			deliverTo.MainAddress.Postcode = "5022";
			deliverTo.MainAddress.OA_RN_NKCountryCode = "SG";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliverTo.MainAddress.PK;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MR Carrier";
			carrier.OH_RL_NKClosestPort = "CNSHA";
			carrier.MainAddress.Address1 = "Unit 1";
			carrier.MainAddress.Address2 = "4 What Lane";
			carrier.MainAddress.City = "Shanghay";
			carrier.MainAddress.Postcode = "5022";
			carrier.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			var bookingReference = Factory.New<CusEntryNumber>();
			bookingReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			bookingReference.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			bookingReference.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			bookingReference.CE_EntryNum = "10207000067891";
			shipment.Numbers.Add(bookingReference);

			var carrierContractNumber = Factory.New<CusEntryNumber>();
			carrierContractNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			carrierContractNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			carrierContractNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			carrierContractNumber.CE_EntryNum = "4350006342";
			shipment.Numbers.Add(carrierContractNumber);

			if (!isStandAloneShipment)
			{
				var consol = CreateConsol(transportMode);
				shipment.Consols.Add(consol);
			}

			return shipment;
		}

		ForwardingConsol CreateConsol(string transportMode = "Air", string receivingAgentName = "ReceivingAgent")
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			PopulateConsolAddresses(consol, receivingAgentName);
			return consol;
		}

		void PopulateConsolAddresses(ForwardingConsol consol, string receivingAgentName = "ReceivingAgent")
		{
			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "1234567890", "CN");

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_OH = sendingForwarder.PK;
			sendingForwarderContact.OC_ContactName = "Sender Name";
			sendingForwarderContact.OC_Email = "name@sender.com";
			sendingForwarderContact.OC_Phone = "1111111";
			sendingForwarderContact.OC_Fax = "2222222";

			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";
			shippingLine.RSL_IsNVO = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "ABC";
			creditor.OH_RL_NKClosestPort = "CNNJI";
			creditor.OH_RSL_ShippingLine = shippingLine.PK;

			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var packDepotOrg = Factory.New<OrgHeader>();
			packDepotOrg.OH_FullName = "pack depot org";
			packDepotOrg.OH_RL_NKClosestPort = "CNCAN";
			packDepotOrg.MainAddress.Address1 = "Unit 888";
			packDepotOrg.MainAddress.Address2 = "111 Drive";
			packDepotOrg.MainAddress.City = "unknown city";
			packDepotOrg.MainAddress.Postcode = "4679";
			packDepotOrg.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_PackDepotAddress = packDepotOrg.MainAddress.PK;

			var unpackDepotOrg = Factory.New<OrgHeader>();
			unpackDepotOrg.OH_FullName = "unpack depot org";
			unpackDepotOrg.OH_RL_NKClosestPort = "SGSIN";
			unpackDepotOrg.MainAddress.Address1 = "Unit 589";
			unpackDepotOrg.MainAddress.Address2 = "625 Drive";
			unpackDepotOrg.MainAddress.City = "unknown city";
			unpackDepotOrg.MainAddress.Postcode = "9541";
			unpackDepotOrg.MainAddress.OA_RN_NKCountryCode = "SG";

			consol.JK_OA_UnpackDepotAddress = unpackDepotOrg.MainAddress.PK;

			var receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_FullName = receivingAgentName;
			receivingAgent.OH_RL_NKClosestPort = "BEANR";
			receivingAgent.MainAddress.CompanyName = receivingAgentName;
			receivingAgent.MainAddress.Address1 = "Unit 589";
			receivingAgent.MainAddress.Address2 = "625 Drive";
			receivingAgent.MainAddress.City = "unknown city";
			receivingAgent.MainAddress.Postcode = "9541";
			receivingAgent.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_OH = receivingAgent.PK;
			receivingForwarderContact.OC_ContactName = "Receiver Name";
			receivingForwarderContact.OC_Email = "name@receiver.com";
			receivingForwarderContact.OC_Phone = "3333333";
			receivingForwarderContact.OC_Fax = "4444444";

			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;
		}

		void AssertTotalWeightHelper(ZDecimal firstPackWeight, ZString firstPackUnit, ZDecimal secondPackWeight, ZString secondPackunit, ZDecimal expectedTotalWeight, ZString expectedTotalUnit)
		{
			var shipment = CreateShipment();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeight = firstPackWeight;
			packLine.JL_ActualWeightUQ = firstPackUnit;

			var secondPackLine = shipment.OuterPackLines.AddNew();
			secondPackLine.JL_ActualWeight = secondPackWeight;
			secondPackLine.JL_ActualWeightUQ = secondPackunit;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(expectedTotalWeight, bookingRequest.TotalCargoWeight.Value);
			AssertEquals(expectedTotalUnit, bookingRequest.TotalCargoWeight.Unit.Code);
		}

		void AssertTotalVolumeHelper(ZDecimal firstPackVolume, ZString firstPackUnit, ZDecimal secondPackVolume, ZString secondPackunit, ZDecimal expectedTotalVolume, ZString expectedTotalUnit)
		{
			var shipment = CreateShipment();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolume = firstPackVolume;
			packLine.JL_ActualVolumeUQ = firstPackUnit;

			var secondPackLine = shipment.OuterPackLines.AddNew();
			secondPackLine.JL_ActualVolume = secondPackVolume;
			secondPackLine.JL_ActualVolumeUQ = secondPackunit;

			var bookingRequest = CreateDocDataObjectBuilder(shipment).Build();

			AssertEquals(expectedTotalVolume, bookingRequest.TotalCargoVolume.Value);
			AssertEquals(expectedTotalUnit, bookingRequest.TotalCargoVolume.Unit.Code);
		}

		#endregion
	}
}
