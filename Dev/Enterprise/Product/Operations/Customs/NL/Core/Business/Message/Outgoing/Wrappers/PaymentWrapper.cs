using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public class PaymentWrapper : IPayment
{
	public PaymentWrapper(ZString g4_MethodOfPayment)
	{
		this.g4_MethodOfPayment = g4_MethodOfPayment;
	}
	readonly ZString g4_MethodOfPayment;

	public string MethodCode => g4_MethodOfPayment;
	public decimal PaymentAmount => decimal.Zero;
}
