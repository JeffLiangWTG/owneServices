using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	sealed class ETailCusSCAHouseDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestPopulatePackingLineCollection

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulatePackingLineCollection_CreatesOnePackingLinePerShipmentPackedItem()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();

			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("3 Packing Lines should be created", 3, houseBill.PackingLines.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulatePackingLineCollection_WhenShipmentHasNoPackedItems_StillCreatesOnePackingLine()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			var itemLines = item.SetPackedItemCollection(() => new List<PackedItem>());

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("1 Packing Line should be created", 1, houseBill.PackingLines.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulatePackingLineCollection_WhenNullShipmentPackedItems_StillCreatesOnePackingLine()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();
			var consignment = shipment.SubShipmentCollection.Single();
			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			var itemLines = item.SetPackedItemCollection(() => null);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("1 Packing Line should be created", 1, houseBill.PackingLines.Count);
		}

		#endregion

		#region TestPopulateGoodsValue

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsValue_UsesConsignmentGoodsValue()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();

			var consignment = shipment.SubShipmentCollection.Single();
			consignment.GoodsValue = 13.33;

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("Goods Value should be set from Consignment Goods Value", consignment.GoodsValue, houseBill.CA_GoodsValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsValue_WhenNoConsignmentGoodsValue_FallsBackToCommercialInvoiceLineCustomsValueSum()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();

			var consignment = shipment.SubShipmentCollection.Single();
			consignment.GoodsValue = 0.00;

			var item = consignment.PackingLineCollection.Single();
			var itemLines = item.PackedItemCollection;
			AssertEquals("Precondition: XML should have 3 item lines", 3, itemLines.Count);

			var invoiceLines = itemLines.Select(line => line.FindMatchingCommercialInvoiceLine(consignment)).ToList();
			invoiceLines[0].CustomsValue = 1.1;
			invoiceLines[1].CustomsValue = 3.3;
			invoiceLines[2].CustomsValue = 5.5;

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("Goods Value should fallback to sum of Invoice Line Customs Values", invoiceLines.Sum(line => line.CustomsValue), houseBill.CA_GoodsValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateGoodsValue_WhenNoConsignmentGoodsValue_AndNoItemLines_FallsBackToZero()
		{
			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));
			var shipment = consol.SubShipmentCollection.Single();

			var consignment = shipment.SubShipmentCollection.Single();
			consignment.GoodsValue = 0.00;

			var item = consignment.PackingLineCollection.Single();
			item.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			var itemLines = item.SetPackedItemCollection(() => null);

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("Goods Value should fallback to zero", 0.00, (double)houseBill.CA_GoodsValue);
		}

		#endregion

		#region TestPopulateShipmentPK

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateShipmentPK()
		{
			var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			forwardingShipment.JS_UniqueConsignRef = "S00001539";

			Factory.SaveForTesting();

			var logger = new DummyLogger();
			var consol = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("HVLVSeaShipment"));

			var oceanBill = ReadShipmentIntoOceanBill(consol, logger);
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBill = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).Single();
			AssertEquals("Should populate the shipment PK from the shipment key.", forwardingShipment.PK, houseBill.CA_JS);
		}

		#endregion

		#region Is Messaging Active

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "123456";
			Factory.SaveForTesting();

			var reader = new DummyEtailCusSCAHouseDataObjectReader(oceanBill, null, new Shipment(), new DummyLogger(), Factory);
			Assert("Messaging is not active", !reader.IsMessagingActive(houseBill));
			AssertEquals(string.Empty, reader.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(houseBill));

			houseBill.CA_MessageStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			Assert("Messaging is active", reader.IsMessagingActive(houseBill));
			AssertEquals("Messaging is active for Sea Cargo House (HBL: 123456)", reader.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(houseBill));
		}

		public void TestIsMessagingActive()
		{
			var expectedInActiveStatuses = new[]
			{
				LowValueManifestStatusList.Codes.ManifestRejected,
				LowValueManifestStatusList.Codes.NotSentToCustoms,
				LowValueManifestStatusList.Codes.ManifestInError,
				LowValueManifestStatusList.Codes.ManifestCancelled,
				string.Empty
			};

			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var reader = new ETailCusSCAHouseDataObjectReader(oceanBill, null, new Shipment(), new DummyLogger(), Factory);

			var houseBill = oceanBill.HouseBills.AddNew();
			Assert("Precondition: houseBill not in Database", !houseBill.IsInDatabase);

			var allStatuses = Factory.GetCachedValue<LowValueManifestStatusList>().GetAllCodes();

			CombineAssertions("IsMessagingActive is false when houseBill is not in database", () =>
			{
				foreach (var status in allStatuses)
				{
					houseBill.CA_MessageStatus = status;
					AssertEquals("status", false, reader.IsMessagingActive(houseBill));
				}
			});

			Factory.SaveForTesting();

			Assert("Precondition: houseBill in Database", houseBill.IsInDatabase);
			CombineAssertions("IsMessagingActive depends on CA_MessageStatus when houseBill is in database", () =>
			{
				foreach (var status in allStatuses.Except(expectedInActiveStatuses))
				{
					houseBill.CA_MessageStatus = status;
					AssertEquals(status, true, reader.IsMessagingActive(houseBill));
				}

				foreach (var status in expectedInActiveStatuses)
				{
					houseBill.CA_MessageStatus = status;
					AssertEquals(status, false, reader.IsMessagingActive(houseBill));
				}
			});

			Assert("false when input is null", !reader.IsMessagingActive(null));
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

	class DummyEtailCusSCAHouseDataObjectReader : ETailCusSCAHouseDataObjectReader
	{
		public DummyEtailCusSCAHouseDataObjectReader(IColumnIndexer oceanBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(oceanBill, hvlvConsolidatorShipmentWrapper, dataObject, logger, factory)
		{
		}

		public new ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusSCAHouse targetBO)
			=> base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
	}
}
