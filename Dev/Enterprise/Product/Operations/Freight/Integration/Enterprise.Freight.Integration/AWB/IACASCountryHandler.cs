namespace Enterprise.Freight.Integration.AWB
{
	public interface IACASCountryHandler
	{
		bool ShouldApplyACAS();

		bool IsVerifiedKnownConsignor();

		bool GetCustomerAccountHolderAndName(out string accountHolder, out string accountName);

		bool GetCustomerAccountIssuerAndNumber(out string accountIssuer, out string accountNumber);

		string GetCustomerAccountShippingFrequency();

		bool GetCustomerAccountEstablishmentDate(out string establishmentDate);

		bool GetCustomerAccountBillingType(out string billingType);

		string GetCustomerAccountCreationIPAddress();

		string GetAWBCreationIPAddress();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodingConvention", "WTG1013:Don't use tuple types in public interfaces.", Justification = "Multiple out or ref parameters are not suggested here")]
		(bool isNaturalPersonOrg, string idType, string idIssuer, string idNumber) GetBiographicData();
	}
}
