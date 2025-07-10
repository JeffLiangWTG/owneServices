using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.ZA
{
	sealed class TPTServiceInstructionDocumentTest : DocumentVisualizer.Testing.StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			AssertContents(ExportConsol, "Service Instruction - Shipping Order (Exports)", CreateShippingOrderExpectedContentWithCarrierCode(string.Empty));

			AssertContents(ImportConsol, "Service Instruction - Landing Order (Imports)", CreateLandingOrderExpectedContentWithCarrierCode(string.Empty));
		}

		[SnailTest]
		public void TestLandingOrder_WhenCarrierOnlyHasCCC_ThenCarrierCodeIsCCC()
		{
			var carrier = ImportConsol.ShippingLine;
			AddSouthAfricaCustomsCodeToCarrier(carrier, OrgCusCode.CodeTypes.CarrierCode, "CCC");

			AssertContents(ImportConsol, "Service Instruction - Landing Order (Imports)", CreateLandingOrderExpectedContentWithCarrierCode("CCC"));
		}

		[SnailTest]
		public void TestShippingOrder_WhenCarrierOnlyHasCCC_ThenCarrierCodeIsCCC()
		{
			var carrier = ExportConsol.ShippingLine;
			AddSouthAfricaCustomsCodeToCarrier(carrier, OrgCusCode.CodeTypes.CarrierCode, "CCC");

			AssertContents(ExportConsol, "Service Instruction - Shipping Order (Exports)", CreateShippingOrderExpectedContentWithCarrierCode("CCC"));
		}

		[SnailTest]
		public void TestLandingOrder_WhenCarrierOnlyHasTNP_ThenCarrierCodeIsTNP()
		{
			var carrier = ImportConsol.ShippingLine;
			AddSouthAfricaCustomsCodeToCarrier(carrier, OrgCusCode.SouthAfricaCodeTypes.TNP, "TNP");

			AssertContents(ImportConsol, "Service Instruction - Landing Order (Imports)", CreateLandingOrderExpectedContentWithCarrierCode("TNP"));
		}

		[SnailTest]
		public void TestShippingOrder_WhenCarrierOnlyHasTNP_ThenCarrierCodeIsTNP()
		{
			var carrier = ExportConsol.ShippingLine;
			AddSouthAfricaCustomsCodeToCarrier(carrier, OrgCusCode.SouthAfricaCodeTypes.TNP, "TNP");

			AssertContents(ExportConsol, "Service Instruction - Shipping Order (Exports)", CreateShippingOrderExpectedContentWithCarrierCode("TNP"));
		}

		[SnailTest]
		public void TestLandingOrder_WhenCarrierHasCCCAndTNP_ThenCarrierCodeIsTNP()
		{
			var carrier = ImportConsol.ShippingLine;
			AddSouthAfricaCustomsCodeToCarrier(carrier, OrgCusCode.CodeTypes.CarrierCode, "CCC");
			AddSouthAfricaCustomsCodeToCarrier(carrier, OrgCusCode.SouthAfricaCodeTypes.TNP, "TNP");

			AssertContents(ImportConsol, "Service Instruction - Landing Order (Imports)", CreateLandingOrderExpectedContentWithCarrierCode("TNP"));
		}

		[SnailTest]
		public void TestShippingOrder_WhenCarrierHasCCCAndTNP_ThenCarrierCodeIsTNP()
		{
			var carrier = ExportConsol.ShippingLine;
			AddSouthAfricaCustomsCodeToCarrier(carrier, OrgCusCode.CodeTypes.CarrierCode, "CCC");
			AddSouthAfricaCustomsCodeToCarrier(carrier, OrgCusCode.SouthAfricaCodeTypes.TNP, "TNP");

			AssertContents(ExportConsol, "Service Instruction - Shipping Order (Exports)", CreateShippingOrderExpectedContentWithCarrierCode("TNP"));
		}

		void AddSouthAfricaCustomsCodeToCarrier(OrgHeader orgHeader, string codeType, string code)
		{
			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
			customsCode.OK_CodeType = codeType;
			customsCode.OK_CustomsRegNo = code;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TemporaryEnvironment = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica);

			ExportConsol = PrepareConsolForShippingOrder();
			ImportConsol = PrepareConsolForLandingPrder();
		}

		protected override void TearDown()
		{
			base.TearDown();

			TemporaryEnvironment.Dispose();
		}

		IDisposable TemporaryEnvironment;

		ForwardingConsol ExportConsol;
		ForwardingConsol ImportConsol;

		ForwardingConsol PrepareConsolForLandingPrder()
		{
			var importConsol = Factory.New<ForwardingConsol>();
			importConsol.JK_TransportMode = Constants.TransportModes.Sea;
			importConsol.JK_RL_NKLoadPort = "AUBNE";
			importConsol.JK_RL_NKDischargePort = "ZA2WC";
			importConsol.JK_MasterBillNum = "WAYBILL0002";

			var container = importConsol.Containers.AddNew();
			container.FillWithValidTestData();

			var terminalAddress = Factory.NewWithValidTestData<OrgAddress>();
			terminalAddress.Header.OH_FullName = "Terminal Organisation";

			var terminalAccountNo = terminalAddress.Header.CustomsCodes.AddNew();
			terminalAccountNo.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
			terminalAccountNo.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.TPT;
			terminalAccountNo.OK_CustomsRegNo = "6666666";

			var exportCarrier = Factory.NewWithValidTestData<OrgAddress>();

			var receivingForwarder = Factory.NewWithValidTestData<OrgAddress>();
			receivingForwarder.OA_CompanyNameOverride = "RECEIVING FORWARDER CO.";
			receivingForwarder.OA_Phone = "0123456";
			receivingForwarder.OA_Email = "receivingForwarder@email.com";

			receivingForwarder.Header.FillWithValidTestData();

			var receivingFrowarderAccountNo = receivingForwarder.Header.CustomsCodes.AddNew();
			receivingFrowarderAccountNo.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
			receivingFrowarderAccountNo.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.TPT;
			receivingFrowarderAccountNo.OK_CustomsRegNo = "123456";

			importConsol.JK_OA_ReceivingForwarderAddress = receivingForwarder.PK;

			var tptTransporter = Factory.New<OrgHeader>();
			tptTransporter.OH_Code = "LTRANSPORTER";
			tptTransporter.OH_FullName = "TPT Landing Transporter";

			var tptTransporterAddress = Factory.New<OrgAddress>();
			tptTransporterAddress.Address1 = "transporter address 1";
			tptTransporterAddress.OA_OH = tptTransporter.PK;

			var tptTransporterAccountNo = tptTransporter.CustomsCodes.AddNew();
			tptTransporterAccountNo.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
			tptTransporterAccountNo.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.TPT;
			tptTransporterAccountNo.OK_CustomsRegNo = "123456";

			importConsol.JK_OA_ArrivalUnpackCFSTransportAddress = tptTransporterAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CNEORG";
			consignee.OH_FullName = "Consignee Co.";

			var consigneeAccountNo = consignee.CustomsCodes.AddNew();
			consigneeAccountNo.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
			consigneeAccountNo.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.TPT;
			consigneeAccountNo.OK_CustomsRegNo = "3333333";

			var importTransportLeg = importConsol.Transports.Cast<Transport>().First();
			importTransportLeg.JW_TransportMode = Constants.TransportModes.Sea;
			importTransportLeg.JW_IsLinked = false;
			importTransportLeg.JW_RL_NKLoadPort = "AUBNE";
			importTransportLeg.JW_RL_NKDiscPort = "ZABAL";
			importTransportLeg.JW_OA_ArrivalLocation = terminalAddress.PK;
			importTransportLeg.JW_Vessel = "Import Vessel Name";
			importTransportLeg.JW_VoyageFlight = "VF001";
			importTransportLeg.JW_ETD = ZDateTime.Today;
			importTransportLeg.JW_ETA = ZDateTime.Today.AddDays(3);
			importTransportLeg.JW_OA_CarrierAddress = exportCarrier.PK;

			var localTransportLeg = importConsol.Transports.AddNew();
			localTransportLeg.JW_LegOrder = 2;
			localTransportLeg.JW_TransportMode = Constants.TransportModes.Rail;
			localTransportLeg.JW_RL_NKLoadPort = "ZABAL";
			localTransportLeg.JW_RL_NKDiscPort = "ZA2WC";
			localTransportLeg.JW_CarrierBookingReference = "RANO0001";
			localTransportLeg.JW_Vessel = "RASLIDING0001";

			var shipment = importConsol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(importConsol, container);
			packline1.JL_ImportRefNumber = "IMP0001";
			packline1.JL_PackageCount = 10;
			packline1.JL_ActualWeight = 11;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 12;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Description = "Pack line 1 description";
			packline1.JL_MarksAndNumbers = "Pack line 1 Marks and Nos";

			var undg1 = packline1.UNDGs.AddNew();
			undg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3019", "c", "IMO").First().PK;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(importConsol, container);
			packline2.JL_ImportRefNumber = "IMP0001";
			packline2.JL_PackageCount = 20;
			packline2.JL_ActualWeight = 21;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 22;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Description = "Pack line 2 description";
			packline2.JL_MarksAndNumbers = "Pack line 2 Marks and Nos";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.SetContainer(importConsol, container);
			packline3.JL_ImportRefNumber = "IMP0002";
			packline3.JL_PackageCount = 30;
			packline3.JL_ActualWeight = 31;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 32;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Description = "Pack line 3 description";
			packline3.JL_MarksAndNumbers = "Pack line 3 Marks and Nos";

			Factory.Save();

			return importConsol;
		}

		ForwardingConsol PrepareConsolForShippingOrder()
		{
			var exportConsol = Factory.New<ForwardingConsol>();
			exportConsol.JK_TransportMode = Constants.TransportModes.Sea;
			exportConsol.JK_RL_NKLoadPort = "ZA2WC";
			exportConsol.JK_RL_NKDischargePort = "AUSYD";
			exportConsol.JK_MasterBillNum = "WAYBILL0001";

			var container = exportConsol.Containers.AddNew();
			container.FillWithValidTestData();

			var terminalAddress = Factory.NewWithValidTestData<OrgAddress>();
			terminalAddress.Header.OH_FullName = "Terminal Organisation";

			var terminalAccountNo = terminalAddress.Header.CustomsCodes.AddNew();
			terminalAccountNo.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
			terminalAccountNo.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.TPT;
			terminalAccountNo.OK_CustomsRegNo = "6666666";

			var etd = ZDateTime.Today;
			var eta = ZDateTime.Today.AddDays(3);

			var exportCarrier = Factory.NewWithValidTestData<OrgAddress>();

			var sendingForwarder = Factory.NewWithValidTestData<OrgAddress>();
			sendingForwarder.OA_CompanyNameOverride = "SENDING FORWARDER CO.";
			sendingForwarder.OA_Phone = "0123456";
			sendingForwarder.OA_Email = "sendingForwarder@email.com";

			var sendingFrowarderAccountNo = sendingForwarder.Header.CustomsCodes.AddNew();
			sendingFrowarderAccountNo.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
			sendingFrowarderAccountNo.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.TPT;
			sendingFrowarderAccountNo.OK_CustomsRegNo = "123456";

			var tptTransporter = Factory.New<OrgHeader>();
			tptTransporter.OH_Code = "TRANSPORTER";
			tptTransporter.OH_FullName = "TPT Transporter Organisation";

			var tptTransporterAccountNo = tptTransporter.CustomsCodes.AddNew();
			tptTransporterAccountNo.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
			tptTransporterAccountNo.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.TPT;
			tptTransporterAccountNo.OK_CustomsRegNo = "123456";

			var tptTransporterAddress = Factory.New<OrgAddress>();
			tptTransporterAddress.OA_OH = tptTransporter.PK;
			tptTransporterAddress.OA_Address1 = "Address 1";

			exportConsol.JK_OA_SendingForwarderAddress = sendingForwarder.PK;
			exportConsol.JK_OA_ArrivalUnpackCFSTransportAddress = tptTransporterAddress.PK;

			var localTransportLeg = exportConsol.Transports.Cast<Transport>().First();
			localTransportLeg.JW_TransportMode = Constants.TransportModes.Road;
			localTransportLeg.JW_RL_NKLoadPort = "ZA2WC";
			localTransportLeg.JW_RL_NKDiscPort = "ZABAL";

			var exportTransportLeg = exportConsol.Transports.AddNew();
			exportTransportLeg.JW_LegOrder = 2;
			exportTransportLeg.JW_TransportMode = Constants.TransportModes.Sea;
			exportTransportLeg.JW_RL_NKLoadPort = "ZABAL";
			exportTransportLeg.JW_RL_NKDiscPort = "AUSYD";
			exportTransportLeg.JW_OA_DepartureLocation = terminalAddress.PK;
			exportTransportLeg.JW_Vessel = "Export Vessel Name";
			exportTransportLeg.JW_VoyageFlight = "VF001";
			exportTransportLeg.JW_ETD = etd;
			exportTransportLeg.JW_ETA = eta;
			exportTransportLeg.JW_OA_CarrierAddress = exportCarrier.PK;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "SHIPPERCODE";
			shipper.OH_FullName = "Shipper Organisation";

			var shipperAccountNo = shipper.CustomsCodes.AddNew();
			shipperAccountNo.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
			shipperAccountNo.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.TPT;
			shipperAccountNo.OK_CustomsRegNo = "877877";

			var shipment = exportConsol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = shipper.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(exportConsol, container);
			packline1.JL_ExportRefNumber = "EXP0001";
			packline1.JL_PackageCount = 10;
			packline1.JL_ActualWeight = 11;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 12;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Description = "Pack line 1 description";
			packline1.JL_MarksAndNumbers = "Pack line 1 Marks and Nos";

			var undg1 = packline1.UNDGs.AddNew();
			undg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3019", "c", "IMO").First().PK;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(exportConsol, container);
			packline2.JL_ExportRefNumber = "EXP0001";
			packline2.JL_PackageCount = 20;
			packline2.JL_ActualWeight = 21;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 22;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Description = "Pack line 2 description";
			packline2.JL_MarksAndNumbers = "Pack line 2 Marks and Nos";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.SetContainer(exportConsol, container);
			packline3.JL_ExportRefNumber = "EXP0002";
			packline3.JL_PackageCount = 30;
			packline3.JL_ActualWeight = 31;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 32;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Description = "Pack line 3 description";
			packline3.JL_MarksAndNumbers = "Pack line 3 Marks and Nos";

			Factory.Save();

			return exportConsol;
		}

		string CreateShippingOrderExpectedContentWithCarrierCode(string expectedCarrierCode)
		{
			return $@"[2,5] Service Instruction - Shipping Order
[2,37] Page 1 of 2
[3,5] Freight Forwarder 
[4,5] SENDING FORWARDER CO.
[5,5] Account Number:
[5,13] 123456
[5,25] Notification Contact 
[5,33] CargoWise Support
[6,5] Contact:
[6,9] CargoWise Support
[6,25] Email
[7,5] Tel:
[7,9] 0123456
[8,5] Email:
[8,9] sendingForwarder@email.com
[9,5] Shipper
[9,25] Transport Provider
[10,5] Shipper Organisation
[10,25] TPT Transporter Organisation
[11,5] TPT Exporter Code:
[11,13] 877877
[11,25] TPT Transporter ID:
[11,33] 123456
[13,5] Main Details
[14,5] Consol Number
[14,13] C00001000
[14,25] Terminal/Receipient
[14,33] Terminal Organisation
[15,5] Bill of Lading
[15,13] WAYBILL0001
[15,25] Account ID
[15,33] 6666666
[16,5] TPT Quotation No.
[17,5] TPT Contract No.
[18,5] Transport Mode
[18,13] ROAD
[19,5] Liner Terms
[21,5] Vessel Routing
[22,5] Main Carriage
[23,5] Vessel Name
[23,13] Export Vessel Name
[23,25] Carrier{(string.IsNullOrEmpty(expectedCarrierCode) ? string.Empty : $"\n[23,33] {expectedCarrierCode}\n")}
[24,5] Voyage Number
[24,13] VF001
[24,25] IMO Number
[25,5] Vessel Type
[25,25] Call Sign
[26,5] Port of Loading
[26,13] ZABAL
[26,25] Port of Discharge
[26,33] AUSYD
[27,5] ETD
[27,13] {ZDateTime.Today}
[27,25] ETA
[27,33] {ZDateTime.Today.AddDays(3)}
[29,5] Cargo Details
[30,5] Total No. Packs
[30,13] 60 PLT
[30,25] Total Weight
[30,33] 63.000 KG
[31,25] Total Volume
[31,33] 66.000 M3
[33,5] No Service Instruction - Shipping Order (Exports) Messages Have Been Sent
[34,5] ☑
[34,7] MRN
[34,13] EXP0001
[34,25] LRN
[34,35] Packs
[34,39] 30 PLT
[35,5] TPT Order Number
[35,25] Status
[36,5] Shipment No.
[36,13] Length
[36,17] Width
[36,21] Height
[36,25] Weight
[36,29] Volume
[36,35] UNDG
[37,5] S00001000
[37,13] 0.000 M
[37,17] 0.000 M
[37,21] 0.000 M
[37,25] 11.000 KG
[37,29] 12.0000 M3
[37,35] 3019c
[38,5] Goods Description
[38,25] Used
[38,29] Coastwise
[38,35] Own Wheels
[38,41] Palletized
[39,5] Pack line 1 description
[39,25] ☐
[39,29] ☐
[39,35] ☐
[39,41] ☑
[40,25] Marks & Numbers
[41,25] Pack line 1 Marks and Nos
[44,5] Service Instruction - Shipping Order
[44,37] Page 2 of 2
[45,5] Shipment No.
[45,13] Length
[45,17] Width
[45,21] Height
[45,25] Weight
[45,29] Volume
[45,35] UNDG
[46,5] S00001000
[46,13] 0.000 M
[46,17] 0.000 M
[46,21] 0.000 M
[46,25] 21.000 KG
[46,29] 22.0000 M3
[47,5] Goods Description
[47,25] Used
[47,29] Coastwise
[47,35] Own Wheels
[47,41] Palletized
[48,5] Pack line 2 description
[48,25] ☐
[48,29] ☐
[48,35] ☐
[48,41] ☑
[49,25] Marks & Numbers
[50,25] Pack line 2 Marks and Nos
[51,5] No Service Instruction - Shipping Order (Exports) Messages Have Been Sent
[52,5] ☑
[52,7] MRN
[52,13] EXP0002
[52,25] LRN
[52,35] Packs
[52,39] 30 PLT
[53,5] TPT Order Number
[53,25] Status
[54,5] Shipment No.
[54,13] Length
[54,17] Width
[54,21] Height
[54,25] Weight
[54,29] Volume
[54,35] UNDG
[55,5] S00001000
[55,13] 0.000 M
[55,17] 0.000 M
[55,21] 0.000 M
[55,25] 31.000 KG
[55,29] 32.0000 M3
[56,5] Goods Description
[56,25] Used
[56,29] Coastwise
[56,35] Own Wheels
[56,41] Palletized
[57,5] Pack line 3 description
[57,25] ☐
[57,29] ☐
[57,35] ☐
[57,41] ☑
[58,25] Marks & Numbers
[59,25] Pack line 3 Marks and Nos";
		}

		string CreateLandingOrderExpectedContentWithCarrierCode(string expectedCarrierCode)
		{
			return $@"[2,5] Service Instruction - Landing Order
[2,37] Page 1 of 2
[3,5] Freight Forwarder 
[4,5] RECEIVING FORWARDER CO.
[5,5] Account Number:
[5,13] 123456
[5,25] Notification Contact 
[5,33] CargoWise Support
[6,5] Contact:
[6,9] CargoWise Support
[6,25] Email
[7,5] Tel:
[7,9] 0123456
[8,5] Email:
[8,9] receivingForwarder@email.com
[9,5] Consignee
[9,25] Transport Provider
[10,5] Consignee Co.
[10,25] TPT Landing Transporter
[11,5] TPT Consignee Code:
[11,13] 3333333
[11,25] TPT Transporter ID:
[11,33] 123456
[13,5] Main Details
[14,5] Consol Number
[14,13] C00001001
[14,25] Terminal/Receipient
[14,33] Terminal Organisation
[15,5] Bill of Lading
[15,13] WAYBILL0002
[15,25] Account ID
[15,33] 6666666
[16,5] TPT Quotation No.
[16,25] Rail Account No.
[16,33] RANO0001
[17,5] TPT Contract No.
[17,25] Rail Sliding No.
[17,33] RASLIDING0001
[18,5] Transport Mode
[18,13] RAIL
[18,25] Rail Destination
[18,33] ZA2WC
[19,5] Liner Terms
[21,5] Vessel Routing
[22,5] Main Carriage
[23,5] Vessel Name
[23,13] Import Vessel Name
[23,25] Carrier{(string.IsNullOrEmpty(expectedCarrierCode) ? string.Empty : $"\n[23,33] {expectedCarrierCode}\n")}
[24,5] Voyage Number
[24,13] VF001
[24,25] IMO Number
[25,5] Vessel Type
[25,25] Call Sign
[26,5] Port of Loading
[26,13] AUBNE
[26,25] Port of Discharge
[26,33] ZABAL
[27,5] ETD
[27,13] {ZDateTime.Today}
[27,25] ETA
[27,33] {ZDateTime.Today.AddDays(3)}
[29,5] Cargo Details
[30,5] Total No. Packs
[30,13] 60 PLT
[30,25] Total Weight
[30,33] 63.000 KG
[31,25] Total Volume
[31,33] 66.000 M3
[33,5] No Service Instruction - Landing Order (Imports) Messages Have Been Sent
[34,5] ☑
[34,7] MRN
[34,13] IMP0001
[34,25] LRN
[34,35] Packs
[34,39] 30 PLT
[35,5] TPT Order Number
[35,25] Status
[36,5] Shipment No.
[36,13] Length
[36,17] Width
[36,21] Height
[36,25] Weight
[36,29] Volume
[36,35] UNDG
[37,5] S00001001
[37,13] 0.000 M
[37,17] 0.000 M
[37,21] 0.000 M
[37,25] 11.000 KG
[37,29] 12.0000 M3
[37,35] 3019c
[38,5] Goods Description
[38,25] Used
[38,29] Coastwise
[38,35] Own Wheels
[38,41] Palletized
[39,5] Pack line 1 description
[39,25] ☐
[39,29] ☐
[39,35] ☐
[39,41] ☑
[40,25] Marks & Numbers
[41,25] Pack line 1 Marks and Nos
[44,5] Service Instruction - Landing Order
[44,37] Page 2 of 2
[45,5] Shipment No.
[45,13] Length
[45,17] Width
[45,21] Height
[45,25] Weight
[45,29] Volume
[45,35] UNDG
[46,5] S00001001
[46,13] 0.000 M
[46,17] 0.000 M
[46,21] 0.000 M
[46,25] 21.000 KG
[46,29] 22.0000 M3
[47,5] Goods Description
[47,25] Used
[47,29] Coastwise
[47,35] Own Wheels
[47,41] Palletized
[48,5] Pack line 2 description
[48,25] ☐
[48,29] ☐
[48,35] ☐
[48,41] ☑
[49,25] Marks & Numbers
[50,25] Pack line 2 Marks and Nos
[51,5] No Service Instruction - Landing Order (Imports) Messages Have Been Sent
[52,5] ☑
[52,7] MRN
[52,13] IMP0002
[52,25] LRN
[52,35] Packs
[52,39] 30 PLT
[53,5] TPT Order Number
[53,25] Status
[54,5] Shipment No.
[54,13] Length
[54,17] Width
[54,21] Height
[54,25] Weight
[54,29] Volume
[54,35] UNDG
[55,5] S00001001
[55,13] 0.000 M
[55,17] 0.000 M
[55,21] 0.000 M
[55,25] 31.000 KG
[55,29] 32.0000 M3
[56,5] Goods Description
[56,25] Used
[56,29] Coastwise
[56,35] Own Wheels
[56,41] Palletized
[57,5] Pack line 3 description
[57,25] ☐
[57,29] ☐
[57,35] ☐
[57,41] ☑
[58,25] Marks & Numbers
[59,25] Pack line 3 Marks and Nos";
		}

		#endregion
	}
}
