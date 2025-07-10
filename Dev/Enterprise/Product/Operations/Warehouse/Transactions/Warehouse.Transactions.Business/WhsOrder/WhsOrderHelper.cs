using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsOrderHelper
	{
		public static CodeDescriptionPairList OrderStatuses
		{
			get
			{
				var status = new DocketStatus();
				status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Finalised));
				status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Putaway));
				status.AddRange(new WhsOrderStatus());
				return status;
			}
		}
	}
}
