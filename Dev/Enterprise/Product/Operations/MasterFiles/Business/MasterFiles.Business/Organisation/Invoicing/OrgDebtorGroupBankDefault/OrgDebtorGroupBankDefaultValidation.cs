namespace Enterprise.MasterFiles.Business
{
	public class OrgDebtorGroupBankDefaultValidation : AutoOrgDebtorGroupBankDefaultValidation
	{
		public OrgDebtorGroupBankDefaultValidation(AutoOrgDebtorGroupBankDefault parent) : base(parent)
		{
		}

		protected override void CheckP6_ABIsNotEmpty()
		{
			if (Parent.P6_OverrideRegistryCurrencyToBankSetting)
			{
				base.CheckP6_ABIsNotEmpty();
			}
		}
	}
}
