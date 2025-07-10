using System.Collections.Generic;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPickLineUpdatesRefreshable
	{
		void Refresh(IEnumerable<WhsPickLine> pickLines);
	}
}
