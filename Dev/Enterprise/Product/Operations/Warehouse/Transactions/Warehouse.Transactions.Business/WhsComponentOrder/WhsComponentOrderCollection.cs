using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsComponentOrderCollection : WhsPickableDocketCollection
	{
		protected WhsComponentOrderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected abstract override IEnumerable<string> DocketTypes { get; }
	}
}
