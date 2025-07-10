using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsDynamicWorkOrderLineDataObjectReaderTest : WhsPickableDocketLineDataObjectReaderTest<WhsDynamicWorkOrder, WhsDynamicWorkOrderLine, WhsDynamicWorkOrderLineDataObjectReader>
	{
		protected override bool SupportsProductCreation => false;

		protected override void CreateInventoryForPickableDocket(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDecimal units)
		{
			var component = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATCOMP"));
			var receive = Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", component, units, warehouse.DefaultLocationInInwardProcessingArea, "");
			var inventory = receive.Lines.Single();
			inventory.WE_BondedEntryKey = "ABC-2";
		}

		public static OrderLine SetupStandardOrderLine(bool addComponentLines = true)
		{
			var orderLine = new WhsDynamicWorkOrderLineDataObjectReaderTest().SetupOrderLine();

			if (addComponentLines)
			{
				AddComponentLine(orderLine);
			}

			return orderLine;
		}

		protected override OrderLine SetupOrderLine()
		{
			var orderLine = base.SetupOrderLine();
			orderLine.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);

			AddComponentLine(orderLine);

			return orderLine;
		}

		static void AddComponentLine(OrderLine orderLine)
		{
			var componentOrderLine = new OrderLine();
			componentOrderLine.Product = new Product { Code = "HATCOMP", Description = "Bowler Hat Component" };
			componentOrderLine.LineNumber = new ZShort(2);
			componentOrderLine.SubLineNumber = new ZShort(4);
			componentOrderLine.OrderedQty = orderLine.OrderedQty;
			componentOrderLine.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData = new CustomsEntryInfo();
			componentCustomsData.InwardsEntryKey = "ABC";
			componentCustomsData.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine.CustomsData = componentCustomsData;
			orderLine.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine });
		}

		protected override void SetOrderedQty(OrderLine orderLineDataObject, bool isUpdate = false)
		{
			orderLineDataObject.OrderedQty = isUpdate ? 20 : 28;
		}

		protected override WhsWarehouse GetWarehouse()
		{
			var warehouse = base.GetWarehouse();
			warehouse.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			return warehouse;
		}

		public static void AssertStandardOrderLineContents(WhsDynamicWorkOrderLine orderLine, bool useSerial)
		{
			new WhsDynamicWorkOrderLineDataObjectReaderTest().AssertContents(orderLine, useSerial);

			var componentLineBO = (WhsDynamicWorkOrderLine)orderLine.ChildComponentLines.Single();
			AssertEquals("componentLineBO.ProductCode", "HATCOMP", componentLineBO.ProductCode);
			AssertEquals("componentLineBO.ProductDesc", "Bowler Hat Component", componentLineBO.ProductDesc);
			AssertEquals("componentLineBO.WE_SubLineNo", new ZShort(2), componentLineBO.WE_LineNo);
			AssertEquals("componentLineBO.WE_SubLineNo", new ZShort(4), componentLineBO.WE_SubLineNo);
			AssertEquals("componentLineBO.WE_TransactionQuantity", 28m, componentLineBO.WE_TransactionQuantity);
			AssertEquals("componentLineBO.ProductUQ", "UNT", componentLineBO.ProductUQ);
			AssertEquals("componentLineBO.WE_BondedEntryKey", "ABC-2", componentLineBO.WE_BondedEntryKey);
		}

		protected override void AssertOrderedQuantity(WhsDynamicWorkOrderLine orderLine)
		{
			AssertEquals("orderLineBO.WE_PackQuantity", 28m, orderLine.WE_PackQuantity);
			AssertEquals("orderLineBO.WE_TransactionQuantity", 28m, orderLine.WE_TransactionQuantity);
		}

		protected override void AssertCustomsData(WhsDynamicWorkOrderLine orderLine, WhsBondedWarehouseAttribute customsDataBO)
		{
			AssertEquals("orderLineBO.WE_BondedEntryKey", "", orderLine.WE_BondedEntryKey);
			AssertEquals("customsDataBO.WB_EntryKey", "", customsDataBO.WB_EntryKey);
			AssertEquals("customsDataBO.WB_EntryLineNo", ZShort.Zero, customsDataBO.WB_EntryLineNo);
			AssertEquals("customsDataBO.OutwardType", "", customsDataBO.WB_OutwardType);
			AssertEquals("customsDataBO.WB_AddInfo", "", customsDataBO.WB_AddInfo);
			AssertEquals("customsDataBO.WB_CustomsQty", 0m, customsDataBO.WB_CustomsQty);
			AssertEquals("customsDataBO.WB_CustomsUnitOfQty", "", customsDataBO.WB_CustomsUnitOfQty);
			AssertEquals("customsDataBO.WB_DeclarationReference", "", customsDataBO.WB_DeclarationReference);
			AssertEquals("customsDataBO.WB_EntryDate", ZDateTime.Empty, customsDataBO.WB_EntryDate);
			AssertEquals("customsDataBO.WB_RN_NKCountryOfOrigin", "", customsDataBO.WB_RN_NKCountryOfOrigin);
			AssertEquals("customsDataBO.WB_TILV", 0m, customsDataBO.WB_TILV);
			AssertEquals("customsDataBO.WB_ValueForDuty", 0m, customsDataBO.WB_ValueForDuty);
			AssertEquals("customsDataBO.WB_CustomsSecondQuantity", 0m, customsDataBO.WB_CustomsSecondQuantity);
			AssertEquals("customsDataBO.WB_CustomsSecondUnitQty", "", customsDataBO.WB_CustomsSecondUnitQty);
			AssertEquals("customsDataBO.WB_Tariff", "", customsDataBO.WB_Tariff);
			AssertEquals("customsDataBO.WB_PrimaryPreference", "", customsDataBO.WB_PrimaryPreference);
			AssertEquals("customsDataBO.WB_CustomsThirdQuantity", 0m, customsDataBO.WB_CustomsThirdQuantity);
			AssertEquals("customsDataBO.WB_CustomsThirdUnitQty", "", customsDataBO.WB_CustomsThirdUnitQty);
			AssertEquals("customsDataBO.WB_ZoneStatus", "", customsDataBO.WB_ZoneStatus);
			AssertEquals("customsDataBO.WB_IsFromAnotherFTZWhs", false, customsDataBO.WB_IsFromAnotherFTZWhs);
			AssertEquals("customsDataBO.WB_IsMainInwardsProcessedItem", true, customsDataBO.WB_IsMainInwardsProcessedItem);
		}

		protected override string ExpectedNotAllowedToChangeRestrictedFieldsMessage => "Cannot change product on an allocated Dynamic Work Order.";

		protected override bool SupportsCustomsDataSource => false;

		protected override bool SupportsCustomFieldsImport => false;

		protected override bool SupportsUnitPriceFields => false;

		protected override string GetNoMatchingDocketLineInfoMessage() => base.GetNoMatchingDocketLineInfoMessage() + GetPopulatingComponentLineInfoMessage();

		protected override string GetMatchingDocketLineInfoMessage() => base.GetMatchingDocketLineInfoMessage() + GetPopulatingComponentLineInfoMessage();

		string GetPopulatingComponentLineInfoMessage() => @"
