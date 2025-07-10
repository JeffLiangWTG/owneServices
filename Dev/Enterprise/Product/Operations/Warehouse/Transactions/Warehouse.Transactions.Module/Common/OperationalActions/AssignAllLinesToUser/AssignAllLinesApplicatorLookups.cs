using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public sealed class AssignAllLinesApplicatorLookups : ZLookups
	{
		public AssignAllLinesApplicatorLookups(BusinessObject parent)
			: base(parent)
		{ }

		public GlbStaffCollection UsersToSelect { get { return new GlbStaffCollection(Factory); } }
	}
}
