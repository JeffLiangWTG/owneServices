using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class DocketIDFountainUniqueIndexFailureHandlerTransfer : DocketIDFountainUniqueIndexFailureHandler<WhsTransfer>
	{
		protected override string GetDocketType() => DocketType.Codes.Transfer;
		protected override string GetDocketSubType() => TransferType.Codes.Internal;
	}
}
