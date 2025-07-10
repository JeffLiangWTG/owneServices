//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvoiceLineValidation
//
//    This class should be used for overriding validation in AutoJobComInvoiceLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// This is base validation class for common JobComInvoiceLine and EMCSJobComInvoiceLineValidation
	/// </summary>
	public class JobComInvoiceLineValidation : AutoJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(AutoJobComInvoiceLine parent)
			: base(parent)
		{
		}
	}
}
