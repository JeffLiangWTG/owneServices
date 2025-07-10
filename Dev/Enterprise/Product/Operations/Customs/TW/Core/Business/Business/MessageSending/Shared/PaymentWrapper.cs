using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class PaymentWrapper : IPayment
	{
		public PaymentWrapper(ZString methodCode)
			: this(methodCode, null)
		{
		}

		public PaymentWrapper(ZString methodCode, ZString referenceID)
		{
			this.methodCode = methodCode;
			this.referenceID = referenceID;
		}

		readonly ZString methodCode;
		readonly ZString referenceID;

		ZString IPayment.MethodCode => methodCode;

		ZString IPayment.ReferenceID => referenceID;
	}
}
