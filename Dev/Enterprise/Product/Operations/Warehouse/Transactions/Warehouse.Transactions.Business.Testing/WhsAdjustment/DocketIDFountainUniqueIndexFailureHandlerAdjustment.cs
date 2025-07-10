using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class DocketIDFountainUniqueIndexFailureHandlerAdjustment : DocketIDFountainUniqueIndexFailureHandler<WhsAdjustment>
	{
		#region GetDocketTypeCore

		protected override string GetDocketType() => DocketType.Codes.Adjustment;
		protected override string GetDocketSubType() => AdjustmentType.Codes.Adjustment;

		#endregion
	}
}
