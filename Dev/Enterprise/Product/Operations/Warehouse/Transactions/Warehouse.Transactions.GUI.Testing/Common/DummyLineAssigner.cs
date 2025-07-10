using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class DummyLineAssigner : ILineStaffAssigner
	{
		public GlbStaff AssignedStaff { get; set; }
		public ZBool IsLineAssignable { get; set; }

		void ILineStaffAssigner.AssignLine(GlbStaff staff) => AssignedStaff = staff;

		bool ILineStaffAssigner.CanAssignOrUnAssignLine() => IsLineAssignable;

		void ILineStaffAssigner.UnAssignLine(GlbStaff staff) => throw new NotImplementedException();
	}
}
