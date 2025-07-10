
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class JobComInvoiceLineTaxValidation : EU.Business.Declaration.JobComInvoiceLineTaxValidation
	{
		public JobComInvoiceLineTaxValidation(JobComInvoiceLineTax parent) : base(parent)
		{
		}

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		protected override void CheckJLT_MethodOfCalculation()
		{
			base.CheckJLT_MethodOfCalculation();
			ListValidation.MessageErrorIfInvalidCode(Parent.JLT_MethodOfCalculationInfo);
		}

		protected override void CheckJLT_RateOverrideReasonCode()
		{
			base.CheckJLT_RateOverrideReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.JLT_RateOverrideReasonCodeInfo);
		}

		protected override void CheckJLT_Rate()
		{
			base.CheckJLT_Rate();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JLT_RateInfo);
		}

		protected override void CheckJLT_BaseValue()
		{
			base.CheckJLT_BaseValue();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JLT_BaseValueInfo);
		}

		protected override void CheckJLT_Amount()
		{
			base.CheckJLT_Amount();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JLT_AmountInfo);
		}
	}
}
