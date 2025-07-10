using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class CancelOrdersController : TransactionSelectionController
	{
		public CancelOrdersController(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid productOwnerPK, string ownerReference) : base(factory)
		{
			this.warehousePK = warehousePK;
			this.productOwnerPK = productOwnerPK;
			this.ownerReference = ownerReference;
		}

		readonly ZGuid warehousePK;
		readonly ZGuid productOwnerPK;
		readonly string ownerReference;

		public override bool CanProceed => !string.IsNullOrEmpty(ownerReference) && base.CanProceed;

		protected override ZQuery TransactionFilter
		{
			get
			{
				var result = new ZDBOnlyQuery(typeof(CusWHSOperatorTransaction));
				result.AddToFilter(CusWHSOperatorTransactionSchema.WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.ORD);
				result.AddToFilter(CusWHSOperatorTransactionSchema.WOT_Status, WarehouseOperatorTransactionStatusList.Codes.VAL);
				result.AddToFilter(CusWHSOperatorTransactionSchema.WOT_OwnerReference, ownerReference);
				result.AddToFilter(CusWHSOperatorTransactionSchema.WOT_OH_ProductOwner, productOwnerPK);

				var lineSubQuery = new ZDBOnlySubQuery(typeof(CusWHSOperatorTransactionLine), CusWHSOperatorTransactionLineSchema.WOL_WOT_WHSOperatorTransactionOrder, true);
				result.AddSubQuery(lineSubQuery, JoinCondition.And);

				var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.WW_OA_WarehouseAddress);
				warehouseSubQuery.AddToFilter(WhsWarehouseSchema.PK, warehousePK);

				var batchSubQuery = new ZDBOnlySubQuery(typeof(CusWHSOperatorTransactionBatch), CusWHSOperatorTransactionSchema.WOT_WOB_CusWHSTransactionBatch);
				batchSubQuery.AddSubQuery(CusWHSOperatorTransactionBatchSchema.WOB_OA_Warehouse, warehouseSubQuery, JoinCondition.And);
				result.AddSubQuery(batchSubQuery, JoinCondition.And);

				return result;
			}
		}

		protected override void ProcessSelectedRecord(CusWHSOperatorTransaction tx)
		{
			tx.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CAN;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			tx.Logs.AddNew(Events.EditedARecord, WarehouseOperatorTransactionStatusList.Codes.CAN);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}
	}
}
