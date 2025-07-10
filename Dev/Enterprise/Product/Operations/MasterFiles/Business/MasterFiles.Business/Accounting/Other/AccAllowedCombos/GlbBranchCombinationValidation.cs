using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchCombinationValidation : ValidationProvider
	{
		public GlbBranchCombinationValidation()
		{
		}

		public static bool ShouldValidateCombination(GlbBranch branch)
		{
			return branch != null && branch.AllowedDepartments.Count > 0;
		}

		public static string CheckBranchDepartmentCombination(GlbBranch branch, GlbDepartment department)
		{
			var errorMsg = string.Empty;

			if (branch != null && department != null)
			{
				var invalidCombination = !branch.IsDepartmentAllowed(department.PK);
				if (invalidCombination)
				{
					errorMsg = Res.GetString("0a42ac4a-7671-4ecd-ba01-e798047539bd", @"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.", department.GE_Code, branch.GB_Code);
				}
			}

			return errorMsg;
		}

		public static void CheckBranchDepartmentCombination(ZPropertyInfo propertyInfo, GlbBranch branch, GlbDepartment department)
		{
			CheckBranchDepartmentCombination(propertyInfo, branch, department, CargoWise.EntityFramework.NotificationType.Error);
		}

		public static void CheckBranchDepartmentCombination(ZPropertyInfo propertyInfo, GlbBranch branch, GlbDepartment department, INotificationType notificationType)
		{
			if (propertyInfo != null)
			{
				var errorMsg = CheckBranchDepartmentCombination(branch, department);

				if (!string.IsNullOrWhiteSpace(errorMsg))
				{
					propertyInfo.AddNotification(notificationType, errorMsg);
				}
			}
		}
	}
}
