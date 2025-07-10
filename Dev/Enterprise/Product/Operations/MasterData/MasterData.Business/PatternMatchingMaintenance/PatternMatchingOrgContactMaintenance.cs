using System;
using System.Globalization;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingOrgContactMaintenance : PatternMatchingSource<OrgContact>
	{
		public PatternMatchingOrgContactMaintenance(OrgContact bizO)
			: base(bizO)
		{
		}

		protected override bool CreateOrUpdateName()
		{
			if (PersonNameComparator.IsDummyContact(bizO.OC_ContactName))
			{
				return false;
			}

			var name = bizO.OC_ContactName;
			var nameToHash = name.Trim().Contains(' ') ? TextStandardizerHelper.StandardizePersonName(name) : string.Empty;
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingName>(bizO, false, GetHash(nameToHash), bizO.Header.PK, bizO.OC_PER, bizO.Header.CountryCode);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingNameSchema.PMN_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateDomain()
		{
			var emailAddress = bizO.OC_Email;
			var domainToHash = TextStandardizerHelper.StandardizeEmailDomain(emailAddress);
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingDomain>(bizO, false, GetHash(domainToHash), bizO.Header.PK, Guid.Empty, bizO.Header.CountryCode);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateRegCode()
		{
			var birthday = bizO.OC_Birthday;
			var birthdayToHash = birthday.IsEmpty ? string.Empty : (birthday.ToDateTime() - new DateTime(1753, 1, 1)).Days.ToString(CultureInfo.InvariantCulture);
			var birthdayMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, false, GetHash(birthdayToHash), bizO.Header.PK, bizO.OC_PER, bizO.Header.CountryCode);

			return birthdayMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdateEmail()
		{
			var emailAddress = bizO.OC_Email;
			var emailToHash = TextStandardizerHelper.StandardizeEmail(emailAddress);
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingEmail>(bizO, false, GetHash(emailToHash), bizO.Header.PK, bizO.OC_PER, bizO.Header.CountryCode);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingEmailSchema.PME_ParentId, bizO.PK);
		}

		protected override bool CreateOrUpdatePhone()
		{
			var phone = bizO.OC_Phone;
			var fax = bizO.OC_Fax;
			var mobile = bizO.OC_Mobile;
			var homePhone = bizO.OC_HomePhone;
			var otherPhone = bizO.OC_OtherPhone;

			var phoneToHash = TextStandardizerHelper.StandardizePhone(phone);
			var faxToHash = TextStandardizerHelper.StandardizePhone(fax);
			var mobileToHash = TextStandardizerHelper.StandardizePhone(mobile);
			var otherPhoneToHash = TextStandardizerHelper.StandardizePhone(otherPhone);
			var homePhoneToHash = TextStandardizerHelper.StandardizePhone(homePhone);

			var phoneMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, false, GetHash(phoneToHash), bizO.Header.PK, bizO.OC_PER, bizO.Header.CountryCode);
			var faxMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(faxToHash), bizO.Header.PK, bizO.OC_PER, bizO.Header.CountryCode);
			var mobileMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(mobileToHash), bizO.Header.PK, bizO.OC_PER, bizO.Header.CountryCode);
			var otherPhoneMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(otherPhoneToHash), bizO.Header.PK, bizO.OC_PER, bizO.Header.CountryCode);
			var homePhoneMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(homePhoneToHash), bizO.Header.PK, bizO.OC_PER, bizO.Header.CountryCode);

			return phoneMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				faxMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				mobileMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				otherPhoneMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
				homePhoneMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK);
		}

		protected override void QueueMasterForProcessing()
		{
			QueueOrgForDeduplicationProcessing(bizO.Header);
			QueuePersonForDeduplicationProcessing(bizO?.Person);
		}
	}
}
