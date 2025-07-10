using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementRecipientCollection : ActiveBusinessObjectCollection<OrgCommissionAgreementRecipient>
	{
		public OrgCommissionAgreementRecipientCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgCommissionAgreementRecipientCollection(OrgCommissionAgreement commissionAgreement)
			: base(commissionAgreement)
		{
			this.commissionAgreement = commissionAgreement;
		}

		readonly OrgCommissionAgreement commissionAgreement;

		#region AddNew

		public OrgCommissionAgreementRecipient AddNew(GlbStaff staff)
		{
			var result = AddNew();
			using (result.GetValidationSuspender())
			{
				result.CAR_GS_NKStaff = staff.GS_Code;
			}

			return result;
		}

		public OrgCommissionAgreementRecipient AddNew(OrgHeader party)
		{
			var result = AddNew();
			using (result.GetValidationSuspender())
			{
				result.CAR_OH_Party = party.PK;
			}

			return result;
		}

		public bool EnableValidationOnCountChange
		{
			get
			{
				return enableValidationOnCountChange;
			}
			set
			{
				enableValidationOnCountChange = value;
			}
		}

		bool enableValidationOnCountChange;

		protected override void OnAdded(OrgCommissionAgreementRecipient bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (enableValidationOnCountChange)
			{
				commissionAgreement?.Validation.ValidateWolfPackMemberExists();
			}
		}

		#endregion

		public override void Delete(OrgCommissionAgreementRecipient bizODelete)
		{
			base.Delete(bizODelete);

			if (enableValidationOnCountChange)
			{
				commissionAgreement?.Validation.ValidateWolfPackMemberExists();
			}
		}

		protected override object[] GetCollectionState()
		{
			return new object[] { enableValidationOnCountChange };
		}
	}
}
