using System;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingOrgAddressMaintenance : PatternMatchingSource<OrgAddress>
	{
		public PatternMatchingOrgAddressMaintenance(OrgAddress bizO)
			: base(bizO)
		{
		}

		protected override bool CreateOrUpdateAddress()
		{
			var addressLine = (bizO.OA_Address1 + bizO.OA_Address2 + bizO.OA_City + bizO.OA_PostCode + bizO.OA_State).ToUpperInvariant();

			if ((!TextStandardizerHelper.IsPlaceholderAddress(bizO.OA_Address1) ||
				string.IsNullOrEmpty(bizO.OA_Address1) && !TextStandardizerHelper.IsPlaceholderAddress(bizO.OA_Address2)) &&
				bizO.Country != null)
			{
				var addressLineMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingAddress>(bizO, false, GetHash(addressLine), bizO.Header.PK, Guid.Empty, bizO.Country.Code);
				return addressLineMaintenance.CreateOrUpdatePatternRecord(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK);
			}

			return base.CreateOrUpdateAddress();
		}

		protected override bool CreateOrUpdateDomain()
		{
			if (bizO.Country != null)
			{
				var emailAddress = bizO.OA_Email;
				var domain = TextStandardizerHelper.ExtractEmailDomain(emailAddress);
				var domainToHash = !TextStandardizerHelper.IsGenericDomain(domain) ? domain : string.Empty;
				var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingDomain>(bizO, false, GetHash(domainToHash), bizO.Header.PK, Guid.Empty, bizO.Country.Code);

				return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK);
			}

			return base.CreateOrUpdateDomain();
		}

		protected override bool CreateOrUpdateEmail()
		{
			if (bizO.Country != null)
			{
				var emailAddress = bizO.OA_Email;
				var emailToHash = TextStandardizerHelper.StandardizeEmail(emailAddress);
				var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingEmail>(bizO, false, GetHash(emailToHash), bizO.Header.PK, Guid.Empty, bizO.Country.Code);

				return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingEmailSchema.PME_ParentId, bizO.PK);
			}

			return base.CreateOrUpdateEmail();
		}

		protected override bool CreateOrUpdateName()
		{
			if (bizO.Country != null)
			{
				var companyNameOverride = bizO.OA_CompanyNameOverride;
				var nameToHash = TextStandardizerHelper.StandardizeCompanyName(companyNameOverride, bizO.Header.CountryCode);
				var companyNameOverrideMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingName>(bizO, false, GetHash(nameToHash), bizO.Header.PK, Guid.Empty, bizO.Country.Code);

				return companyNameOverrideMaintenance.CreateOrUpdatePatternRecord(PatternMatchingNameSchema.PMN_ParentId, bizO.PK);
			}

			return base.CreateOrUpdateName();
		}

		protected override bool CreateOrUpdatePhone()
		{
			if (bizO.Country != null)
			{
				var phone = bizO.OA_Phone;
				var fax = bizO.OA_Fax;
				var mobile = bizO.OA_Mobile;
				var phoneToHash = TextStandardizerHelper.StandardizePhone(phone);
				var faxToHash = TextStandardizerHelper.StandardizePhone(fax);
				var mobileToHash = TextStandardizerHelper.StandardizePhone(mobile);
				var phoneMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, false, GetHash(phoneToHash), bizO.Header.PK, Guid.Empty, bizO.Country.Code);
				var faxMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(faxToHash), bizO.Header.PK, Guid.Empty, bizO.Country.Code);
				var mobileMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, true, GetHash(mobileToHash), bizO.Header.PK, Guid.Empty, bizO.Country.Code);

				return phoneMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
					faxMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK) |
					mobileMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK);
			}

			return base.CreateOrUpdatePhone();
		}

		protected override void QueueMasterForProcessing()
		{
			QueueOrgForDeduplicationProcessing(bizO.Header);
		}
	}
}
