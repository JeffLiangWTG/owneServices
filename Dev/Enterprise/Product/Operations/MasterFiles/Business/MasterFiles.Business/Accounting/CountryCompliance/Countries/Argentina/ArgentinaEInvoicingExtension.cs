using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Argentina
{
	public interface IArgentinaEInvoicingExtension
	{
		(ZString regType, ZString regValue) GetRegistrationNumberOrganization(ITransactionQRCodeDataProvider transactionData);
		(ZString regType, ZString regValue) GetRegistrationNumberTransactionInfo(TransactionInfo transaction);
		(ZString MonendaId, ZString MonedaCtz) GetCurrencyAndExchangeRateTransactionInfo(TransactionInfo transaction);
		(ZString MonendaId, ZString MonedaCtz) GetCurrencyAndExchangeRate(ITransactionQRCodeDataProvider transactionData);
		ZString GetDocumentType(ZString complianceSubtype);
	}

	public class ArgentinaEInvoicingExtension : IArgentinaEInvoicingExtension
	{
		(ZString regType, ZString regValue) IArgentinaEInvoicingExtension.GetRegistrationNumberOrganization(ITransactionQRCodeDataProvider transactionData)
		{
			(ZString regType, ZString regValue) = (ZString.Empty, ZString.Empty);

			var organization = transactionData.OrgHeader;
			if (organization != null)
			{
				(regType, regValue) = GetRegistrationNumber(organization.MainAddress.OA_RN_NKCountryCode, orgCusCode => GetRegistrationNumber(orgCusCode, organization));
			}

			return (regType, regValue);
		}

		(ZString regType, ZString regValue) IArgentinaEInvoicingExtension.GetRegistrationNumberTransactionInfo(TransactionInfo transaction)
		{
			var organizationCountry = transaction?.OrganizationAddress?.Country?.Code ?? ZString.Empty;

			return GetRegistrationNumber(organizationCountry, orgCusCode => GetRegistrationNumber(orgCusCode, transaction));
		}

		(ZString regType, ZString regValue) GetRegistrationNumber(ZString organizationCountry, Func<ZString, ZString> getGetRegistrationNumberByCusCode)
		{
			(ZString regType, ZString regValue) regNo = (ZString.Empty, ZString.Empty);

			if (!organizationCountry.IsEmpty)
			{
				if (organizationCountry == CountryCodes.Argentina)
				{
					regNo = GetOrganizationRegistrationNumber(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, getGetRegistrationNumberByCusCode);
					if (regNo.regValue.IsEmpty)
					{
						regNo = GetOrganizationRegistrationNumber(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL, getGetRegistrationNumberByCusCode);
					}
					if (regNo.regValue.IsEmpty)
					{
						regNo = GetOrganizationRegistrationNumber(ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI, getGetRegistrationNumberByCusCode);
					}
				}
				else
				{
					regNo = GetOrganizationRegistrationNumber(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF, getGetRegistrationNumberByCusCode);
					if (regNo.regValue.IsEmpty)
					{
						regNo = GetOrganizationRegistrationNumber(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, getGetRegistrationNumberByCusCode);
					}
					if (regNo.regValue.IsEmpty)
					{
						regNo = GetOrganizationRegistrationNumber(OrgCusCode.CodeTypes.PassportID, getGetRegistrationNumberByCusCode);
					}
				}
			}

			return (AFIPEquivalents.OrgCusCodeEquivalentCodes.GetRegTypeCodeEquivalent(regNo.regType), regNo.regValue);
		}

		(ZString regType, ZString regValue) GetOrganizationRegistrationNumber(ZString orgCusCode, Func<ZString, ZString> getGetRegistrationNumberByCusCode)
		{
			(ZString regType, ZString regValue) = (ZString.Empty, ZString.Empty);

			var registrationNumber = getGetRegistrationNumberByCusCode(orgCusCode);
			if (!registrationNumber.IsEmpty)
			{
				(regType, regValue) = (orgCusCode, ((ZString)registrationNumber).RemoveNonNumericCharacters());
			}

			return (regType, regValue);
		}

		ZString GetRegistrationNumber(ZString orgCusCode, TransactionInfo transactionInfo) => transactionInfo?.OrganizationAddress?.RegistrationNumberCollection?.FirstOrDefault(x => (x.Type?.Code ?? ZString.Empty) == orgCusCode)?.Value ?? ZString.Empty;

		ZString GetRegistrationNumber(ZString orgCusCode, OrgHeader organization) => organization.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == orgCusCode)?.OK_CustomsRegNo ?? ZString.Empty;

		(ZString MonendaId, ZString MonedaCtz) IArgentinaEInvoicingExtension.GetCurrencyAndExchangeRateTransactionInfo(TransactionInfo transaction)
		{
			var currencyCode = transaction.OSCurrency?.Code ?? ZString.Empty;
			var exchangeRate = transaction.ExchangeRate.GetValueOrDefault(ZDecimal.Zero);

			return GetCurrencyAndExchangeRate(currencyCode, exchangeRate);
		}

		(ZString MonendaId, ZString MonedaCtz) IArgentinaEInvoicingExtension.GetCurrencyAndExchangeRate(ITransactionQRCodeDataProvider transactionData)
		{
			var currencyCode = transactionData.TransactionCurrency;
			var exchangeRate = transactionData.ExchangeRate;

			return GetCurrencyAndExchangeRate(currencyCode, exchangeRate);
		}

		(ZString CurrencyId, ZString ExchangeRate) GetCurrencyAndExchangeRate(ZString currency, ZDecimal exchangeRate)
		{
			var cotiz = ZString.Empty;

			if (AFIPEquivalents.CurrencyEquivalentCodes.CurrencyCodes.TryGetValue(currency, out string equivalentCode))
			{
				cotiz = currency == CurrencyCodes.Argentina ? exchangeRate.ToString("F0", CultureInfo.InvariantCulture) : exchangeRate.ToString("F6", CultureInfo.InvariantCulture);
			}

			return (equivalentCode ?? string.Empty, cotiz);
		}

		ZString IArgentinaEInvoicingExtension.GetDocumentType(ZString complianceSubtype) =>
			AFIPEquivalents.DocumentTypeEquivalentCodes.EquivalentCodes.TryGetValue(complianceSubtype, out string result) ? result : string.Empty;
	}
}
