using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Business.Messaging
{
	public class ACASCountryHandler : IACASCountryHandler
	{
		// By default, do not include ACAS lines in FWB/FHL
		public bool ShouldApplyACAS() => false;

		// This is a US specific function so by default set to false for other countries
		public bool IsVerifiedKnownConsignor() => false;

		public bool GetCustomerAccountHolderAndName(out string accountHolder, out string accountName)
		{
			accountHolder = string.Empty;
			accountName = string.Empty;
			return false;
		}

		public bool GetCustomerAccountIssuerAndNumber(out string accountIssuer, out string accountNumber)
		{
			accountIssuer = string.Empty;
			accountNumber = string.Empty;
			return false;
		}

		public string GetCustomerAccountShippingFrequency()
		{
			return string.Empty;
		}

		public bool GetCustomerAccountEstablishmentDate(out string establishmentDate)
		{
			establishmentDate = string.Empty;
			return false;
		}

		public bool GetCustomerAccountBillingType(out string billingType)
		{
			billingType = string.Empty;
			return false;
		}

		public string GetCustomerAccountCreationIPAddress()
		{
			return string.Empty;
		}

		public string GetAWBCreationIPAddress()
		{
			return string.Empty;
		}

		public (bool isNaturalPersonOrg, string idType, string idIssuer, string idNumber) GetBiographicData()
		{
			return default;
		}
	}
}
