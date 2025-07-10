using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class AUSeaCargoReportConverterTest : CustomsRelatedBusinessObjectConverterBaseTest<SeaOceanBillConverter>
	{
		public void TestConvertShipmentToCargoReport_EndToEnd_Australia()
		{
			AssertConvertShipmentToCargoReport_EndToEnd(TransportModes.Sea, CusSCAHouseSchema.Constants.Prefix);
		}

		public void TestConvertShipmentToCargoReport_Cancel_Australia()
		{
			AssertConvertShipmentToCargoReport_Cancel(TransportModes.Sea, CusSCAOceanBillSchema.Constants.Prefix, CusSCAHouseSchema.Constants.Prefix);
		}

		public void TestConvertShipmentToCargoReport_ShouldShowErrorWhenContainerIsNotProvided()
		{
			var shipment = SetupTestShipment(TransportModes.Sea);
			foreach (var item in shipment.HVLVItems)
			{
				item.HVI_ContainerNumber = string.Empty;
			}

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var success = converter.TryConvert(out var errorMsg);

				Assert("Creating cargo report should be failed", !success);
				AssertEquals("Error message should popup to block creating.", @"Failed to read into SeaOceanBill from Shipment [EBM22Q33TU475BXH3P60]
Cannot populate CusSCAPivot because:
Cannot find any matching container for container number 
Cannot populate CusSCAPivot because:
Cannot find any matching container for container number ", errorMsg);
			}
		}

		public void TestSyncShipmentToCargoReport_HouseBillHaveActiveMessageStatus_WillNotBlockOtherHouseBillsFromSyncing()
		{
			var origin = GetOriginPort(isDestinationSameAsLoginCountry: true);
			var destination = GetDestinationPort(isDestinationSameAsLoginCountry: true);

			var arrivalConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			arrivalConsol.JK_TransportMode = TransportModes.Sea;
			arrivalConsol.JK_MasterBillNum = "08138374491";
			arrivalConsol.JK_RL_NKLoadPort = origin.Code;
			arrivalConsol.JK_RL_NKDischargePort = destination.Code;

			var container = arrivalConsol.Containers.AddNew();
			container.ContainerNumberForBinding = "Container001";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_HouseBill = "UWM97N872947";
			shipment.JS_RL_NKOrigin = origin.Code;
			shipment.JS_RL_NKDestination = destination.Code;

			arrivalConsol.Shipments.Add(shipment);

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item1.HVI_ContainerNumber = container.ContainerNumberForBinding;

			var consignment1 = item1.Consignment;
			consignment1.HVC_WaybillNumber = "HVC00001";
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_ContainerNumber = container.ContainerNumberForBinding;

			var consignment2 = item2.Consignment;
			consignment2.HVC_WaybillNumber = "HVC00002";
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			var success = converter.TryConvert(out var errorMsg);
			Assert("Convert successfully", success);
			AssertNullOrEmpty("No error occured", errorMsg);

			var masterBill = converter.CustomsRelatedBusinessCollection.Cast<CusSCAOceanBill>().Single();
			AssertEquals("Pre-condition: 2 house bills", 2, masterBill.HouseBills.Count);

			var houseBill1 = masterBill.HouseBills.Cast<CusSCAHouse>().Single(h => h.CA_HouseBill == "HVC00001");
			houseBill1.CA_MessageStatus = CMRBaseStatuses.Codes.AmendmentAccepted;

			masterBill.Factory.Save();

			consignment1.HVC_GoodsValue = 10;
			consignment1.HVC_RX_NKGoodsValueCurrency = "CNY";
			consignment2.HVC_GoodsValue = 10;
			consignment2.HVC_RX_NKGoodsValueCurrency = "CNY";

			var item3 = Factory.NewWithValidTestData<HVLVItem>();
			item3.HVI_JS_LoadedOnShipment = shipment.PK;
			item3.HVI_ContainerNumber = container.ContainerNumberForBinding;

			var consignment3 = item3.Consignment;
			consignment3.HVC_WaybillNumber = "HVC00003";
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			success = converter.TryConvert(out errorMsg);
			CombineAssertions(() =>
			{
				Assert("Convert successfully", success);
				AssertNullOrEmpty("No error occured", errorMsg);
			});

			masterBill = converter.CustomsRelatedBusinessCollection.Single() as CusSCAOceanBill;
			var houseBills = masterBill.HouseBills.Cast<CusSCAHouse>();
			AssertEquals("consignment3 should be synced as the 3rd house bill", 3, houseBills.Count());
			AssertEquals("HVC00001 is message active so cannot be updated", 0m, houseBills.Single(h => h.CA_HouseBill == "HVC00001").CA_GoodsValue);
			AssertEquals("HVC00002 is updated", 10m, houseBills.Single(h => h.CA_HouseBill == "HVC00002").CA_GoodsValue);
		}

		public void TestConvertShipmentToCargoReport_ShouldShowErrorWhenContainerIsInvalid()
		{
			var shipment = SetupTestShipment(TransportModes.Sea);
			foreach (var item in shipment.HVLVItems)
			{
				item.HVI_ContainerNumber = "INVALID";
			}

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var success = converter.TryConvert(out var errorMsg);

				Assert("Creating cargo report should be failed", !success);
				AssertEquals("Error message should popup to block creating.", @"Failed to read into SeaOceanBill from Shipment [EBM22Q33TU475BXH3P60]
Cannot populate CusSCAPivot because:
Cannot find any matching container for container number INVALID
Cannot populate CusSCAPivot because:
Cannot find any matching container for container number INVALID", errorMsg);
			}
		}

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new AUSeaCargoReportCommand(shipment);

		protected override ZString LoginCountry => CountryCodes.Australia;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Sea };

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = Factory.NewWithValidTestData<CusSCAOceanBill>();
			result.CB_MessageReference = "TestReference";
			return result;
		}

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((CusSCAOceanBill)existingJob).CB_MessageReference;
	}
}
