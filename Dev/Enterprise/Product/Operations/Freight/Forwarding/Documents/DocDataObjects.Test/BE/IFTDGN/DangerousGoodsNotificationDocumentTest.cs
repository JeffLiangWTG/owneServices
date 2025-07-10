using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DangerousGoodsNotificationDocumentTest : DocumentVisualizer.Testing.StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("BECOMP", true, true, "BEANR");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			var testCompany = testObjectCreator.CreateNewCompany("BE", "BE", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("ANR", "AntwerpBranche", testCompany, testOrgProxy);

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNS001", Core.Constants.CountryCodes.Belgium);
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI001", Core.Constants.CountryCodes.Belgium);
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN006", Core.Constants.CountryCodes.Belgium);

			Factory.Save();

			CreateRefVessels();

			using (testBranch.SetAsTemporaryContext())
			{
				var importConsol = CreateConsol();

				CreateLog(importConsol,
					Events.MessageAccepted,
					new ZDateTimeOffset(2020, 02, 02),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "Example DGN Security Number"));

				CreateAddresses(importConsol);
				AssertContents(importConsol, "Dangerous Goods Notification - Import", CreatContentImport());
			}

			using (testBranch.SetAsTemporaryContext())
			{
				var exportConsol = CreateConsol("EXPORT");

				CreateLog(exportConsol,
					Events.MessageAccepted,
					new ZDateTimeOffset(2020, 02, 02),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNExport),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "Example DGN Security Number"));

				CreateAddresses(exportConsol);
				AssertContents(exportConsol, "Dangerous Goods Notification - Export", CreateContentExport());
			}
		}

		string CreatContentImport() => $@"[2,4] Consol Number
