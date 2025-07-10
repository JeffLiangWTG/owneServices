using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsWorkOrderLineDataObjectReaderTest : WhsPickableDocketLineDataObjectReaderTest<WhsWorkOrder, WhsWorkOrderLine, WhsWorkOrderLineDataObjectReader>
	{
		#region Implementation

		public static OrderLine SetupStandardOrderLine() => new WhsWorkOrderLineDataObjectReaderTest().SetupOrderLine();
		public static void AssertStandardOrderLineContents(WhsWorkOrderLine orderLine, bool useSerial) => new WhsWorkOrderLineDataObjectReaderTest().AssertContents(orderLine, useSerial);

		protected override bool SupportsCustomData => false;

		protected override bool SupportsCustomsDataSource => false;

		protected override string GetDocketType() => "Work Order";

		protected override string ExpectedNotAllowedToChangeRestrictedFieldsMessage => "Cannot change product on an allocated Work Order.";

		protected override WhsWorkOrder GetNewDocket(OrgHeader client, WhsWarehouse warehouse) => Helper.CreateWhsWorkOrder(client, warehouse);

		protected override WhsWorkOrderLineDataObjectReader GetNewReader(OrderLine docketLineDataObject, IXmlImportLogger logger, WhsWorkOrder order, bool useCleanFactory = true)
			=> new WhsWorkOrderLineDataObjectReaderForTestingForTesting(docketLineDataObject, logger, useCleanFactory ? new UniversalObjectFactory() : Factory, order, Enumerable.Empty<WhsWorkOrderLine>());

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseWorkOrder);

		protected override void CreateInventoryForPickableDocket(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDecimal units)
		{
			var productBOM = product.BillOfMaterials.FirstOrDefault();
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", productBOM.Component, 100m, warehouse.FindLocation("A-1"), "");
		}

		protected override bool SupportsProductCreation => false;

		protected override void SetOrderLineColumnReadOnly(WhsWorkOrderLineDataObjectReader reader, bool value)
		{
			((WhsWorkOrderLineDataObjectReaderForTestingForTesting)reader).IsColumnReadonlySetValueForTesting = value;
		}

		protected override bool IsColumnReadonlyExposed(WhsWorkOrderLineDataObjectReader reader, WhsWorkOrderLine line, string columnName)
		{
			return ((WhsWorkOrderLineDataObjectReaderForTestingForTesting)reader).IsColumnReadonlyExposed(line, columnName);
		}

		#endregion
	}

	#region WhsWorkOrderLineDataObjectReaderForTesting

	class WhsWorkOrderLineDataObjectReaderForTestingForTesting : WhsWorkOrderLineDataObjectReader
	{
		internal WhsWorkOrderLineDataObjectReaderForTestingForTesting(OrderLine orderLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, WhsWorkOrder parent, IEnumerable<WhsWorkOrderLine> matchedLines)
			: base(orderLineDataObject, logger, factory, parent, matchedLines)
		{
		}

		protected override bool IsColumnReadonly(WhsWorkOrderLine line, string columnName) => IsColumnReadonlySetValueForTesting ?? base.IsColumnReadonly(line, columnName);

		public bool IsColumnReadonlyExposed(WhsWorkOrderLine line, string columnName) => IsColumnReadonly(line, columnName);

		public bool? IsColumnReadonlySetValueForTesting { set; private get; }
	}

	#endregion
}
