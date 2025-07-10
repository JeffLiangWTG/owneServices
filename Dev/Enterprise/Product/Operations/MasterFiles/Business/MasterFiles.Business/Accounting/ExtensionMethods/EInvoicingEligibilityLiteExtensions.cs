using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.MasterFiles.Business
{
	// NOTE: Only implement simple helper methods that operate on the "Lite" interfaces.
	//       If more complex logic is required, see ITransactionHeaderWrapper as an alternative.

	public static class IEInvoicingEligibilityLiteTransactionExtensions
	{
		/// <summary>
		/// Equivalent to the AH_IsDisbursementCalc property
		/// </summary>
		public static bool IsDisbursementInvoice(this IEInvoicingEligibilityLiteTransaction transaction)
			=> AccTransactionHeader.IsDisbursementInvoiceType(transaction?.TransactionCategory);
	}

	public static class IEInvoicingEligibilityLiteOrgHeaderExtensions
	{
		public static bool HasAnyRegistrationCode(this IEInvoicingEligibilityLiteOrgHeader orgHeader, ZString registrationCode, ZString countryCode)
		{
			Argument.NotNull(orgHeader, nameof(orgHeader));

			return orgHeader.RegistrationCodes
						.Any(rc => rc.CountryCode == countryCode
								&& rc.CodeType == registrationCode
								&& !rc.RegistrationNumber.IsEmpty);
		}

		public static bool HasRegistrationCode(this IEInvoicingEligibilityLiteOrgHeader orgHeader, ZString registrationCode, ZString countryCode, ZString registrationNumber)
		{
			Argument.NotNull(orgHeader, nameof(orgHeader));

			return orgHeader.RegistrationCodes
						.Any(rc => rc.CountryCode == countryCode
								&& rc.CodeType == registrationCode
								&& rc.RegistrationNumber == registrationNumber);
		}

		public static bool HasEligibleComplianceSubType(this IEInvoicingEligibilityLiteTransaction transaction)
		{
			Argument.NotNull(transaction, nameof(transaction));

			var complianceSubTypeForEInvoicingInfo = CountryComplianceFactory.GetICountryEligibleComplianceSubTypeForEInvoice(transaction.CountryCode);
			return complianceSubTypeForEInvoicingInfo?.IsComplianceSubTypeElegibleForEInvoicing(transaction.ComplianceSubType) ?? false;
		}
	}
}
