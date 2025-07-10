namespace Enterprise.Customs.NZ.Business
{
	public class PaymentMethodList : Enterprise.MasterFiles.Business.PaymentMethodList
	{
		public static string GetCustomsCode(string paymentPartyListCode)
		{
			string result = "";
			switch (paymentPartyListCode)
			{
				case Codes.CashPaidByClient:
				case Codes.CashPaidByBroker:
					result = CustomsPaymentTypeList.Codes.CashPayment;
					break;
				case Codes.BrokerDeferred:
					result = CustomsPaymentTypeList.Codes.BrokerDeferred;
					break;
				case Codes.ClientDeferred:
					result = CustomsPaymentTypeList.Codes.ClientDeferred;
					break;
			}
			return result;
		}

		public static string GetPaymentPartyCode(string customsCode, string currentPaymentPartyCode)
		{
			string result = "";
			switch (customsCode)
			{
				case CustomsPaymentTypeList.Codes.CashPayment:
					switch (currentPaymentPartyCode)
					{
						case Codes.CashPaidByBroker:
						case Codes.BrokerDeferred:
							result = Codes.CashPaidByBroker;
							break;
						case Codes.CashPaidByClient:
						case Codes.ClientDeferred:
							result = Codes.CashPaidByClient;
							break;
					}
					break;
				case CustomsPaymentTypeList.Codes.BrokerDeferred:
					result = Codes.BrokerDeferred;
					break;
				case CustomsPaymentTypeList.Codes.ClientDeferred:
					result = Codes.ClientDeferred;
					break;
			}
			return result;
		}

		public static bool IsPaidByBroker(string code)
		{
			return code == Codes.BrokerDeferred || code == Codes.CashPaidByBroker;
		}

		public static bool IsValidCode(string code)
		{
			bool result = false;
			switch (code)
			{
				case Codes.CashPaidByClient:
				case Codes.CashPaidByBroker:
				case Codes.BrokerDeferred:
				case Codes.ClientDeferred:
					result = true;
					break;
			}

			return result;
		}
	}
}
