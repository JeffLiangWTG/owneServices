using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceProcessingErrorLogValidation : AutoAccDraftInvoiceProcessingErrorLogValidation
	{
		public AccDraftInvoiceProcessingErrorLogValidation(AutoAccDraftInvoiceProcessingErrorLog parent) : base(parent)
		{
		}

		protected override void CheckAIL_Code()
		{
			base.CheckAIL_Code();

			ListValidation.ErrorIfInvalidCode(Parent.AIL_CodeInfo);
		}
	}
}