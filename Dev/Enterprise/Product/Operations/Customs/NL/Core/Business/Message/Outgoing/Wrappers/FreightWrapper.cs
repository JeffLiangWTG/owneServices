using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class FreightWrapper : IFreight
{
	public FreightWrapper(JobDeclaration declaration)
	{
		PaymentMethodCode = Argument.NotNull(declaration, nameof(declaration)).JE_PaymentMethod;
	}

	public FreightWrapper(JobComInvoiceHeader invoiceHeader)
	{
		PaymentMethodCode = Argument.NotNull(invoiceHeader, nameof(invoiceHeader)).ZG_TransportChargesMethodOfPayment;
	}

	public string PaymentMethodCode { get; }
}
