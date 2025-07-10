using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ICrossDockManager
	{
		void AllocateCrossDockedLines(BusinessObjectFactory factory, IEnumerable<WhsReceiveLine> inventories);
	}
}
