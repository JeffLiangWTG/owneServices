using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IMasterStaffAssigner
	{
		IEnumerable<ILineStaffAssigner> Lines { get; }
		BusinessObjectFactory Factory { get; }
		bool CanAssignOrUnAssignAnyLines { get; }
		bool IsJobAssignable { get; }
	}
}
