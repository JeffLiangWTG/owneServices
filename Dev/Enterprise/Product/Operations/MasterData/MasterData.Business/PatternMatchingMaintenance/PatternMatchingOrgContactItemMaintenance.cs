using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingOrgContactItemMaintenance : PatternMatchingSource<OrgContactItem>
	{
		public PatternMatchingOrgContactItemMaintenance(OrgContactItem bizO) : base(bizO)
		{
		}

		protected override bool CreateOrUpdatePhone()
		{
			if (bizO.OI_ContactItemType == OrgContactItemTypes.Codes.Phone)
			{
				var phone = bizO.OI_Address;
				var phoneToHash = TextStandardizerHelper.StandardizePhone(phone);
				var phoneMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingPhone>(bizO, false, GetHash(phoneToHash), bizO.Contact.OC_OH, bizO.Contact.OC_PER, bizO.Contact.Header.CountryCode);

				return phoneMaintenance.CreateOrUpdatePatternRecord(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK);
			}

			return false;
		}

		protected override bool CreateOrUpdateEmail()
		{
			if (bizO.OI_ContactItemType == OrgContactItemTypes.Codes.Email)
			{
				var emailAddress = bizO.OI_Address;
				var emailToHash = TextStandardizerHelper.StandardizeEmail(emailAddress);
				var emailMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingEmail>(bizO, false, GetHash(emailToHash), bizO.Contact.OC_OH, bizO.Contact.OC_PER, bizO.Contact.Header.CountryCode);

				return emailMaintenance.CreateOrUpdatePatternRecord(PatternMatchingEmailSchema.PME_ParentId, bizO.PK);
			}

			return false;
		}

		protected override void QueueMasterForProcessing()
		{
			QueuePersonForDeduplicationProcessing(bizO.Contact?.Person);
		}
	}
}
