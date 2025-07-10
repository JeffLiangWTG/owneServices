using System;
using System.Globalization;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingGlbStaffMaintenance : PatternMatchingSource<GlbStaff>
	{
		public PatternMatchingGlbStaffMaintenance(GlbStaff bizO) : base(bizO)
		{
		}

		protected override bool CreateOrUpdateName()
		{
			var name = bizO.GS_FullName;
			var nameToHash = name.Trim().Contains(' ') ? TextStandardizerHelper.StandardizePersonName(name) : string.Empty;
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingName>(bizO, false, GetHash(nameToHash), Guid.Empty, bizO.GS_PER, bizO.GS_RN_NKCountryCodeInternal);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingNameSchema.PMN_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateRegCode()
		{
			var birthday = bizO.GS_Birthdate;
			var birthdayToHash = birthday.IsEmpty ? string.Empty : (birthday.ToDateTime() - new DateTime(1753, 1, 1)).Days.ToString(CultureInfo.InvariantCulture);
			var birthdayMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, false, GetHash(birthdayToHash), Guid.Empty, bizO.GS_PER, bizO.GS_RN_NKCountryCodeInternal);

			var passport = bizO.GS_Passport;
			var passportToHash = passport.IsEmpty ? string.Empty : CodeTypeIdentifier.Passport + TextStandardizerHelper.StandardizeRegCode(passport);
			var passportMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, true, GetHash(passportToHash), Guid.Empty, bizO.GS_PER, bizO.GS_RN_NKCountryCodeInternal);

			var certification = bizO.GS_EnterpriseCertificationID;
			var certificationToHash = certification.IsEmpty ? string.Empty : CodeTypeIdentifier.Certificate + TextStandardizerHelper.StandardizeRegCode(certification);
			var certificationMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, true, GetHash(certificationToHash), Guid.Empty, bizO.GS_PER, bizO.GS_RN_NKCountryCodeInternal);

			return birthdayMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK) |
				passportMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK) |
				certificationMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdatePhone()
		{
			var homePhone = bizO.GS_HomePhone;
			var fax = bizO.GS_FaxNum;
			var mobile = bizO.GS_MobilePhone;
			var workPhone = bizO.GS_WorkPhone;

			var homePhoneToHash = TextStandardizerHelper.StandardizePhone(homePhone);
			var faxToHash = TextStandardizerHelper.StandardizePhone(fax);
			var mobileToHash = TextStandardizerHelper.StandardizePhone(mobile);
			var workPhoneToHash = TextStandardizerHelper.StandardizePhone(workPhone);

			var homePhoneMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, false, GetHash(homePhoneToHash), Guid.Empty, bizO.GS_PER, bizO.GS_RN_NKCountryCodeInternal);
			var faxMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(faxToHash), Guid.Empty, bizO.GS_PER, bizO.GS_RN_NKCountryCodeInternal);
			var mobileMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(mobileToHash), Guid.Empty, bizO.GS_PER, bizO.GS_RN_NKCountryCodeInternal);
			var workPhoneMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(workPhoneToHash), Guid.Empty, bizO.GS_PER, bizO.GS_RN_NKCountryCodeInternal);

			return homePhoneMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				faxMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				mobileMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				workPhoneMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateEmail()
		{
			var emailAddress = bizO.GS_EmailAddress;
			var emailToHash = TextStandardizerHelper.StandardizeEmail(emailAddress);
			var emailMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingEmail>(bizO, false, GetHash(emailToHash), Guid.Empty, bizO.GS_PER, bizO.GS_RN_NKCountryCodeInternal);

			return emailMaintenance.CreateOrUpdatePatternRecord(PatternMatchingEmailSchema.PME_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateAddress()
		{
			if (!TextStandardizerHelper.IsPlaceholderAddress(bizO.GS_UserAddress1Internal) ||
				string.IsNullOrEmpty(bizO.GS_UserAddress1Internal) && !TextStandardizerHelper.IsPlaceholderAddress(bizO.GS_UserAddress2Internal))
			{
				var addressLine = (bizO.GS_UserAddress1Internal + bizO.GS_UserAddress2Internal + bizO.GS_CityInternal + bizO.GS_PostcodeInternal + bizO.GS_StateInternal).ToUpperInvariant();
				var addressLineMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingAddress>(bizO, false, GetHash(addressLine), Guid.Empty, bizO.GS_PER, bizO.GS_RN_NKCountryCodeInternal);
				return addressLineMaintenance.CreateOrUpdatePatternRecord(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK);
			}

			return base.CreateOrUpdateAddress();
		}

		protected override void QueueMasterForProcessing()
		{
			QueuePersonForDeduplicationProcessing(bizO?.Person);
		}
	}
}
