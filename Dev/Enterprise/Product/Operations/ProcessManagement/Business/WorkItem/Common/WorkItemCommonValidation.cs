using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemCommonValidation : WorkItemValidation
	{
		public WorkItemCommonValidation(AutoWorkItem parent)
			: base(parent)
		{
		}

		new WorkItemCommon Parent
		{
			get { return (WorkItemCommon)base.Parent; }
		}

		protected override void CheckWKI_GE_AssignedDepartment()
		{
			if (!Parent.IsInDatabase || Parent.WKI_GE_AssignedDepartmentInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidPK(Parent.WKI_GE_AssignedDepartmentInfo, Parent.Lookups.AssignedDepartments);
			}
		}

		protected override void CheckWKI_GC_AssignedCompany()
		{
			if (!Parent.IsInDatabase || Parent.WKI_GC_AssignedCompanyInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidPK(Parent.WKI_GC_AssignedCompanyInfo, Parent.Lookups.AssignedCompanies);
			}
		}

		protected override void CheckWKI_PortOrCountry()
		{
			if (!Parent.IsInDatabase || Parent.WKI_PortOrCountryInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(Parent.WKI_PortOrCountryInfo);
			}
		}

		#region WKI_SystemCreateUser

		public void ValidateWKI_SystemCreateUser()
		{
			((IValidationInternals)this).Validate(Parent.WKI_SystemCreateUserInfo, GetWKI_SystemCreateUserValidationInvoker());
		}

		RunValidationInvoker GetWKI_SystemCreateUserValidationInvoker()
		{
			return delegate
			{
				CheckWKI_SystemCreateUserIsWesternEuropean();
				CheckWKI_SystemCreateUser();
			};
		}

		protected virtual void CheckWKI_SystemCreateUserIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.WKI_SystemCreateUserInfo);
		}

		protected virtual void CheckWKI_SystemCreateUser()
		{
			// sub-classes should override to implement custom validation
			if (!Parent.IsInDatabase || Parent.WKI_SystemCreateUserInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(Parent.WKI_SystemCreateUserInfo);
			}
			if (Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.WKI_SystemCreateUserInfo);
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateWKI_SystemCreateUser();
		}
	}
}