Information - No matching Component WhsDynamicWorkOrderLine found, creating new Component WhsDynamicWorkOrderLine.
Information - Populating Component WhsDynamicWorkOrderLine...";

		protected override string GetDocketType()
			=> "Dynamic Work Order";

		protected override WhsDynamicWorkOrder GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
			=> Helper.CreateWhsDynamicWorkOrder(client, warehouse);

		protected override WhsDynamicWorkOrderLineDataObjectReader GetNewReader(OrderLine docketLineDataObject, IXmlImportLogger logger, WhsDynamicWorkOrder whsDocket, bool useCleanFactory = true)
			=> new WhsDynamicWorkOrderLineDataObjectReaderForTesting(docketLineDataObject, logger, useCleanFactory ? new UniversalObjectFactory() : Factory, whsDocket, Enumerable.Empty<WhsDynamicWorkOrderLine>());

		protected override TestDataForUniversal GetNewTestData()
			=> new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseWorkOrder);

		protected override bool IsColumnReadonlyExposed(WhsDynamicWorkOrderLineDataObjectReader reader, WhsDynamicWorkOrderLine line, string columnName)
			=> ((WhsDynamicWorkOrderLineDataObjectReaderForTesting)reader).IsColumnReadonlyExposed(line, columnName);

		protected override void SetOrderLineColumnReadOnly(WhsDynamicWorkOrderLineDataObjectReader reader, bool value)
			=> ((WhsDynamicWorkOrderLineDataObjectReaderForTesting)reader).IsColumnReadonlySetValueForTesting = value;

		protected override decimal GetExpectedQuantityForReadOnlyForPickedLineTest() => 28m;
	}

	#region WhsDynamicWorkOrderLineDataObjectReaderForTesting

	class WhsDynamicWorkOrderLineDataObjectReaderForTesting : WhsDynamicWorkOrderLineDataObjectReader
	{
		internal WhsDynamicWorkOrderLineDataObjectReaderForTesting(OrderLine orderLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, WhsDynamicWorkOrder parent, IEnumerable<WhsDynamicWorkOrderLine> matchedLines)
			: base(orderLineDataObject, logger, factory, parent, matchedLines)
		{
		}

		protected override bool IsColumnReadonly(WhsDynamicWorkOrderLine line, string columnName) => IsColumnReadonlySetValueForTesting ?? base.IsColumnReadonly(line, columnName);

		public bool IsColumnReadonlyExposed(WhsDynamicWorkOrderLine line, string columnName) => IsColumnReadonly(line, columnName);

		public bool? IsColumnReadonlySetValueForTesting { set; private get; }
	}

	#endregion
}
