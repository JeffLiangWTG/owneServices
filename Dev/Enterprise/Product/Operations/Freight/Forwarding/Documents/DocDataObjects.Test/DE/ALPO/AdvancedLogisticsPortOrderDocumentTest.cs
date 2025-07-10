using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DE
{
	class AdvancedLogisticsPortOrderDocumentTest : StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			var pivotPK = new ZGuid("a70b966d-3a74-44de-99aa-cdfc73311b6b");

			var consol = CreateConsol();
			CreateBasicGoodsWithoutEquipment(consol);

			AssertContents(consol, pivotPK, ContentWithoutEquipment);

			consol.Shipments.RemoveAll();
			CreateBasicGoodsAndEquipment(consol);

			AssertContents(consol, pivotPK, ContentWithEquipment);
		}

		const string ContentWithoutEquipment = @"[2,4] Operational Port
[2,16] Direction
[2,24] Mode
[2,28] Advanced Logistics Port Order
[3,4] DEBRV - Bremerhaven
[3,16] Export
[3,24] FCL
[4,4] Origin
[4,16] Destination
[5,4] DEBRV - Bremerhaven
[5,16] NLRTM - Rotterdam
[6,4] Freight Forwarder Ref
[6,16] Booking Reference
[6,28] BOL Number
[6,40] ALPO Reference
[7,4] C00002000
[7,16] BookingReference
[7,28] BOL_Reference
[8,4] Port of Load
[8,16] ETD
[8,28] Port of Discharge
[8,40] ETA
[9,4] DEBRV - Bremerhaven
[9,16] 23-Jan-2021 07:35
[9,28] NLRTM - Rotterdam
[9,40] 25-Jan-2021 15:55
[10,4] SIS Number
[10,16] Vessel
[10,28] Lloyds/IMO
[10,40] Voyage
[11,16] MSC UBERTY
[11,28] 489541
[11,40] 12401
[12,4] Pre-Carriage Mode
[12,16] Pre-Carriage ID
[12,28] Marks: marks & numbers
[13,4] RAI
[13,16] COUCH124
[14,4] ALPO User Id
[17,4] Parties
[18,4] Sending Forwarder
[18,16] Carrier
[18,28] Sending Party
[18,40] Warehouse
[19,4] SENDING FORWARDER
[19,16] MAERSK
[19,28] EDI CUSTOMS BROKERS
[19,40] I'M HANDLING THE STUFF TO BE SEND
[20,4] UNIT 13
[20,16] UNIT 13
[20,28] 10 HUTCHESON STREET
[20,40] UNIT 200
[21,4] 4 LOST LANE
[21,16] 4 LOST LANE
[21,28] ALBION  QLD
[21,40] 55 WHY LANE
[22,4] ANTWERP
[22,12] VAN
[22,16] AALBORG
[22,40] ANTWERP
[22,48] VAN
[23,4] BELGIUM
[23,12] 2000
[23,16] BELGIUM
[23,24] 2000
[23,28] AUSTRALIA
[23,36] 4010
[23,40] BELGIUM
[23,48] 2000
[24,4] EORI Number
[24,12] Suffix
[24,16] Carrier Code
[24,40] Warehouse Code
[27,4] Goods Details
[28,4] S00001816
[28,16] Origin: AUSYD
[28,28] Consignor: 
[28,40] EORI Number
[28,48] Suffix
[29,4] Goods Value: 0.00
[29,16] Destination: BEANR
[29,28] Consignee: 
[30,4]    Packs:
[30,8] 8
[30,14] PLT
[30,16] Marks: marks & numbers
[30,34] Goods Description: ROOF COVERING
[31,8] 18.000
[31,14] KG
[32,8] 9.000
[32,14] M3
[33,4] Commodity Code: 1248
[33,16] Outturn Comments: Outturn Comment
[34,4] HC: 
[35,16] Entry Type
[35,22] Document Number
[37,3] DG:
[37,16] 0503b, AIR BAG MODULES, SL1, II,  class 1.4G (85.00 C C)  Quantity: 6BOX, Weight: 142KG, Volume: 9M3
[37,40] Net Explosive Weight (KG)
[38,40] 0.000
[39,4]    Packs:
[39,8] 16
[39,14] PLT
[39,16] Marks: marks & numbers
[39,34] Goods Description: ROOF COVERING ADDITIONAL PARTS
[40,8] 36.000
[40,14] KG
[41,8] 18.000
[41,14] M3
[42,4] Commodity Code: 1248
[42,16] Outturn Comments: Outturn Comment 2
[43,4] HC: 
[44,16] Entry Type
[44,22] Document Number
[48,42] Created By";

		const string ContentWithEquipment = @"[2,4] Operational Port