[2,16] BOL
[2,27] 
Notification Import
[2,28] Dangerous Goods Notification
[3,4] C00002000
[3,16] BOL_Reference
[4,4] Carrier Booking Ref.
[4,16] DGN security Number
[5,4] BookingReference
[5,16] Example DGN Security Number
[7,4] Goods and Equipment Details 
[8,4] Container
[8,16] Pack Count in Container
[9,4] MSCU1245787
[9,16] 5
[10,4] #
[10,6] DG Substance
[10,16] IMO Class
[10,22] Packing Grp
[10,28] Packs
[10,34] Pack Type
[10,40] Weight
[11,4] 1
[11,6] 0503b
[11,16] 1.4G
[11,22] II
[11,28] 6
[11,34] BOX
[11,40] 142.000
[11,48] KG
[12,6] Technical Name
[12,22] Marine Poll.
[12,28] Goods Description
[12,46] DG Reg. Code
[13,6] Airbag Mercedes C
[13,28] goods description
[13,46] IMO
[14,6] Flash Point (C)
[14,16] Limited Qty
[14,22] Excepted Qty
[14,28] Emergency Schedule Code
[14,40] Medical First Aid Guide
[15,6] 85.00 C
[15,16] ☐
[15,22] ☐
[15,28] Fire: F-I, Spillage: S-S
[16,4] #
[16,6] DG Substance
[16,16] IMO Class
[16,22] Packing Grp
[16,28] Packs
[16,34] Pack Type
[16,40] Weight
[17,4] 2
[17,6] 0453a
[17,16] 1.4G
[17,22] II
[17,28] 9
[17,34] BOX
[17,40] 213.000
[17,48] KG
[18,6] Technical Name
[18,22] Marine Poll.
[18,28] Goods Description
[18,46] DG Reg. Code
[19,6] Ejection Seat
[19,28] goods description
[19,46] IMO
[20,6] Flash Point (C)
[20,16] Limited Qty
[20,22] Excepted Qty
[20,28] Emergency Schedule Code
[20,40] Medical First Aid Guide
[21,6] 670.00 C
[21,16] ☐
[21,22] ☐
[21,28] Fire: F-B, Spillage: S-X
[22,4] #
[22,6] DG Substance
[22,16] IMO Class
[22,22] Packing Grp
[22,28] Packs
[22,34] Pack Type
[22,40] Weight
[23,4] 3
[23,6] 0503b
[23,16] 1.4G
[23,22] II
[23,28] 2
[23,34] BOX
[23,40] 120.000
[23,48] KG
[24,6] Technical Name
[24,22] Marine Poll.
[24,28] Goods Description
[24,46] DG Reg. Code
[25,6] Airbag Mercedes B
[25,28] AIR BAG Detailed Description
[25,46] IMO
[26,6] Flash Point (C)
[26,16] Limited Qty
[26,22] Excepted Qty
[26,28] Emergency Schedule Code
[26,40] Medical First Aid Guide
[27,6] 210.00 C
[27,16] ☑
[27,22] ☐
[27,28] Fire: F-I, Spillage: S-S
[28,4] Container
[28,16] Pack Count in Container
[29,4] MSCU8757656
[29,16] 6
[30,4] #
[30,6] DG Substance
[30,16] IMO Class
[30,22] Packing Grp
[30,28] Packs
[30,34] Pack Type
[30,40] Weight
[31,4] 1
[31,6] 2911b
[31,16] 7
[31,22] II
[31,28] 12
[31,34] BOX
[31,40] 55.000
[31,48] KG
[32,6] Technical Name
[32,22] Marine Poll.
[32,28] Goods Description
[32,46] DG Reg. Code
[33,6] Uranium
[33,22] M
[33,28] Experimental Fuel
[33,46] IMO
[34,6] Flash Point (C)
[34,16] Limited Qty
[34,22] Excepted Qty
[34,28] Emergency Schedule Code
[34,40] Medical First Aid Guide
[35,6] 450.00 C
[35,16] ☐
[35,22] ☐
[35,28] Fire: F-I, Spillage: S-S
[36,6] Radioactive Index
[36,16] Criticality Safety Index
[36,28] Radioactive Quantity
[36,40] Net Explosive Weight
[37,6] 0
[37,16] 0
[37,28] 0
[37,40] 0
[39,4] Port Details
[40,4] Ship's Stay No.
[40,12] Discharge Port
[40,20] Vessel ETA
[40,28] Vessel ETD
[40,36] Unloading Date
[40,44] Handling Instruction
[41,12] BEANR - Antwerpen
[41,20] 22-Oct-2020
[41,44] LDI
[43,4] Transport Details
[44,4] Vessel Type
[44,12] Vessel Name
[44,20] Voyage
[44,28] Lloyds/IMO
[44,36] Radio Call Sign
[44,44] Country Of Reg.
[45,4] CV
[45,12] COSCO NEBULA
[45,20] 85475
[45,28] 9795622
[45,36] VRRW8
[45,44] HK
[46,4] Load Port
[46,12] ETD
[46,20] Discharge Port
[46,28] ETA
[46,36] Carrier
[47,4] CNYTN - Yantian Pt
[47,12] 06-Oct-2020 08:45
[47,20] BEANR - Antwerpen
[47,28] 22-Oct-2020 12:15
[47,36] MAERSK
[49,4] Port Collection Details
[50,4] Transport Mode
[50,16] Vessel Name
[50,28] ENI Number
[50,40] Expected Departure at Port
[51,4] ROA
[51,16] TRAILER
[51,40] 23-Oct-2020 07:35
[54,42] Created By
[56,4] Additional Parties
[57,4] Forwarder
[57,16] Carrier
[57,28] Sending Party
[57,40] Arrival CTO
[58,4] RECEIVING FORWARDER
[58,16] MAERSK
[58,28] TEST COMPANY NAME
[58,40] I'M HANDLING THE STUFF TO BE RECEIVED
[59,4] UNIT 2
[59,16] UNIT 13
[59,28] 184 BOURKE ROAD
[59,40] UNIT 200
[60,4] 60 WHAT LANE
[60,16] 4 LOST LANE
[60,40] 55 WHY LANE
[61,4] ANTWERP
[61,12] VAN
[61,16] AALBORG
[61,28] ALEXANDRIA
[61,36] VAN
[61,40] ANTWERP
[61,48] VAN
[62,4] BELGIUM
[62,12] 2000
[62,16] BELGIUM
[62,24] 2000
[62,28] BELGIUM
[62,40] BELGIUM
[62,48] 2000
[63,4] Contact: Black Panther
Tel: 112 
Email: black_panther@marvel.com
[65,4] Forwarder's Port ID
[65,16] Carrier's Port ID
[65,28] Port ID
[65,34] DUNS Number
[65,40] Terminal Code
[66,4] PSN003
[66,16] PSN001
[66,28] PSN006
[66,34] DUNS001
[66,40] PSN005
[70,42] Created By
";

		string CreateContentExport() => $@"[2,4] Consol Number
