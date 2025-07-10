using System.Collections.Generic;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPickForm
	{
		WhsOrder CurrentOrder { get; }
		List<WhsPickableDocketLine> SelectedOrderLines { get; }
	}
}
