using System;
using System.Globalization;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingGlbPersonMaintenance : PatternMatchingSource<GlbPerson>
	{
		public PatternMatchingGlbPersonMaintenance(GlbPerson bizO)
			: base(bizO)
		{
		}

		protected override bool CreateOrUpdateName()
		{
			if (PersonNameComparator.IsDummyContact(bizO.PER_FullName))
			{
				return false;
			}

			var name = bizO.PER_FullName;
			var nameToHash = name.Trim().Contains(' ') ? TextStandardizerHelper.StandardizePersonName(name) : string.Empty;
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingName>(bizO, false, GetHash(nameToHash), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingNameSchema.PMN_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateRegCode()
		{
			var birthday = bizO.PER_BirthDate;
			var birthdayToHash = birthday.IsEmpty ? string.Empty : (birthday.ToDateTime() - new DateTime(1753, 1, 1)).Days.ToString(CultureInfo.InvariantCulture);
			var birthdayMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, false, GetHash(birthdayToHash), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);

			var passport = bizO.PER_Passport;
			var passportToHash = passport.IsEmpty ? string.Empty : CodeTypeIdentifier.Passport + TextStandardizerHelper.StandardizeRegCode(passport);
			var passportMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, !birthday.IsEmpty, GetHash(passportToHash), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);

			var license = bizO.PER_DriversLicenseNumber;
			var licenseToHash = license.IsEmpty ? string.Empty : CodeTypeIdentifier.License + TextStandardizerHelper.StandardizeRegCode(license);
			var licenseMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, !(birthday.IsEmpty && passport.IsEmpty), GetHash(licenseToHash), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);

			return birthdayMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK) |
				passportMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK) |
				licenseMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdatePhone()
		{
			var homePhone = bizO.PER_HomePhone;
			var fax = bizO.PER_FaxNumber;
			var mobile = bizO.PER_MobilePhone;
			var mobile2 = bizO.PER_MobilePhone2;

			var homePhoneToHash = TextStandardizerHelper.StandardizePhone(homePhone);
			var faxToHash = TextStandardizerHelper.StandardizePhone(fax);
			var mobileToHash = TextStandardizerHelper.StandardizePhone(mobile);
			var mobile2ToHash = TextStandardizerHelper.StandardizePhone(mobile2);

			var homePhoneMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, false, GetHash(homePhoneToHash), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);
			var faxMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(faxToHash), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);
			var mobileMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(mobileToHash), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);
			var mobile2Maintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(mobile2ToHash), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);

			return homePhoneMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				faxMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				mobileMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				mobile2Maintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateEmail()
		{
			var emailAddress = bizO.PER_EmailAddress;
			var emailAddress2 = bizO.PER_EmailAddress2;

			var emailToHash = TextStandardizerHelper.StandardizeEmail(emailAddress);
			var emailToHash2 = TextStandardizerHelper.StandardizeEmail(emailAddress2);

			var emailMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingEmail>(bizO, false, GetHash(emailToHash), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);
			var emailMaintenance2 = new PatternMatchingMaintenanceUtilities<PatternMatchingEmail>(bizO, true, GetHash(emailToHash2), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);

			return emailMaintenance.CreateOrUpdatePatternRecord(PatternMatchingEmailSchema.PME_ParentId, bizO.PK) | emailMaintenance2.CreateOrUpdatePatternRecord(PatternMatchingEmailSchema.PME_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateAddress()
		{
			if (!TextStandardizerHelper.IsPlaceholderAddress(bizO.PER_HomeAddress1Internal) ||
				string.IsNullOrEmpty(bizO.PER_HomeAddress1Internal) && !TextStandardizerHelper.IsPlaceholderAddress(bizO.PER_HomeAddress2Internal))
			{
				var addressLine = (bizO.PER_HomeAddress1Internal + bizO.PER_HomeAddress2Internal + bizO.PER_CityInternal + bizO.PER_PostcodeInternal + bizO.PER_StateInternal).ToUpperInvariant();
				var addressLineMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingAddress>(bizO, false, GetHash(addressLine), Guid.Empty, bizO.PK, bizO.PER_RN_NKCountryInternal);
				return addressLineMaintenance.CreateOrUpdatePatternRecord(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK);
			}

			return base.CreateOrUpdateAddress();
		}

		protected override void QueueMasterForProcessing()
		{
			QueuePersonForDeduplicationProcessing(bizO);
		}
	}
}
