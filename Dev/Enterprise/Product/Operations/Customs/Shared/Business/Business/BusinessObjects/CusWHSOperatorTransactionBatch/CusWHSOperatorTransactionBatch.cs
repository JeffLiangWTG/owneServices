using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusWHSOperatorTransactionBatch : AutoCusWHSOperatorTransactionBatch
	{
		public CusWHSOperatorTransactionBatch(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ChildEditable(true)]
		public CusWHSOperatorTransactionCollection WarehouseOperatorTransactions => warehouseOperatorTransactions ?? (warehouseOperatorTransactions = GetWarehouseOperatorTransactions());
		CusWHSOperatorTransactionCollection warehouseOperatorTransactions;

		CusWHSOperatorTransactionCollection GetWarehouseOperatorTransactions()
		{
			var result = new CusWHSOperatorTransactionCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		public override void Delete()
		{
			WarehouseOperatorTransactions.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override bool SupportsCloneCore() => true;
	}
}

