namespace Enterprise.MasterFiles.Business
{
	public class OrgInvoiceRollupOrGroupValidation : AutoOrgInvoiceRollupOrGroupValidation
	{
		public OrgInvoiceRollupOrGroupValidation(AutoOrgInvoiceRollupOrGroup parent) : base(parent)
		{
		}

		OrgInvoiceRollupOrGroup ParentInvoiceRollupOrGroup
		{
			get { return Parent as OrgInvoiceRollupOrGroup; }
		}

		protected override void CheckPG_InvoicePostingStyle()
		{
			base.CheckPG_InvoicePostingStyle();
			if (IsValidationApplicable())
			{
				ParentInvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.ValidateInvoicePostingStyle();
			}
		}

		protected override void CheckPG_InvoiceLineDisplayOption()
		{
			base.CheckPG_InvoiceLineDisplayOption();

			if (IsValidationApplicable())
			{
				ParentInvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.ValidateInvoiceLineDisplayOption();
			}
		}

		protected override void CheckPG_JobType()
		{
			base.CheckPG_JobType();

			if (IsValidationApplicable())
			{
				ParentInvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.ValidateJobType();
			}
		}

		protected override void CheckPG_ServiceDirection()
		{
			base.CheckPG_ServiceDirection();

			if (IsValidationApplicable())
			{
				ParentInvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.ValidateServiceDirection();
			}
		}

		protected override void CheckPG_TransportMode()
		{
			base.CheckPG_TransportMode();

			if (IsValidationApplicable())
			{
				ParentInvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.ValidateTransportMode();
			}
		}

		protected override void CheckPG_GroupOrSubtotalStyle()
		{
			base.CheckPG_GroupOrSubtotalStyle();

			if (IsValidationApplicable())
			{
				ParentInvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.ValidateGroupOrSubtotalStyle();
			}
		}

		protected override void CheckPG_GroupOrSubTotal()
		{
			base.CheckPG_GroupOrSubTotal();

			if (IsValidationApplicable())
			{
				ParentInvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.ValidateGroupOrSubTotal();
			}
		}

		protected override void CheckPG_RX_NKInvoicePostingCurrency()
		{
			if (IsValidationApplicable())
			{
				ParentInvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.ValidateInvoicePostingCurrency();
			}
		}

		protected bool IsValidationApplicable()
		{
			return (Parent.CompanyData != null && Parent.CompanyData.OB_IsDebtor && ParentInvoiceRollupOrGroup != null);
		}
	}
}
