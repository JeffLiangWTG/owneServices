using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsComponentOrderValidationTest<T> : WhsPickableDocketValidationTest<T>
		where T : WhsComponentOrder
	{
		#region TestFinaliseWorkOrderAlwaysFinalisingPick

		public void TestFinaliseWorkOrderAlwaysFinalisingPick()
		{
			var pick = Factory.New<WhsPick>();
			var workOrder = GetNewBusinessObject();
			workOrder.WD_WP = pick.PK;
			workOrder.Validation.Validate_FinalizePickWhenWorkOrderIsFinalized();
			AssertEquals(0, workOrder.RowErrors.Count());

			workOrder.WD_FinalisedDate = ZDateTimeOffset.Now;
			workOrder.WD_DocketStatus = DocketStatus.Codes.Finalised;
			workOrder.Validation.Validate_FinalizePickWhenWorkOrderIsFinalized();
			AssertHasRowError(workOrder, @"Job was finalized but Pick was not finalized.");

			pick.AddRowError("Oops!");
			workOrder.Validation.Validate_FinalizePickWhenWorkOrderIsFinalized();
			AssertHasRowError(workOrder, @"Job was finalized but Pick was not finalized.
Error - Pick: Oops!");
		}

		#endregion
	}
}
