using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Recruiter.Business
{
	class HRJobApplicantViewEditRights
	{
		public HRJobApplicantViewEditRights(HRJobApplicant applicant)
		{
			this.applicant = applicant;
		}
		readonly HRJobApplicant applicant;

		#region View Rights

		public bool ViewMobilePhoneForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantViewMobilePhone); }
		}

		public bool ViewHomePhoneForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantViewHomePhone); }
		}

		public bool ViewFaxForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantViewFax); }
		}

		public bool ViewNameForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantViewName); }
		}

		public bool ViewBirthdateForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantViewBirthdate); }
		}

		public bool ViewGenderForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantViewGender); }
		}

		public bool ViewUserAddressForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantViewUserAddress); }
		}

		public bool ViewPassportForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantViewPassport); }
		}

		public bool ViewDriversLicenseNumberForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantViewDriversLicenseNumber); }
		}

		public bool ViewNationalityForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantViewNationality); }
		}

		#endregion

		#region Edit Rights

		public bool EditMobilePhoneForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantMobilePhone); }
		}

		public bool EditHomePhoneForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantHomePhone); }
		}

		public bool EditFaxForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantFax); }
		}

		public bool EditNameForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantName); }
		}

		public bool EditBirthdateForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantBirthdate); }
		}

		public bool EditGenderForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantGender); }
		}

		public bool EditUserAddressForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantUserAddress); }
		}

		public bool EditPassportForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantPassport); }
		}

		public bool EditDriversLicenseNumberForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantDriversLicenseNumber); }
		}

		public bool EditNationalityForJobApplicant
		{
			get { return HasRight(Env.Security.HRJobApplicantNationality); }
		}

		#endregion

		bool HasRight(SecurityCheckpoint checkpoint)
		{
			var result = true;
			if (Env.Security != null && applicant.IsInDatabase)
			{
				result = checkpoint.IsAllowed;
			}

			return result;
		}
	}
}
