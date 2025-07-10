using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class CargoControlAndTransitHouseManifestDocumentTest : DocumentVisualizer.Testing.StandardDocumentContentTest
	{
		[TestDate(2023, 09, 20)]
		public override void TestDocumentContent()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var cctHouseManifestMenuItemQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, "CCT House Manifest");
			var cctHouseManifestMenuItem = Factory.Load<StmMenuItem>(cctHouseManifestMenuItemQuery);

			Assert("There's no menu with name 'CCT House Manifest'", cctHouseManifestMenuItem.Length >= 1);
			AssertEquals("There's more than one menu with name 'CCT House Manifest'", 1, cctHouseManifestMenuItem.Length);

			var cctHouseManifestMenuTemplatePivotQuery = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, cctHouseManifestMenuItem.First().PK);
			var cctHouseManifestMenuTemplatePivot = Factory.Load<VisualizerMenuTemplatePivot>(cctHouseManifestMenuTemplatePivotQuery);

			AssertContents(consol, cctHouseManifestMenuTemplatePivot.Single(), CreateContentWithoutShipment());

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			var documentData = CreateDocumentData(shipment);
			AddLog(documentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));

			AssertContents(consol, cctHouseManifestMenuTemplatePivot.Single(), CreateContentWithShipment());
		}

		string CreateContentWithoutShipment()
		{
			return $@"[2,4] Sending Party
[2,15] Receiving Agent
[2,26] CCT House Manifest
[3,4] EDI CUSTOMS BROKERS
[3,15] ReceivingForwarder
[4,4] 10 HUTCHESON STREET
[4,15] Av Paulista 291
[5,4] ALBION  QLD
[5,15] Consolacao
[6,15] Sao Paulo
[6,23] SP
[7,4] Australia
[7,11] 4010
[7,15] China
[7,23] 11157802
[8,15] CNPJ: 
[10,4] MAWB
[10,15] Consol Number
[10,27] Packs
[10,39] Weight
[11,4] 215-98757411
[11,15] C00001004
[11,27] 0
[11,39] 0.00 KG
[12,4] Load Port
[12,15] Discharge Port
[13,4] AUSYD
[13,15] BRSAO
[17,40] Created by
";
		}

		string CreateContentWithShipment()
		{
			return $@"[2,4] Sending Party
[2,15] Receiving Agent
[2,26] CCT House Manifest
[3,4] EDI CUSTOMS BROKERS
[3,15] ReceivingForwarder
[4,4] 10 HUTCHESON STREET
[4,15] Av Paulista 291
[5,4] ALBION  QLD
[5,15] Consolacao
[6,15] Sao Paulo
[6,23] SP
[7,4] Australia
[7,11] 4010
[7,15] China
[7,23] 11157802
[8,15] CNPJ: 
[10,4] MAWB
[10,15] Consol Number
[10,27] Packs
[10,39] Weight
[11,4] 215-98757411
[11,15] C00001004
[11,27] 25
[11,39] 12.00 KG
[12,4] Load Port
[12,15] Discharge Port
[13,4] AUSYD
[13,15] BRSAO
[15,3]  Shipments
[16,4] Shipment No
[16,9]  HAWB
[16,15] Origin
[16,19] Dest.
[16,23] Packs
[16,27] Weight
[16,31] Status
[16,43] Event Date/Time
[17,4] S00001000
[17,9] 081001
[17,15] AUSYD
[17,19] BRSAO
[17,23] 25
[17,27] 12.00 KG
[17,31] Advanced Cargo Report Message Sent to Customs, BR
[17,43] 19/09/2023 19:00
[19,4] Goods Description
[21,4] goods1
[26,40] Created by
";
		}

		#region Implementation

		void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "ReceivingForwarder";
			receivingForwarder.OH_RL_NKClosestPort = "BRSAO";
			receivingForwarder.MainAddress.Address1 = "Av Paulista 291";
			receivingForwarder.MainAddress.Address2 = "Consolacao";
			receivingForwarder.MainAddress.City = "Sao Paulo";
			receivingForwarder.MainAddress.Postcode = "11157802";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SendingForwarder";
			sendingForwarder.OH_RL_NKClosestPort = "BRSAO";
			sendingForwarder.MainAddress.Address1 = "Av Paulista 291";
			sendingForwarder.MainAddress.Address2 = "Consolacao";
			sendingForwarder.MainAddress.City = "Salvador";
			sendingForwarder.MainAddress.Postcode = "11157802";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BR";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NZAKL";

			var transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_LegOrder = 2;
			transportLeg2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg2.JW_RL_NKLoadPort = "NZAKL";
			transportLeg2.JW_RL_NKDiscPort = "CLSCL";

			var transportLeg3 = consol.Transports.AddNew();
			transportLeg3.JW_LegOrder = 3;
			transportLeg3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg3.JW_RL_NKLoadPort = "CLSCL";
			transportLeg3.JW_RL_NKDiscPort = "BRSAO";
			transportLeg3.JW_ETD = ZDateTime.Today;
		}

		void PopulateShipment(ForwardingShipment shipment, ZString hawb, ZDecimal weight, ZInt packs, ZString desc)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = hawb;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = desc;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = packs;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Consignor";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit52";
			shipper.MainAddress.Address2 = "Dorcus yamadai";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2017";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "BRSAO";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = "BR";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
		}

		VisualizerDocumentData CreateDocumentData(ForwardingShipment shipment)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_Name = ShipmentDocumentDataStoreNames.AdvancedCargoReportBR;

			return documentData;
		}

		void AddLog(VisualizerDocumentData documentData, Event evenType, ZDateTimeOffset eventDateTime)
		{
			var paramList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("MST", DocDataObjects.DocumentNames.AdvancedCargoReport),
				new KeyValuePair<string, string>("LOC", "BR"),
				new KeyValuePair<string, string>("DEP", "Customs")
			};

			documentData.Logs.AddNew(evenType, eventDateTime, paramList.ToArray());

			Thread.Sleep(1);
			Factory.Save();
		}

		#endregion
	}
}
