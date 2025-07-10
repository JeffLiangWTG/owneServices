using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class DocketIDFountainUniqueIndexFailureHandlerWorkOrder : DocketIDFountainUniqueIndexFailureHandler<WhsWorkOrder>
	{
		protected override string GetDocketType() => DocketType.Codes.WorkOrder;
		protected override string GetDocketSubType() => WorkOrderType.Codes.Assemble;
	}
}
