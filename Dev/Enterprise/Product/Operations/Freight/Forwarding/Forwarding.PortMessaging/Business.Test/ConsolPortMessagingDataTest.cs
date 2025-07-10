using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	[TestedType(typeof(ConsolPortMessagingData))]
	sealed class ConsolPortMessagingDataTest : PortMessagingDataTest
	{
		public void TestCheckWarehouse()
		{
			const string error = "Warehouse 'SAMM' cannot be used with Entry Type 'SAC' (Consolidated Container)";

			var consol = Factory.New<ForwardingConsol>();
			var cto = Factory.New<OrgHeader>();
			var cusCode = cto.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "ABC", Constants.CountryCodes.Germany);
			consol.JK_OA_DepartureCTOAddress = cto.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;

			var portMessagingData = new ConsolPortMessagingData(consol);
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
			var portMessagingData = new ConsolPortMessagingData(consol);
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
			var portMessagingData = new ConsolPortMessagingData(consol);
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
			return new ConsolPortMessagingData(Consol);
		}

		protected override IStmALogParent GetLogParent()
		{
			return Consol;
		}

		ForwardingConsol Consol
		{
			get { return consol ?? (consol = Factory.New<ForwardingConsol>()); }
		}
		ForwardingConsol consol;

		#endregion
	}
}
