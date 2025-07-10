using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business
{
	class ViewingRights
	{
		public ViewingRights(GlbStaff staff)
		{
			this.staff = staff;
		}
		readonly GlbStaff staff;

		public bool ViewBirthDateAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewBirthDate); }
		}

		public bool ViewBankDetailsAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewBankingDetails); }
		}

		public bool ViewHomeAddressAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewHomeAddressDetails); }
		}

		public bool ViewEmergencyContactAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewEmergencyContact); }
		}

		public bool ViewOtherReferencesAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherReferences); }
		}

		public bool ViewGenderDetailsAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherGender); }
		}

		public bool ViewPassportAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherPassport); }
		}

		public bool ViewNationalityAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherNationality); }
		}

		public bool ViewWorkExtensionAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherWorkExtension); }
		}

		public bool ViewTitleAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherTitle); }
		}

		public bool ViewMobilePhoneAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherMobilePhone); }
		}

		public bool ViewBrokerInfoAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherBrokerInfo); }
		}

		public bool ViewPersonalEDIMailBoxAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherPersonalEDIMailBox); }
		}

		public bool ViewResidencyStatusAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherResidencyStatus); }
		}

		public bool ViewSecurityCardNumberAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherSecurityCardNumber); }
		}

		public bool ViewCertificatesAllowedForStaff
		{
			get { return HasViewRight(Env.Security.StaffViewOtherCertificates); }
		}

		bool HasViewRight(SecurityCheckpoint checkpoint)
		{
			var result = true;
			if (Env.Security != null && (staff.IsInDatabase || staff.IsFromUniversalCopy))
			{
				var currentUser = GlbStaff.CurrentUser;
				if (currentUser != null)
				{
					result = currentUser.PK == staff.PK ||
									 checkpoint.IsAllowed ||
									 staff.IsCurrentUserLocalAdminForThisStaff;
				}
			}
			return result;
		}
	}
}
