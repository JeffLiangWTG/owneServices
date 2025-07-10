using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class SGAccessManifestConverterTest : BaseAsycudaManifestConverterTest<SGAccessManifestConverter>
	{
		public void TestConvertShipmentToCargoReport_EndToEnd_Singapore()
		{
			var sgRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertConvertShipmentToCargoReport_EndToEnd(TransportModes.Air, AsycudaBillSchema.Constants.Prefix, 3);
		}

		public void TestConvertShipmentToCargoReport_Singapore_AccessMapping()
		{
			#region Set Up
			#region Registry
			var sgRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			#endregion

			#region Carrier Organization
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			var airline = Factory.New<RefAirline>();
			airline.RM_AirlineName1 = "Best Airline Ever";
			airline.RM_TwoCharacterCode = "XD";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";

			var org = orgAddress.Header;
			org.OH_Code = "AIRLINE_WW";

			orgAddress.OA_Address1 = "456 Main Address Road";
			orgAddress.OA_Code = "Test";
			orgAddress.OA_City = "Auckland";
			orgAddress.OA_PostCode = "1234";

			var miscServ = org.MiscServ;
			miscServ.OM_RM_Airline = airline.PK;
			#endregion

			#region JobConsol
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "08138374491";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKPortOfFirstArrival = "SGCCK";
			consol.JK_RL_NKDischargePort = "SGPUB";
			consol.JK_OA_ShippingLineAddress = orgAddress.PK;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "HighWind";
			vessel.RV_LloydsNumber = "9174622";

			var transportBO = consol.Transports[0];
			transportBO.JW_Vessel = vessel.RV_Code;
			transportBO.JW_VoyageFlight = "SG1234";
			#endregion

			#region JobShipment
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = "AIR";
			shipment.JS_HouseBill = "UWM97N872947";
			shipment.JS_E_DEP = new ZDateTime(2020, 6, 15);
			shipment.JS_E_ARV = new ZDateTime(2020, 6, 18);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGCCK";
			consol.Shipments.Add(shipment);
			#endregion

			#region HVLVConsignments
			#region consignment1

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_WaybillNumber = "HVC00001";
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WeightUQ = "KG";
			consignment1.HVC_VolumeUQ = "M3";
			consignment1.HVC_GoodsDescription = "Toy guns";
			consignment1.HVC_INCO = "FOB";
			consignment1.HVC_GoodsValue = 1234.56m;
			consignment1.HVC_RX_NKGoodsValueCurrency = "SGD";
			consignment1.HVC_ShipperName = "Test shipper";
			consignment1.HVC_ShipperAddress1 = "Test shipper add1";
			consignment1.HVC_ShipperAddress2 = "Test shipper add2";
			consignment1.HVC_ShipperCity = "ALAWA";
			consignment1.HVC_ShipperPhone = "+8613913913913";
			consignment1.HVC_RN_NKShipperCountryCode = "AU";
			consignment1.HVC_ShipperState = "NT";
			consignment1.HVC_ShipperPostcode = "741852";
			consignment1.HVC_ConsigneeName = "Test Consignee";
			consignment1.HVC_ConsigneeAddress1 = "Test Consignee add1";
			consignment1.HVC_ConsigneeAddress2 = "Test Consignee add2";
			consignment1.HVC_ConsigneeCity = "Singapore";
			consignment1.HVC_RN_NKConsigneeCountryCode = "SG";
			consignment1.HVC_ConsigneeState = "SIN";
			consignment1.HVC_ConsigneePostcode = "741852";
			consignment1.HVC_ConsigneePhone = "852746234";

			var item1_1 = consignment1.Items.AddNew();
			item1_1.HVI_JS_LoadedOnShipment = shipment.PK;
			item1_1.HVI_F3_NKPackType = "BAG";
			item1_1.HVI_GoodsDescription = "Toy machine gun";
			item1_1.HVI_ManifestedWeight = 8.8m;
			item1_1.HVI_ManifestedVolume = 4.4m;

			var item1_2 = consignment1.Items.AddNew();
			item1_2.HVI_JS_LoadedOnShipment = shipment.PK;
			item1_2.HVI_HVC_Consignment = consignment1.PK;
			item1_2.HVI_F3_NKPackType = "BOT";
			item1_2.HVI_GoodsDescription = "Toy hand gun";
			item1_2.HVI_ManifestedWeight = 2.2m;
			item1_2.HVI_ManifestedVolume = 1.1m;
			item1_2.HVI_ActualWeight = 7.7m;
			item1_2.HVI_ActualVolume = 5.5m;

			var line1_1_1 = item1_1.Lines.AddNew();
			line1_1_1.HVS_Quantity = 5;
			line1_1_1.HVS_GoodsDescription = "M4A1";
			line1_1_1.HVS_FormattedDestinationTariff = "1234.56.78";
			line1_1_1.HVS_CustomsValue = 1.8m;
			line1_1_1.HVS_RN_NKOriginCountryCode = "CN";

			var line1_1_2 = item1_1.Lines.AddNew();
			line1_1_2.HVS_Quantity = 8;
			line1_1_2.HVS_GoodsDescription = "AK47";
			line1_1_2.HVS_FormattedDestinationTariff = "4567.89.01";
			line1_1_2.HVS_CustomsValue = 7.2m;
			line1_1_2.HVS_RN_NKOriginCountryCode = "AU";

			var line1_2_1 = item1_2.Lines.AddNew();
			line1_2_1.HVS_Quantity = 3;
			line1_2_1.HVS_GoodsDescription = "DE";
			line1_2_1.HVS_FormattedDestinationTariff = "741.85.23";
			line1_2_1.HVS_CustomsValue = 9.5m;
			line1_2_1.HVS_RN_NKOriginCountryCode = "NZ";
			#endregion

			#region consignment2
			var item2_1 = Factory.NewWithValidTestData<HVLVItem>();
			item2_1.HVI_JS_LoadedOnShipment = shipment.PK;
			var consignment2 = item2_1.Consignment;
			consignment2.HVC_WaybillNumber = "HVC00002";
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WeightUQ = "T";
			consignment2.HVC_VolumeUQ = "D3";
			consignment2.HVC_GoodsDescription = "Books";
			consignment2.HVC_INCO = "DAT";
			consignment2.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment2.HVC_GoodsValue = 6543.21m;
			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			consignment2.HVC_OA_ShipperAddress = orgAddress1.PK;
			consignment2.HVC_OA_ConsigneeAddress = orgAddress2.PK;

			item2_1.HVI_GoodsDescription = "Black Hole";
			item2_1.HVI_ManifestedWeight = 1.2m;
			item2_1.HVI_ManifestedVolume = 3.4m;
			#endregion
			#endregion

			Factory.Save();
			#endregion

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var success = converter.TryConvert(out var errorMsg);
				Assert("Convert successfully", success);
				AssertNullOrEmpty("No error occured", errorMsg);

				var converterFactory = converter.CustomsRelatedBusinessCollection[0].Factory;
				converterFactory.Save();

				var manifestHeaderType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AsycudaManifestHeaderSchema.Constants.Prefix);
				var manifestHeader = Factory.Load(manifestHeaderType, new ZQuery()).SingleOrDefault();
				AssertNotNull("New Asycuda Header should be created", manifestHeader);
				Assert("New Asycuda Header should be saved", manifestHeader.IsInDatabase);

				var asycudaBillType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AsycudaBillSchema.Constants.Prefix);
				var asycudaBills = Factory.Load(asycudaBillType, new ZQuery());
				AssertEquals("should have 1 master bill and 2 house bills", 3, asycudaBills.Length);
				Assert("New Asycuda Bills should be saved", asycudaBills.All(h => h.IsInDatabase));

				#region Mapping Properties

				#region Header
				var header = manifestHeader as AsycudaManifestHeader;
				Assert("Type should be Enterprise.Customs.SG.Access.Business.AsycudaManifestHeader", header.GetType().FullName == "Enterprise.Customs.SG.Access.Business.AsycudaManifestHeader");
				AssertEquals("Header.AMA_TransportMode", consol.JK_TransportMode, header.AMA_TransportMode);
				AssertEquals("Header.AMA_ManifestType", "MGI", header.AMA_ManifestType);
				AssertEquals("Header.AMA_Voyage", consol.JK_JX_JV_VoyageFlight, header.AMA_Voyage);
				AssertEquals("Header.AMA_CustomsLoadPort", consol.JK_RL_NKLoadPort, header.AMA_CustomsLoadPort);
				AssertEquals("Header.AMA_RL_NKPortOfLoading", consol.JK_RL_NKLoadPort, header.AMA_RL_NKPortOfLoading);
				AssertEquals("Header.AMA_E_DEP", shipment.JS_E_DEP, header.AMA_E_DEP);
				AssertEquals("Header.AMA_E_ARV", shipment.JS_E_ARV, header.AMA_E_ARV);
				AssertEquals("Header.AMA_RL_NKPortOfFirstArrival", consol.JK_RL_NKPortOfFirstArrival, header.AMA_RL_NKPortOfFirstArrival);
				AssertEquals("Header.AMA_RL_NKPortOfDischarge", consol.JK_RL_NKDischargePort, header.AMA_RL_NKPortOfDischarge);
				AssertEquals("Header.AMA_CustomsDischargePort", consol.JK_RL_NKDischargePort, header.AMA_CustomsDischargePort);
				AssertEquals("Header.AMA_MasterBill", consol.JK_MasterBillNum.FormatAirMAWB(), header.AMA_MasterBill);
				AssertEquals("Header.AMA_OA_Carrier_ZAddress.OrgPK", org.PK, header.AMA_OA_Carrier_ZAddress.OrgPK);
				AssertEquals("Header.AMA_CarrierCode", airline.RM_TwoCharacterCode, header.AMA_CarrierCode);
				#endregion

				#region Bills
				var bill1 = header.Bills.AsEnumerable().Single(x => x.ABL_BillNumber == consignment1.HVC_WaybillNumber);
				var bill_1 = asycudaBills.Single(x => (x as AsycudaBill).ABL_BillNumber == consignment1.HVC_WaybillNumber) as AsycudaBill;
				Assert("Type should be Enterprise.Customs.SG.Access.Business.AsycudaBill", bill_1.GetType().FullName == "Enterprise.Customs.SG.Access.Business.AsycudaBill");
				AssertEquals(bill1, bill_1);
				AssertEquals("bill1.ABL_ManifestQty", 2, bill1.ABL_ManifestQty);
				AssertEquals("bill1.ABL_ManifestUQ", "BAG", bill1.ABL_ManifestUQ);
				AssertEquals("bill1.ABL_GrossWeight", 7.7m, bill1.ABL_GrossWeight);
				AssertEquals("bill1.ABL_GrossWeightUQ", consignment1.HVC_WeightUQ, bill1.ABL_GrossWeightUQ);
				AssertEquals("bill1.ABL_GrossVolume", 5.5m, bill1.ABL_Volume);
				AssertEquals("bill1.ABL_GrossVolumeUQ", consignment1.HVC_VolumeUQ, bill1.ABL_VolumeUQ);
				AssertEquals("bill1.ABL_GoodsDescription", consignment1.HVC_GoodsDescription, bill1.ABL_GoodsDescription);
				AssertEquals("bill1.ABL_PrepaidCollect", "CLT", bill1.ABL_PrepaidCollect);
				AssertEquals("bill1.ABL_FreightValue", consignment1.HVC_GoodsValue, bill1.ABL_FreightValue);
				AssertEquals("bill1.ABL_RX_NKFreightValueCurrency", consignment1.HVC_RX_NKGoodsValueCurrency, bill1.ABL_RX_NKFreightValueCurrency);
				AssertEquals("bill1.ABL_ShipperName", consignment1.HVC_ShipperName, bill1.ABL_ShipperName);
				AssertEquals("bill1.ABL_ShipperPhone", consignment1.HVC_ShipperPhone, bill1.ABL_ShipperPhone);
				AssertEquals("bill1.ABL_ShipperStreet1", consignment1.HVC_ShipperAddress1, bill1.ABL_ShipperStreet1);
				AssertEquals("bill1.ABL_ShipperStreet2", consignment1.HVC_ShipperAddress2, bill1.ABL_ShipperStreet2);
				AssertEquals("bill1.ABL_ShipperCity", consignment1.HVC_ShipperCity, bill1.ABL_ShipperCity);
				AssertEquals("bill1.ABL_RN_NKShipperCountry", consignment1.HVC_RN_NKShipperCountryCode, bill1.ABL_RN_NKShipperCountry);
				AssertEquals("bill1.ABL_ShipperState", consignment1.HVC_ShipperState, bill1.ABL_ShipperState);
				AssertEquals("bill1.ABL_ShipperPostcode", consignment1.HVC_ShipperPostcode, bill1.ABL_ShipperPostcode);
				AssertEquals("bill1.ABL_ConsigneeName", consignment1.HVC_ConsigneeName, bill1.ABL_ConsigneeName);
				AssertEquals("bill1.ABL_ConsigneeStreet1", consignment1.HVC_ConsigneeAddress1, bill1.ABL_ConsigneeStreet1);
				AssertEquals("bill1.ABL_ConsigneeStreet2", consignment1.HVC_ConsigneeAddress2, bill1.ABL_ConsigneeStreet2);
				AssertEquals("bill1.ABL_ConsigneeCity", consignment1.HVC_ConsigneeCity, bill1.ABL_ConsigneeCity);
				AssertEquals("bill1.ABL_RN_NKConsigneeCountry", consignment1.HVC_RN_NKConsigneeCountryCode, bill1.ABL_RN_NKConsigneeCountry);
				AssertEquals("bill1.ABL_ConsigneeState", consignment1.HVC_ConsigneeState, bill1.ABL_ConsigneeState);
				AssertEquals("bill1.ABL_ConsigneePostcode", consignment1.HVC_ConsigneePostcode, bill1.ABL_ConsigneePostcode);
				AssertEquals("bill1.ABL_ConsigneePhone", consignment1.HVC_ConsigneePhone, bill1.ABL_ConsigneePhone);

				var bill2 = header.Bills.AsEnumerable().Single(x => x.ABL_BillNumber == consignment2.HVC_WaybillNumber);
				var bill_2 = asycudaBills.Single(x => (x as AsycudaBill).ABL_BillNumber == consignment2.HVC_WaybillNumber) as AsycudaBill;
				Assert("Type should be Enterprise.Customs.SG.Access.Business.AsycudaBill", bill_2.GetType().FullName == "Enterprise.Customs.SG.Access.Business.AsycudaBill");
				AssertEquals(bill2, bill_2);
				AssertEquals("bill2.ABL_ManifestQty", 1, bill2.ABL_ManifestQty);
				AssertEquals("bill2.ABL_ManifestUQ", "", bill2.ABL_ManifestUQ);
				AssertEquals("bill2.ABL_GrossWeight", 1.2m, bill2.ABL_GrossWeight);
				AssertEquals("bill2.ABL_GrossVolume", 3.4m, bill2.ABL_Volume);
				AssertEquals("bill2.ABL_GrossWeightUQ", consignment2.HVC_WeightUQ, bill2.ABL_GrossWeightUQ);
				AssertEquals("bill2.ABL_GrossVolumeUQ", consignment2.HVC_VolumeUQ, bill2.ABL_VolumeUQ);
				AssertEquals("bill2.ABL_GoodsDescription", consignment2.HVC_GoodsDescription, bill2.ABL_GoodsDescription);
				AssertEquals("bill2.ABL_PrepaidCollect", "PPD", bill2.ABL_PrepaidCollect);
				AssertEquals("bill2.ABL_FreightValue", consignment2.HVC_GoodsValue, bill2.ABL_FreightValue);
				AssertEquals("bill2.ABL_RX_NKFreightValueCurrency", consignment2.HVC_RX_NKGoodsValueCurrency, bill2.ABL_RX_NKFreightValueCurrency);
				AssertEquals("bill2.ABL_OA_Shipper", orgAddress1.PK, bill2.ABL_OA_Shipper);
				AssertEquals("bill2.ABL_OA_Consignee", orgAddress2.PK, bill2.ABL_OA_Consignee);
				#endregion

				#region Packs and Packed Items
				AssertPackAndPackedItem(bill1, line1_1_1, item1_1, consignment1);
				AssertPackAndPackedItem(bill1, line1_1_2, item1_1, consignment1);
				AssertPackAndPackedItem(bill1, line1_2_1, item1_2, consignment1);

				var pack2_1 = bill2.Packs.OfType<AsycudaPack>().Single();
				Assert("Type should be Enterprise.Customs.SG.Access.Business.AsycudaPack", pack2_1.GetType().FullName == "Enterprise.Customs.SG.Access.Business.AsycudaPack");
				AssertNotNull(pack2_1);
				AssertEquals("pack2_1.APA_PackUQ", item2_1.HVI_F3_NKPackType, pack2_1.APA_PackUQ);
				AssertEquals("pack2_1.APA_GoodsDescription", item2_1.HVI_GoodsDescription, pack2_1.APA_GoodsDescription);
				AssertEquals("pack2_1.APA_Weight", item2_1.HVI_ManifestedWeight, pack2_1.APA_Weight);
				AssertEquals("pack2_1.APA_WeightUQ", consignment2.HVC_WeightUQ, pack2_1.APA_WeightUQ);
				AssertEquals("pack2_1.APA_Volume", item2_1.HVI_ManifestedVolume, pack2_1.APA_Volume);
				AssertEquals("pack2_1.APA_VolumeUQ", consignment2.HVC_VolumeUQ, pack2_1.APA_VolumeUQ);
				var packedItem2_1 = pack2_1.PackedItem;
				AssertNotNull(packedItem2_1);
				AssertEquals("packedItem1_3.API_GoodsDescription", item2_1.HVI_GoodsDescription, packedItem2_1.API_GoodsDescription);
				AssertEquals("packedItem1_3.API_FormattedTariff", "", packedItem2_1.API_FormattedTariff);
				AssertEquals("packedItem1_3.API_CustomsQty", 0m, packedItem2_1.API_CustomsQty);
				AssertEquals("packedItem1_3.API_CustomsUQ", "NMB", packedItem2_1.API_CustomsUQ);
				AssertEquals("packedItem1_3.API_CustomsValue", 0m, packedItem2_1.API_CustomsValue);
				AssertEquals("packedItem1_3.API_RN_NKGoodsOrigin", "AU", packedItem2_1.API_RN_NKGoodsOrigin);
				#endregion

				#endregion
			}
		}

		public void TestConvertShipmentToCargoReport_Cancel_Singapore()
		{
			var sgRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertConvertShipmentToCargoReport_Cancel(TransportModes.Air, AsycudaManifestHeaderSchema.Constants.Prefix, AsycudaBillSchema.Constants.Prefix);
		}

		protected override ZString LoginCountry => CountryCodes.Singapore;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Air, TransportModes.Sea };

		protected override IRegistryItem RegistryItemToEnable => ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;

		protected override string GetBillHumanReadableName(string billNumber) => $"Manifest Bill {billNumber}";

		void AssertPackAndPackedItem(AsycudaBill bill, HVLVItemLine line, HVLVItem item, HVLVConsignment consignment)
		{
			var pack = bill.Packs.OfType<AsycudaPack>().SingleOrDefault(x => x.APA_PackQty == line.HVS_Quantity);
			Assert("Type should be Enterprise.Customs.SG.Access.Business.AsycudaPack", pack.GetType().FullName == "Enterprise.Customs.SG.Access.Business.AsycudaPack");
			AssertNotNull(pack);
			AssertEquals("APA_PackUQ", item.HVI_F3_NKPackType, pack.APA_PackUQ);
			AssertEquals("APA_GoodsDescription", item.HVI_GoodsDescription, pack.APA_GoodsDescription);
			AssertEquals("APA_Weight", item.HVI_ActualWeight.IsDefault ? item.HVI_ManifestedWeight : item.HVI_ActualWeight, pack.APA_Weight);
			AssertEquals("APA_WeightUQ", consignment.HVC_WeightUQ, pack.APA_WeightUQ);
			AssertEquals("APA_Volume", item.HVI_ActualVolume.IsDefault ? item.HVI_ManifestedVolume : item.HVI_ActualVolume, pack.APA_Volume);
			AssertEquals("APA_VolumeUQ", consignment.HVC_VolumeUQ, pack.APA_VolumeUQ);
			var packedItem = pack.PackedItem;
			AssertNotNull(packedItem);
			AssertEquals("API_GoodsDescription", line.HVS_GoodsDescription, packedItem.API_GoodsDescription);
			AssertEquals("API_FormattedTariff", line.HVS_FormattedDestinationTariff, packedItem.API_FormattedTariff);
			AssertEquals("API_CustomsQty", (ZDecimal)line.HVS_Quantity, packedItem.API_CustomsQty);
			AssertEquals("API_CustomsValue", line.HVS_CustomsValue, packedItem.API_CustomsValue);
			AssertEquals("API_RN_NKGoodsOrigin", line.HVS_RN_NKOriginCountryCode, packedItem.API_RN_NKGoodsOrigin);
			AssertEquals("API_CustomsUQ", item.HVI_F3_NKPackType, packedItem.API_CustomsUQ);
		}

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new SGCargoReportCommand(shipment);

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			result.AMA_JobReference = "TestReference" + shipment.JS_TransportMode;
			return result;
		}

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((AsycudaManifestHeader)existingJob).AMA_JobReference;
	}
}
