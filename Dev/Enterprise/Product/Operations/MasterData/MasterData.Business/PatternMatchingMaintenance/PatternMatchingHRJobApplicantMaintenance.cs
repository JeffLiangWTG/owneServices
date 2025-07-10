using System;
using System.Globalization;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingHRJobApplicantMaintenance : PatternMatchingSource<HRJobApplicant>
	{
		public PatternMatchingHRJobApplicantMaintenance(HRJobApplicant bizO) : base(bizO)
		{
		}

		protected override bool CreateOrUpdateName()
		{
			var name = bizO.HA_FullName;
			var nameToHash = name.Trim().Contains(' ') ? TextStandardizerHelper.StandardizePersonName(name) : string.Empty;
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingName>(bizO, false, GetHash(nameToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingNameSchema.PMN_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateAddress()
		{
			if ((!TextStandardizerHelper.IsPlaceholderAddress(bizO.HA_UserAddress1) ||
				string.IsNullOrEmpty(bizO.HA_UserAddress1) && !TextStandardizerHelper.IsPlaceholderAddress(bizO.HA_UserAddress2)))
			{
				var addressLine = (bizO.HA_UserAddress1 + bizO.HA_UserAddress2 + bizO.HA_City + bizO.HA_Postcode + bizO.HA_State).ToUpperInvariant();
				var addressLineMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingAddress>(bizO, false, GetHash(addressLine), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);
				return addressLineMaintenance.CreateOrUpdatePatternRecord(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK);
			}

			return base.CreateOrUpdateAddress();
		}

		protected override bool CreateOrUpdateDomain()
		{
			var emailAddress = bizO.HA_EmailAddress;
			var domainToHash = TextStandardizerHelper.StandardizeEmailDomain(emailAddress);
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingDomain>(bizO, false, GetHash(domainToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateEmail()
		{
			var emailAddress = bizO.HA_EmailAddress;
			var emailToHash = TextStandardizerHelper.StandardizeEmail(emailAddress);
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingEmail>(bizO, false, GetHash(emailToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingEmailSchema.PME_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateRegCode()
		{
			var birthday = bizO.HA_Birthdate;
			var birthdayToHash = birthday.IsEmpty ? string.Empty : (birthday.ToDateTime() - new DateTime(1753, 1, 1)).Days.ToString(CultureInfo.InvariantCulture);
			var birthdayMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, false, GetHash(birthdayToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);

			var passport = bizO.HA_Passport;
			var passportToHash = passport.IsEmpty ? string.Empty : CodeTypeIdentifier.Passport + TextStandardizerHelper.StandardizeRegCode(passport);
			var passportMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, !birthday.IsEmpty, GetHash(passportToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);

			var license = bizO.HA_DriversLicenseNumber;
			var licenseToHash = license.IsEmpty ? string.Empty : CodeTypeIdentifier.License + TextStandardizerHelper.StandardizeRegCode(license);
			var licenseMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, !(birthday.IsEmpty && passport.IsEmpty), GetHash(licenseToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);

			var other = bizO.HA_OtherIdentityDocument;
			var otherToHash = other.IsEmpty ? string.Empty : CodeTypeIdentifier.Other + TextStandardizerHelper.StandardizeRegCode(other);
			var otherMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, !(birthday.IsEmpty && passport.IsEmpty && license.IsEmpty), GetHash(otherToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);

			return birthdayMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK) |
				passportMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK) |
				licenseMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK) |
				otherMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdatePhone()
		{
			var homePhone = bizO.HA_HomePhone;
			var fax = bizO.HA_FaxNum;
			var mobile = bizO.HA_MobilePhone;
			var workPhone = bizO.HA_WorkPhone;

			var homePhoneToHash = TextStandardizerHelper.StandardizePhone(homePhone);
			var faxToHash = TextStandardizerHelper.StandardizePhone(fax);
			var mobileToHash = TextStandardizerHelper.StandardizePhone(mobile);
			var workPhoneToHash = TextStandardizerHelper.StandardizePhone(workPhone);

			var homePhoneMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, false, GetHash(homePhoneToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);
			var faxMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(faxToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);
			var mobileMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(mobileToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);
			var workPhoneMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(workPhoneToHash), Guid.Empty, bizO.HA_PER, bizO.HA_RN_NKCountry);

			return homePhoneMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				faxMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				mobileMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				workPhoneMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK);
		}

		protected override void QueueMasterForProcessing()
		{
			QueuePersonForDeduplicationProcessing(bizO?.Person);
		}
	}
}
