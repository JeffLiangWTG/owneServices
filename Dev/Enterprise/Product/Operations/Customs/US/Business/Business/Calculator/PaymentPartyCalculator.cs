using System;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class PaymentPartyCalculator
	{
		public string CalculatePaymentParty(GlbCompany company, ZString payerUnitNo, ZString paymentType, ZString branchDesignation)
		{
			string result = PaymentTypeList.IsPaidByBroker(paymentType) ? PaymentPartyList.Codes.Broker : PaymentPartyList.Codes.Importer;

			if (company != null && payerUnitNo != "")
			{
				result = "";
				var brokerAccounts = USCustomsDataRegistry.Instance.BrokersAccounts.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

				if (brokerAccounts != null && brokerAccounts.ContainsPayerUnitNo(payerUnitNo, branchDesignation))
				{
					result = PaymentPartyList.Codes.Broker;
				}

				if (string.IsNullOrEmpty(result))
				{
					result = PaymentPartyList.Codes.Importer;
				}
			}

			return result;
		}
	}

	public class BankAccountCalculator
	{
		public ZGuid CalculateBankAccount(GlbCompany company, ZString payerUnitNo, ZString branchDesignation)
		{
			var result = ZGuid.Empty;

			if (company != null)
			{
				var brokerAccounts = USCustomsDataRegistry.Instance.BrokersAccounts.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

				if (brokerAccounts != null)
				{
					result = brokerAccounts.GetAssociatedBankAccount(payerUnitNo, branchDesignation);
				}
			}

			return result;
		}
	}
}
