using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	sealed class ETailCusSCAPackingLineDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestPopulateGoodsDescription

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsDescription_UsesItemLineDescription()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			itemLines[0].Description = "TESTLINEDESCA";
			itemLines[1].Description = "TESTLINEDESCB";
			itemLines[2].Description = "TESTLINEDESCC";

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var lineDescriptions = houseBill.PackingLines.Select(line => line.CV_GoodsDescription);

			AssertContainsExactElementsInAnyOrder("Goods Description should be set from Item Line Description", new[] { "TESTLINEDESCA", "TESTLINEDESCB", "TESTLINEDESCC" }, lineDescriptions);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsDescription_UsesItemLineDescription_MaxLength()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();

			var itemLines = item.PackedItemCollection;
			var maxDesc1 = new ZString('Z', CusSCAPackingLine.Schema.CV_GoodsDescriptionMaxLength);
			var maxDesc2 = new ZString('X', CusSCAPackingLine.Schema.CV_GoodsDescriptionMaxLength);
			itemLines[0].Description = "common desc";
			itemLines[1].Description = maxDesc1;
			itemLines[2].Description = maxDesc2 + "1";

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var lineDescriptions = houseBill.PackingLines.Select(line => line.CV_GoodsDescription);

			AssertEquals("common desc", true, lineDescriptions.Contains("common desc"));
			AssertEquals("maxlength value", true, lineDescriptions.Contains(maxDesc1));
			AssertEquals("No error when setting over-maxlength value", true, lineDescriptions.Contains(maxDesc2));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsDescription_WhenMissingItemLineDescription_FallsbackToItemDescription()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();

			var item = consignment.PackingLineCollection.Single();
			item.GoodsDescription = "TESTITEMDESC";

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			itemLines[0].Description = string.Empty;
			itemLines[1].Description = string.Empty;
			itemLines[2].Description = string.Empty;

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var lineDescriptions = houseBill.PackingLines.Select(line => line.CV_GoodsDescription);

			AssertContainsExactElementsInAnyOrder("Goods Description should fallback to Item Description", new[] { "TESTITEMDESC", "TESTITEMDESC", "TESTITEMDESC" }, lineDescriptions);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsDescription_WhenMissingItemDescription_FallsbackToConsignmentDescription()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();

			var consignment = shipment.SubShipmentCollection.Single();
			consignment.GoodsDescription = "TESTCONSDESC";

			var item = consignment.PackingLineCollection.Single();
			item.GoodsDescription = string.Empty;

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			itemLines[0].Description = string.Empty;
			itemLines[1].Description = string.Empty;
			itemLines[2].Description = string.Empty;

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var lineDescriptions = houseBill.PackingLines.Select(line => line.CV_GoodsDescription);

			AssertContainsExactElementsInAnyOrder("Goods Description should fallback to Consignment Goods Description", new[] { "TESTCONSDESC", "TESTCONSDESC", "TESTCONSDESC" }, lineDescriptions);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsDescription_WhenNoItemLines_UsesItemDescription()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();

			var consignment = shipment.SubShipmentCollection.Single();

			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			item.GoodsDescription = "TESTITEMDESC";

			item.SetPackedItemCollection(() => new List<PackedItem>());
			AssertEquals("Precondition: XML should have no item lines", 0, item.PackedItemCollection.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("When no Item Lines, should still create a single Packing Line", 1, houseBill.PackingLines.Count);

			var lineDescription = houseBill.PackingLines.Single().CV_GoodsDescription;
			AssertEquals("Goods Description should use Item Goods Description when no Item Lines", "TESTITEMDESC", lineDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsDescription_WhenNoItemLines_AndMissingItemDescription_UsesConsignmentDescription()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();

			var consignment = shipment.SubShipmentCollection.Single();
			consignment.GoodsDescription = "TESTCONSDESC";

			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			item.GoodsDescription = string.Empty;

			item.SetPackedItemCollection(() => new List<PackedItem>());
			AssertEquals("Precondition: XML should have no item lines", 0, item.PackedItemCollection.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("When no Item Lines, should still create a single Packing Line", 1, houseBill.PackingLines.Count);

			var lineDescription = houseBill.PackingLines.Single().CV_GoodsDescription;
			AssertEquals("Goods Description should fallback to Consignment Goods Description when missing Item description", "TESTCONSDESC", lineDescription);
		}

		#endregion

		#region TestPopulateHarmonisedTariffNums

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateHarmonisedTariffNums_WhenBillIsImport_UsesInvoiceLineHarmonisedCode()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			consol.PortOfLoading = new UNLOCO() { Code = "AUTES" };
			consol.PortOfDischarge = new UNLOCO() { Code = "NZTES" };

			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();

			var item = consignment.PackingLineCollection.Single();

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var invoiceLines = itemLines.Select(line => line.FindMatchingCommercialInvoiceLine(consignment)).ToList();
			invoiceLines[0].HarmonisedCode = "6666";
			invoiceLines[1].HarmonisedCode = "5555";
			invoiceLines[2].HarmonisedCode = "4444";

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);
			AssertEquals("Should have an import bill", true, oceanBill.IsImport);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var harmonisedTariffNums = houseBill.PackingLines.Select(line => line.CV_HarmonisedTariffNums);

			AssertContainsExactElementsInAnyOrder("When Import, Harmonised Tariff should be set from Invoice Line Harmonised Code", new[] { "6666", "5555", "4444" }, harmonisedTariffNums);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateHarmonisedTariffNums_WhenBillIsExport_UsesInvoiceLineCustomsInformationTariff()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			consol.PortOfLoading = new UNLOCO() { Code = "NZTES" };
			consol.PortOfDischarge = new UNLOCO() { Code = "AUTES" };

			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();

			var item = consignment.PackingLineCollection.Single();

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var invoiceLines = itemLines.Select(line => line.FindMatchingCommercialInvoiceLine(consignment).CustomsSupportingInformationCollection).ToList();
			invoiceLines[0].Add(new CustomsSupportingInformation() { Tariff = "1111" });
			invoiceLines[1].Add(new CustomsSupportingInformation() { Tariff = "2222" });
			invoiceLines[2].Add(new CustomsSupportingInformation() { Tariff = "3333" });

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);
			AssertEquals("Should have an export bill", true, oceanBill.IsExport);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var harmonisedTariffNums = houseBill.PackingLines.Select(line => line.CV_HarmonisedTariffNums);

			AssertContainsExactElementsInAnyOrder("When Export, Harmonised Tariff should be set from Invoice Line Customs Tariff", new[] { "1111", "2222", "3333" }, harmonisedTariffNums);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateHarmonisedTariffNums_WhenNoItemLines_DoesNotSetTariffNums()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);

			item.SetPackedItemCollection(() => new List<PackedItem>());
			AssertEquals("Precondition: XML should have no item lines", 0, item.PackedItemCollection.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("When no Item Lines, should still create a single Packing Line", 1, houseBill.PackingLines.Count);

			var harmonisedTariffNum = houseBill.PackingLines.Single().CV_HarmonisedTariffNums;
			AssertEquals("Harmonised Tariff should be empty when no Item Lines", string.Empty, harmonisedTariffNum);
		}

		#endregion

		#region TestPopulateGoodsValue

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsValue_UsesInvoiceLineCustomsValue()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var invoiceLines = itemLines.Select(line => line.FindMatchingCommercialInvoiceLine(consignment)).ToList();
			invoiceLines[0].CustomsValue = 11.11;
			invoiceLines[1].CustomsValue = 22.22;
			invoiceLines[2].CustomsValue = 33.33;

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var goodsValue = houseBill.PackingLines.Select(line => (double)line.CV_GoodsValue);

			AssertContainsExactElementsInAnyOrder("Goods Value should be set from Invoce Line Customs Value", new[] { 11.11, 22.22, 33.33 }, goodsValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsValue_WhenNoItemLines_UsesConsignmentGoodsValue()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();

			var consignment = shipment.SubShipmentCollection.Single();
			consignment.GoodsValue = 44.44;

			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			item.SetPackedItemCollection(() => new List<PackedItem>());
			AssertEquals("Precondition: XML should have no item lines", 0, item.PackedItemCollection.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("When no Item Lines, should still create a single Packing Line", 1, houseBill.PackingLines.Count);

			var goodsValue = (double)houseBill.PackingLines.Single().CV_GoodsValue;
			AssertEquals("Goods Value should be set from Consignment when no Item Lines", 44.44, goodsValue);
		}

		#endregion

		#region TestPopulateGoodsCurrency

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsCurrency_UsesConsignmentGoodsValueCurrency()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();

			var consignment = shipment.SubShipmentCollection.Single();
			consignment.GoodsValueCurrency = new Currency() { Code = "TCC" };

			var item = consignment.PackingLineCollection.Single();
			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var currencies = houseBill.PackingLines.Select(line => line.CV_RX_NKGoodsCurrency);

			AssertContainsExactElementsInAnyOrder("Goods Currency should be set from Consignment Goods Value Currency", new[] { "TCC", "TCC", "TCC" }, currencies);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsCurrency_WhenNoItemLines_UsesConsignmentGoodsValueCurrency()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();

			var consignment = shipment.SubShipmentCollection.Single();
			consignment.GoodsValueCurrency = new Currency() { Code = "TCC" };

			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			item.SetPackedItemCollection(() => new List<PackedItem>());
			AssertEquals("Precondition: XML should have no item lines", 0, item.PackedItemCollection.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("When no Item Lines, should still create a single Packing Line", 1, houseBill.PackingLines.Count);

			var goodsCurrency = houseBill.PackingLines.Single().CV_RX_NKGoodsCurrency;
			AssertEquals("Goods Currency should still be set from Consignment when no Item Lines", "TCC", goodsCurrency);
		}

		#endregion

		#region TestPopulatePackageType

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertCustomsPackageType()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();

			var item = consignment.PackingLineCollection.Single();
			item.PackType = new PackageType() { Code = "BOX" };

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var packageTypes = houseBill.PackingLines.Select(line => line.CV_PackageType);

			AssertContainsExactElementsInAnyOrder("Package Type should be set from Item Pack Type", new[] { "BX", "BX", "BX" }, packageTypes);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulatePackageType_UsesItemPackType()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();

			var item = consignment.PackingLineCollection.Single();
			item.PackType = new PackageType() { Code = "TPK" };

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var packageTypes = houseBill.PackingLines.Select(line => line.CV_PackageType);

			AssertContainsExactElementsInAnyOrder("Package Type should be set from Item Pack Type", new[] { "TPK", "TPK", "TPK" }, packageTypes);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulatePackageType_WhenNoItemLines_UsesItemPackType()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();

			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			item.PackType = new PackageType() { Code = "TPK" };

			item.SetPackedItemCollection(() => new List<PackedItem>());
			AssertEquals("Precondition: XML should have no item lines", 0, item.PackedItemCollection.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("When no Item Lines, should still create a single Packing Line", 1, houseBill.PackingLines.Count);

			var packageType = houseBill.PackingLines.Single().CV_PackageType;
			AssertEquals("Package Type should still be set from Item Pack Type when no Item Lines", "TPK", packageType);
		}

		#endregion

		#region TestPopulateWeight

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateWeight_UsesInvoiceLineGrossWeight()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var invoiceLines = itemLines.Select(line => line.FindMatchingCommercialInvoiceLine(consignment)).ToList();
			invoiceLines[0].Weight = 10.0;
			invoiceLines[1].Weight = 20.0;
			invoiceLines[2].Weight = 30.0;

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var weights = houseBill.PackingLines.Select(line => (double)line.CV_Weight);

			AssertContainsExactElementsInAnyOrder("Weights should be set from Invoice Line Weight", new[] { 10.0, 20.0, 30.0 }, weights);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateWeight_WhenNoInvoiceLineGrossWeight_FallsBackToInvoiceLineNetWeight()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var invoiceLines = itemLines.Select(line => line.FindMatchingCommercialInvoiceLine(consignment)).ToList();
			invoiceLines[0].Weight = 0.0;
			invoiceLines[1].Weight = 0.0;
			invoiceLines[2].Weight = 0.0;

			invoiceLines[0].NetWeight = 5.0;
			invoiceLines[1].NetWeight = 15.0;
			invoiceLines[2].NetWeight = 25.0;

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var weights = houseBill.PackingLines.Select(line => (double)line.CV_Weight);

			AssertContainsExactElementsInAnyOrder("Weights should fallback to Invoice Line Net Weight", new[] { 5.0, 15.0, 25.0 }, weights);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateWeight_WhenNoItemLines_UsesItemManifestedWeight()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();

			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			item.ManifestedWeight = 45.54;

			var itemLines = item.SetPackedItemCollection(() => new List<PackedItem>());
			AssertEquals("Precondition: XML should have no item lines", 0, item.PackedItemCollection.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("When no Item Lines, should still create a single Packing Line", 1, houseBill.PackingLines.Count);

			var weight = (double)houseBill.PackingLines.Single().CV_Weight;
			AssertEquals("Weight should still set from Item Manifested Weight when no Item Lines", 45.54, weight);
		}

		#endregion

		#region TestPopulatePackageCount

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulatePackageCount_UsesInvoiceLineCustomsQuantity()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var invoiceLines = itemLines.Select(line => line.FindMatchingCommercialInvoiceLine(consignment)).ToList();
			invoiceLines[0].CustomsQuantity = 3;
			invoiceLines[1].CustomsQuantity = 6;
			invoiceLines[2].CustomsQuantity = 9;

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var packageCounts = houseBill.PackingLines.Select(line => (int)line.CV_PackageCount);

			AssertContainsExactElementsInAnyOrder("Package Counts should be set from Invoice Line Customs Quantity", new[] { 3, 6, 9 }, packageCounts);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulatePackageCount_WhenNoInvoiceLineCustomQuantity_FallsBackTo1()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var invoiceLines = itemLines.Select(line => line.FindMatchingCommercialInvoiceLine(consignment)).ToList();
			invoiceLines[0].CustomsQuantity = 0;
			invoiceLines[1].CustomsQuantity = 0;
			invoiceLines[2].CustomsQuantity = 0;

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var packageCounts = houseBill.PackingLines.Select(line => (int)line.CV_PackageCount);

			AssertContainsExactElementsInAnyOrder("Package count should fallback to 1", new[] { 1, 1, 1 }, packageCounts);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulatePackageCount_WhenNoItemLines_IsAlways1()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);

			item.SetPackedItemCollection(() => new List<PackedItem>());
			AssertEquals("Precondition: XML should have no item lines", 0, item.PackedItemCollection.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("When no Item Lines, should still create a single Packing Line", 1, houseBill.PackingLines.Count);

			var packageCount = (int)houseBill.PackingLines.Single().CV_PackageCount;
			AssertEquals("Package Count should always be 1 when no Item Lines", 1, packageCount);
		}

		#endregion

		#region TestPopulateContainerNumber

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateContainerNumber_UsesItemContainerNumber()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();

			var item = consignment.PackingLineCollection.Single();
			item.ContainerNumber = "TES123";

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var container = consol.ContainerCollection.First();
			container.ContainerNumber = "TES123";

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			var containerNumbers = houseBill.PackingLines.Select(line => line.CV_CN);

			var query = new ZQuery(CusSCAContainerSchema.CN_CB, oceanBill.PK);
			query.AddToFilter(CusSCAContainerSchema.CN_ContainerNumber, "TES123");
			var expectedContainerPK = Factory.BOFactory.Load<CusSCAContainer>(query).Single().PK;

			AssertContainsExactElementsInAnyOrder("Container Number should be set from Item Container Number", new[] { expectedContainerPK, expectedContainerPK, expectedContainerPK }, containerNumbers);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateContainerNumber_WhenNoItemLines_UsesItemContainerNumber()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.First();
			var consignment = shipment.SubShipmentCollection.First();

			var item = consignment.PackingLineCollection.First();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			item.ContainerNumber = "TES123";

			item.SetPackedItemCollection(() => new List<PackedItem>());
			AssertEquals("Precondition: XML should have no item lines", 0, item.PackedItemCollection.Count);

			var container = consol.ContainerCollection.First();
			container.ContainerNumber = "TES123";

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("When no Item Lines, should still create a single Packing Line", 1, houseBill.PackingLines.Count);

			var containerNumber = houseBill.PackingLines.Single().CV_CN;

			var query = new ZQuery(CusSCAContainerSchema.CN_CB, oceanBill.PK);
			query.AddToFilter(CusSCAContainerSchema.CN_ContainerNumber, "TES123");
			var expectedContainerPK = Factory.BOFactory.Load<CusSCAContainer>(query).Single().PK;

			AssertEquals("Container Number should still be set from Item Container Number when no Item Lines", expectedContainerPK, containerNumber);
		}

		#endregion

		#region Implementation

		CusSCAOceanBill ReadShipmentIntoOceanBill(Shipment shipment, IXmlImportLogger logger)
		{
			BusinessObject targetBO = null;
			var reader = MultipleTopLevelObjectReadersWrapper.CreateWrapper(shipment, isHVLVShipment: true, hls => new[] { new CusSCAOceanBillDataObjectReader(shipment, hls, logger, Factory) });
			reader.ReadIntoBusinessObject(ref targetBO);
			Factory.SaveForTesting();
			return targetBO as CusSCAOceanBill;
		}

		#endregion
	}
}
