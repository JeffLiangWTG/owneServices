using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class LoginLocationBusinessObjectValidation : AutoLoginLocationBusinessObjectValidation
	{
		public LoginLocationBusinessObjectValidation(AutoLoginLocationBusinessObject parent)
			: base(parent)
		{ }

		protected override void CheckCompanyCode()
		{
			base.CheckCompanyCode();
			ListValidation.ErrorIfInvalidCode(Parent.CompanyCodeInfo);
			ValidateDepartmentCode();
		}

		protected override void CheckBranchCode()
		{
			base.CheckBranchCode();
			ListValidation.ErrorIfInvalidCode(Parent.BranchCodeInfo);
			ValidateDepartmentCode();
		}

		protected override void CheckDepartmentCode()
		{
			base.CheckDepartmentCode();
			ListValidation.ErrorIfInvalidCode(Parent.DepartmentCodeInfo);
		}
	}
}
