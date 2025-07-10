using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public abstract class CountryComplianceInfo : ICountryComplianceInfo, ICountryComplianceInfoBase
	{
		public abstract ZString CountryCode { get; }

		#region ICountryComplianceInfo

		string ICountryComplianceInfo.GetBusinessRegistrationCode() => GetBusinessRegistrationCode();
		string ICountryComplianceInfo.GetConsumptionTaxRegistrationCode() => GetConsumptionTaxRegistrationCode();
		string ICountryComplianceInfo.GetConsumptionTaxCode() => GetConsumptionTaxCode();
		string ICountryComplianceInfo.GetComplianceSequencePrefixErrorMessage() => GetComplianceSequencePrefixErrorMessage();
		string ICountryComplianceInfo.GetComplianceSequencePrefixRegex() => GetComplianceSequencePrefixRegex();
		string ICountryComplianceInfo.GetLocalBusinessRegNoCodeType() => GetLocalBusinessRegNoCodeType();
		string ICountryComplianceInfo.GetRecipientLocalBusinessRegNumberCodeType() => GetRecipientLocalBusinessRegNumberCodeType();
		string ICountryComplianceInfo.GetRecipientLocalBusinessReg2NumberCodeType() => GetRecipientLocalBusinessReg2NumberCodeType();
		string ICountryComplianceInfo.GetRecipientLocalBusinessRegHeading() => GetRecipientLocalBusinessRegHeading();
		string ICountryComplianceInfo.GetRecipientLocalBusinessReg2Heading() => GetRecipientLocalBusinessReg2Heading();
		bool? ICountryComplianceInfo.GetIsReciprocal() => GetIsReciprocal();
		bool? ICountryComplianceInfo.GetIsRightHandSideAdressCountry() => GetIsRightHandSideAdressCountry();
		string ICountryComplianceInfo.GetRecipientTaxIDHeading() => GetRecipientTaxIDHeading();
		bool? ICountryComplianceInfo.HasExtraTaxInfo() => HasExtraTaxInfo();
		string ICountryComplianceInfo.GetExtraTaxDescription(string extraTaxTypeCode) => GetExtraTaxDescription(extraTaxTypeCode);
		string ICountryComplianceInfo.GetComplianceVersionNo() => GetComplianceVersionNo();
		ResourceStringData ICountryComplianceInfo.GetExtraTaxOSAmountCaption() => GetExtraTaxOSAmountCaption();
		ResourceStringData ICountryComplianceInfo.GetExtraTaxLocalAmountCaption() => GetExtraTaxLocalAmountCaption();
		ResourceStringData ICountryComplianceInfo.GetTaxOSAmountCaption() => GetTaxOSAmountCaption();
		ResourceStringData ICountryComplianceInfo.GetTaxLocalAmountCaption() => GetTaxLocalAmountCaption();
		bool ICountryComplianceInfo.GetIsTransactionSequencingRequired(string ledger, string transactionType) => GetIsTransactionSequencingRequired(ledger, transactionType);

		#region Registry Item Defaults

		bool? ICountryComplianceInfo.GetDefaultValueForDisplayRecipientTaxIDRegistry() => GetDefaultValueForDisplayRecipientTaxIDRegistry();

		#endregion

		protected virtual string GetBusinessRegistrationCode() => null;
		protected virtual string GetConsumptionTaxRegistrationCode() => null;
		protected virtual string GetConsumptionTaxCode() => null;
		protected virtual string GetComplianceSequencePrefixErrorMessage() => null;
		protected virtual string GetComplianceSequencePrefixRegex() => null;
		protected virtual string GetLocalBusinessRegNoCodeType() => null;
		protected virtual string GetRecipientLocalBusinessRegNumberCodeType() => null;
		protected virtual string GetRecipientLocalBusinessReg2NumberCodeType() => null;
		protected virtual string GetRecipientLocalBusinessRegHeading() => null;
		protected virtual string GetRecipientLocalBusinessReg2Heading() => null;
		protected virtual bool? GetIsReciprocal() => null;
		protected virtual bool? GetIsRightHandSideAdressCountry() => null;
		protected virtual string GetRecipientTaxIDHeading() => null;
		protected virtual string GetComplianceVersionNo() => string.Empty;
		protected virtual bool GetIsTransactionSequencingRequired(string ledger, string transactionType) => false;

		#region Registry Item Defaults

		protected virtual bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => null;

		#endregion

		#endregion

		protected virtual bool? HasExtraTaxInfo() => null;
		protected virtual IEnumerable<ICodeDescription> GetExtraTaxTypes() => (HasExtraTaxInfo().HasValue && HasExtraTaxInfo().Value) ? new CodeDescriptionPair[] { new CodeDescriptionPair(AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, GetDescriptionForQCTExtraTaxType()) } : Array.Empty<CodeDescriptionPair>();
		protected virtual string GetExtraTaxDescription(string extraTaxTypeCode) => GetExtraTaxTypes().FirstOrDefault(et => et.Code == extraTaxTypeCode)?.Description ?? string.Empty;
		protected virtual ResourceStringData GetExtraTaxOSAmountCaption() => ResourceStringData.Empty;
		protected virtual ResourceStringData GetExtraTaxLocalAmountCaption() => ResourceStringData.Empty;
		protected virtual ResourceStringData GetTaxOSAmountCaption() => ResourceStringData.Empty;
		protected virtual ResourceStringData GetTaxLocalAmountCaption() => ResourceStringData.Empty;
		protected virtual string GetDescriptionForQCTExtraTaxType() => Res.GetString("6F87029C-114F-40A1-B7E1-449DDB5EDA52", "QST");

		#region Static

		public static bool GetIsRightHandSideAdressCountry(string countryCode) =>
			CountryComplianceFactory.GetICountryComplianceInfo(countryCode)?.GetIsRightHandSideAdressCountry() ?? false;

		public static string GetLocalBusinessRegNoCodeType(string countryCode)
		{
			var result = CountryComplianceFactory.GetICountryComplianceInfo(countryCode)?.GetLocalBusinessRegNoCodeType();
			if (result != null)
			{
				return result;
			}

			throw new NotSupportedException($"Country {countryCode} not supported for GetLocalBusinessRegNoCodeType");
		}

		#endregion
	}
}
