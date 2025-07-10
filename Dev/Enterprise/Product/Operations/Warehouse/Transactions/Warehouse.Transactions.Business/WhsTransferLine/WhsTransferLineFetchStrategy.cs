using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsTransferLineFetchStrategy : WhsDocketLineFetchStrategy
	{
		public WhsTransferLineFetchStrategy(WhsTransferLine docketLine)
			: base(docketLine)
		{
		}

		#region FetchForLoadCore

		protected override void FetchForLoadCore(WhsDocketLine line)
		{
			base.FetchForLoadCore(line);

			var transferLine = (WhsTransferLine)line;
			var transfer = transferLine.Docket;

			if (transfer.WD_DocketSubType != TransferType.Codes.Internal)
			{
				if (transfer.WD_DocketSubType == TransferType.Codes.InterWhsSource)
				{
					Factory.AddFetchHint(WhsDocketLineSchema.WE_WE_MatchingLine, transferLine.PK);

					if (transferLine.WE_WL.IsEmpty && transferLine.IsChildTransferLine)
					{
						Factory.AddFetchHint(WhsDocketLineSchema.Constants.TableName, transferLine.WE_WE_ParentDocketLine);
					}
				}
				else // transfer.WD_DocketSubType == TransferType.Codes.InterWhsDestination
				{
					Factory.AddFetchHint(WhsLocationViewSchema.Constants.TableName, transferLine.WE_WL_TransferFrom);
				}
			}
		}

		#endregion
	}
}
