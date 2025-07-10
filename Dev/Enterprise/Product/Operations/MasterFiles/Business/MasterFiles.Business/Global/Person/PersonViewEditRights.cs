using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business
{
	class PersonViewEditRights
	{
		public PersonViewEditRights(GlbPerson person)
		{
			this.person = person;
		}
		readonly GlbPerson person;

		#region View Rights

		public bool ViewBirthDateForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewDateOfBirth); }
		}

		public bool ViewLegalNameForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewLegalName); }
		}

		public bool ViewEmailForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewEmail); }
		}

		public bool ViewGenderForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewGender); }
		}

		public bool ViewMobileForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewMobile); }
		}

		public bool ViewHomePhoneForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewHomePhone); }
		}

		public bool ViewHomeAddressForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewHomeAddress); }
		}

		public bool ViewNationalityForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewNationality); }
		}

		public bool ViewPersonalInfoForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewPersonalInformation); }
		}

		public bool ViewImageForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewImage); }
		}

		public bool ViewPassportForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewPassport); }
		}

		public bool ViewDriversLicenseNumberForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceViewDriversLicenseNumber); }
		}

		public bool ViewPrimaryWorkplace => HasRight(Env.Security.PersonIntelligenceViewPrimaryWorkplace);

		public bool ViewChallengePhraseForPerson => HasRight(Env.Security.PersonIntelligenceViewChallengePhrase);

		public bool ViewFaxNumberForPerson => HasRight(Env.Security.PersonIntelligenceViewFaxNumber);

		#endregion

		#region Edit Rights

		public bool EditBirthDateForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceDateOfBirth); }
		}

		public bool EditLegalNameForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceLegalName); }
		}

		public bool EditEmailForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceEmail); }
		}

		public bool EditGenderForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceGender); }
		}

		public bool EditMobileForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceMobile); }
		}

		public bool EditHomePhoneForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceHomePhone); }
		}

		public bool EditHomeAddressForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceHomeAddress); }
		}

		public bool EditNationalityForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceNationality); }
		}

		public bool EditPersonalInfoForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligencePersonalInformation); }
		}

		public bool EditImageForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceImage); }
		}

		public bool EditPassportForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligencePassport); }
		}

		public bool EditDriversLicenseNumberForPerson
		{
			get { return HasRight(Env.Security.PersonIntelligenceDriversLicenseNumber); }
		}

		#endregion

		bool HasRight(SecurityCheckpoint checkpoint)
		{
			var result = true;
			if (Env.Security != null && person.IsInDatabase)
			{
				result = checkpoint.IsAllowed;
			}

			return result;
		}
	}
}
