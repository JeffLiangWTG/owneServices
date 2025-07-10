using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface IInvoiceDateValidation
	{
		ResourceString ValidateInvoiceDate(AccTransactionHeader header);
		ResourceString ValidateInvoiceDate(ZDateTime invoiceDate);
	}
}
