using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	[TestedType(typeof(ShipmentPortMessagingData))]
	sealed class ShipmentPortMessagingDataTest : PortMessagingDataTest
	{
		public void TestBookingReferenceFromShipmentReferenceNumberBKG()
		{
			var coLoadWith = Factory.New<OrgHeader>();
			coLoadWith.OH_FullName = "CoLoad Shipping";
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "Shipping Line";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "UAIEV";
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "M00000123";
			consol.JK_CoLoadMasterBill = "C00000123";
			consol.JK_BookingReference = "BOOKING1";
			consol.JK_CoLoadBookingReference = "BOOKING2";
			AssertEquals("Pre-Condition", true, consol.IsCoLoad);

			var shipment = consol.Shipments.AddNew();

			var shipmentPortMessaging = new ShipmentPortMessagingData(consol, shipment);
			shipmentPortMessaging.ValidateAll();
			AssertEquals("BookingReference", "BOOKING2", shipmentPortMessaging.BookingReference);

			var entryNum = shipment.Numbers.AddNew();
			entryNum.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			entryNum.CE_EntryNum = "BOOKING3";
			entryNum.CE_RN_NKCountryCode = "DE";
			shipmentPortMessaging = new ShipmentPortMessagingData(consol, shipment);
			shipmentPortMessaging.ValidateAll();

			AssertEquals("BookingReference", "BOOKING3", shipmentPortMessaging.BookingReference);
		}

		public void TestCheckWarehouse()
		{
			const string error = "Warehouse 'SAMM' cannot be used with Entry Type 'SAC' (Consolidated Container)";

			var consol = Factory.New<ForwardingConsol>();
			var cto = Factory.New<OrgHeader>();
			var cusCode = cto.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "ABC", Constants.CountryCodes.Germany);
			consol.JK_OA_DepartureCTOAddress = cto.MainAddress.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;

			var portMessagingData = new ShipmentPortMessagingData(consol, shipment);
			portMessagingData.ValidateAll();
			AssertNoMessageError(portMessagingData.WarehouseInfo, error);
			AssertEquals("WareHouse CTO", "ABC", portMessagingData.Warehouse);

			cusCode.OK_CustomsRegNo = "SAMM";
			portMessagingData.ValidateAll();
			AssertHasMessageError(portMessagingData.WarehouseInfo, error);

			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			portMessagingData.ValidateAll();
			AssertNoMessageError(portMessagingData.WarehouseInfo, error);

			var packLine = shipment.OuterPackLines.AddNew();
			var packLinePortMessaging = PackLinePortMessaging.LoadOrCreate(packLine);

			shipmentPortMessaging.JSM_EntryType = ZString.Empty;
			packLinePortMessaging.JLM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;
			portMessagingData.ValidateAll();
			AssertHasMessageError(portMessagingData.WarehouseInfo, error);

			packLinePortMessaging.JLM_EntryType = EntryTypeList.Codes.Message;
			portMessagingData.ValidateAll();
			AssertNoMessageError(portMessagingData.WarehouseInfo, error);
			AssertEquals("WareHouse CTO", "SAMM", portMessagingData.Warehouse);
		}

		public void TestCheckDepartureReference_VoyageOriginLoadPort()
		{
			const string error = "Load Port Departure Reference must be populated. Operate > Schedules > Sailing Schedule > Load Ports > Departure Ref.";
			var consol = CreateNewConsol();
			var shipment = consol.Shipments.AddNew();
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;

			consol.JK_RL_NKLoadPort = "AUBNE";
			var portMessagingData = new ShipmentPortMessagingData(consol, shipment);
			portMessagingData.ValidateAll();
			AssertNoMessageError(portMessagingData.DepartureReferenceInfo, error);

			consol.JK_RL_NKLoadPort = "DEHAM";
			portMessagingData.ValidateAll();
			AssertHasMessageError(portMessagingData.DepartureReferenceInfo, error);
			portMessagingData.DepartureReferenceInfo.ClearAllNotifications();

			consol.Voyage.Origins[0].JA_DepartReference = "REF111111";
			portMessagingData.ValidateAll();
			AssertNoMessageError(portMessagingData.DepartureReferenceInfo, error);
		}

		public void TestCheckDepartureReference_TransportLoadPort()
		{
			const string error = "Load Port Departure Reference must be populated. Operate > Schedules > Sailing Schedule > Load Ports > Departure Ref.";
			var consol = CreateNewConsol();
			var shipment = consol.Shipments.AddNew();
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;

			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.Transports[0].JW_RL_NKLoadPort = "AUBNE";
			var portMessagingData = new ShipmentPortMessagingData(consol, shipment);
			portMessagingData.ValidateAll();
			AssertNoMessageError(portMessagingData.DepartureReferenceInfo, error);

			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.Transports[0].JW_RL_NKLoadPort = "DEHAM";
			portMessagingData.ValidateAll();
			AssertHasMessageError(portMessagingData.DepartureReferenceInfo, error);
			portMessagingData.DepartureReferenceInfo.ClearAllNotifications();

			consol.Voyage.Origins[0].JA_DepartReference = "REF111111";
			portMessagingData.ValidateAll();
			AssertNoMessageError(portMessagingData.DepartureReferenceInfo, error);
		}

		public void TestWarehouse_ShipmentPackingModeIsLCL_WhenDPCCodeNotExistedInShipmentDepartureCFSAddress()
		{
			TestWarehouse_UseCfsDpcCode_WhenPackingModeIs(Constants.ContainerModes.LCL);
		}

		public void TestWarehouse_ShipmentPackingModeIsBCN_WhenDPCCodeNotExistedInShipmentDepartureCFSAddress()
		{
			TestWarehouse_UseCfsDpcCode_WhenPackingModeIs(Constants.ContainerModes.BuyersConsol);
		}

		public void TestWarehouse_UseCfsDpcCode_WhenPackingModeIs(ZString packingMode)
		{
			var consol = CreateNewConsol();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = packingMode;

			var shipmentPortMessagingData = new ShipmentPortMessagingData(consol, shipment);
			var consolPortMessagingData = new ConsolPortMessagingData(consol);

			AssertEquals($"Shipment's WareHouse use CFS's DPC Code when Shipment.PackingMode is {packingMode}", "dakosysfc", shipmentPortMessagingData.Warehouse);
			AssertEquals("Consol's WareHouse use CTO's DPC Code", "BRT", consolPortMessagingData.Warehouse);
		}

		public void TestWarehouse_ShipmentPackingModeIsLCL_WhenDPCCodeExistedInShipmentDepartureCFSAddresss()
		{
			TestWarehouse_UseJS_OA_ExportReceivingDepotDpcCode_WhenPackingModeIs(Constants.ContainerModes.LCL);
		}

		public void TestWarehouse_ShipmentPackingModeIsBCN_WhenDPCCodeExistedInShipmentDepartureCFSAddresss()
		{
			TestWarehouse_UseJS_OA_ExportReceivingDepotDpcCode_WhenPackingModeIs(Constants.ContainerModes.BuyersConsol);
		}

		public void TestWarehouse_UseJS_OA_ExportReceivingDepotDpcCode_WhenPackingModeIs(ZString packingMode)
		{
			var consol = CreateNewConsol();

			var exportReceivingDepot = Factory.New<OrgHeader>();
			exportReceivingDepot.OH_Code = "AAA";
			exportReceivingDepot.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "BBB", Constants.CountryCodes.Germany);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = packingMode;
			shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.MainAddress.PK;

			var shipmentPortMessagingData = new ShipmentPortMessagingData(consol, shipment);
			var consolPortMessagingData = new ConsolPortMessagingData(consol);

			AssertEquals("Shipment's WareHouse use JS_OA_ExportReceivingDepot's DPC Code", "BBB", shipmentPortMessagingData.Warehouse);
			AssertEquals("Consol's WareHouse use CTO's DPC Code", "BRT", consolPortMessagingData.Warehouse);
		}

		public void TestWarehouseCTO_ShipmentPackingModeIsNotLCLOrBCN()
		{
			var consol = CreateNewConsol();

			foreach (var packingMode in new[]
			{
				Constants.ContainerModes.FCL,
				Constants.ContainerModes.Groupage,
				Constants.ContainerModes.Loose,
				Constants.ContainerModes.LTL,
				Constants.ContainerModes.AIR,
				Constants.ContainerModes.FreightAllKind,
				Constants.ContainerModes.ULD
			})
			{
				var newShipment = consol.Shipments.AddNew();
				newShipment.JS_PackingMode = packingMode;

				var newPortMessagingData = new ShipmentPortMessagingData(consol, newShipment);
				AssertEquals("WareHouse use CTO's DPC Code when PackingMode not LCL or BCN", "BRT", newPortMessagingData.Warehouse);
			}
		}

		public void TestCheckWarehouseInfo()
		{
			var consol = Factory.New<ForwardingConsol>();
			var lclshipment = Factory.New<ForwardingShipment>();
			var lclPortMessagingData = new ShipmentPortMessagingData(consol, lclshipment);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			var portMessagingData = new ShipmentPortMessagingData(consol, shipment);

			lclPortMessagingData.ValidateAll();
			portMessagingData.ValidateAll();

			AssertHasMessageError(lclPortMessagingData.WarehouseInfo, "The DAKOSY Participant Code (DPC) must be populated. Shipment > Pickup > CFS or Consol > Departure > CFS Address > Details > Config > Registration Numbers/Codes > DPC Code for DE.");
			AssertHasMessageError(portMessagingData.WarehouseInfo, "The DAKOSY Participant Code (DPC) must be populated. Consol > Departure > CTO Address > Details > Config > Registration Numbers / Codes > DPC Code for DE.");
		}

		[TestDate(2023, 3, 1)]
		public void TestCheckEORI()
		{
			using (FreightDataRegistry.Instance.ConsignorShipperTerminology.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Shipper"))
			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				const string message = "Entry Type AE1 requires Shipper or Sending Agent EORI. Go to either Shipper or Sending Agent organization > Config > Registration Numbers.";

				var shipper = Factory.NewWithValidTestData<OrgHeader>();
				var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "DEHAM";
				consol.JK_RL_NKDischargePort = "UAIEV";
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorPK = shipper.PK;

				var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
				shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.AE1ExportDeclaration;

				var shipmentPortMessagingData = new ShipmentPortMessagingData(consol, shipment);
				shipmentPortMessagingData.ValidateAll();
				AssertHasMessageError(shipmentPortMessagingData.ShipperEORIInfo, message);
				AssertHasMessageError(shipmentPortMessagingData.AgentEORIInfo, message);

				var shipperNum = shipper.CustomsCodes.AddNew();
				shipperNum.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				shipperNum.OK_CustomsRegNo = "N001";
				shipperNum.OK_RN_NKCodeCountry = "DE";

				var sendingAgentNum = sendingAgent.CustomsCodes.AddNew();
				sendingAgentNum.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				sendingAgentNum.OK_CustomsRegNo = "N002";
				sendingAgentNum.OK_RN_NKCodeCountry = "DE";

				shipmentPortMessagingData = new ShipmentPortMessagingData(consol, shipment);
				AssertEquals("ShipperEORI", "N001", shipmentPortMessagingData.ShipperEORI);
				AssertEquals("AgentEORI", "N002", shipmentPortMessagingData.AgentEORI);

				shipmentPortMessagingData.ValidateAll();
				AssertNoMessageError(shipmentPortMessagingData.ShipperEORIInfo, message);
				AssertNoMessageError(shipmentPortMessagingData.AgentEORIInfo, message);
			}
		}

		#region Implementation

		ForwardingConsol CreateNewConsol()
		{
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "maersk";
			shippingLine.OH_FullName = "shipping line";

			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_Code = "kermit";
			forwarder.OH_FullName = "shipper";

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_Code = "cfs";

			var cto = Factory.New<OrgHeader>();
			cto.OH_Code = "cto";

			forwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "dakosysendercode", Constants.CountryCodes.Germany);
			forwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "zapsendercode", Constants.CountryCodes.Germany);
			cfs.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "dakosysfc", Constants.CountryCodes.Germany);
			cto.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "BRT", Constants.CountryCodes.Germany);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "PLGDY";
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = cto.MainAddress.PK;
			consol.JK_MasterBillNum = "billno0000-00112";

			var transport = consol.Transports[0];
			transport.JW_JX = CreateNewSailing(vessel, "voyageNo", "DEHAM", "AUSYD", ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(1)).PK;
			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_ETD = ZDateTime.BrettsBirthday;

			var origin1 = consol.Voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "UAODS";
			origin1.JA_Berth = "AAA";
			var origin2 = consol.Voyage.Origins.OfType<VoyageOrigin>().First(o => o.JA_RL_NKPortOfLoading.Equals("DEHAM"));
			origin2.JA_RL_NKPortOfLoading = "DEHAM";
			origin2.JA_Berth = "BBB";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "PLGDY";

			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			shipmentPortMessaging.JSM_MovementReferenceNumber = "15DE333444455555E2";
			shipmentPortMessaging.JSM_ForwardingCustomsOfficeCode = "83031478";

			Factory.Save();

			return consol;
		}

		JobSailing CreateNewSailing(RefVessel vessel, ZString voyageNo, ZString loadPort, ZString dischargePort, ZDateTime departureTime, ZDateTime arrivalTime)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageNo;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadPort;
			origin.JA_E_DEP = departureTime;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischargePort;
			destination.JB_E_ARV = arrivalTime;
			voyage.GenerateSailings();

			var result = voyage.Sailings.AddNew();
			result.JX_JB = destination.PK;
			result.JX_JA = origin.PK;
			return result;
		}

		protected override PortMessagingData GetNewPortMessagingData()
		{
			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			return new ShipmentPortMessagingData(Factory.New<ForwardingConsol>(), Shipment);
		}

		protected override IStmALogParent GetLogParent()
		{
			return Shipment;
		}

		ForwardingShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.New<ForwardingShipment>()); }
		}
		ForwardingShipment shipment;

		#endregion
	}
}
