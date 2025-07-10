using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using BriefCustomsDeclaration = Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader;
using ForwarderManifest = Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class TWForwarderManifestConverterTest : BaseAsycudaManifestConverterTest<TWForwarderManifestConverter>
	{
		public void TestConvertShipmentToForwarderManifest_Taiwan_EndToEnd()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "TESTCONSOL";
				consol.JK_RL_NKLoadPort = "TWAPG";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = consol.TransportMode;
				shipment.JS_HouseBill = "TESTSHIPMENT";

				var destination = Factory.NewWithValidTestData<RefUNLOCO>();
				destination.RL_RN_NKCountryCode = CountryCodes.NewZealand;
				shipment.JS_RL_NKDestination = destination.Code;
				shipment.JS_RL_NKOrigin = "TWAPG";
				shipment.JS_RL_NKDestination = "NZABY";

				consol.Shipments.Add(shipment);

				var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
				undgSubstance.DG_Code = "AA";

				var item1 = Factory.NewWithValidTestData<HVLVItem>();
				item1.HVI_JS_LoadedOnShipment = shipment.PK;
				item1.UNDGs.AddNew().DI_DG = undgSubstance.PK;

				var itemLine1 = item1.Lines.AddNew();
				itemLine1.HVS_Quantity = 1;
				itemLine1.HVS_GoodsDescription = "ItemLine1Desc";

				var consignment1 = item1.Consignment;
				consignment1.HVC_WaybillNumber = "TESTCONSIGNMENT1";
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.HVC_GoodsDescription = "ConsignmentDesc1";
				consignment1.HVC_ConsignmentId = "CONSIGNMENT1";

				var item2 = Factory.NewWithValidTestData<HVLVItem>();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;
				item2.HVI_GoodsDescription = "ItemDesc2";

				var itemLine2 = item2.Lines.AddNew();
				itemLine2.HVS_Quantity = 1;
				itemLine2.HVS_RN_NKOriginCountryCode = "TW";
				itemLine2.HVS_OriginTariff = "0001";

				var consignment2 = item2.Consignment;
				consignment2.HVC_WaybillNumber = "TESTCONSIGNMENT2";
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment2.HVC_GoodsDescription = "ConsignmentDesc2";

				Factory.Save();

				var masterBill = (ForwarderManifest)ConvertToRelatedJob<ForwarderManifest>(shipment);
				AssertNotNull("New masterBill should be created", masterBill);
				Assert("New masterBill should not be saved", !masterBill.IsInDatabase);

				var houseBill1 = masterBill.Bills.Cast<AsycudaBill>().FirstOrDefault(b => b.ABL_BillNumber == "TESTCONSIGNMENT1");
				var houseBill2 = masterBill.Bills.Cast<AsycudaBill>().FirstOrDefault(b => b.ABL_BillNumber == "TESTCONSIGNMENT2");
				AssertNotNull(houseBill1);
				AssertNotNull(houseBill2);

				CombineAssertions("Package Description should come from Goods Description on item level then fallback to consignment level.", () =>
				{
					AssertEquals(consignment1.HVC_GoodsDescription, houseBill1.ABL_Remarks);
					AssertEquals(item2.HVI_GoodsDescription, houseBill2.ABL_Remarks);
				});

				AssertEquals("UCR Number comes from consignment id", consignment1.HVC_ConsignmentId, houseBill1.ABL_UCRNumber);
				AssertEquals("Spilt Quantity should be equal to item count.", 1, houseBill1.ABL_SplitQuantity);
				AssertEquals("Port of loading", consol.JK_RL_NKLoadPort, houseBill1.ABL_RL_NKPortOfLoading);

				var pack1 = houseBill1.Packs.Cast<AsycudaPack>().Single();
				AssertEquals("DG Code", undgSubstance.DG_Code, pack1.UNDGs.UNDGSubstanceManager.Value);

				var packedItem1 = pack1.PackedItem;
				var packedItem2 = houseBill2.Packs.Cast<AsycudaPack>().Single().PackedItem;
				AssertEquals("HS Code", "0001", packedItem2.API_Tariff);
				CombineAssertions("Goods Description should come from Goods Description in item line level then fallback to consignment level.", () =>
				{
					AssertEquals(itemLine1.HVS_GoodsDescription, packedItem1.API_GoodsDescription);
					AssertEquals(consignment2.HVC_GoodsDescription, packedItem2.API_GoodsDescription);
				});
			}
		}

		public void TestConvertShipmentToForwarderManifest_WhenBriefCustomsDeclarationExists_ShouldConvertSuccessfully()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "TESTCONSOL";
				consol.JK_RL_NKLoadPort = "TWAPG";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = consol.TransportMode;
				shipment.JS_HouseBill = "TESTSHIPMENT";

				var destination = Factory.NewWithValidTestData<RefUNLOCO>();
				destination.RL_RN_NKCountryCode = CountryCodes.NewZealand;
				shipment.JS_RL_NKDestination = destination.Code;
				shipment.JS_RL_NKOrigin = "TWAPG";
				shipment.JS_RL_NKDestination = "NZABY";

				consol.Shipments.Add(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_WaybillNumber = "TESTCONSIGNMENT1";
				consignment.HVC_GoodsDescription = "ConsignmentDesc1";
				consignment.HVC_ConsignmentId = "CONSIGNMENT1";

				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				var bcdConverter = new TWBriefCustomsDeclarationConverter(new TWBriefCustomsDeclarationCommand(shipment));
				var success = bcdConverter.TryConvert(out _);
				Assert("Pre-condition: Convert to Brief Customs Declaration succeed.", success);

				var bcd = (BriefCustomsDeclaration)bcdConverter.CustomsRelatedBusinessCollection.Single();
				AssertNotNull("Pre-condition: Brief Customs Declaration is created", bcd);

				bcd.Factory.Save();
				Factory.Save();

				var converter = new TWForwarderManifestConverter(new TWForwarderManifestCommand(shipment));
				success = converter.TryConvert(out _);
				Assert("Convert to Forwarder Manifest should succeed", success);

				var forwarderManifest = converter.CustomsRelatedBusinessCollection.Single() as ForwarderManifest;
				AssertNotNull("Forwarder Manifest should be created.", forwarderManifest);
			}
		}

		protected override ZString LoginCountry => CountryCodes.Taiwan;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Air, TransportModes.Sea };

		protected override string GetExpectedMessageTypeCode(ForwardingShipment shipment) => TWManifestTypes.Codes.MAN;

		protected override string GetBillHumanReadableName(string billNumber) => $"Manifest Bill {billNumber}";

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new TWForwarderManifestCommand(shipment);

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = Factory.NewWithValidTestData<ForwarderManifest>();
			result.AMA_JobReference = "TestReference" + shipment.JS_TransportMode;
			result.AMA_ApplicationCode = "NVC";
			return result;
		}

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((ForwarderManifest)existingJob).AMA_JobReference;
	}
}
