using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementRecipientRateCollection : ActiveBusinessObjectCollection<OrgCommissionAgreementRecipientRate>
	{
		public OrgCommissionAgreementRecipientRateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgCommissionAgreementRecipientRateCollection(OrgCommissionAgreementRecipient commissionAgreementRecipient)
			: base(commissionAgreementRecipient)
		{
		}
	}
}
