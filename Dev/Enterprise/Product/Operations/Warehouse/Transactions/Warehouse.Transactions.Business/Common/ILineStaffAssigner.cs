using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ILineStaffAssigner
	{
		void AssignLine(GlbStaff staff);
		bool CanAssignOrUnAssignLine();
		void UnAssignLine(GlbStaff staff);
	}
}
