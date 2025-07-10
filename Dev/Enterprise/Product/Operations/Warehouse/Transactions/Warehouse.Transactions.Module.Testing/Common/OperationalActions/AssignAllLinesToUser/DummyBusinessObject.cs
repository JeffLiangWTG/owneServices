using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class DummyBusinessObject : NonPersistentBusinessObject, IMasterStaffAssigner
	{
		public DummyBusinessObject() { }

		#region IMasterAssigner

		IEnumerable<ILineStaffAssigner> IMasterStaffAssigner.Lines => throw new System.NotImplementedException();

		bool IMasterStaffAssigner.CanAssignOrUnAssignAnyLines => throw new System.NotImplementedException();

		bool IMasterStaffAssigner.IsJobAssignable => throw new System.NotImplementedException();

		public bool CanUnAssignAnyLines => throw new System.NotImplementedException();

		#endregion
	}
}
