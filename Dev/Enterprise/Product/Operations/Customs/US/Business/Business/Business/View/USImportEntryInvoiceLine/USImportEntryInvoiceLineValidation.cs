//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSImportEntryInvoiceLineValidation
//
//    This class should be used for overriding validation in AutoUSImportEntryInvoiceLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USImportEntryInvoiceLineValidation : AutoUSImportEntryInvoiceLineValidation
	{
		public USImportEntryInvoiceLineValidation(AutoUSImportEntryInvoiceLine parent)
			: base(parent)
		{
		}
	}
}
