using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	public class eTailUSLVConsignmentDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[ExpectNoExceptions]
		public void TestPopulateItemForClearanceComesFromLoadPortWithoutContryStates()
		{
			var shipment = eTailUSLVClearanceDataObjectReaderTest.SetupETailTestingShipmentWithMinimumRequirements();
			shipment.PortOfLoading = new UNLOCO() { Code = "HKHKG" };
			var clearance = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, shipment);

			AssertNull("post condition: clearance's Port of Loading doesn't have CountryStates", clearance.PortOfLoading.CountryStates);
		}

		public void TestClearAndCreateNewItemCollectionForConsignment()
		{
			var shipment = eTailUSLVClearanceDataObjectReaderTest.SetupETailTestingShipmentWithMinimumRequirements();
			var consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, shipment).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();

			var item = consignment.CusUSLVItems[0];

			var shouldBeOriginal_Consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, shipment).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
			AssertEquals("precondition: original consignment matched", consignment.PK, shouldBeOriginal_Consignment.PK);
			var shouldBeNew_Item = consignment.CusUSLVItems[0];
			AssertNotEquals("new item created", item.PK, shouldBeNew_Item.PK);
		}

		public void TestContainerNumberFromPackingLineCollection()
		{
			var shipment = eTailUSLVClearanceDataObjectReaderTest.SetupETailTestingShipmentWithMinimumRequirements();
			var consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, shipment).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
			AssertEquals("CTNR000001", consignment.ULB_EquipmentNumber);

			shipment.SubShipmentCollection[0].SubShipmentCollection[0].PackingLineCollection.Add(
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerNumber = "CTNR009933"
				});

			consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, shipment).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
			AssertEquals(ZString.Empty, consignment.ULB_EquipmentNumber);
		}

		public void TestDeclarationReferanceForConsignment()
		{
			var shipment = eTailUSLVClearanceDataObjectReaderTest.SetupETailTestingShipmentWithMinimumRequirements();
			var consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, shipment).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
			AssertEquals("ABC123456", consignment.CE_EntryLineReference);
			AssertEquals(false, consignment.ULB_IsActive);
		}

		public void TestPopulateConsignments()
		{
			var consignment = eTailUSLVClearanceDataObjectReaderTest.GetSampleClearance(Factory).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
			var item = consignment.CusUSLVItems[0];
			CombineAssertions(() =>
			{
				AssertEquals((short)1, consignment.ULB_NumberOfPacks);
				AssertEquals("TESTVENDOR", consignment.ULB_SellerIdentifier);
				AssertEquals("EIN123", consignment.ULB_ConsigneeIdentifier);
				AssertEquals("CNY", item.ULI_RX_NKCurrency);
				AssertEquals("CN", item.ULI_RN_NKCountryOfOrigin);
				AssertEquals("NIKE BASKETBALL SHOES", item.ULI_GoodsDescription);
				AssertEquals("7891238949", item.ULI_Tariff);
				AssertEquals(999m, item.ULI_GoodsValue);
				AssertEquals("AA", item.ULI_PartNo);
			});
		}

		public void TestPopulateItemCountryOfOrigin_WhenShipmentFromCanadaAndGoodsOriginIsCanada_PopulateCanadianProvinceCode()
		{
			var shipmentFromCanada = eTailUSLVClearanceDataObjectReaderTest.SetupETailTestingShipmentWithMinimumRequirements();
			shipmentFromCanada.PortOfLoading = new UNLOCO() { Code = "CATOR" };
			var invoiceLine = shipmentFromCanada.SubShipmentCollection[0].SubShipmentCollection[0].CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.First(x => x.Description.Value == "Some goods");
			invoiceLine.CountryOfOrigin = new Country() { Code = "CA" };
			var consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, shipmentFromCanada).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
			var item = consignment.CusUSLVItems.OfType<CusUSLVItem>().First(x => x.ULI_GoodsDescription == "SOME GOODS");
			CombineAssertions(() =>
			{
				AssertEquals((short)1, consignment.ULB_NumberOfPacks);
				AssertEquals("CNY", item.ULI_RX_NKCurrency);
				AssertEquals(CanadaProvinceTerritoryCodes.Codes.XO, item.ULI_RN_NKCountryOfOrigin);
				AssertEquals("7891238948", item.ULI_Tariff);
				AssertEquals(1m, item.ULI_GoodsValue);
				AssertEquals("BB", item.ULI_PartNo);
			});
		}

		public void TestPopulateItemCountryOfOrigin_WhenShipmentFromCanadaAndGoodsOriginEmpty_LeaveEmpty()
		{
			var shipmentFromCanada = eTailUSLVClearanceDataObjectReaderTest.SetupETailTestingShipmentWithMinimumRequirements();
			shipmentFromCanada.PortOfLoading = new UNLOCO() { Code = "CATOR" };
			var invoiceLine = shipmentFromCanada.SubShipmentCollection[0].SubShipmentCollection[0].CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.First(x => x.Description.Value == "Some goods");
			var customsSupportInfo = invoiceLine.CustomsSupportingInformationCollection[0];
			customsSupportInfo.Country.Code = ZString.Empty;
			var consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, shipmentFromCanada).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
			var item = consignment.CusUSLVItems.OfType<CusUSLVItem>().First(x => x.ULI_GoodsDescription == "SOME GOODS");
			CombineAssertions(() =>
			{
				AssertEquals((short)1, consignment.ULB_NumberOfPacks);
				AssertEquals("CNY", item.ULI_RX_NKCurrency);
				AssertEquals(ZString.Empty, item.ULI_RN_NKCountryOfOrigin);
				AssertEquals("7891238948", item.ULI_Tariff);
				AssertEquals(1m, item.ULI_GoodsValue);
				AssertEquals("BB", item.ULI_PartNo);
			});
		}

		public void TestPopulateItemCountryOfOrigin_WhenShipmentFromCanadaAndGoodsOriginIsNotCanada_PopulateFromGoodsOrigin()
		{
			var shipmentFromCanada = eTailUSLVClearanceDataObjectReaderTest.SetupETailTestingShipmentWithMinimumRequirements();
			shipmentFromCanada.PortOfLoading = new UNLOCO() { Code = "CATOR" };
			var invoiceLine = shipmentFromCanada.SubShipmentCollection[0].SubShipmentCollection[0].CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.First(x => x.Description.Value == "Some goods");
			invoiceLine.CountryOfOrigin = new Country() { Code = "AU" };
			var customsSupportInfo = invoiceLine.CustomsSupportingInformationCollection[0];
			customsSupportInfo.Country.Code = "AU";
			var consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, shipmentFromCanada).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
			var item = consignment.CusUSLVItems.OfType<CusUSLVItem>().First(x => x.ULI_GoodsDescription == "SOME GOODS");
			CombineAssertions(() =>
			{
				AssertEquals((short)1, consignment.ULB_NumberOfPacks);
				AssertEquals("CNY", item.ULI_RX_NKCurrency);
				AssertEquals("AU", item.ULI_RN_NKCountryOfOrigin);
				AssertEquals("7891238948", item.ULI_Tariff);
				AssertEquals(1m, item.ULI_GoodsValue);
				AssertEquals("BB", item.ULI_PartNo);
			});
		}

		public void TestPopulateItemCountryOfOrigin_WhenShipmentNotFromCanadaAndGoodsOriginIsCanada_PopulateFromGoodsOrigin()
		{
			var shipmentFromCanada = eTailUSLVClearanceDataObjectReaderTest.SetupETailTestingShipmentWithMinimumRequirements();
			var invoiceLine = shipmentFromCanada.SubShipmentCollection[0].SubShipmentCollection[0].CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.First(x => x.Description.Value == "Some goods");
			invoiceLine.CountryOfOrigin = new Country() { Code = "CA" };
			var consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, shipmentFromCanada).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
			var item = consignment.CusUSLVItems.OfType<CusUSLVItem>().First(x => x.ULI_GoodsDescription == "SOME GOODS");
			CombineAssertions(() =>
			{
				AssertEquals((short)1, consignment.ULB_NumberOfPacks);
				AssertEquals("CNY", item.ULI_RX_NKCurrency);
				AssertEquals("CA", item.ULI_RN_NKCountryOfOrigin);
				AssertEquals("7891238948", item.ULI_Tariff);
				AssertEquals(1m, item.ULI_GoodsValue);
				AssertEquals("BB", item.ULI_PartNo);
			});
		}

		public void TestPopulateGoodsDescriptionFallback()
		{
			var universalShipment = eTailUSLVClearanceDataObjectReaderTest.SetupETailTestingShipmentWithMinimumRequirements();
			AssertItemDescriptionEquals("NIKE BASKETBALL SHOES");

			var invoiceLine = universalShipment.SubShipmentCollection.Single().SubShipmentCollection.Single().CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection[0];
			invoiceLine.Description = "";
			AssertItemDescriptionEquals("BASKETBALL SHOES");

			var packedItem = universalShipment.SubShipmentCollection.Single().SubShipmentCollection.Single().PackingLineCollection.Single().PackedItemCollection.Single();
			packedItem.Description = "";
			AssertItemDescriptionEquals("SHOES");

			void AssertItemDescriptionEquals(string expected)
			{
				var consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, universalShipment).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
				var item = consignment.CusUSLVItems[0];
				AssertEquals(expected, item.ULI_GoodsDescription);
			}
		}

		public void TestPopulateGoodsValueFallback()
		{
			var universalShipment = eTailUSLVClearanceDataObjectReaderTest.SetupETailTestingShipmentWithMinimumRequirements();
			AssertItemValueEquals(999);

			var invoiceLine = universalShipment.SubShipmentCollection.Single().SubShipmentCollection.Single().CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection[0];
			invoiceLine.CustomsValue = 700;
			AssertItemValueEquals(700);

			void AssertItemValueEquals(decimal expected)
			{
				var consignment = eTailUSLVClearanceDataObjectReaderTest.ReadIntoUSLVClearance(Factory, universalShipment).CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
				var item = consignment.CusUSLVItems[0];
				AssertEquals(expected, item.ULI_GoodsValue);
			}
		}
	}
}
