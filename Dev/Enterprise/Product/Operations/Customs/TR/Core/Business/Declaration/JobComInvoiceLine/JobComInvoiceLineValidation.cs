using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class JobComInvoiceLineValidation : EU.Business.Declaration.JobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		protected override void CheckExposedCusEntryLineErrorProperty()
		{
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			base.CheckJI_PreviousEntryNumber();
			if (Parent.IsPreviousEntryAvailable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PreviousEntryNumberInfo);
			}
		}

		protected override void CheckJI_PreviousEntryLineNumber()
		{
			base.CheckJI_PreviousEntryLineNumber();
			if (Parent.IsPreviousEntryAvailable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PreviousEntryLineNumberInfo);
			}
		}

		protected override void CheckJI_ValuationCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_ValuationCodeInfo, Parent.Lookups.NatureOfTransactionList);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ValuationCodeInfo);
		}

		protected override bool IsJIDescriptionMandatory => false;
	}
}
