using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

public class TransportChargesProvider : ITransportCharges
{
	readonly NctsBill bill;

	public TransportChargesProvider(NctsBill bill)
	{
		this.bill = Argument.NotNull(bill, nameof(bill));
	}

	public string MethodOfPayment => bill.B0_TransportPaymentMethod;
}
