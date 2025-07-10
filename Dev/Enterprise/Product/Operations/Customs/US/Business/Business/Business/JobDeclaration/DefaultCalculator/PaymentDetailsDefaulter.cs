using System;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business
{
	public class PaymentDetailsDefaulter
	{
		public void Default(JobDeclaration declaration)
		{
			OrgHeaderWrapper importerOfRecord = declaration.IORWrapper;
			if (importerOfRecord != null)
			{
				declaration.US_PaymentType = importerOfRecord.ZO_PaymentType;

				if (!importerOfRecord.ZO_TaxDeferredInd.IsEmpty)
				{
					declaration.US_TaxDeferIndicator = importerOfRecord.ZO_TaxDeferredInd;
				}

				var brokerToPay = importerOfRecord.ZO_BrokerToPay;
				if (brokerToPay.IsEmpty)
				{
					brokerToPay = GetDefaultBrokerToPayIndicator(importerOfRecord.ZO_AccountNo, importerOfRecord.ZO_PaymentType, declaration.RegistryCompanyPK);
				}

				declaration.BrokerToPayIndicator = brokerToPay;
			}
		}

		public ZString GetDefaultBrokerToPayIndicator(ZString accountNumber, ZString paymentType, Guid companyPK)
		{
			var brokerToPay = ZString.Empty;
			var brokerAccounts = USCustomsDataRegistry.Instance.BrokersAccounts.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			var accountNumberBelongsToBroker = brokerAccounts != null && brokerAccounts.ContainsPayerUnitNo(accountNumber);

			var isPaidByBroker = PaymentTypeList.IsPaidByBroker(paymentType) ||
							(PaymentTypeList.IsPaidByImporter(paymentType) && !accountNumber.IsEmpty && accountNumberBelongsToBroker);

			if (isPaidByBroker)
			{
				brokerToPay = YesNoDefaultList.Codes.Yes;
			}
			else if (paymentType == PaymentTypeList.Codes.IndividualBasis)
			{
				brokerToPay = "";
			}
			else
			{
				brokerToPay = YesNoDefaultList.Codes.No;
			}
			return brokerToPay;
		}

		public ZString GetDefaultBrokerToPayIndicatorBasedOnPaymentType(ZString paymentType)
		{
			var brokerToPay = ZString.Empty;
			if (PaymentTypeList.IsPaidByBroker(paymentType))
			{
				brokerToPay = YesNoDefaultList.Codes.Yes;
			}

			if (PaymentTypeList.IsPaidByImporter(paymentType))
			{
				brokerToPay = YesNoDefaultList.Codes.No;
			}

			return brokerToPay;
		}
	}
}
