//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvoiceLineTaxValidation
//
//    This class should be used for overriding validation in AutoJobComInvoiceLineTaxValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceLineTaxValidation : AutoJobComInvoiceLineTaxValidation
	{
		public JobComInvoiceLineTaxValidation(AutoJobComInvoiceLineTax parent) : base(parent)
		{
		}
	}
}
