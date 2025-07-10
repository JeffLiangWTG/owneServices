using System;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public class PaymentTermInfo
	{
		public PaymentTermInfo(PaymentTermType paymentTermType, CostSell costOrSell, string value)
		{
			InfoType = paymentTermType;
			CostOrSell = costOrSell;

			if (string.IsNullOrEmpty(value))
			{
				throw new InvalidOperationException("PaymentTermInfo cannot have empty value.");
			}

			Value = value;
		}

		public readonly PaymentTermType InfoType;
		public readonly CostSell CostOrSell;
		public readonly string Value;
	}
}
