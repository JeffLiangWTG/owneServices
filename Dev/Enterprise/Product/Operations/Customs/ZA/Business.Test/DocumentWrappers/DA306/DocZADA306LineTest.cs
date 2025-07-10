using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocZADA306Line))]
	public class DocZADA306LineTest : DocumentWrapperTestCase
	{
		public void TestFlightNumber()
		{
			using (Factory.GetDocWrapperContextManager().SetContextValuesTemporarily(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV, new Moq.Mock<IContactType>().Object))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var item = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew().Items.AddNew();

				var wrapper = DocZADA306Line.New(item, Factory);
				AssertEquals(string.Empty, wrapper.FlightNumber);

				shipment.JS_RL_NKOrigin = "AUMEL";
				shipment.JS_RL_NKDestination = "ZAJNB";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "AUMEL";
				departureConsol.JK_RL_NKDischargePort = "NZAKL";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "NZAKL";
				arrivalConsol.JK_RL_NKDischargePort = "ZAJNB";

				var transport = arrivalConsol.Transports[0];
				transport.JW_VoyageFlight = "123";

				AssertEquals("123", wrapper.FlightNumber);
			}
		}

		public void TestMasterAirwayBillNumber()
		{
			using (Factory.GetDocWrapperContextManager().SetContextValuesTemporarily(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV, new Moq.Mock<IContactType>().Object))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var item = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew().Items.AddNew();

				var wrapper = DocZADA306Line.New(item, Factory);
				AssertEquals(string.Empty, wrapper.MasterAirwayBillNumber);

				shipment.JS_RL_NKOrigin = "AUMEL";
				shipment.JS_RL_NKDestination = "ZAJNB";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "AUMEL";
				departureConsol.JK_RL_NKDischargePort = "NZAKL";
				departureConsol.JK_MasterBillNum = "1";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "NZAKL";
				arrivalConsol.JK_RL_NKDischargePort = "ZAJNB";
				arrivalConsol.JK_MasterBillNum = "2";

				AssertEquals("2", wrapper.MasterAirwayBillNumber);
			}
		}

		public void TestHouseAirwayBillNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);
			AssertEquals(string.Empty, wrapper.HouseAirwayBillNumber);

			consignment.HVC_WaybillNumber = "1";
			AssertEquals("1", wrapper.HouseAirwayBillNumber);
		}

		public void TestCountryOfOrigin()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			var line2 = item.Lines.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);
			AssertEquals(string.Empty, wrapper.CountryOfOrigin);

			line1.HVS_RN_NKOriginCountryCode = "AU";
			line2.HVS_RN_NKOriginCountryCode = "CN";

			AssertContainsExactElementsInAnyOrder(new[] { "AU", "CN" }, wrapper.CountryOfOrigin.Split(","));
		}

		public void TestShipper()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var wrapper = DocZADA306Line.New(item, Factory);

			AssertEquals(string.Empty, wrapper.Shipper);

			consignment.HVC_ShipperName = "Shipper Name";
			Assert("Precondition: consignment shipper is not org", !consignment.ShipperIsOrganisation);
			AssertEquals("Shipper Name", wrapper.Shipper);

			var shipperOrgHeader = Factory.New<OrgHeader>();
			shipperOrgHeader.OH_FullName = "Shipper Organization FullName";
			var shipperAddress = shipperOrgHeader.Addresses.AddNew();
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;

			Assert("Precondition: consignment shipper is org", consignment.ShipperIsOrganisation);
			AssertEquals("Shipper Organization FullName", wrapper.Shipper);
		}

		public void TestDestinationCityTown()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);
			AssertEquals(string.Empty, wrapper.DestinationCityTown);

			consignment.HVC_ConsigneeCity = "Consignee City";
			AssertEquals("Consignee City", wrapper.DestinationCityTown);
		}

		public void TestConsigneeFullNames()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var wrapper = DocZADA306Line.New(item, Factory);

			AssertEquals(string.Empty, wrapper.ConsigneeFullNames);

			consignment.HVC_ConsigneeName = "Consignee Name";
			Assert("Precondition: consignment consignee is not org", !consignment.ConsigneeIsOrganisation);
			AssertEquals("Consignee Name", wrapper.ConsigneeFullNames);

			var consigneeOrgHeader = Factory.New<OrgHeader>();
			consigneeOrgHeader.OH_FullName = "Consignee Organization FullName";
			var consigneeAddress = consigneeOrgHeader.Addresses.AddNew();
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			Assert("Precondition: consignment consignee is org", consignment.ConsigneeIsOrganisation);
			AssertEquals("Consignee Organization FullName", wrapper.ConsigneeFullNames);
		}

		public void TestConsigneeID()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var wrapper = DocZADA306Line.New(item, Factory);

			AssertEquals(string.Empty, wrapper.ConsigneeID);

			var cpaNum = consignment.CustomsReferenceNumbers.AddNew();
			cpaNum.CE_EntryType = "CPA";
			cpaNum.CE_EntryNum = "111";

			AssertEquals("111", wrapper.ConsigneeID);

			var cidNum = consignment.CustomsReferenceNumbers.AddNew();
			cidNum.CE_EntryType = "CID";
			cidNum.CE_EntryNum = "222";

			AssertEquals("222", wrapper.ConsigneeID);
		}

		public void TestNumberOfPieces()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			var line2 = item.Lines.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);

			AssertEquals(0, wrapper.NumberOfPieces);

			line1.HVS_Quantity = 2;
			line2.HVS_Quantity = 1;

			AssertEquals(3, wrapper.NumberOfPieces);
		}

		public void TestWeightOfPackage()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);
			AssertEquals(0m, wrapper.WeightOfPackage);

			item.HVI_ManifestedWeight = 1;
			consignment.HVC_ManifestedWeight = 10;
			AssertEquals("Precondition: consignment has only 1 item", (ZShort)1, consignment.HVC_ItemCount);

			AssertEquals("HVC_ManifestedWeight", 10m, wrapper.WeightOfPackage);
			consignment.HVC_ActualWeight = 11;
			AssertEquals("HVC_ActualWeight", 11m, wrapper.WeightOfPackage);

			consignment.Items.AddNew();
			AssertLessThan("Precondition: consignment has more than 1 item", 1, consignment.HVC_ItemCount);
			AssertEquals("HVI_ManifestedWeight", 1m, wrapper.WeightOfPackage);

			item.HVI_ActualWeight = 2;
			AssertEquals("HVI_ActualWeight", 2m, wrapper.WeightOfPackage);
		}

		public void TestDescriptionOfPackage()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);
			AssertEquals(string.Empty, wrapper.DescriptionOfPackage);

			item.HVI_GoodsDescription = "Goods description";
			AssertEquals("Goods description", wrapper.DescriptionOfPackage);
		}

		public void TestClassificationHSCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			var line2 = item.Lines.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);

			AssertEquals(string.Empty, wrapper.ClassificationHSCode);

			line1.HVS_FormattedDestinationTariff = "123";
			line2.HVS_FormattedDestinationTariff = "789";

			AssertContainsExactElementsInAnyOrder(new[] { "123", "789" }, wrapper.ClassificationHSCode.Split(","));
		}

		public void TestForeignValue()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			var line2 = item.Lines.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);

			AssertEquals(0m, wrapper.ForeignValue);

			consignment.HVC_GoodsValue = 10;
			line1.HVS_IntrinsicValue = 1;
			line2.HVS_IntrinsicValue = 2;

			AssertEquals("Precondition: 1 item in consignment", (ZShort)1, consignment.HVC_ItemCount);
			wrapper = DocZADA306Line.New(item, Factory);
			AssertEquals("Consignment goods value", 10m, wrapper.ForeignValue);

			consignment.Items.AddNew();
			AssertLessThan("Precondition: consignment has more than 1 item", 1, consignment.HVC_ItemCount);
			wrapper = DocZADA306Line.New(item, Factory);
			AssertEquals("sum of lines intrinsic value", 3m, wrapper.ForeignValue);
		}

		public void TestCurrency()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);
			AssertEquals(string.Empty, wrapper.Currency);

			consignment.HVC_RX_NKGoodsValueCurrency = "AUD";
			AssertEquals("AUD", wrapper.Currency);
		}

		public void TestValueRand()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);
			AssertEquals(0m, wrapper.ValueRand);

			consignment.HVC_RX_NKGoodsValueCurrency = "ZAR";
			consignment.HVC_GoodsValue = 10;

			wrapper = DocZADA306Line.New(item, Factory);
			AssertEquals(10m, wrapper.ValueRand);
		}

		public void TestCustomsDuty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			var line2 = item.Lines.AddNew();

			var wrapper = DocZADA306Line.New(item, Factory);

			consignment.HVC_RX_NKGoodsValueCurrency = "ZAR";
			line1.HVS_CustomsValue = 1;
			line2.HVS_CustomsValue = 2;

			AssertEquals("20% of sum of customs value", 0.6m, wrapper.CustomsDuty);
		}

		public void TestATVAndVAT()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			consignment.HVC_RX_NKGoodsValueCurrency = "ZAR";
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			var line2 = item.Lines.AddNew();
			line1.HVS_CustomsValue = 1;
			line2.HVS_CustomsValue = 2;

			CombineAssertions(() =>
			{
				foreach (var countryCode in new[] { "BW", "SZ", "LS", "NA" })
				{
					line1.HVS_RN_NKOriginCountryCode = countryCode;
					line2.HVS_RN_NKOriginCountryCode = countryCode;
					var wrapper = DocZADA306Line.New(item, Factory);
					AssertEquals($"{countryCode}: specified countries with unity multiplier", 3m, wrapper.ATV);
					AssertEquals($"{countryCode}: VAT is 15% ATV", 0.45m, wrapper.VAT);
				}

				foreach (var countryCode in new[] { "AU", "CN" })
				{
					line1.HVS_RN_NKOriginCountryCode = countryCode;
					line2.HVS_RN_NKOriginCountryCode = countryCode;
					var wrapper = DocZADA306Line.New(item, Factory);
					AssertEquals($"{countryCode}: unspecified countries get 1.1 multiplier", 3.3m, wrapper.ATV);
					AssertEquals($"{countryCode}: VAT is 15% ATV", 0.495m, wrapper.VAT);
				}
			});
		}

		public void TestTotalPayable()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			consignment.HVC_RX_NKGoodsValueCurrency = "ZAR";
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			var line2 = item.Lines.AddNew();
			line1.HVS_CustomsValue = 1;
			line2.HVS_CustomsValue = 2;
			line1.HVS_RN_NKOriginCountryCode = "BW";
			line2.HVS_RN_NKOriginCountryCode = "BW";

			var wrapper = DocZADA306Line.New(item, Factory);

			// Total Payable = CustomsDuty + VAT
			AssertEquals("CustomsDuty", 0.6m, wrapper.CustomsDuty);
			AssertEquals("VAT", 0.45m, wrapper.VAT);
			AssertEquals("TotalPayable", 1.05m, wrapper.TotalPayable);
		}

		public void TestDescriptionAsPerHSCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			var line2 = item.Lines.AddNew();
			var wrapper = DocZADA306Line.New(item, Factory);

			AssertEquals(string.Empty, wrapper.DescriptionAsPerHSCode);

			line1.HVS_GoodsDescription = "line 1 description";
			line2.HVS_GoodsDescription = "line 2 description";

			AssertContainsExactElementsInAnyOrder(new[] { "line 1 description", "line 2 description" }, wrapper.DescriptionAsPerHSCode.Split(","));
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var item = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew().Items.AddNew();
			return new DocumentWrapper[]
			{
				DocZADA306Line.New(item, Factory)
			};
		}
	}
}
