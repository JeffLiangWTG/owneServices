using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsWorkOrderLineDataObjectWriterTest : WhsPickableDocketLineDataObjectWriterTest<WhsWorkOrderLine, WhsWorkOrderLineDataObjectWriter>
	{
		#region Implementation

		protected override DataContextType DataContextType => DataContextType.WarehouseWorkOrder;

		protected override WhsWorkOrderLine GetNewDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			return Helper.CreateWhsWorkOrderLine(order, data.Part1, 10m);
		}

		protected override void EnableCustomsTransactions(WhsWorkOrderLine docketLine)
		{
			docketLine.Docket.WD_IsInwardsProcessingJob = true;
		}

		protected override WhsWorkOrderLineDataObjectWriter GetNewDataObjectWriter(BusinessObject topLevelBO)
		{
			return new WhsWorkOrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO)));
		}

		protected override void AssertContentsCore(OrderLine lineData)
		{
			AssertNull("lineData.ReservedQuantity", lineData.ReservedQuantity);
			AssertNull("lineData.ShortfallQuantity", lineData.ShortfallQuantity);
		}

		protected override WhsPickableDocket GetDocket(BusinessObjectFactory factory) => Factory.NewWithValidTestData<WhsWorkOrder>();

		#endregion
	}
}