[2,16] BOL
[2,27] 
Notification Import
[2,28] Dangerous Goods Notification
[3,4] C00002000
[3,16] BOL_Reference
[4,4] Carrier Booking Ref.
[4,16] DGN security Number
[5,4] BookingReference
[5,16] Example DGN Security Number
[7,4] Goods and Equipment Details 
[8,4] Container
[8,16] Pack Count in Container
[9,4] MSCU1245787
[9,16] 5
[10,4] #
[10,6] DG Substance
[10,16] IMO Class
[10,22] Packing Grp
[10,28] Packs
[10,34] Pack Type
[10,40] Weight
[11,4] 1
[11,6] 0503b
[11,16] 1.4G
[11,22] II
[11,28] 6
[11,34] BOX
[11,40] 142.000
[11,48] KG
[12,6] Technical Name
[12,22] Marine Poll.
[12,28] Goods Description
[12,46] DG Reg. Code
[13,6] Airbag Mercedes C
[13,28] goods description
[13,46] IMO
[14,6] Flash Point (C)
[14,16] Limited Qty
[14,22] Excepted Qty
[14,28] Emergency Schedule Code
[14,40] Medical First Aid Guide
[15,6] 85.00 C
[15,16] ☐
[15,22] ☐
[15,28] Fire: F-I, Spillage: S-S
[16,4] #
[16,6] DG Substance
[16,16] IMO Class
[16,22] Packing Grp
[16,28] Packs
[16,34] Pack Type
[16,40] Weight
[17,4] 2
[17,6] 0453a
[17,16] 1.4G
[17,22] II
[17,28] 9
[17,34] BOX
[17,40] 213.000
[17,48] KG
[18,6] Technical Name
[18,22] Marine Poll.
[18,28] Goods Description
[18,46] DG Reg. Code
[19,6] Ejection Seat
[19,28] goods description
[19,46] IMO
[20,6] Flash Point (C)
[20,16] Limited Qty
[20,22] Excepted Qty
[20,28] Emergency Schedule Code
[20,40] Medical First Aid Guide
[21,6] 670.00 C
[21,16] ☐
[21,22] ☐
[21,28] Fire: F-B, Spillage: S-X
[22,4] #
[22,6] DG Substance
[22,16] IMO Class
[22,22] Packing Grp
[22,28] Packs
[22,34] Pack Type
[22,40] Weight
[23,4] 3
[23,6] 0503b
[23,16] 1.4G
[23,22] II
[23,28] 2
[23,34] BOX
[23,40] 120.000
[23,48] KG
[24,6] Technical Name
[24,22] Marine Poll.
[24,28] Goods Description
[24,46] DG Reg. Code
[25,6] Airbag Mercedes B
[25,28] AIR BAG Detailed Description
[25,46] IMO
[26,6] Flash Point (C)
[26,16] Limited Qty
[26,22] Excepted Qty
[26,28] Emergency Schedule Code
[26,40] Medical First Aid Guide
[27,6] 210.00 C
[27,16] ☑
[27,22] ☐
[27,28] Fire: F-I, Spillage: S-S
[28,4] Container
[28,16] Pack Count in Container
[29,4] MSCU8757656
[29,16] 6
[30,4] #
[30,6] DG Substance
[30,16] IMO Class
[30,22] Packing Grp
[30,28] Packs
[30,34] Pack Type
[30,40] Weight
[31,4] 1
[31,6] 2911b
[31,16] 7
[31,22] II
[31,28] 12
[31,34] BOX
[31,40] 55.000
[31,48] KG
[32,6] Technical Name
[32,22] Marine Poll.
[32,28] Goods Description
[32,46] DG Reg. Code
[33,6] Uranium
[33,22] M
[33,28] Experimental Fuel
[33,46] IMO
[34,6] Flash Point (C)
[34,16] Limited Qty
[34,22] Excepted Qty
[34,28] Emergency Schedule Code
[34,40] Medical First Aid Guide
[35,6] 450.00 C
[35,16] ☐
[35,22] ☐
[35,28] Fire: F-I, Spillage: S-S
[36,6] Radioactive Index
[36,16] Criticality Safety Index
[36,28] Radioactive Quantity
[36,40] Net Explosive Weight
[37,6] 0
[37,16] 0
[37,28] 0
[37,40] 0
[39,4] Port Details
[40,4] Ship's Stay No.
[40,12] Load Port
[40,20] Vessel ETA
[40,28] Vessel ETD
[40,36] Loading Date
[40,44] Handling Instruction
[41,12] BEANR - Antwerpen
[41,28] 06-Oct-2020
[41,44] LLO
[43,4] Transport Details
[44,4] Vessel Type
[44,12] Vessel Name
[44,20] Voyage
[44,28] Lloyds/IMO
[44,36] Radio Call Sign
[44,44] Country Of Reg.
[45,4] CV
[45,12] COSCO NEBULA
[45,20] 85475
[45,28] 9795622
[45,36] VRRW8
[45,44] HK
[46,4] Load Port
[46,12] ETD
[46,20] Discharge Port
[46,28] ETA
[46,36] Carrier
[47,4] BEANR - Antwerpen
[47,12] 06-Oct-2020 08:45
[47,20] CNYTN - Yantian Pt
[47,28] 22-Oct-2020 12:15
[47,36] MAERSK
[49,4] Port Delivery Details
[50,4] Transport Mode
[50,16] Vessel Name
[50,28] ENI Number
[50,40] Expected Arrival at Port
[51,4] ROA
[51,16] TRAILER
[51,40] 04-Oct-2020 21:25
[54,42] Created By
[56,4] Additional Parties
[57,4] Forwarder
[57,16] Carrier
[57,28] Sending Party
[57,40] Departure CTO
[58,4] SENDING FORWARDER
[58,16] MAERSK
[58,28] TEST COMPANY NAME
[58,40] I'M HANDLING THE STUFF TO BE SEND
[59,4] UNIT 13
[59,16] UNIT 13
[59,28] 184 BOURKE ROAD
[59,40] UNIT 200
[60,4] 4 LOST LANE
[60,16] 4 LOST LANE
[60,40] 55 WHY LANE
[61,4] ANTWERP
[61,12] VAN
[61,16] AALBORG
[61,28] ALEXANDRIA
[61,36] VAN
[61,40] ANTWERP
[61,48] VAN
[62,4] BELGIUM
[62,12] 2000
[62,16] BELGIUM
[62,24] 2000
[62,28] BELGIUM
[62,40] BELGIUM
[62,48] 2000
[63,4] Contact: SPIDERMAN
Tel: 911 
Email: spider@marvel.com
[65,4] Forwarder's Port ID
[65,16] Carrier's Port ID
[65,28] Port ID
[65,34] DUNS Number
[65,40] Terminal Code
[66,4] PSN002
[66,16] PSN001
[66,28] PSN006
[66,34] DUNS001
[66,40] PSN004
[70,42] Created By
";

		#region Implement

		ForwardingConsol CreateConsol(string messageType = "IMPORT", string transportMode = Core.Constants.TransportModes.Road)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "FRPRA";
			consol.JK_RL_NKDischargePort = "BEANR";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			var carrierBookingRequest = consol.Notes.AddNew();
			carrierBookingRequest.ST_Description = PredefinedNoteTypes.Instance.CarrierBookingRequest.Description;
			carrierBookingRequest.ST_NoteText = "carrier booking request";

			CreateAddresses(consol);

			if (messageType.Equals("IMPORT"))
			{
				CreateImportTransports(consol, transportMode);
			}
			else if (messageType.Equals("EXPORT"))
			{
				CreateExportTransports(consol, transportMode);
			}

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "MSCU1245787";
			container1.JC_DeliveryMode = "CY/CY";
			container1.JC_IsShipperOwned = false;
			container1.JC_GrossWeightUQ = "KG";
			container1.JC_TareWeight = 1000;
			container1.JC_DunnageWeight = 1000;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP").PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "MSCU8757656";
			container2.JC_DeliveryMode = "CY/CY";
			container2.JC_IsShipperOwned = false;
			container2.JC_GrossWeightUQ = "KG";
			container2.JC_TareWeight = 1000;
			container2.JC_DunnageWeight = 1000;
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP").PK;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "MSCU1247856";
			container3.JC_DeliveryMode = "CY/CY";
			container3.JC_IsShipperOwned = false;
			container3.JC_GrossWeightUQ = "KG";
			container3.JC_TareWeight = 1000;
			container3.JC_DunnageWeight = 1000;
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP").PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00001816";
			shipment.JS_HouseBill = "S00001816";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BEANR";
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 2;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 1;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 18;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 0;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Description = "ROOF COVERING";
			packline1.JL_DetailedDescription = "";
			packline1.JL_ContainerPackingOrder = 3;
			packline1.JL_Calc_ContainerNumber = "MSCU1245787";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 1;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 15;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 0;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Description = "";
			packline2.JL_DetailedDescription = "";
			packline2.JL_ContainerPackingOrder = 4;
			packline2.JL_Calc_ContainerNumber = "MSCU1245787";

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
			var undg1 = packline2.UNDGs.AddNew();
			undg1.LinkDefault(subs);
			undg1.Substance.DG_ExceptedQuantityCode = "E0";
			undg1.Substance.DG_PG = "II";
			undg1.Substance.DG_PSN = "AIR BAG MODULES";
			undg1.Substance.DG_EMS = "F-I,S-S";
			undg1.Substance.DG_Class = "1.4G";
			undg1.Substance.DG_Standard = "IMO";

			undg1.DI_DGFlashPoint = 85m;
			undg1.DI_IsCombustible = true;
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

			var subs2 = UNDGSubstanceLoader.LoadSubstances(Factory, "0453", "a", "IMO").FirstOrDefault();
			if (subs2 == null)
			{
				subs2 = Factory.New<UNDGSubstance>();
				subs2.DG_UNNO = "0453";
				subs2.DG_Variant = "a";
				subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var undg2 = packline2.UNDGs.AddNew();
			undg2.LinkDefault(subs2);
			undg2.Substance.DG_ExceptedQuantityCode = "E1";
			undg2.Substance.DG_PG = "II";
			undg2.Substance.DG_PSN = "ROCKETS, LINE-THROWING";
			undg2.Substance.DG_EMS = "F-B,S-X";
			undg2.Substance.DG_Class = "1.4G";
			undg2.Substance.DG_Standard = "IMO";

			undg2.DI_TechnicalName = "Ejection Seat";
			undg2.DI_DGFlashPoint = 670.0m;
			undg2.DI_IsCombustible = true;
			undg2.DI_MPMarinePollutant = "";
			undg2.DI_DGVolume = 0m;
			undg2.DI_UnitOfVolume = "M3";
			undg2.DI_DGWeight = 213m;
			undg2.DI_UnitOfWeight = "KG";
			undg2.DI_IsLimitedQuantity = false;
			undg2.DI_PackageCount = 9;
			undg2.DI_F3_NKPackType = "BOX";
			undg2.DI_OC_DGContact = contact.PK;

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 2140;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 0;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Description = "AIR BAG";
			packline3.JL_DetailedDescription = "AIR BAG Detailed Description";
			packline3.JL_ContainerPackingOrder = 1;
			packline3.JL_Calc_ContainerNumber = "MSCU1245787";

			var undg3 = packline3.UNDGs.AddNew();
			undg3.LinkDefault(subs);
			undg3.Substance.DG_ExceptedQuantityCode = "E0";
			undg3.Substance.DG_PG = "II";
			undg3.Substance.DG_PSN = "AIR BAG MODULES";
			undg3.Substance.DG_EMS = "F-I,S-S";
			undg3.Substance.DG_Class = "1.4G";
			undg3.Substance.DG_Standard = "IMO";

			undg3.DI_TechnicalName = "Airbag Mercedes B";
			undg3.DI_IMOClass = "1.4G";
			undg3.DI_DGFlashPoint = 210.0m;
			undg3.DI_IsCombustible = false;
			undg3.DI_MPMarinePollutant = "";
			undg3.DI_DGVolume = 0m;
			undg3.DI_UnitOfVolume = "M3";
			undg3.DI_DGWeight = 120m;
			undg3.DI_UnitOfWeight = "KG";
			undg3.DI_IsLimitedQuantity = true;
			undg3.DI_PackageCount = 2;
			undg3.DI_F3_NKPackType = "BOX";
			undg3.DI_OC_DGContact = contact.PK;

			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 5;
			packline4.JL_F3_NKPackType = "PLT";
			packline4.JL_ActualWeight = 256;
			packline4.JL_ActualWeightUQ = "KG";
			packline4.JL_ActualVolume = 0;
			packline4.JL_ActualVolumeUQ = "M3";
			packline4.JL_Description = "DASHBOARD MERCEDES";
			packline4.JL_DetailedDescription = "";
			packline4.JL_ContainerPackingOrder = 2;
			packline4.JL_Calc_ContainerNumber = "MSCU8757656";

			var packline5 = shipment.OuterPackLines.AddNew();
			packline5.JL_PackageCount = 1;
			packline5.JL_F3_NKPackType = "PLT";
			packline5.JL_ActualWeight = 56;
			packline5.JL_ActualWeightUQ = "KG";
			packline5.JL_ActualVolume = 0;
			packline5.JL_ActualVolumeUQ = "M3";
			packline5.JL_Description = "Experimental Fuel";
			packline5.JL_DetailedDescription = "";
			packline5.JL_ContainerPackingOrder = 5;
			packline5.JL_Calc_ContainerNumber = "MSCU8757656";

			var subs3 = UNDGSubstanceLoader.LoadSubstances(Factory, "2911", "b", "IMO").FirstOrDefault();
			if (subs3 == null)
			{
				subs3 = Factory.New<UNDGSubstance>();
				subs3.DG_UNNO = "2911";
				subs3.DG_Variant = "b";
				subs3.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}

			var undg4 = packline5.UNDGs.AddNew();
			undg4.LinkDefault(subs3);
			undg4.Substance.DG_ExceptedQuantityCode = "E0";
			undg4.Substance.DG_SubLabel1 = "SP290";
			undg4.Substance.DG_SubLabel2 = "";
			undg4.Substance.DG_PG = "II";
			undg4.Substance.DG_PSN = "RADIOACTIVE MATERIAL, EXCEPTED PACKAGE - ARTICLES";
			undg4.Substance.DG_EMS = "F-I,S-S";
			undg4.Substance.DG_Class = "7";
			undg4.Substance.DG_Standard = "IMO";

			undg4.DI_TechnicalName = "Uranium";
			undg4.DI_DGFlashPoint = 450.0m;
			undg4.DI_IsCombustible = false;
			undg4.DI_MPMarinePollutant = "M";
			undg4.DI_DGVolume = 0m;
			undg4.DI_UnitOfVolume = "M3";
			undg4.DI_DGWeight = 55m;
			undg4.DI_UnitOfWeight = "KG";
			undg4.DI_IsLimitedQuantity = false;
			undg4.DI_PackageCount = 12;
			undg4.DI_F3_NKPackType = "BOX";
			undg4.DI_OC_DGContact = contact.PK;

			var packline6 = shipment.OuterPackLines.AddNew();
			packline6.JL_PackageCount = 12;
			packline6.JL_F3_NKPackType = "PLT";
			packline6.JL_ActualWeight = 18;
			packline6.JL_ActualWeightUQ = "KG";
			packline6.JL_ActualVolume = 0;
			packline6.JL_ActualVolumeUQ = "M3";
			packline6.JL_Description = "Manual";
			packline6.JL_DetailedDescription = "";
			packline6.JL_ContainerPackingOrder = 6;
			packline6.JL_Calc_ContainerNumber = "MSCU1247856";

			container1.PackLines.Add(packline1);
			container1.PackLines.Add(packline2);
			container1.PackLines.Add(packline3);
			container2.PackLines.Add(packline4);
			container2.PackLines.Add(packline5);
			container3.PackLines.Add(packline6);

			return consol;
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

			var carrierPortSystemNumber = carrier.CustomsCodes.AddNew();
			carrierPortSystemNumber.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			carrierPortSystemNumber.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
			carrierPortSystemNumber.OK_CustomsRegNo = "PSN001";

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "Sending Forwarder";
			sendingForwarder.OH_RL_NKClosestPort = "BEANR";
			sendingForwarder.MainAddress.Address1 = "Unit 13";
			sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
			sendingForwarder.MainAddress.City = "Antwerp";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";

			var sendingForwarderContact = sendingForwarder.Contacts.AddNew();
			sendingForwarderContact.OC_ContactName = "SPIDERMAN";
			sendingForwarderContact.OC_Email = "spider@marvel.com";
			sendingForwarderContact.OC_Phone = "911";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;

			var sendingForwarderPortSystemNumber = sendingForwarder.CustomsCodes.AddNew();
			sendingForwarderPortSystemNumber.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			sendingForwarderPortSystemNumber.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
			sendingForwarderPortSystemNumber.OK_CustomsRegNo = "PSN002";

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "Receiving Forwarder";
			receivingForwarder.OH_RL_NKClosestPort = "BEANR";
			receivingForwarder.MainAddress.Address1 = "Unit 2";
			receivingForwarder.MainAddress.Address2 = "60 What Lane";
			receivingForwarder.MainAddress.City = "Antwerp";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";

			var receivingForwarderContact = receivingForwarder.Contacts.AddNew();
			receivingForwarderContact.OC_ContactName = "Black Panther";
			receivingForwarderContact.OC_Email = "black_panther@marvel.com";
			receivingForwarderContact.OC_Phone = "112";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;

			var receivingForwarderPortSystemNumber = receivingForwarder.CustomsCodes.AddNew();
			receivingForwarderPortSystemNumber.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			receivingForwarderPortSystemNumber.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
			receivingForwarderPortSystemNumber.OK_CustomsRegNo = "PSN003";

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_FullName = "I'm Handling the Stuff to be send";
			departureCTOAddress.OH_RL_NKClosestPort = "BEANR";
			departureCTOAddress.MainAddress.Address1 = "Unit 200";
			departureCTOAddress.MainAddress.Address2 = "55 Why Lane";
			departureCTOAddress.MainAddress.City = "Antwerp";
			departureCTOAddress.MainAddress.Postcode = "2000";
			departureCTOAddress.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var departureCTOPortSystemNumber = departureCTOAddress.CustomsCodes.AddNew();
			departureCTOPortSystemNumber.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			departureCTOPortSystemNumber.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
			departureCTOPortSystemNumber.OK_CustomsRegNo = "PSN004";

			var arrivalCTOAddress = Factory.New<OrgHeader>();
			arrivalCTOAddress.OH_FullName = "I'm Handling the Stuff to be received";
			arrivalCTOAddress.OH_RL_NKClosestPort = "BEANR";
			arrivalCTOAddress.MainAddress.Address1 = "Unit 200";
			arrivalCTOAddress.MainAddress.Address2 = "55 Why Lane";
			arrivalCTOAddress.MainAddress.City = "Antwerp";
			arrivalCTOAddress.MainAddress.Postcode = "2000";
			arrivalCTOAddress.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ArrivalCTOAddress = arrivalCTOAddress.MainAddress.PK;

			var arrivalCTOPortSystemNumber = arrivalCTOAddress.CustomsCodes.AddNew();
			arrivalCTOPortSystemNumber.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			arrivalCTOPortSystemNumber.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
			arrivalCTOPortSystemNumber.OK_CustomsRegNo = "PSN005";

			var currentBranchMainAddressHeaderSystemNumber = GlbBranch.CurrentBranch.OrgProxy.MainAddress.Header.CustomsCodes.AddNew();
			currentBranchMainAddressHeaderSystemNumber.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			currentBranchMainAddressHeaderSystemNumber.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
			currentBranchMainAddressHeaderSystemNumber.OK_CustomsRegNo = "PSN006";
		}

		void CreateRefVessels()
		{
			var vesselSea = Factory.NewWithValidTestData<RefVessel>();
			vesselSea.RV_Name = "COSCO NEBULA";
			vesselSea.RV_LloydsNumber = "9795622";
			vesselSea.RV_VesselType = "CV";
			vesselSea.RV_RN_NKCountryOfReg = "HK";
			vesselSea.RV_RadioCallSign = "VRRW8";

			var vesselBarge = Factory.NewWithValidTestData<RefVessel>();
			vesselBarge.RV_Name = "MSC POOLSTER";
			vesselBarge.RV_VesselType = "BA";
			vesselBarge.RV_RN_NKCountryOfReg = "BE";
			vesselBarge.RV_RadioCallSign = "OT5325";
		}

		void CreateImportTransports(ForwardingConsol consol, ZString transportMode)
		{
			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNYTN";
			transport.JW_RL_NKDiscPort = "BEANR";
			transport.JW_Vessel = "COSCO NEBULA";
			transport.JW_VoyageFlight = "85475";
			transport.JW_ETD = new ZDateTime(2020, 10, 6, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2020, 10, 22, 12, 15, 00);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = transportMode;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transport2.JW_RL_NKLoadPort = "BEANR";
			transport2.JW_RL_NKDiscPort = "BEWJG";
			transport2.JW_ETD = new ZDateTime(2020, 10, 23, 7, 35, 00);
			transport2.JW_ETA = new ZDateTime(2020, 10, 25, 15, 55, 00);
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Road:
					transport2.JW_Vessel = "TRAILER";
					transport2.JW_VoyageFlight = "1-TRU-CK1";
					break;
				case Core.Constants.TransportModes.Rail:
					transport2.JW_Vessel = "JOURNEY REF";
					transport2.JW_VoyageFlight = "124575";
					break;
				case Core.Constants.TransportModes.Sea:
					transport2.JW_Vessel = "MSC POOLSTER";
					transport2.JW_VoyageFlight = "124";
					break;
			}
		}

		void CreateExportTransports(ForwardingConsol consol, ZString transportMode)
		{
			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 2;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "BEANR";
			transport.JW_RL_NKDiscPort = "CNYTN";
			transport.JW_Vessel = "COSCO NEBULA";
			transport.JW_VoyageFlight = "85475";
			transport.JW_ETD = new ZDateTime(2020, 10, 6, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2020, 10, 22, 12, 15, 00);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 1;
			transport2.JW_TransportMode = transportMode;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			transport2.JW_RL_NKLoadPort = "BEWJG";
			transport2.JW_RL_NKDiscPort = "BEANR";
			transport2.JW_ETD = new ZDateTime(2020, 10, 2, 11, 43, 00);
			transport2.JW_ETA = new ZDateTime(2020, 10, 4, 21, 25, 00);
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Road:
					transport2.JW_Vessel = "TRAILER";
					transport2.JW_VoyageFlight = "1-TRU-CK1";
					break;
				case Core.Constants.TransportModes.Rail:
					transport2.JW_Vessel = "JOURNEY REF";
					transport2.JW_VoyageFlight = "124575";
					break;
				case Core.Constants.TransportModes.Sea:
					transport2.JW_Vessel = "MSC POOLSTER";
					transport2.JW_VoyageFlight = "124";
					break;
			}
		}

		void CreateLog(ForwardingConsol consol, Event @event, ZDateTimeOffset time, params KeyValuePair<string, string>[] parameters)
		{
			consol.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time, "", parameters);
			Thread.Sleep(1);
		}

		#endregion
	}
}
