//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvoiceHeaderValidation
//
//    This class should be used for overriding validation in AutoJobComInvoiceHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.Customs.Business
{
	/// <summary>
	/// This is base validation class for JobComInvoiceHeader, JobComInvoiceGroupHeader and EMCSJobComInvoiceHeaderValidation
	/// to be able to override. The validation class for JobComInvoiceHeader is InvoiceHeaderValidation
	/// </summary>
	public class JobComInvoiceHeaderValidation : AutoJobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(AutoJobComInvoiceHeader parent) : base(parent)
		{
		}
	}
}
