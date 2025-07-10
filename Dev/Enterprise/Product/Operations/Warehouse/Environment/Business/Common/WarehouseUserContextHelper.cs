using System;
using System.Linq;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Environment.Business
{
	public static class WarehouseUserContextHelper
	{
		public static IDisposable SetUserContextForWarehouse(WhsWarehouse warehouse)
		{
			var branch = warehouse.RelatedCompanyBranch;
			var departmentPK = GetDepartmentForBranch(branch);
			return Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), departmentPK);
		}

		static Guid GetDepartmentForBranch(GlbBranch branch)
		{
			var result = Env.CurrentDepartment.PK;
			if (branch != null && !branch.IsDepartmentAllowed(result))
			{
				result = branch.AllowedDepartments.OrderBy(x => x.DepartmentCode).Select(x => x.AAB_GE_Department).FirstOrDefault().ToGuid();
			}
			return result;
		}
	}
}
