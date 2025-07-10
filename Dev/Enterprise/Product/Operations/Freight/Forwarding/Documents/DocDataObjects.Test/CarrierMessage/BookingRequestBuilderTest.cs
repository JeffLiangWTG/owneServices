using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class BookingRequestBuilderTest : CarrierMessageDataBuilderTest
	{
		#region TestPopulateIsOutOfGauge

		public void TestPopulateHasOverhangDimensionContainersAndIsOutOfGauge()
		{
			var consol = CreateConsol();
			consol.Containers[0].JC_OverhangBack = 1d;

			Assert(CreateDocDataObjectBuilder(consol).Build().HasOverhangDimensionContainers);
			Assert(CreateDocDataObjectBuilder(consol).Build().IsOutOfGauge);

			consol.Containers.RemoveAll();
			Assert(!CreateDocDataObjectBuilder(consol).Build().HasOverhangDimensionContainers);
			Assert(!CreateDocDataObjectBuilder(consol).Build().IsOutOfGauge);
		}

		#endregion

		#region Validation

		public void TestContainerNumberValidation()
		{
			var messageError = "There are no packs in this container. Please pack this container.";

			var consol = CreateConsol();
			consol.Containers.RemoveAll();
			var containerBO = consol.Containers.AddNew();
			containerBO.PackLines.RemoveAndDeleteAll();

			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var data = CreateDocDataObjectBuilder(consol).Build();
			var containerDO = data.Containers.Cast<Container>().First();
			AssertHasMessageError(@"ONLY when 
									1.agent type is direct 
									2.consol is containerized 
									3. container is NOT empty 
									4. there is no packlines related to this container
								 should have this message error ", containerDO.NumberInfo, messageError);

			void RefreshContainer()
			{
				data = CreateDocDataObjectBuilder(consol).Build();
				containerDO = data.Containers.Cast<Container>().First();
			}

			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			RefreshContainer();
			AssertNoMessageError(containerDO.NumberInfo, messageError);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.Agent;
			RefreshContainer();
			AssertNoMessageError(containerDO.NumberInfo, messageError);

			consol.JK_AgentType = Constants.AgentType.Direct;
			var packline = consol.Shipments.Cast<CommonShipment>().First().OuterPackLines.AddNew();
			containerBO.PackLines.Add(packline);
			AssertNoMessageError(containerDO.NumberInfo, messageError);

			containerBO.JC_IsEmptyContainer = true;
			containerBO.PackLines.RemoveAll();
			RefreshContainer();
			AssertNoMessageError(containerDO.NumberInfo, messageError);
		}

		public void PackingLinesValidation()
		{
			var requireContainerAndPackingLinesMessageError = "Container and Packing Lines details are required for Booking Request and Amendment messages.";
			var maximumPackingLinesCountMessageError = "Maximum 999 packlines can be included in a Booking Request message.";
			var requirePackingLinesWhenNonContainerizedMessageError = "Packing Lines details are required for Booking Request and Amendment messages.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var shipment = consol.Shipments.AddNew();

			void Insert1000PackingLines()
			{
				shipment.OuterPackLines.RemoveAndDeleteAll();
				for (int i = 0; i < 1000; i++)
				{
					shipment.OuterPackLines.AddNew();
				}
			}

			var data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(data.ErrorPlaceHolderInfo, requireContainerAndPackingLinesMessageError);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(data.ErrorPlaceHolderInfo, requireContainerAndPackingLinesMessageError);

			shipment.OuterPackLines.AddNew();
			data = CreateDocDataObjectBuilder(consol).Build();

			AssertNoMessageError(data.ErrorPlaceHolderInfo, requireContainerAndPackingLinesMessageError);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, maximumPackingLinesCountMessageError);

			Insert1000PackingLines();
			AssertHasMessageError(data.ErrorPlaceHolderInfo, maximumPackingLinesCountMessageError);

			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			shipment.OuterPackLines.RemoveAndDeleteAll();

			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(data.ErrorPlaceHolderInfo, requirePackingLinesWhenNonContainerizedMessageError);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, maximumPackingLinesCountMessageError);

			shipment.OuterPackLines.AddNew();
			data = CreateDocDataObjectBuilder(consol).Build();

			AssertNoMessageError(data.ErrorPlaceHolderInfo, requirePackingLinesWhenNonContainerizedMessageError);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, maximumPackingLinesCountMessageError);

			Insert1000PackingLines();
			AssertHasMessageError(data.ErrorPlaceHolderInfo, maximumPackingLinesCountMessageError);
		}

		public void TestContainerHasNoShipmentsValidation()
		{
			var warningMessage1 = "There are no shipments allocated to the container/s.\r\nIf you are to proceed with this Booking Request, 1 unit of Freight of All Kind will be reported to the carriers.\r\nBooking Request replacement message can be sent later if required, when correct values are known.";
			var warningMessage2 = "The value entered here will be lost, once shipments are packed to this container.\r\nBooking Request replacement message can be sent later if required, when correct value is known.";

			var consol = CreateConsol();
			Assert("precondition", consol.Shipments.Cast<CommonShipment>().Any() || consol.IsDirect);

			var data = CreateDocDataObjectBuilder(consol).Build();

			AssertNoWarning(data.Containers.First().PackCountInfo, warningMessage1);
			AssertNoWarning(data.Containers.First().GoodsWeight.ValueInfo, warningMessage2);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.Shipments.RemoveAndDeleteAll();

			data = CreateDocDataObjectBuilder(consol).Build();

			AssertHasWarning(data.Containers.First().PackCountInfo, warningMessage1);
			AssertHasWarning(data.Containers.First().GoodsWeight.ValueInfo, warningMessage2);

			var warningMessage3 = "There are no shipments allocated to the container/s.\r\nIf you are to proceed with this Booking Request, 1 unit of GENERAL will be reported to the carriers.\r\nBooking Request replacement message can be sent later if required, when correct values are known.";
			consol.Containers[0].JC_RH_NKContainerCommodityCode = "GEN";

			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasWarning(data.Containers.First().PackCountInfo, warningMessage3);
		}

		public void TestContainerHasNoShipmentsValidation_ZeroContainerNetWeight()
		{
			var errorMessage = "Total cargo weight (net weight) is required. Please enter a value.";
			var consol = CreateConsol();
			Assert("precondition", consol.Shipments.Cast<CommonShipment>().Any() || consol.IsDirect);

			var data = CreateDocDataObjectBuilder(consol).Build();

			AssertNoError(data.Containers.First().GoodsWeight.ValueInfo, errorMessage);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.Shipments.RemoveAndDeleteAll();

			data = CreateDocDataObjectBuilder(consol).Build();
			var goodsWeight = data.Containers.First().GoodsWeight;

			AssertEquals(goodsWeight.Value, ZDecimal.Zero);
			AssertHasMessageError(goodsWeight.ValueInfo, errorMessage);

			goodsWeight.Value = 1;
			AssertNoMessageErrors(goodsWeight.ValueInfo);
		}

		public void TestVoyageValidation()
		{
			var cantSendMessageError = @"The Booking Request can only be sent if the following data is entered:
1. Place of Receipt and Earliest Departure; OR 
2. Place of Delivery  and Latest Delivery; OR
3. Vessel Name (or Lloyds Code) and Voyage Number.";

			var consol = CreateConsol();
			var bookingRequestData = CreateDocDataObjectBuilder(consol).Build();

			AssertNoMessageError(bookingRequestData.ErrorPlaceHolderInfo, cantSendMessageError);

			bookingRequestData.Transports.Main.Vessel.Name = ZString.Empty;
			bookingRequestData.Transports.Main.Vessel.LloydsIMO = ZString.Empty;
			bookingRequestData.Transports.Main.VoyageFlightNumber = ZString.Empty;
			bookingRequestData.PlaceOfReceipt.Code = ZString.Empty;
			bookingRequestData.EarliestDepartureDate = ZDateTime.Empty;
			bookingRequestData.PlaceOfDelivery.Code = ZString.Empty;
			bookingRequestData.LatestDeliveryDate = ZDateTime.Empty;

			AssertHasMessageError("All informations are missing", bookingRequestData.ErrorPlaceHolderInfo, cantSendMessageError);

			bookingRequestData.Transports.Main.Vessel.LloydsIMO = "111";
			bookingRequestData.Transports.Main.VoyageFlightNumber = "K8050";
			AssertNoMessageError("vaoyage info is valid", bookingRequestData.ErrorPlaceHolderInfo, cantSendMessageError);

			bookingRequestData.Transports.Main.Vessel.LloydsIMO = ZString.Empty;
			bookingRequestData.Transports.Main.Vessel.Name = "AAA";
			bookingRequestData.Transports.Main.VoyageFlightNumber = "K8050";
			AssertNoMessageError("vaoyage info is valid", bookingRequestData.ErrorPlaceHolderInfo, cantSendMessageError);

			bookingRequestData.Transports.Main.Vessel.Name = string.Empty;
			bookingRequestData.Transports.Main.VoyageFlightNumber = string.Empty;

			bookingRequestData.PlaceOfReceipt.Code = "CNSHA";
			AssertHasMessageError("Place Of Receipt info is invalid", bookingRequestData.ErrorPlaceHolderInfo, cantSendMessageError);

			bookingRequestData.PlaceOfReceipt.Code = ZString.Empty;
			bookingRequestData.EarliestDepartureDate = ZDateTime.Now;
			AssertHasMessageError("Place Of Receipt info is invalid", bookingRequestData.ErrorPlaceHolderInfo, cantSendMessageError);

			bookingRequestData.PlaceOfReceipt.Code = "CNSHA";
			AssertNoMessageError("Place Of Receipt info is valid", bookingRequestData.ErrorPlaceHolderInfo, cantSendMessageError);

			bookingRequestData.PlaceOfReceipt.Code = ZString.Empty;
			bookingRequestData.EarliestDepartureDate = ZDateTime.Empty;

			bookingRequestData.PlaceOfDelivery.Code = "CNSHA";
			AssertHasMessageError("Place Of Delivery info is invalid", bookingRequestData.ErrorPlaceHolderInfo, cantSendMessageError);

			bookingRequestData.PlaceOfDelivery.Code = ZString.Empty;
			bookingRequestData.LatestDeliveryDate = ZDateTime.Now;
			AssertHasMessageError("Place Of Delivery info is invalid", bookingRequestData.ErrorPlaceHolderInfo, cantSendMessageError);

			bookingRequestData.PlaceOfDelivery.Code = "CNSHA";
			AssertNoMessageError("Place Of Delivery info is valid", bookingRequestData.ErrorPlaceHolderInfo, cantSendMessageError);
		}

		public void TestCarrierBookingReferencesValidation()
		{
			var warning = @"Carrier Booking Request Number is populated during booking confirmation.
If you need to send Carrier Booking Number at the time of a Booking Request, please ensure this is the number pre-assigned by the carrier in advance.";

			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters();

			var bookingRequestData = CreateDocDataObjectBuilder(consol, parameters).Build();
			AssertHasWarning(bookingRequestData.BookingReferenceInfo, warning);

			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();

			parameters.LogProvider?.Logs.RemoveAndDeleteAll();
			bookingRequestData = CreateDocDataObjectBuilder(consol, parameters).Build();
			AssertHasWarning(bookingRequestData.BookingReferenceInfo, warning);

			CreateBookingRequestLog(parameters.LogProvider, Events.MessageSent);
			bookingRequestData = CreateDocDataObjectBuilder(consol, parameters).Build();
			AssertNoWarning(bookingRequestData.BookingReferenceInfo, warning);

			CreateBookingRequestLog(parameters.LogProvider, Events.StatusUpdated);
			bookingRequestData = CreateDocDataObjectBuilder(consol, parameters).Build();
			AssertHasWarning(bookingRequestData.BookingReferenceInfo, warning);

			bookingRequestData.BookingReference = string.Empty;
			AssertNoWarning(bookingRequestData.BookingReferenceInfo, warning);
		}

		public void TestCarrierBookingOfficeValidation_Build()
		{
			var warning = "Carrier booking office does not match to UNLOCO or country code of Place of Receipt/Port of Loading.\r\nPlease provide a valid Carrier Booking Office to avoid booking rejection by carrier.";

			var consol = CreateConsol();
			var firstSeaPort = consol.Transports.Cast<Freight.Business.Transport>().First(t => t.JW_TransportMode == Constants.TransportModes.Sea);

			consol.JK_RL_NKCarrierBookingOffice = "AUSYD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			firstSeaPort.JW_RL_NKLoadPort = "CNSHA";
			var bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUSYD", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			consol.JK_RL_NKLoadPort = "AUBNE";
			bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUBNE", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			consol.JK_RL_NKLoadPort = "DEHAM";
			bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", bookingRequestData.PortOfLoading.Code);
			AssertHasWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			firstSeaPort.JW_RL_NKLoadPort = "AUSYD";
			bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUSYD", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			firstSeaPort.JW_RL_NKLoadPort = "AUBNE";
			bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUBNE", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			consol.JK_RL_NKCarrierBookingOffice = "";
			firstSeaPort.JW_RL_NKLoadPort = "CNSHA";
			bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			consol.JK_RL_NKCarrierBookingOffice = "AUSYD";
			consol.JK_RL_NKLoadPort = "AUBNE";
			firstSeaPort.JW_RL_NKLoadPort = "AUMEL";
			bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUBNE", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);
		}

		public void TestCarrierBookingOfficeValidation_OnValueChanged()
		{
			var warning = "Carrier booking office does not match to UNLOCO or country code of Place of Receipt/Port of Loading.\r\nPlease provide a valid Carrier Booking Office to avoid booking rejection by carrier.";

			var consol = CreateConsol();
			var firstSeaPort = consol.Transports.Cast<Freight.Business.Transport>().First(t => t.JW_TransportMode == Constants.TransportModes.Sea);

			consol.JK_RL_NKCarrierBookingOffice = "AUSYD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			firstSeaPort.JW_RL_NKLoadPort = "CNSHA";
			var bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUSYD", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			bookingRequestData.PlaceOfReceipt.Code = "AUBNE";
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUBNE", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			bookingRequestData.PlaceOfReceipt.Code = "DEHAM";
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", bookingRequestData.PortOfLoading.Code);
			AssertHasWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			bookingRequestData.PortOfLoading.Code = "AUSYD";
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUSYD", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			bookingRequestData.PortOfLoading.Code = "AUBNE";
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUBNE", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			bookingRequestData.CarrierBookingOffice.Code = "";
			bookingRequestData.PortOfLoading.Code = "CNSHA";
			AssertEquals("", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);

			bookingRequestData.CarrierBookingOffice.Code = "AUSYD";
			bookingRequestData.PlaceOfReceipt.Code = "AUBNE";
			bookingRequestData.PortOfLoading.Code = "AUMEL";
			AssertEquals("AUSYD", bookingRequestData.CarrierBookingOffice.Code);
			AssertEquals("AUBNE", bookingRequestData.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", bookingRequestData.PortOfLoading.Code);
			AssertNoWarning(bookingRequestData.CarrierBookingOffice.CodeInfo, warning);
		}

		public void TestCarrierBookingOfficeMandatory()
		{
			var carrierBookingOfficeMandatoryMessage = "The Carrier Booking Office is mandatory.\r\nPlease provide it on Consol > Details > Docs > Carrier Booking Office.";

			var consol = CreateConsol();
			var bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertNoMessageError(bookingRequest.CarrierBookingOffice.CodeInfo, carrierBookingOfficeMandatoryMessage);

			consol.CarrierBookingOffice.Code = string.Empty;
			bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertHasMessageError(bookingRequest.CarrierBookingOffice.CodeInfo, carrierBookingOfficeMandatoryMessage);
		}

		void CreateBookingRequestLog(IStmALogProvider logProvider, Event @event)
		{
			logProvider?.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				"|MST=Booking Request");

			Thread.Sleep(5);
			Factory.Save();
		}

		public void TestAdvanceDatesValidation()
		{
			var errorMessageForETDETA = "ETD/ETA must not be more than 400 days in advance.";
			var errorMessageForEarlierDeparture = "Earlier Departure must not be more than 400 days in advance.";
			var errorMessageForLatestDelivery = "Latest Delivery must not be more than 400 days in advance.";

			var consol = CreateConsol();
			var bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			foreach (Transport transport in bookingRequestData.Transports)
			{
				AssertNoMessageError(transport.ETAInfo, errorMessageForETDETA);
				AssertNoMessageError(transport.ETDInfo, errorMessageForETDETA);
			}
			AssertNoMessageError(bookingRequestData.EarliestDepartureDateInfo, errorMessageForEarlierDeparture);
			AssertNoMessageError(bookingRequestData.LatestDeliveryDateInfo, errorMessageForLatestDelivery);

			bookingRequestData.EarliestDepartureDate = ZDateTime.Now.AddDays(401);
			bookingRequestData.LatestDeliveryDate = ZDateTime.Now.AddDays(401);
			AssertHasMessageError(bookingRequestData.EarliestDepartureDateInfo, errorMessageForEarlierDeparture);
			AssertHasMessageError(bookingRequestData.LatestDeliveryDateInfo, errorMessageForLatestDelivery);

			foreach (Transport transport in bookingRequestData.Transports)
			{
				transport.ETA = ZDateTime.Now.AddDays(401);
				transport.ETD = ZDateTime.Now.AddDays(401);
				AssertHasMessageError(transport.ETAInfo, errorMessageForETDETA);
				AssertHasMessageError(transport.ETDInfo, errorMessageForETDETA);
			}
		}

		public void TestEstCargoPickupDateTimeValidation()
		{
			{
				EstCargoPickupDateTimeValidation(Constants.AgentType.Agent, Constants.ContainerModes.FCL, "CY/CY", false, false);

				EstCargoPickupDateTimeValidation(Constants.AgentType.CoLoad, Constants.ContainerModes.FCL, "CY/CY", false, false);

				EstCargoPickupDateTimeValidation(Constants.AgentType.CoLoad, Constants.ContainerModes.LCL, "CY/CY", false, false);

				EstCargoPickupDateTimeValidation(Constants.AgentType.CoLoad, Constants.ContainerModes.LCL, "CFS/CY", false, true);

				EstCargoPickupDateTimeValidation(Constants.AgentType.CoLoad, Constants.ContainerModes.LCL, "CFS/CFS", false, true);

				EstCargoPickupDateTimeValidation(Constants.AgentType.Agent, Constants.ContainerModes.LCL, "CFS/CY", false, false);

				EstCargoPickupDateTimeValidation(Constants.AgentType.Agent, Constants.ContainerModes.LCL, "CFS/CFS", false, false);

				EstCargoPickupDateTimeValidation(Constants.AgentType.CoLoad, Constants.ContainerModes.LCL, "CY/CFS", false, false);
			}

			{
				EstCargoPickupDateTimeValidation(Constants.AgentType.Agent, Constants.ContainerModes.FCL, "CY/CY", true, false);

				EstCargoPickupDateTimeValidation(Constants.AgentType.CoLoad, Constants.ContainerModes.FCL, "CY/CY", true, false);

				EstCargoPickupDateTimeValidation(Constants.AgentType.CoLoad, Constants.ContainerModes.LCL, "CY/CY", true, false);

				EstCargoPickupDateTimeValidation(Constants.AgentType.CoLoad, Constants.ContainerModes.LCL, "CFS/CY", true, true);

				EstCargoPickupDateTimeValidation(Constants.AgentType.CoLoad, Constants.ContainerModes.LCL, "CFS/CFS", true, true);

				EstCargoPickupDateTimeValidation(Constants.AgentType.Agent, Constants.ContainerModes.LCL, "CFS/CY", true, true);

				EstCargoPickupDateTimeValidation(Constants.AgentType.Agent, Constants.ContainerModes.LCL, "CFS/CFS", true, true);

				EstCargoPickupDateTimeValidation(Constants.AgentType.CoLoad, Constants.ContainerModes.LCL, "CY/CFS", true, false);
			}
		}

		void EstCargoPickupDateTimeValidation(string agentType, string containerMode, string deliveryMode, bool isNVO, bool isSatisfied)
		{
			var errorMessageForEstCargoPickupDateTime = "Est. Cargo Pickup Date Time is required.";
			var consol = CreateConsol();
			consol.JK_AgentType = agentType;
			consol.JK_ConsolMode = containerMode;
			consol.Containers.RemoveAll();
			var newContainer = consol.Containers.AddNew();
			newContainer.JC_ContainerNum = "X";
			newContainer.JC_DeliveryMode = deliveryMode;
			newContainer.JC_IsShipperOwned = false;
			newContainer.JC_GrossWeightUQ = "KG";
			newContainer.JC_TareWeight = 1000;
			newContainer.JC_DunnageWeight = 1000;
			newContainer.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;
			newContainer.JC_IsEmptyContainer = false;
			var packline = consol.Shipments.Cast<CommonShipment>().First().OuterPackLines.AddNew();
			newContainer.PackLines.Add(packline);

			if (isNVO)
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_IsNVO = true;
				var carrier = Factory.New<OrgHeader>();
				carrier.OH_FullName = "Carrier";
				carrier.OH_RL_NKClosestPort = "AUMEL";
				carrier.OH_RSL_ShippingLine = shippingLine.PK;

				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			}

			var bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			if (isSatisfied)
			{
				AssertHasMessageError(bookingRequestData.EstCargoPickupDateTimeInfo, errorMessageForEstCargoPickupDateTime);
			}
			else
			{
				AssertNoMessageError(bookingRequestData.EstCargoPickupDateTimeInfo, errorMessageForEstCargoPickupDateTime);
			}

			if (isSatisfied)
			{
				newContainer.JC_DepartureEstimatedPickup = new ZDateTime(2020, 4, 27);
				bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
				AssertEquals(newContainer.JC_DepartureEstimatedPickup, bookingRequestData.EstCargoPickupDateTime);
			}

			if (isSatisfied)
			{
				bookingRequestData.IsDoorPickup = false;
				AssertNoMessageError(bookingRequestData.EstCargoPickupDateTimeInfo, errorMessageForEstCargoPickupDateTime);
			}
		}

		public void TestAddHSCodeValidationForMalaysia()
		{
			var toErrorMessage = "It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes for Sea exports and imports to Malaysia.";
			var fromErrorMessage = "It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes for Sea exports and imports from Malaysia.";
			var warningMessage = "It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "MYABU";
			transport.JW_Vessel = "ANRO ASIA";
			transport.JW_VoyageFlight = "324443";
			transport.JW_ETD = new ZDateTime(2019, 12, 1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "MYABU";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = string.Empty;
			packLine.JL_PackageCount = 2;
			packLine.JL_F3_NKPackType = "PLT";

			var shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
			var harmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().HarmonizedCode;
			AssertHasWarningContaining("Harmonized Code is recommended for Malaysia", harmonizedCode.CodeInfo, toErrorMessage);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

			void RefreshImportHarmonizedCode()
			{
				shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
				harmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().HarmonizedCode;
			}

			transport.JW_RL_NKDiscPort = "CNSHA";
			transport.JW_RL_NKLoadPort = "MYABU";
			RefreshImportHarmonizedCode();
			AssertHasWarningContaining("Harmonized Code is recommended for Malaysia", harmonizedCode.CodeInfo, fromErrorMessage);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

			packLine.JL_HarmonisedCode = "11111";
			RefreshImportHarmonizedCode();
			AssertNoWarnings("Harmonized Code has no warnings", harmonizedCode.CodeInfo);

			packLine.JL_HarmonisedCode = string.Empty;
			var harmonizedCodes = packLine.HarmonisedCodes.AddNew();
			harmonizedCodes.JLH_RN_NKCountry = "MY";
			harmonizedCodes.JLH_Code = "55555";
			RefreshImportHarmonizedCode();
			AssertNoWarnings("Harmonized Code has no warnings", harmonizedCode.CodeInfo);

			packLine.JL_HarmonisedCode = string.Empty;
			packLine.HarmonisedCodes.DeleteAll();
			RefreshImportHarmonizedCode();
			AssertHasWarningContaining("Harmonized Code is recommended for Malaysia", harmonizedCode.CodeInfo, fromErrorMessage);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "AUSYD";
			RefreshImportHarmonizedCode();
			AssertHasWarning("Harmonized Code is recommended", harmonizedCode.CodeInfo, warningMessage);

			packLine.JL_HarmonisedCode = string.Empty;
			packLine.HarmonisedCodes.DeleteAll();
			RefreshImportHarmonizedCode();
			AssertHasWarning("Harmonized Code is recommended", harmonizedCode.CodeInfo, warningMessage);
		}

		public void TestAddHSCodeValidationForMalaysia_IsGroupAndConsolidatePackingLines()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var toErrorMessage = "It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes for Sea exports and imports to Malaysia.";
				var fromErrorMessage = "It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes for Sea exports and imports from Malaysia.";
				var warningMessage = "It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes.";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "MYABU";
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

				var container = consol.Containers.AddNew();

				var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
				transport.JW_LegOrder = 1;
				transport.JW_TransportMode = Constants.TransportModes.Sea;
				transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
				transport.JW_RL_NKLoadPort = "CNSHA";
				transport.JW_RL_NKDiscPort = "MYABU";
				transport.JW_Vessel = "ANRO ASIA";
				transport.JW_VoyageFlight = "324443";
				transport.JW_ETD = new ZDateTime(2019, 12, 1);

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "MYABU";
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_HarmonisedCode = ZString.Empty;
				packLine1.JL_PackageCount = 2;
				packLine1.JL_F3_NKPackType = "PLT";
				packLine1.JL_JC = container.PK;

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_HarmonisedCode = ZString.Empty;
				packLine2.JL_PackageCount = 2;
				packLine2.JL_F3_NKPackType = "PLT";
				packLine2.JL_JC = container.PK;

				var data = CreateDocDataObjectBuilder(consol).Build();
				var harmonizedCode = (HarmonizedCode)data.Shipments.First().PackingLines.First().HarmonizedCode;
				AssertHasWarningContaining("Harmonized Code is recommended for Malaysia", harmonizedCode.CodeInfo, toErrorMessage);
				AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

				packLine2.JL_HarmonisedCode = "12345";
				RefreshImportHarmonizedCode();
				AssertNoWarning(harmonizedCode.CodeInfo, toErrorMessage);
				AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

				void RefreshImportHarmonizedCode()
				{
					data = CreateDocDataObjectBuilder(consol).Build();
					harmonizedCode = (HarmonizedCode)data.Shipments.First().PackingLines.First().HarmonizedCode;
				}

				transport.JW_RL_NKDiscPort = "CNSHA";
				transport.JW_RL_NKLoadPort = "MYABU";
				packLine2.JL_HarmonisedCode = ZString.Empty;
				RefreshImportHarmonizedCode();
				AssertHasWarningContaining("Harmonized Code is recommended for Malaysia", harmonizedCode.CodeInfo, fromErrorMessage);
				AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

				packLine1.JL_HarmonisedCode = "11111";
				RefreshImportHarmonizedCode();
				AssertNoWarnings("Harmonized Code has no warnings", harmonizedCode.CodeInfo);

				packLine1.JL_HarmonisedCode = ZString.Empty;
				var harmonizedCodes = packLine1.HarmonisedCodes.AddNew();
				harmonizedCodes.JLH_RN_NKCountry = "MY";
				harmonizedCodes.JLH_Code = "55555";
				RefreshImportHarmonizedCode();
				AssertNoWarnings("Harmonized Code has no warnings", harmonizedCode.CodeInfo);

				packLine1.JL_HarmonisedCode = ZString.Empty;
				packLine1.HarmonisedCodes.DeleteAll();
				RefreshImportHarmonizedCode();
				AssertHasWarningContaining("Harmonized Code is recommended for Malaysia", harmonizedCode.CodeInfo, fromErrorMessage);
				AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

				packLine2.JL_HarmonisedCode = "12345";
				RefreshImportHarmonizedCode();
				AssertNoWarning(harmonizedCode.CodeInfo, fromErrorMessage);
				AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

				transport.JW_RL_NKLoadPort = "CNSHA";
				transport.JW_RL_NKDiscPort = "AUSYD";
				packLine2.JL_HarmonisedCode = ZString.Empty;
				RefreshImportHarmonizedCode();
				AssertHasWarning("Harmonized Code is recommended", harmonizedCode.CodeInfo, warningMessage);

				packLine1.JL_HarmonisedCode = ZString.Empty;
				packLine1.HarmonisedCodes.DeleteAll();
				RefreshImportHarmonizedCode();
				AssertHasWarning("Harmonized Code is recommended", harmonizedCode.CodeInfo, warningMessage);
			}
		}

		public void TestTransportDetailsValidation()
		{
			var dischargeErrorMessage = "Port of Discharge is required in all Transport Legs.";
			var loadErrorMessage = "Port of Load is required in all Transport Legs.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var containerBO = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "MYABU";
			transport.JW_Vessel = "ANRO ASIA";
			transport.JW_VoyageFlight = "324443";
			transport.JW_ETD = new ZDateTime(2019, 12, 1);

			var data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(data.ErrorPlaceHolderInfo, loadErrorMessage);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, dischargeErrorMessage);

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "";

			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(data.ErrorPlaceHolderInfo, loadErrorMessage);
			AssertHasMessageError(data.ErrorPlaceHolderInfo, dischargeErrorMessage);

			transport.JW_RL_NKLoadPort = "";
			transport.JW_RL_NKDiscPort = "AUSYD";

			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(data.ErrorPlaceHolderInfo, loadErrorMessage);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, dischargeErrorMessage);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "CNSHA";
			transport2.JW_RL_NKDiscPort = "";

			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(data.ErrorPlaceHolderInfo, dischargeErrorMessage);
			AssertHasMessageError(data.ErrorPlaceHolderInfo, loadErrorMessage);
		}

		public void TestFreightForwarderReference()
		{
			var consol = CreateConsol();

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C00000001";
			Factory.Save();

			var bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("FreightForwarderReference", "C00000001", bookingRequest.FreightForwarderReference);
		}

		public void TestZeroPackagesAndNotLinkedToContainerValidation()
		{
			var zeroPackagesAndNotLinkedToContainerErrorMessage = "You have not entered a value or there are unpacked packings with 0 quantity. If this is intended, please flag the container as empty.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			var packingLine = shipment.OuterPackLines.AddNew();
			packingLine.JL_PackageCount = 0;

			var bookingRequest = CreateDocDataObjectBuilder(consol).Build();
			foreach (var c in bookingRequest.Containers)
			{
				AssertHasMessageError(c.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);
			}

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_AgentType = Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "TBNN1234560";

			var shipment1 = consol1.Shipments.AddNew();
			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 55;

			bookingRequest = CreateDocDataObjectBuilder(consol1).Build();
			foreach (var c in bookingRequest.Containers)
			{
				AssertNoMessageError(c.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);
			}

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_AgentType = Constants.AgentType.Agent;
			consol2.JK_ConsolMode = Constants.ContainerModes.LCL;
			var container2 = consol.Containers.AddNew();

			var shipment2 = consol.Shipments.AddNew();
			var packingLine2 = shipment.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 0;

			bookingRequest = CreateDocDataObjectBuilder(consol2).Build();
			foreach (var c in bookingRequest.Containers)
			{
				AssertNoMessageError(c.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);
			}

			var consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_AgentType = Constants.AgentType.CoLoad;
			consol3.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container3 = consol.Containers.AddNew();

			var shipment3 = consol.Shipments.AddNew();
			var packingLine3 = shipment.OuterPackLines.AddNew();
			packingLine3.JL_PackageCount = 0;

			bookingRequest = CreateDocDataObjectBuilder(consol3).Build();
			foreach (var c in bookingRequest.Containers)
			{
				AssertNoMessageError(c.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);
			}

			var consol4 = Factory.New<ForwardingConsol>();
			consol4.JK_AgentType = Constants.AgentType.Agent;
			consol4.JK_ConsolMode = Constants.ContainerModes.FCL;
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_IsNVO = true;
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var container4 = consol.Containers.AddNew();

			var shipment4 = consol.Shipments.AddNew();
			var packingLine4 = shipment.OuterPackLines.AddNew();
			packingLine4.JL_PackageCount = 0;

			bookingRequest = CreateDocDataObjectBuilder(consol4).Build();
			foreach (var c in bookingRequest.Containers)
			{
				AssertNoMessageError(c.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);
			}

			var consol5 = Factory.New<ForwardingConsol>();
			consol5.JK_AgentType = Constants.AgentType.Agent;
			consol5.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container5 = consol.Containers.AddNew();
			container5.JC_IsEmptyContainer = true;

			var shipment5 = consol.Shipments.AddNew();
			var packingLine5 = shipment.OuterPackLines.AddNew();
			packingLine5.JL_PackageCount = 0;

			bookingRequest = CreateDocDataObjectBuilder(consol5).Build();
			foreach (var c in bookingRequest.Containers)
			{
				AssertNoMessageError(c.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);
			}

			var consol6 = Factory.New<ForwardingConsol>();
			consol6.JK_AgentType = Constants.AgentType.Agent;
			consol6.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container6 = consol.Containers.AddNew();

			bookingRequest = CreateDocDataObjectBuilder(consol6).Build();
			foreach (var c in bookingRequest.Containers)
			{
				AssertNoMessageError(c.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);
			}
		}

		public void TestZeroPackagesAndNotLinkedToContainerValidation_IsGroupAndConsolidatePackingLines()
		{
			var zeroPackagesAndNotLinkedToContainerErrorMessage = "You have not entered a value or there are unpacked packings with 0 quantity. If this is intended, please flag the container as empty.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			container1.JC_IsEmptyContainer = false;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = ZString.Empty;
			container2.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine1.JL_JC = container1.PK;
			var outerPackLine2 = PopulatePackLine(shipment.OuterPackLines.AddNew(), 0, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine2.JL_JC = container2.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bookingRequest = CreateDocDataObjectBuilder(consol).Build();
				var container1DO = bookingRequest.Containers.First(x => x.Number == "CONTAINER1");
				var container2DO = bookingRequest.Containers.First(x => x.Number.IsEmpty);

				AssertNotEquals(0, container1DO.PackCount);
				AssertHasMessageError(container1DO.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);

				AssertEquals(0, container2DO.PackCount);
				AssertHasMessageError(container2DO.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);

				container2.JC_ContainerNum = "CONTAINER2";
				bookingRequest = CreateDocDataObjectBuilder(consol).Build();
				container1DO = bookingRequest.Containers.First(x => x.Number == "CONTAINER1");
				container2DO = bookingRequest.Containers.First(x => x.Number == "CONTAINER2");

				AssertNotEquals(0, container1DO.PackCount);
				AssertNoMessageError(container1DO.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);

				AssertEquals(0, container2DO.PackCount);
				AssertHasMessageError(container2DO.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);

				outerPackLine2.JL_PackageCount = 1;
				bookingRequest = CreateDocDataObjectBuilder(consol).Build();
				container1DO = bookingRequest.Containers.First(x => x.Number == "CONTAINER1");
				container2DO = bookingRequest.Containers.First(x => x.Number == "CONTAINER2");

				AssertNotEquals(0, container1DO.PackCount);
				AssertNoMessageError(container1DO.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);

				AssertNotEquals(0, container2DO.PackCount);
				AssertNoMessageError(container2DO.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);

				var outerPackLine3 = PopulatePackLine(shipment.OuterPackLines.AddNew(), 0, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
				outerPackLine3.JL_JC = ZGuid.Empty;
				bookingRequest = CreateDocDataObjectBuilder(consol).Build();
				container1DO = bookingRequest.Containers.First(x => x.Number == "CONTAINER1");
				container2DO = bookingRequest.Containers.First(x => x.Number == "CONTAINER2");

				AssertNotEquals(0, container1DO.PackCount);
				AssertHasMessageError(container1DO.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);

				AssertNotEquals(0, container2DO.PackCount);
				AssertHasMessageError(container2DO.PackCountInfo, zeroPackagesAndNotLinkedToContainerErrorMessage);
			}
		}

		public void TestContainerIsEmptyError()
		{
			var emptyContainerErrorMessage = "Carrier does not support booking requests for empty containers.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container = consol.Containers.AddNew();
			container.JC_IsEmptyContainer = true;

			var bookingRequest = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(bookingRequest.ErrorPlaceHolderInfo, emptyContainerErrorMessage);

			container.JC_IsEmptyContainer = false;
			bookingRequest = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(bookingRequest.ErrorPlaceHolderInfo, emptyContainerErrorMessage);
		}

		public void TestDirectConsolWithtoutShipmentError()
		{
			var directConsolWithoutShipmentErrorMessage = "At least one Shipment must exist to send booking request when Consol type is Direct.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertNoMessageError(bookingRequest.ErrorPlaceHolderInfo, directConsolWithoutShipmentErrorMessage);

			consol.JK_AgentType = Constants.AgentType.Direct;
			bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertHasMessageError(bookingRequest.ErrorPlaceHolderInfo, directConsolWithoutShipmentErrorMessage);

			var shipment = consol.Shipments.AddNew();
			bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertNoMessageError(bookingRequest.ErrorPlaceHolderInfo, directConsolWithoutShipmentErrorMessage);
		}

		public void TestCoLoadConsolAndCoLoaderIsNVOWithoutShipmentError()
		{
			var errorMessage = "The Booking Request cannot be sent to NVOCC when there is no cargo details. Please attach at least one Shipment to the Consolidation.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var refShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			refShippingLine.RSL_IsNVO = false;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_RSL_ShippingLine = refShippingLine.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertNoMessageError(bookingRequest.ErrorPlaceHolderInfo, errorMessage);

			refShippingLine.RSL_IsNVO = true;
			bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertHasMessageError(bookingRequest.ErrorPlaceHolderInfo, errorMessage);

			var shipment = consol.Shipments.AddNew();
			bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertNoMessageError(bookingRequest.ErrorPlaceHolderInfo, errorMessage);
		}

		public void TestNoCoLoadConsolAndCarrierIsNVOWithoutShipmentError()
		{
			var errorMessage = "The Booking Request cannot be sent to NVOCC when there is no cargo details. Please attach at least one Shipment to the Consolidation.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var refShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			refShippingLine.RSL_IsNVO = false;
			var shippingline = Factory.NewWithValidTestData<OrgHeader>();
			shippingline.OH_RSL_ShippingLine = refShippingLine.PK;
			consol.JK_OA_ShippingLineAddress = shippingline.MainAddress.PK;

			var bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertNoMessageError(bookingRequest.ErrorPlaceHolderInfo, errorMessage);

			refShippingLine.RSL_IsNVO = true;
			bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertHasMessageError(bookingRequest.ErrorPlaceHolderInfo, errorMessage);

			var shipment = consol.Shipments.AddNew();
			bookingRequest = CreateDocDataObjectBuilder(consol).Build();

			AssertNoMessageError(bookingRequest.ErrorPlaceHolderInfo, errorMessage);
		}

		public void TestTransportBookingPickupDeliveryInfos()
		{
			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "";
			container.JC_ContainerCount = 1;
			container.JC_DeliveryMode = "CFS/CFS";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var address = CreateOrgHeader("ORG", "Unit ", "Lost Lane").MainAddress;
			var address1 = CreateOrgHeader("ORG1", "Unit 1", "1 Lost Lane").MainAddress;
			var address2 = CreateOrgHeader("ORG2", "Unit 2", "2 Lost Lane").MainAddress;
			var address3 = CreateOrgHeader("ORG3", "Unit 3", "3 Lost Lane").MainAddress;
			var address4 = CreateOrgHeader("ORG4", "Unit 4", "4 Lost Lane").MainAddress;
			var address5 = CreateOrgHeader("ORG5", "Unit 5", "5 Lost Lane").MainAddress;
			var address6 = CreateOrgHeader("ORG6", "Unit 6", "6 Lost Lane").MainAddress;
			var address7 = CreateOrgHeader("ORG7", "Unit 7", "7 Lost Lane").MainAddress;
			var address8 = CreateOrgHeader("ORG8", "Unit 8", "8 Lost Lane").MainAddress;

			var consolidation1 = Helper.CreateConsolidation();
			consolidation1.KB_ParentID = consol.PK;
			consolidation1.KB_ParentTableCode = consol.TablePrefix;
			consolidation1.KB_JobDirection = "PIC";

			var consolidation2 = Helper.CreateConsolidation();
			consolidation2.KB_ParentID = consol.PK;
			consolidation2.KB_ParentTableCode = consol.TablePrefix;
			consolidation2.KB_JobDirection = "DLV";
			var package1 = CreatePkgPackage(consolidation1, "P001");
			var package2 = CreatePkgPackage(consolidation2, "P002");

			var noTransportCompanyBooking = CreateDtbBooking(consolidation1, null, address, address, "EXP", "TB001", package1, "PIC");
			var otherOrgBooking = CreateDtbBooking(consolidation1, consol.ShippingLine, address, address, "EXP", "TB003", package1, "PIC", orgType: "CTO");
			var pickupBooking1 = CreateDtbBooking(consolidation1, consol.ShippingLine, address1, address2, "EXP", "TB011", package1, "PIC");
			var pickupBooking2 = CreateDtbBooking(consolidation1, consol.ShippingLine, address3, address4, "ORG", "TB012", package1, "PIC");
			var deliveryBooking1 = CreateDtbBooking(consolidation2, consol.ShippingLine, address5, address6, "IMP", "TB021", package2, "DLV");
			var deliveryBooking2 = CreateDtbBooking(consolidation2, consol.ShippingLine, address7, address8, "DST", "TB022", package2, "DLV");

			Factory.Save();
			var data = CreateDocDataObjectBuilder(consol);
			var bookingRequest = data.Build();

			var container1 = bookingRequest.Containers.FirstOrDefault(c => c.Number == "AAAA0000007");
			AssertEquals(8, container1.TransportBookingPickupDeliveryInfos.Count);

			var container2 = bookingRequest.Containers.FirstOrDefault(c => c.Number == "");
			var pickupInfos = container2.TransportBookingPickupDeliveryInfos.Where(info => info.Type == "PickupFrom");
			AssertEquals(4, pickupInfos.Count());

			AssertEquals("ORG1", pickupInfos.ElementAt(0).Address.CompanyName);
			AssertEquals(new ZDateTime(2021, 11, 1), pickupInfos.ElementAt(0).AddressETD);
			AssertEquals("ORG2", pickupInfos.ElementAt(1).Address.CompanyName);
			AssertEquals(new ZDateTime(2021, 12, 1), pickupInfos.ElementAt(1).AddressETD);
			AssertEquals("ORG3", pickupInfos.ElementAt(2).Address.CompanyName);
			AssertEquals(new ZDateTime(2021, 11, 1), pickupInfos.ElementAt(2).AddressETD);
			AssertEquals("ORG4", pickupInfos.ElementAt(3).Address.CompanyName);
			AssertEquals(new ZDateTime(2021, 12, 1), pickupInfos.ElementAt(3).AddressETD);

			var deliveryInfos = container2.TransportBookingPickupDeliveryInfos.Where(info => info.Type == "DeliveryTo");
			AssertEquals(4, deliveryInfos.Count());

			AssertEquals("ORG5", deliveryInfos.ElementAt(0).Address.CompanyName);
			AssertEquals(new ZDateTime(2021, 11, 1), deliveryInfos.ElementAt(0).AddressETD);
			AssertEquals("ORG6", deliveryInfos.ElementAt(1).Address.CompanyName);
			AssertEquals(new ZDateTime(2021, 12, 1), deliveryInfos.ElementAt(1).AddressETD);
			AssertEquals("ORG7", deliveryInfos.ElementAt(2).Address.CompanyName);
			AssertEquals(new ZDateTime(2021, 11, 1), deliveryInfos.ElementAt(2).AddressETD);
			AssertEquals("ORG8", deliveryInfos.ElementAt(3).Address.CompanyName);
			AssertEquals(new ZDateTime(2021, 12, 1), deliveryInfos.ElementAt(3).AddressETD);

			var tickedPickupMessage = "There are Pickup Transport Booking(s) available, please update Delivery mode via Consol > Containers > Delivery Mode field to default pickup addresses from transport booking(s).";
			var untickedPickupMessage = "The pickup addresses were defaulted from transport booking(s), please update Delivery mode via Consol > Containers > Delivery Mode field to remove pickup addresses from booking request.";
			var tickedDeliveryMessage = "There are Delivery Transport Booking(s) available, please update Delivery mode via Consol > Containers > Delivery Mode field to default delivery addresses from transport booking(s).";
			var untickedDeliveryMessage = "The delivery addresses were defaulted from transport booking(s), please update Delivery mode via Consol > Containers > Delivery Mode field to remove delivery addresses from booking request.";

			AssertNoMessageError(bookingRequest.IsDoorPickupInfo, tickedPickupMessage);
			AssertNoMessageError(bookingRequest.IsDoorPickupInfo, untickedPickupMessage);
			AssertNoMessageError(bookingRequest.IsDoorDeliveryInfo, tickedDeliveryMessage);
			AssertNoMessageError(bookingRequest.IsDoorDeliveryInfo, untickedDeliveryMessage);

			bookingRequest.IsDoorPickup = false;
			bookingRequest.IsDoorDelivery = false;
			AssertHasMessageError(bookingRequest.IsDoorPickupInfo, untickedPickupMessage);
			AssertHasMessageError(bookingRequest.IsDoorDeliveryInfo, untickedDeliveryMessage);

			foreach (ForwardingContainer containerBO in consol.Containers)
			{
				containerBO.JC_DeliveryMode = "CY/CFS";
			}
			bookingRequest = data.Build();
			AssertNoMessageError(bookingRequest.IsDoorPickupInfo, untickedPickupMessage);

			bookingRequest.IsDoorPickup = true;
			AssertHasMessageError(bookingRequest.IsDoorPickupInfo, tickedPickupMessage);

			foreach (ForwardingContainer containerBO in consol.Containers)
			{
				containerBO.JC_DeliveryMode = "CFS/CY";
			}
			bookingRequest = data.Build();
			AssertNoMessageError(bookingRequest.IsDoorPickupInfo, tickedPickupMessage);

			bookingRequest.IsDoorDelivery = true;
			AssertHasMessageError(bookingRequest.IsDoorDeliveryInfo, tickedDeliveryMessage);

			bookingRequest.IsDoorDelivery = false;
			AssertNoMessageError(bookingRequest.IsDoorDeliveryInfo, untickedDeliveryMessage);
		}

		public void TestBookingRequestPopulatePorts()
		{
			var frtChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			frtChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			frtChargeCode.AC_Code = "MYFREIGHT";
			frtChargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			frtChargeCode.AC_Desc = "Description";
			frtChargeCode.AC_ChargeGroup = "FRT";

			var nonFrtChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			nonFrtChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			nonFrtChargeCode.AC_Code = "NOTFREIGHT";
			nonFrtChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			nonFrtChargeCode.AC_Desc = "BLAH";
			nonFrtChargeCode.AC_ChargeGroup = "ORG";

			Env.Registry.FreightChargeCode = frtChargeCode.PK.ToGuid();

			Factory.Save();

			var consol = CreateConsol();
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			var parameters = new DummyDocDataObjectParameters();

			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = frtChargeCode.PK;
			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;

			var bookingRequestData = CreateDocDataObjectBuilder(consol, parameters).Build();

			AssertEquals("OperationalPort", "CNSHA", bookingRequestData.OperationalPort.Code);
			AssertEquals("FreighPayableAt PPD", "CNNJI", bookingRequestData.FreightPayableAt.Code);

			AssertNoMessageError(bookingRequestData.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(bookingRequestData.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			bookingRequestData = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("OperationalPort", "CNSHA", bookingRequestData.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", "AUSYD", bookingRequestData.FreightPayableAt.Code);

			AssertNoMessageError(bookingRequestData.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(bookingRequestData.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			consolCost.Delete();
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			bookingRequestData = CreateDocDataObjectBuilder(consol, parameters).Build();

			AssertEquals("OperationalPort", "CNSHA", bookingRequestData.OperationalPort.Code);
			AssertEquals("FreighPayableAt PPD", "CNSHA", bookingRequestData.FreightPayableAt.Code);

			AssertNoMessageError(bookingRequestData.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(bookingRequestData.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			bookingRequestData = CreateDocDataObjectBuilder(consol, parameters).Build();

			AssertEquals("OperationalPort", "CNSHA", bookingRequestData.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", "AUSYD", bookingRequestData.FreightPayableAt.Code);

			AssertNoMessageError(bookingRequestData.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(bookingRequestData.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			consol.JK_RL_NKLoadPort = "ABCDE";
			consol.JK_RL_NKDischargePort = "ABCDE";
			bookingRequestData = CreateDocDataObjectBuilder(consol, parameters).Build();

			AssertEquals("OperationalPort", "ABCDE", bookingRequestData.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", "ABCDE", bookingRequestData.FreightPayableAt.Code);

			AssertHasMessageError(bookingRequestData.OperationalPort.CodeInfo, "You have not entered a valid un loco.");
			AssertHasMessageError(bookingRequestData.FreightPayableAt.CodeInfo, "You have not entered a valid un loco.");

			consol.JK_RL_NKLoadPort = string.Empty;
			consol.JK_RL_NKDischargePort = string.Empty;
			bookingRequestData = CreateDocDataObjectBuilder(consol, parameters).Build();

			AssertEquals("OperationalPort", string.Empty, bookingRequestData.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", string.Empty, bookingRequestData.FreightPayableAt.Code);

			AssertHasMessageError(bookingRequestData.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertHasMessageError(bookingRequestData.FreightPayableAt.CodeInfo, "Freight Payable At is required.");
		}

		public void TestBookingRequestContainerTemperatureValidation()
		{
			var errorMessage = "Maximum three digits are allowed for temperature in Booking Request, when integer +-999, or when decimal +-99.9.\r\nPlease change temperature in Consol>Containers>Refrigeration.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container = consol.Containers.AddNew();

			container.JC_SetPointTemp = 9999.999m;
			var bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(bookingRequestData.Containers.First().SetTemperature.ValueInfo, errorMessage);

			container.JC_SetPointTemp = -1000m;
			bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(bookingRequestData.Containers.First().SetTemperature.ValueInfo, errorMessage);

			container.JC_SetPointTemp = 0m;
			bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(bookingRequestData.Containers.First().SetTemperature.ValueInfo, errorMessage);

			container.JC_SetPointTemp = -99.9m;
			bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(bookingRequestData.Containers.First().SetTemperature.ValueInfo, errorMessage);

			container.JC_SetPointTemp = 999m;
			bookingRequestData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(bookingRequestData.Containers.First().SetTemperature.ValueInfo, errorMessage);
		}

		#endregion

		public void TestCarrierMessagingRequirementsValidation_IEL()
		{
			var errorMessage = "This carrier only supports integration via email to local office.\r\nContact name and email address are required to send Booking Request.\r\nPlease maintain contact name and email address in carrier Organization > Contact > Email and Receiving Documents > Group SHP.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "ALL";

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			shippingLine.RSL_BookingRequestAvailable = true;
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNullOrEmpty(carrierMessageData.Recipient.Contact);

			shippingLine.RSL_BookingRequestAvailable = false;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("TEST NAME",carrierMessageData.Recipient.Contact);

			var shippingLineMessagingRequirement = shippingLine.ShippingLineMessagingRequirements.AddNew();
			shippingLineMessagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.IntegrationViaEmailToCarrierLocalOffice;
			shippingLineMessagingRequirement.RSR_IsBookingRequest = true;

			shippingLine.RSL_BookingRequestAvailable = true;
			AssertEquals("test@test.com", carrierMessageData.Recipient.Email);
			AssertNoMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			contact.Documents.RemoveAll();
			shippingLine.RSL_BookingRequestAvailable = false;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "SHP";
			shippingLine.RSL_BookingRequestAvailable = true;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);
		}

		public void TestIsFreightAsAgreedKeepTheSameValueWithIsPayableElsewhere()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			carrierMessageData.IsFreightAsAgreed = false;

			var optionalChargeBasicFreight = (OptionalCharge)carrierMessageData.OptionalChargeBasicFreight;

			optionalChargeBasicFreight.IsPayableElsewhere = true;
			AssertEquals(true, carrierMessageData.IsFreightAsAgreed);

			optionalChargeBasicFreight.IsPayableElsewhere = false;
			AssertEquals(false, carrierMessageData.IsFreightAsAgreed);
		}

		#region Implementation

		protected override CarrierMessageDataBuilder CreateDocDataObjectBuilder(ForwardingConsol consol)
		{
			var parameters = new DummyDocDataObjectParameters()
			{
				LogProvider = Factory.New<DummyEnterpriseBusinessObject>()
			};
			return new BookingRequestBuilder(consol, parameters);
		}

		CarrierMessageDataBuilder CreateDocDataObjectBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			return new BookingRequestBuilder(consol, parameters);
		}

		protected override string GetCarrierMessageDataBuilderName()
		{
			return DataContext.BookingRequest;
		}

		IDtbBooking CreateDtbBooking(IDtbBookingConsolidation consolidation, OrgHeader companyAddress, OrgAddress address1, OrgAddress address2, string direction, string kmJobID, PkgPackage package, string instructionType, string orgType = "CFS", string template = "EFPR")
		{
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = kmJobID;
			booking.KM_KT_NKBookingTemplate = template;
			booking.KM_Direction = direction;
			if (companyAddress != null)
			{
				booking.Address.OrganisationPK = companyAddress.PK;
			}

			helper.CreateInstruction(booking, instructionType, orgType, address1, package, new ZDateTime(2021, 11, 1));
			helper.CreateInstruction(booking, instructionType, orgType, address2, package, new ZDateTime(2021, 12, 1));

			return booking;
		}

		PkgPackage CreatePkgPackage(IDtbBookingConsolidation consolidation, string kjJobID)
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;
			packageJob.KJ_JobID = kjJobID;
			packageJob.KJ_ParentTableCode = "KB";

			var package = packageJob.Packages.AddNew("CNT");
			package.KP_PackageQty = 1;
			package.KP_F3_NKPackType = "CNT";
			package.KP_DimensionUQ = "M";
			package.KP_Length = 3;
			package.KP_Width = 4;
			package.KP_Height = 2;
			package.KP_WeightUQ = "KG";
			package.KP_Weight = 1000;
			package.KP_VolumeUQ = "M3";
			package.KP_Volume = 24;
			package.Container.K0_RC_ContainerType = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			return package;
		}

		OrgHeader CreateOrgHeader(string name, string address1, string address2)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = name;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = address1;
			orgHeader.MainAddress.Address2 = address2;
			orgHeader.MainAddress.City = "Sydney";
			orgHeader.MainAddress.Postcode = "2000";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";

			return orgHeader;
		}

		ITransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = ObjectFactory.New<ITransportBookingTestHelper>(Factory)); }
		}

		ITransportBookingTestHelper helper;
		#endregion
	}
}
