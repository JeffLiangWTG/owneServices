using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using IAsycudaBill = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaBill;
using IAsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader;
using IAsycudaPackItem = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaPackedItem;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class TRETradeManifestConverterTest : BaseAsycudaManifestConverterTest<TRETradeManifestConverter>
	{
		public void TestEndToEndConvert()
		{
			#region Setup

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "LLM";
			var consigneeWithVAT = Factory.NewWithValidTestData<OrgHeader>();
			consigneeWithVAT.OH_Code = "OVO";

			var consignorAddress = consignor.Addresses.AddNew();
			consignorAddress.OA_Address1 = "RHODES ISLAND";
			var vat1 = consignor.CustomsCodes.AddNew();
			vat1.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vat1.OK_CustomsRegNo = "2233";
			vat1.OK_OA_PremisesAddress = consignorAddress.PK;

			var consigneeAddress = consigneeWithVAT.Addresses.AddNew();
			consigneeAddress.OA_Address1 = "LONGMEN";
			var vat2 = consigneeWithVAT.CustomsCodes.AddNew();
			vat2.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vat2.OK_RN_NKCodeCountry = CountryCodes.Turkey;
			vat2.OK_CustomsRegNo = "3322";
			vat2.OK_OA_PremisesAddress = consigneeAddress.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKDestination = "TRIST";
			shipment.JS_RL_NKOrigin = "AUSYD";

			var consol = shipment.Consols.AddNew();
			var transport = consol.Transports.AddNew();
			transport.JW_VoyageFlight = "AA1111";
			consol.JK_RL_NKLoadPort = "AUSYD";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_WaybillNumber = "ARKNIGHTS";
			consignment1.HVC_WeightUQ = Weight.Grams;
			consignment1.HVC_OA_ShipperAddress = consignorAddress.PK;
			consignment1.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment1.HVC_RX_NKGoodsValueCurrency = "CNY";

			var item1 = consignment1.Items.AddNew();
			item1.HVI_F3_NKPackType = PkgUnit.Package;
			item1.HVI_ManifestedWeight = 10000;
			item1.HVI_ActualWeight = 11000;
			var itemLine1 = item1.Lines.AddNew();
			itemLine1.HVS_Quantity = 3;
			itemLine1.HVS_RN_NKOriginCountryCode = CountryCodes.Singapore;
			itemLine1.HVS_CustomsValue = 1.23m;
			itemLine1.HVS_GoodsDescription = "THERMOS";
			itemLine1.HVS_NetWeight = 9;
			itemLine1.HVS_GrossWeight = 10;
			itemLine1.HVS_WeightUnit = Weight.Kilograms;
			itemLine1.HVS_IntrinsicValue = 4.56m;
			itemLine1.HVS_DestinationTariff = "123456";

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_WaybillNumber = "RHODES";
			var item2 = consignment2.Items.AddNew();
			var itemLine2 = item2.Lines.AddNew();
			itemLine2.HVS_Quantity = 1;
			itemLine2.HVS_WeightUnit = Weight.Pounds;
			itemLine2.HVS_GrossWeight = 10;

			Factory.Save();

			#endregion

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (ObjectFactory.Get<Enterprise.Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeETradeModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);
				var success = converter.TryConvert(out var errorMsg);
				Assert("Convert successfully", success);
				AssertNullOrEmpty("No error occured", errorMsg);

				var converterFactory = converter.CustomsRelatedBusinessCollection[0].Factory;
				converterFactory.Save();

				var manifestHeaderType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AsycudaManifestHeaderSchema.Constants.Prefix);
				var manifestHeader = Factory.Load(manifestHeaderType, new ZQuery()).SingleOrDefault();
				AssertNotNull("New Asycuda Header should be created", manifestHeader);
				Assert("New Asycuda Header should be saved", manifestHeader.IsInDatabase);

				var asycudaPackItemType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AsycudaPackedItemSchema.Constants.Prefix);
				var asycudaPackItems = Factory.Load(asycudaPackItemType, new ZQuery());

				CombineAssertions("End to end mapping", () =>
				{
					var header = manifestHeader as IAsycudaManifestHeader;
					AssertEquals($"Header: DepartureFlight from consol {nameof(ForwardingConsol.JK_JX_JV_VoyageFlight)}", "AA1111", header.DepartureFlight);
					AssertEquals($"Header: DepartureCountryCode from consol {nameof(ForwardingConsol.JK_RL_NKLoadPort)}", "AU", header.DepartureCountryCode);

					var bill1 = header.Bills.OfType<IAsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == consignment1.HVC_WaybillNumber);
					var bill2 = header.Bills.OfType<IAsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == consignment2.HVC_WaybillNumber);
					AssertEquals($"Bill: ABL_ManifestUQ {nameof(HVLVItem.HVI_F3_NKPackType)}", "PKG", bill1.ABL_ManifestUQ);
					AssertEquals($"Bill: ABL_ManifestUQ default to BI when no item pack type", "BI", bill2.ABL_ManifestUQ);
					AssertEquals($"Bill: ABL_NetWeight from {nameof(HVLVConsignment.HVC_ManifestedWeight)}", 10000m, bill1.ABL_NetWeight);
					AssertEquals($"Bill: ABL_NetWeightUQ from {nameof(HVLVConsignment.HVC_WeightUQ)}", "G", bill1.ABL_NetWeightUQ);
					AssertEquals($"Bill: ABL_ShipperRegNo from consignor VAT", "2233", bill1.ABL_ShipperRegNo);
					AssertEquals($"Bill: ABL_ConsigneeRegNo from consignee VAT", "3322", bill1.ABL_ConsigneeRegNo);
					AssertEquals($"Bill: ArrivalCountry default to TR", "TR", bill1.ArrivalCountry);

					var packedItem1 = asycudaPackItems.Single(x => (x as IAsycudaPackItem).API_ABL_Bill == bill1.PK) as IAsycudaPackItem;
					AssertNotNull(packedItem1);
					var packedItem2 = asycudaPackItems.Single(x => (x as IAsycudaPackItem).API_ABL_Bill == bill2.PK) as IAsycudaPackItem;
					AssertNotNull(packedItem2);
					AssertEquals($"PackedItem: API_GoodsValue from {nameof(HVLVItemLine.HVS_CustomsValue)}", 1.23m, packedItem1.API_GoodsValue);
					AssertEquals($"PackedItem: API_GoodsDescription from {nameof(HVLVItemLine.HVS_GoodsDescription)}", "THERMOS", packedItem1.API_GoodsDescription);
					AssertEquals($"PackedItem: API_CustomsQty from {nameof(HVLVItemLine.HVS_Quantity)}", 3m, packedItem1.API_CustomsQty);
					AssertEquals($"PackedItem: API_CustomsQty2 from {nameof(HVLVItemLine.HVS_GrossWeight)} and converted into KG", 4.535924m, packedItem2.API_CustomsQty2);
					AssertEquals($"PackedItem: API_GrossWeight from {nameof(HVLVItemLine.HVS_GrossWeight)}", 10m, packedItem1.API_GrossWeight);
					AssertEquals($"PackedItem: API_GrossWeightUQ from {nameof(HVLVItemLine.HVS_WeightUnit)}", "KG", packedItem1.API_GrossWeightUQ);
					AssertEquals($"PackedItem: API_NetWeight from {nameof(HVLVItemLine.HVS_NetWeight)}", 9m, packedItem1.API_NetWeight);
					AssertEquals($"PackedItem: API_NetWeightUQ from {nameof(HVLVItemLine.HVS_WeightUnit)}", "KG", packedItem1.API_NetWeightUQ);
					AssertEquals($"PackedItem: API_CustomsValue from {nameof(HVLVItemLine.HVS_IntrinsicValue)}", 4.56m, packedItem1.API_CustomsValue);
					AssertEquals($"PackedItem: API_RX_NKGoodsValueCurrency from {nameof(HVLVConsignment.HVC_RX_NKGoodsValueCurrency)}", "CNY", packedItem1.API_RX_NKGoodsValueCurrency);
					AssertEquals($"PackedItem: API_RN_NKGoodsOrigin from {nameof(HVLVItemLine.HVS_RN_NKOriginCountryCode)}", "SG", packedItem1.API_RN_NKGoodsOrigin);
					AssertEquals($"PackedItem: API_Tariff from {nameof(HVLVItemLine.HVS_DestinationTariff)} when import", "123456", packedItem1.API_Tariff);
				});
			}
		}

		protected override ZString LoginCountry => CountryCodes.Turkey;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Air, TransportModes.Sea };

		protected override string GetBillHumanReadableName(string billNumber) => $"Manifest Bill {billNumber}";

		protected override string GetExpectedMessageTypeCode(ForwardingShipment shipment) => TRETradeManifestTypes.Codes.TRETrade;

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new TRETradeManifestCommand(shipment);

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = Factory.New<IAsycudaManifestHeader>();
			result.AMA_JobReference = "TestReference" + shipment.JS_TransportMode;
			result.AMA_ApplicationCode = "ETR";
			return result as BusinessObject;
		}

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((IAsycudaManifestHeader)existingJob).AMA_JobReference;
	}
}
