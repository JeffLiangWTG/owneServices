//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccAllowedBranchDepartmentComboValidation
//
//    This class should be used for overriding validation in AutoAccAllowedBranchDepartmentComboValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;

	public class AccAllowedBranchDepartmentComboValidation : AutoAccAllowedBranchDepartmentComboValidation
	{
		public AccAllowedBranchDepartmentComboValidation(AutoAccAllowedBranchDepartmentCombo parent) : base(parent)
		{
		}

		protected override void CheckAAB_GE_Department()
		{
			base.CheckAAB_GE_Department();

			ListValidation.ErrorIfInvalidPK(Parent.AAB_GE_DepartmentInfo);

			if (Parent.Branch != null)
			{
				foreach (AccAllowedBranchDepartmentCombo combo in Parent.Branch.AllowedDepartments)
				{
					if (combo.PK != Parent.PK && combo.AAB_GE_Department == Parent.AAB_GE_Department)
					{
						Parent.AAB_GE_DepartmentInfo.AddError(Res.GetString("2f19ddd7-b444-43c8-87d1-0ae4ea215929", "This department is already added. Please enter another department."));
						break;
					}
				}
			}
		}
	}
}
