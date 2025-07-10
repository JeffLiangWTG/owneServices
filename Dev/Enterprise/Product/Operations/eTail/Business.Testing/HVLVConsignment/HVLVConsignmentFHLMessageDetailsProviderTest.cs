using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVConsignmentFHLMessageDetailsProviderTest : TestCaseWithFactory
	{
		#region FHL Message Details Provider

		public void TestFHLMessageDetails_ConsignmentDetails()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WaybillNumber = "Test Waybill";
			consignment.HVC_GoodsDescription = "Test Goods";
			consignment.HVC_GoodsValue = 123m;
			consignment.HVC_RX_NKGoodsValueCurrency = "USD";
			consignment.HVC_WeightUQ = "KG";

			var item = consignment.Items.AddNew();
			item.HVI_ManifestedWeight = 456m;
			item.HVI_GoodsDescription = "Test Item";
			var line1 = item.Lines.AddNew();
			line1.HVS_Quantity = 1;
			var line2 = item.Lines.AddNew();
			line2.HVS_Quantity = 2;

			consignment.HVC_ItemCount = 1;
			consignment.HVC_ManifestedVolume = 12.3;
			consignment.HVC_VolumeUQ = "M3";

			var messageDetails = consignment.GetFHLMessageDetailsProvider();

			CombineAssertions(() =>
			{
				AssertEquals("HouseBill", "Test Waybill", messageDetails.HouseBill);
				AssertEquals("ShippingLoadAndCount", 1, messageDetails.ShippingLoadAndCount);
				AssertEquals("ManifestDescriptionOfGoods", "Test Goods", messageDetails.ManifestDescriptionOfGoods);
				AssertEquals("DetailedGoodsDescription", "Test Goods 1 VOL 12.3 M3", messageDetails.DetailedGoodsDescription);

				AssertEquals("DeclaredValue", 0m, messageDetails.DeclaredValue);
				AssertEquals("CustomsValue", 123m, messageDetails.CustomsValue);
				AssertEquals("InsuranceValue", 0m, messageDetails.InsuranceValue);

				AssertEquals("HouseInsuranceValueCurrency", ZString.Empty, messageDetails.HouseInsuranceValueCurrency);
				AssertEquals("HouseCustomsValueCurrency", "USD", messageDetails.HouseCustomsValueCurrency);
				AssertEquals("HouseDeclaredValueCurrency", ZString.Empty, messageDetails.HouseDeclaredValueCurrency);

				AssertEquals("TotalNoOfPieces", 3, messageDetails.TotalNoOfPieces);
				AssertEquals("TotalGrossWeight", 456m, messageDetails.TotalGrossWeight);

				AssertEquals(1, messageDetails.AWBRateLines.Count);
				var firstAWBRateLine = messageDetails.AWBRateLines.First();
				AssertEquals("NatureAndQtyOfGoodsDescription", "Test Goods", firstAWBRateLine.NatureAndQtyOfGoodsDescription);
				AssertEquals("WeightInLBsOrKGs", "K", firstAWBRateLine.WeightInLBsOrKGs);
			});
		}

		public void TestFHLMessageDetails_DetailedGoodsDescription_DefaultValue()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var messageDetails = consignment.GetFHLMessageDetailsProvider();
			AssertEquals("DetailedGoodsDescription", "0 VOL 0 M3", messageDetails.DetailedGoodsDescription);
		}

		public void TestFHLMessageDetails_ShipperDetails()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ShipperName = "Test Shipper";
			consignment.HVC_ShipperContact = "Test Contact";
			consignment.HVC_ShipperPhone = "87654321";

			consignment.HVC_ShipperAddress1 = "Test Address1";
			consignment.HVC_ShipperAddress2 = "Test Address2";
			consignment.HVC_ShipperCity = "Sydney";
			consignment.HVC_ShipperState = "NSW";
			consignment.HVC_ShipperPostcode = "12345678";
			consignment.HVC_RN_NKShipperCountryCode = "AU";

			var messageDetails = consignment.GetFHLMessageDetailsProvider();

			CombineAssertions(() =>
			{
				AssertEquals("Shipper Name", "Test Shipper", messageDetails.ShipperName);
				AssertEquals("Shipper Contact Name", "Test Contact", messageDetails.ShipperContactName);
				AssertEquals("Shipper Contact Detail", "87654321", messageDetails.ShipperContactDetail);
				AssertEquals("Shipper Contact Code", "TE", messageDetails.ShipperContactCode);

				AssertEquals("Shipper Address", "Test Address1", messageDetails.ShipperAddress);
				AssertEquals("Shipper Address2", "Test Address2", messageDetails.ShipperAddress2);
				AssertEquals("Shipper City", "Sydney", messageDetails.ShipperPlace);
				AssertEquals("Shipper State", "NSW", messageDetails.ShipperState);
				AssertEquals("Shipper Postcode", "12345678", messageDetails.ShipperPostCode);
				AssertEquals("Shipper Country Code", "AU", messageDetails.ShipperCountryCode);
			});
		}

		public void TestFHLMessageDetails_ConsigneeDetails()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ConsigneeName = "Test Consignee";
			consignment.HVC_ConsigneeContact = "Test Contact";
			consignment.HVC_ConsigneePhone = "87654321";

			consignment.HVC_ConsigneeAddress1 = "Test Address1";
			consignment.HVC_ConsigneeAddress2 = "Test Address2";
			consignment.HVC_ConsigneeCity = "Los Angeles";
			consignment.HVC_ConsigneeState = "CA";
			consignment.HVC_ConsigneePostcode = "12345678";
			consignment.HVC_RN_NKConsigneeCountryCode = "US";

			var messageDetails = consignment.GetFHLMessageDetailsProvider();

			CombineAssertions(() =>
			{
				AssertEquals("Consignee Name", "Test Consignee", messageDetails.ConsigneeName);
				AssertEquals("Consignee Contact Name", "Test Contact", messageDetails.ConsigneeContactName);
				AssertEquals("Consignee Contact Detail", "87654321", messageDetails.ConsigneeContactDetail);
				AssertEquals("Consignee Contact Code", "TE", messageDetails.ConsigneeContactCode);

				AssertEquals("Consignee Address", "Test Address1", messageDetails.ConsigneeAddress);
				AssertEquals("Consignee Address2", "Test Address2", messageDetails.ConsigneeAddress2);
				AssertEquals("Consignee City", "Los Angeles", messageDetails.ConsigneePlace);
				AssertEquals("Consignee State", "CA", messageDetails.ConsigneeState);
				AssertEquals("Consignee Postcode", "12345678", messageDetails.ConsigneePostCode);
				AssertEquals("Consignee Country Code", "US", messageDetails.ConsigneeCountryCode);
			});
		}

		public void TestFHLMessageDetails_HarmonicCodes()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			line1.HVS_Quantity = 1;
			line1.HVS_DestinationTariff = "H123456";
			var line2 = item.Lines.AddNew();
			line2.HVS_Quantity = 2;
			line2.HVS_DestinationTariff = "H876543";

			var messageDetails = consignment.GetFHLMessageDetailsProvider();

			AssertContainsExactElementsInAnyOrder("Harmonic codes", new[] { "H123456", "H876543" }, messageDetails.GetAvailableHarmonisedCodes());
		}

		public void TestFHLMessageDetails_HarmonicCodes_DoNotHaveDuplicates()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			line1.HVS_Quantity = 1;
			line1.HVS_DestinationTariff = "H123456";
			var line2 = item.Lines.AddNew();
			line2.HVS_Quantity = 2;
			line2.HVS_DestinationTariff = "H123456";

			var messageDetails = consignment.GetFHLMessageDetailsProvider();

			AssertEquals("Harmonic codes do not contain duplicates", 1, messageDetails.GetAvailableHarmonisedCodes().Count);
			AssertContainsExactElementsInAnyOrder("Harmonic codes", new[] { "H123456" }, messageDetails.GetAvailableHarmonisedCodes());
		}

		public void TestFHLMessageDetails_HarmonicCodes_DoNotIncludeInactiveItems()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			line1.HVS_Quantity = 1;
			line1.HVS_DestinationTariff = "H123456";

			var inactiveItem = consignment.Items.AddNew();
			var line2 = inactiveItem.Lines.AddNew();
			line2.HVS_Quantity = 2;
			line2.HVS_DestinationTariff = "H100200";
			inactiveItem.HVI_IsActive = false;

			var messageDetails = consignment.GetFHLMessageDetailsProvider();

			AssertEquals("Harmonic codes do not include inactive items", 1, messageDetails.GetAvailableHarmonisedCodes().Count);
			AssertContainsExactElementsInAnyOrder("Harmonic codes", new[] { "H123456" }, messageDetails.GetAvailableHarmonisedCodes());
		}

		public void TestFHLMessageDetails_HarmonicCodes_MultipleConsignmentsSameBookingHeader()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			var item1 = consignment1.Items.AddNew();
			var line1 = item1.Lines.AddNew();
			line1.HVS_Quantity = 1;
			line1.HVS_DestinationTariff = "H123456";

			var consignment2 = bookingHeader.Consignments.AddNew();
			var item2 = consignment2.Items.AddNew();
			var line2 = item2.Lines.AddNew();
			line2.HVS_Quantity = 1;
			line2.HVS_DestinationTariff = "H987654";

			var messageDetails = consignment1.GetFHLMessageDetailsProvider();

			AssertEquals("Harmonic codes only contain codes from relevant consignment", 1, messageDetails.GetAvailableHarmonisedCodes().Count);
			AssertContainsExactElementsInAnyOrder("Harmonic codes", new[] { "H123456" }, messageDetails.GetAvailableHarmonisedCodes());
		}

		public void TestDGUNNOValues_ContainsUNDGsOfItemsOfConsignment()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			item1.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			var item2 = consignment.Items.AddNew();
			item2.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1066", "", "IMO").First().PK;

			var messageDetails = consignment.GetFHLMessageDetailsProvider();

			AssertContainsExactElementsInAnyOrder(new[] { "0004", "1066" }, messageDetails.DGUNNOValues());
		}

		#endregion
	}
}
