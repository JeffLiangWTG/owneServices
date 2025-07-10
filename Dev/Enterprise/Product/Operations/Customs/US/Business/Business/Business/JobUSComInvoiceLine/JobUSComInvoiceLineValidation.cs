//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobUSComInvoiceLineValidation
//
//    This class should be used for overriding validation in AutoJobUSComInvoiceLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class JobUSComInvoiceLineValidation : AutoJobUSComInvoiceLineValidation
	{
		public JobUSComInvoiceLineValidation(AutoJobUSComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceLine Parent => ((JobUSComInvoiceLine)base.Parent).InvoiceLine;
	}
}
