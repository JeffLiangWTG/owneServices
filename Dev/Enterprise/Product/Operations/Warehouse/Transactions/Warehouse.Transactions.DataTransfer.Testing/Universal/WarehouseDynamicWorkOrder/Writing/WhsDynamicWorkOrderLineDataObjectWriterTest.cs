using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsDynamicWorkOrderLineDataObjectWriterTest : WhsDocketLineDataObjectWriterTest<WhsDynamicWorkOrderLine, WhsDynamicWorkOrderLineDataObjectWriter>
	{
		protected override WhsDynamicWorkOrderLineDataObjectWriter GetNewDataObjectWriter(BusinessObject topLevelBO)
			=> new WhsDynamicWorkOrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WDO, topLevelBO)));

		protected override WhsDynamicWorkOrderLine GetNewDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			return Helper.CreateWhsDynamicWorkOrderLine(order, data.Part1, 10m);
		}

		protected override TestDataForUniversal GetNewTestData()
			=> new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseDynamicWorkOrder);

		protected override bool IsCustomFieldsSupported => false;

		protected override bool IsCustomsTransactionByDefault => true;

		protected override void EnableCustomsTransactions(WhsDynamicWorkOrderLine docketLine)
		{
			docketLine.Docket.WD_IsInwardsProcessingJob = true;
		}

		protected override void AssertCustomsData(CustomsEntryInfo customsData)
		{
			WhsDynamicWorkOrderBondedWarehouseAttributeDataObjectWriterTest.AssertContents(customsData, isComponentLine: false);
		}
	}
}
