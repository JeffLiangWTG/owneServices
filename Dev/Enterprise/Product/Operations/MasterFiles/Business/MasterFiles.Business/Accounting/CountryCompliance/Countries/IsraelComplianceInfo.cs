using System;
using System.Security.Cryptography;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class IsraelComplianceInfo : CountryComplianceInfo,
		IComplianceInfoElectronicInvoicing,
		IEInvoiceCredentialsProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Israel;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccTransactionHeaderSchema.Constants.AH_GovernmentAllocatedID;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Israel;

		#endregion

		#region IEInvoiceCredentialsProvider

		bool IEInvoiceCredentialsProvider.ShouldShowCredentialsTab(ICompany company)
		{
			var complianceDate = AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.GetValueWithoutFallback(company.PK, Guid.Empty, Guid.Empty);
			var isComplianceDateReached = complianceDate != DateTime.MinValue && complianceDate <= ZDateTime.Today;

			return isComplianceDateReached;
		}

		bool IEInvoiceCredentialsProvider.ShouldShowCertificatesTab(ICompany company) => false;

		string IEInvoiceCredentialsProvider.GetAuthorizationURL(ICompany company)
		{
			var oauthWebUrl = AccountingMasterFilesRegistry.Instance.IsraelEInvoicingITAOAuthWebURL.Value;
			var callbackSiteWebUrl = AccountingMasterFilesRegistry.Instance.IsraelEInvoicingWTCCallbackSiteWebURL.Value;
			var credentials = AccountingMasterFilesRegistry.Instance.IsraelEInvoicingCredentials.Value;

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseKey = registrationKey.EnterpriseCode + company.Code + registrationKey.ServerCode;
			var environment = Env.Instance.IsProductionSystem ? "PROD" : "TEST";
			var state = $"{enterpriseKey}.{credentials.ClientId}.{credentials.ClientSecret}.{environment}";

			var aesEncryptionKey = ObjectFactory.Get<IAccounting>().RSADecrypt(AccountingMasterFilesRegistry.Instance.IsraelEncryptionKey.Value);
			var aesCrypto = new AESCrypto(HashAlgorithmName.SHA256);
			var encryptedState = aesCrypto.EncryptStringAES(state, aesEncryptionKey);

			var launchUrl =
				$"{oauthWebUrl}?response_type=code&scope=scope&client_id={credentials.ClientId}&redirect_uri={callbackSiteWebUrl}&state={encryptedState}";

			return launchUrl;
		}

		void IEInvoiceCredentialsProvider.CreateOrUpdateEInvoicingCredential(ICompany company)
			=> OAuthHelper.CreateOrUpdateEInvoicingCredential(company);

		EInvoiceOAuthData IEInvoiceCredentialsProvider.CreateOAuthData(ICompany company)
			=> OAuthHelper.CreateOAuthData(company);

		#endregion
	}
}