[2,16] Direction
[2,24] Mode
[2,28] Advanced Logistics Port Order
[3,4] DEBRV - Bremerhaven
[3,16] Export
[3,24] FCL
[4,4] Origin
[4,16] Destination
[5,4] DEBRV - Bremerhaven
[5,16] NLRTM - Rotterdam
[6,4] Freight Forwarder Ref
[6,16] Booking Reference
[6,28] BOL Number
[6,40] ALPO Reference
[7,4] C00002000
[7,16] BookingReference
[7,28] BOL_Reference
[8,4] Port of Load
[8,16] ETD
[8,28] Port of Discharge
[8,40] ETA
[9,4] DEBRV - Bremerhaven
[9,16] 23-Jan-2021 07:35
[9,28] NLRTM - Rotterdam
[9,40] 25-Jan-2021 15:55
[10,4] SIS Number
[10,16] Vessel
[10,28] Lloyds/IMO
[10,40] Voyage
[11,16] MSC UBERTY
[11,28] 489541
[11,40] 12401
[12,4] Pre-Carriage Mode
[12,16] Pre-Carriage ID
[12,28] Marks: 
[13,4] RAI
[13,16] COUCH124
[14,4] ALPO User Id
[17,4] Parties
[18,4] Sending Forwarder
[18,16] Carrier
[18,28] Sending Party
[18,40] Warehouse
[19,4] SENDING FORWARDER
[19,16] MAERSK
[19,28] EDI CUSTOMS BROKERS
[19,40] I'M HANDLING THE STUFF TO BE SEND
[20,4] UNIT 13
[20,16] UNIT 13
[20,28] 10 HUTCHESON STREET
[20,40] UNIT 200
[21,4] 4 LOST LANE
[21,16] 4 LOST LANE
[21,28] ALBION  QLD
[21,40] 55 WHY LANE
[22,4] ANTWERP
[22,12] VAN
[22,16] AALBORG
[22,40] ANTWERP
[22,48] VAN
[23,4] BELGIUM
[23,12] 2000
[23,16] BELGIUM
[23,24] 2000
[23,28] AUSTRALIA
[23,36] 4010
[23,40] BELGIUM
[23,48] 2000
[24,4] EORI Number
[24,12] Suffix
[24,16] Carrier Code
[24,40] Warehouse Code
[27,4] Goods and Equipment Details 
[28,4] Container
[28,16] ISO Type
[28,22] Empty
[28,28] Net (KG)
[28,34] Tare (KG)
[28,40] Gross (KG)
[28,46] Volume (M3)
[29,4] MSCU1245787
[29,16] 22P2
[29,22] ☐ Empty
[29,28] 18.000
[29,34] 954.000
[29,40] 972.000
[29,46] 9.000
[30,16] Seal 1: Seal1
[30,28] Seal 2: Seal2
[30,40] Seal 3: Seal3
[31,4] S00001816
[31,16] Origin: AUSYD
[31,28] Consignor: Test2 Name
[31,40] EORI Number
[31,48] Suffix
[32,4] Goods Value: 0.00
[32,16] Destination: BEANR
[32,28] Consignee: Test1 Name
[33,4]    Packs:
[33,8] 8
[33,14] PLT
[33,16] Marks: MarksAndNosPL1
[33,34] Goods Description: ROOF COVERING
[34,8] 18.000
[34,14] KG
[35,8] 9.000
[35,14] M3
[36,4] Commodity Code: 1234
[36,16] Outturn Comments: Outturn Comment
[37,4] HC: HC Code
[38,16] Entry Type
[38,22] Document Number
[39,22] EXP001
[40,3] DG:
[40,16] 0503b, AIR BAG MODULES, SL1, II,  class 1.4G (85.00 C C)
Quantity: 6BOX, Weight: 142KG, Volume: 9M3
[40,40] Net Explosive Weight (KG)
[41,40] 0.000
[44,42] Created By";

		#region Implementation

		ForwardingConsol CreateConsol(string loadPort = "FRPAR", string dischargePort = "BEANR")
		{
			CreateRefVessels();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			CreateAddresses(consol);

			AddExtraTransport(consol, Constants.TransportPlanningType.PreCarriage, "RAI", "DEAAH", "DEBRV", legOrder: 1);
			AddMainTransport(consol, "DEBRV", "NLRTM", legOrder: 2);

			return consol;
		}

		void AddMainTransport(ForwardingConsol consol, string loadPort, string discPort, byte legOrder = 2)
		{
			var mainTransport = consol.Transports.AddNew();
			mainTransport.JW_LegOrder = legOrder;
			mainTransport.JW_TransportMode = Constants.TransportModes.Sea;
			mainTransport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = loadPort;
			mainTransport.JW_RL_NKDiscPort = discPort;
			mainTransport.JW_ETD = new ZDateTime(2021, 01, 23, 7, 35, 00);
			mainTransport.JW_ETA = new ZDateTime(2021, 01, 25, 15, 55, 00);
			mainTransport.JW_Vessel = "MSC UBERTY";
			mainTransport.JW_VoyageFlight = "12401";
		}

		void AddExtraTransport(ForwardingConsol consol, ZString transportType, ZString transportMode, string loadPort, string discPort, byte legOrder)
		{
			var preCarriageTransport = consol.Transports.AddNew();
			preCarriageTransport.JW_LegOrder = legOrder;
			preCarriageTransport.JW_TransportMode = transportMode;
			preCarriageTransport.JW_TransportType = transportType;
			preCarriageTransport.JW_RL_NKLoadPort = loadPort;
			preCarriageTransport.JW_RL_NKDiscPort = discPort;
			preCarriageTransport.JW_ETD = new ZDateTime(2021, 01, 21, 7, 35, 00);
			preCarriageTransport.JW_ETA = new ZDateTime(2021, 01, 22, 15, 55, 00);
			switch (transportMode)
			{
				case Constants.TransportModes.Road:
					preCarriageTransport.JW_Vessel = "TRAILER";
					preCarriageTransport.JW_VoyageFlight = "1-TRU-CK1";
					break;
				case Constants.TransportModes.Rail:
					preCarriageTransport.JW_Vessel = "JOURNEY REF";
					preCarriageTransport.JW_VoyageFlight = "COUCH124";
					break;
				case Constants.TransportModes.Sea:
					preCarriageTransport.JW_Vessel = "MSC POOLSTER";
					preCarriageTransport.JW_VoyageFlight = "124";
					break;
			}
		}

		void CreateRefVessels()
		{
			var vesselSea = Factory.NewWithValidTestData<RefVessel>();
			vesselSea.RV_Name = "COSCO NEBULA";
			vesselSea.RV_LloydsNumber = "9795622";
			vesselSea.RV_VesselType = "CV";
			vesselSea.RV_RN_NKCountryOfReg = "HK";
			vesselSea.RV_RadioCallSign = "VRRW8";

			var vesselSea2 = Factory.NewWithValidTestData<RefVessel>();
			vesselSea2.RV_Name = "MSC UBERTY";
			vesselSea2.RV_LloydsNumber = "489541";
			vesselSea2.RV_VesselType = "CV";
			vesselSea2.RV_RN_NKCountryOfReg = "DE";
			vesselSea2.RV_RadioCallSign = "OP5DR";

			var vesselBarge = Factory.NewWithValidTestData<RefVessel>();
			vesselBarge.RV_Name = "MSC POOLSTER";
			vesselBarge.RV_VesselType = "BA";
			vesselBarge.RV_RN_NKCountryOfReg = "BE";
			vesselBarge.RV_RadioCallSign = "OT5325";
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "Sending Forwarder";
			sendingForwarder.OH_RL_NKClosestPort = "BEANR";
			sendingForwarder.MainAddress.Address1 = "Unit 13";
			sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
			sendingForwarder.MainAddress.City = "Antwerp";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "Receiving Forwarder";
			receivingForwarder.OH_RL_NKClosestPort = "BEANR";
			receivingForwarder.MainAddress.Address1 = "Unit 13";
			receivingForwarder.MainAddress.Address2 = "4 Lost Lane";
			receivingForwarder.MainAddress.City = "Antwerp";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_FullName = "I'm Handling the Stuff to be send";
			departureCTOAddress.OH_RL_NKClosestPort = "BEANR";
			departureCTOAddress.MainAddress.Address1 = "Unit 200";
			departureCTOAddress.MainAddress.Address2 = "55 Why Lane";
			departureCTOAddress.MainAddress.City = "Antwerp";
			departureCTOAddress.MainAddress.Postcode = "2000";
			departureCTOAddress.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var arrivalCTOAddress = Factory.New<OrgHeader>();
			arrivalCTOAddress.OH_FullName = "I'm Handling the Stuff to be received";
			arrivalCTOAddress.OH_RL_NKClosestPort = "BEANR";
			arrivalCTOAddress.MainAddress.Address1 = "Unit 200";
			arrivalCTOAddress.MainAddress.Address2 = "55 Why Lane";
			arrivalCTOAddress.MainAddress.City = "Antwerp";
			arrivalCTOAddress.MainAddress.Postcode = "2000";
			arrivalCTOAddress.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ArrivalCTOAddress = arrivalCTOAddress.MainAddress.PK;
		}

		void CreateBasicGoodsAndEquipment(ForwardingConsol consol)
		{
			var container = CreateContainer(consol);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TS1";
			consignee.OH_FullName = "Test1 Name";

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TS2";
			consignor.OH_FullName = "Test2 Name";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001816";
			shipment1.JS_HouseBill = "S00001816";
			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment1.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "BEANR";
			shipment1.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment1.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment1.JS_ShippedOnBoard = "SHP";
			shipment1.JS_ShippedOnBoardDate = ZDate.Today;
			shipment1.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment1.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment1.JS_GoodsDescription = "goods description";
			shipment1.JS_MarksAndNumbers = "marks & numbers";
			shipment1.JS_BookingReference = "BKG000001";
			shipment1.JS_NoOriginalBills = 1;
			shipment1.JS_NoCopyBills = 2;
			shipment1.ConsigneePK = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;

			shipment1.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 8;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 18;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 9;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Description = "ROOF COVERING";
			packline1.JL_OutturnComment = "Outturn Comment";
			packline1.JL_MarksAndNumbers = "MarksAndNosPL1";
			packline1.JL_DetailedDescription = "";
			packline1.JL_ContainerPackingOrder = 3;
			packline1.JL_Calc_ContainerNumber = "MSCU1245787";
			packline1.JL_JC = container.PK;
			packline1.JL_RH_NKCommodityCode = "1248";
			packline1.JL_RefNumber = "MRN00001";
			packline1.JL_ImportRefNumber = "IMP001";
			packline1.JL_ExportRefNumber = "EXP001";
			packline1.JL_HarmonisedCode = "HC Code";

			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "1248";
			var map = commodity.RefCommodityCodeMaps.AddNew();
			map.LC_RH_NKCommodityCode = "1248";
			map.LC_LocalCode = "1234";
			map.LC_RN_NKCountry = Constants.CountryCodes.Germany;
			map.LC_LocalCodeProvider = DELocalCommodityCodeProviderList.Codes.DE_DBH;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0503", "b", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "0503";
				subs.DG_Variant = "b";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var undg1 = packline1.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.Substance.DG_ExceptedQuantityCode = "E0";
			undg1.Substance.DG_PG = "II";
			undg1.Substance.DG_PSN = "AIR BAG MODULES";
			undg1.Substance.DG_EMS = "F-I,S-S";
			undg1.Substance.DG_Class = "1.4G";
			undg1.Substance.DG_SubLabel1 = "SL1";

			undg1.DI_IsCombustible = false;
			undg1.DI_DGFlashPoint = 85m;
			undg1.DI_TechnicalName = "Airbag Mercedes C";
			undg1.DI_MPMarinePollutant = "";
			undg1.DI_DGVolume = 0m;
			undg1.DI_UnitOfVolume = "M3";
			undg1.DI_DGWeight = 142m;
			undg1.DI_UnitOfWeight = "KG";
			undg1.DI_IsLimitedQuantity = false;
			undg1.DI_PackageCount = 6;
			undg1.DI_F3_NKPackType = "BOX";
			undg1.DI_OC_DGContact = contact.PK;

			container.PackLines.Add(packline1);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001817";
			shipment2.JS_HouseBill = "S00001817";
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment2.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "BEANR";
			shipment2.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment2.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment2.JS_ShippedOnBoard = "SHP";
			shipment2.JS_ShippedOnBoardDate = ZDate.Today;
			shipment2.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment2.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment2.JS_GoodsDescription = "goods description 2";
			shipment2.JS_MarksAndNumbers = "marks & numbers 2";
			shipment2.JS_BookingReference = "BKG000001";
			shipment2.JS_NoOriginalBills = 1;
			shipment2.JS_NoCopyBills = 2;

			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var packline2 = shipment2.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 16;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 36;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 18;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Description = "ROOF COVERING EXTRA";
			packline2.JL_OutturnComment = "Outturn Comment";
			packline2.JL_MarksAndNumbers = "MarksAndNosPL2";
			packline2.JL_DetailedDescription = "";
			packline2.JL_ContainerPackingOrder = 3;
			packline2.JL_RH_NKCommodityCode = "1248";
			packline2.JL_RefNumber = "MRN00002";
			packline2.JL_Calc_ContainerNumber = "";
		}

		ForwardingContainer CreateContainer(ForwardingConsol consol)
		{
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_Code = "20GP";
			refContainer.RC_ISOType = "22P2";
			refContainer.RC_TareWeight = 954;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "MSCU1245787";
			container.JC_DeliveryMode = "CY/CY";
			container.JC_IsEmptyContainer = false;
			container.JC_GrossWeightUQ = "KG";
			container.JC_GrossWeight = 1950;
			container.JC_RC = refContainer.PK;
			container.JC_SealNum = "Seal1";
			container.JC_AdditionalSealNum = "Seal2";
			container.JC_Additional2SealNum = "Seal3";
			return container;
		}

		void CreateBasicGoodsWithoutEquipment(ForwardingConsol consol)
		{
			var shipment = CreateShipment(consol);
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 8;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 18;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 9;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Description = "ROOF COVERING";
			packline1.JL_OutturnComment = "Outturn Comment";
			packline1.JL_DetailedDescription = "";
			packline1.JL_ContainerPackingOrder = 3;
			packline1.JL_Calc_ContainerNumber = "MSCU1245787";
			packline1.JL_RH_NKCommodityCode = "1248";
			packline1.JL_RefNumber = "MRN00001";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0503", "b", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "0503";
				subs.DG_Variant = "b";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var undg1 = packline1.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.Substance.DG_ExceptedQuantityCode = "E0";
			undg1.Substance.DG_PG = "II";
			undg1.Substance.DG_PSN = "AIR BAG MODULES";
			undg1.Substance.DG_EMS = "F-I,S-S";
			undg1.Substance.DG_Class = "1.4G";
			undg1.Substance.DG_SubLabel1 = "SL1";

			undg1.DI_IsCombustible = true;
			undg1.DI_DGFlashPoint = 85m;
			undg1.DI_TechnicalName = "Airbag Mercedes C";
			undg1.DI_MPMarinePollutant = "";
			undg1.DI_DGVolume = 0m;
			undg1.DI_UnitOfVolume = "M3";
			undg1.DI_DGWeight = 142m;
			undg1.DI_UnitOfWeight = "KG";
			undg1.DI_IsLimitedQuantity = false;
			undg1.DI_PackageCount = 6;
			undg1.DI_F3_NKPackType = "BOX";
			undg1.DI_OC_DGContact = contact.PK;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 16;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 36;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 18;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Description = "ROOF COVERING ADDITIONAL PARTS";
			packline2.JL_OutturnComment = "Outturn Comment 2";
			packline2.JL_DetailedDescription = "";
			packline2.JL_ContainerPackingOrder = 3;
			packline2.JL_Calc_ContainerNumber = "MSCU1245787";
			packline2.JL_RH_NKCommodityCode = "1248";
			packline2.JL_RefNumber = "MRN00002";
		}

		ForwardingShipment CreateShipment(ForwardingConsol consol)
		{
			var shipment = consol.Shipments.AddNew();

			shipment.JS_UniqueConsignRef = "S00001816";
			shipment.JS_HouseBill = "S00001816";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BEANR";
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 2;

			return shipment;
		}

		#endregion
	}
}
