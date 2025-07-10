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
	public class RomaniaComplianceInfo : CountryComplianceInfo, IEInvoiceCredentialsProvider, IComplianceInfoElectronicInvoicing
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Romania;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.RomaniaCodeTypes.TVA;
		protected override string GetConsumptionTaxCode() => OrgCusCode.RomaniaCodeTypes.TVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => Env.Instance.IsProductionSystem ? new ZDate(2024, 04, 01) : new ZDate(2024, 01, 01);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Romania;

		#endregion

		#region IEInvoiceCredentialsProvider

		bool IEInvoiceCredentialsProvider.ShouldShowCredentialsTab(ICompany company) => true;

		bool IEInvoiceCredentialsProvider.ShouldShowCertificatesTab(ICompany company) => false;

		string IEInvoiceCredentialsProvider.GetAuthorizationURL(ICompany company)
		{
			var oauthWebUrl = AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingANAFOauthWebURL.Value;
			var callbackSiteWebUrl = GetOAuthCallbackServerURL();
			var clientId = GetEInvoicingCredentialsRegistryItem().Value.ClientId;
			var clientSecret = GetEInvoicingCredentialsRegistryItem().Value.ClientSecret;

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseKey = registrationKey.EnterpriseCode + company.Code + registrationKey.ServerCode;
			var state = $"{enterpriseKey}.{clientId}.{clientSecret}";

			var aesEncryptionKey = ObjectFactory.Get<IAccounting>().RSADecrypt(AccountingMasterFilesRegistry.Instance.RomaniaEncryptionKey.Value);
			var aesCrypto = new AESCrypto(HashAlgorithmName.SHA256);
			var encryptedState = aesCrypto.EncryptStringAES(state, aesEncryptionKey);

			var launchUrl =
				$"{oauthWebUrl}?response_type=code&client_id={clientId}&redirect_uri={callbackSiteWebUrl}&token_content_type=jwt&state={encryptedState}";

			return launchUrl;
		}

		void IEInvoiceCredentialsProvider.CreateOrUpdateEInvoicingCredential(ICompany company)
			=> OAuthHelper.CreateOrUpdateEInvoicingCredential(company);

		EInvoiceOAuthData IEInvoiceCredentialsProvider.CreateOAuthData(ICompany company)
			=> OAuthHelper.CreateOAuthData(company);

		#endregion

		#region Helpers

		public EInvoicingCredentialsRegistryItem GetEInvoicingCredentialsRegistryItem()
		{
			if (isDeveloperOrUATSystem)
			{
				return AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingTestCredentials;
			}

			return AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingProductionCredentials;
		}

		internal string GetOAuthCallbackServerURL()
		{
			if (isDeveloperOrUATSystem)
			{
				return AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingTestWTCCallbackWebSiteURL.Value;
			}

			return AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingProductionWTCCallbackWebSiteURL.Value;
		}

		bool isDeveloperOrUATSystem => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalDeveloperSystem() || ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalUATSystem();

		#endregion
	}
}
