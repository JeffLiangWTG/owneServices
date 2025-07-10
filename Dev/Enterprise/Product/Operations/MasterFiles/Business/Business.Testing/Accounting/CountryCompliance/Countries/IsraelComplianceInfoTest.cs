using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(IsraelComplianceInfo))]
	sealed class IsraelComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Israel;

		protected override ZDate ExpectedEInvoicingComplianceDate => ZDate.Empty;

		protected override string ExpectedGovernmentAllocatedNumberColumnName => AccTransactionHeaderSchema.Constants.AH_GovernmentAllocatedID;

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Israel;

		protected override ZString ExpectedDefaultEInvoicingSubmitPivotStatus => Constants.EInvoicingPivotState.Queued;

		protected override ZString ExpectedDefaultEInvoicingPivotPendingStatusDescription => (NoResString)"Pending User Action";

		#region IEInvoiceCredentialsProvider

		protected override bool ExpectedOAuthShouldShowCredentialsTab => true;

		protected override string ExpectedOAuthEncryptionKey => AccountingMasterFilesRegistry.Instance.IsraelEncryptionKey.Value;

		const string ExpectedOAuthAuthorizationURL
			= "https://example.com/?response_type=code&scope=scope&client_id=0819a6689265e8db236e6e37c6237e8a7e3ee71dc75c6765&redirect_uri=https://www.wisetechglobal.com/callback&state=EDIABCDAT.0819a6689265e8db236e6e37c6237e8a7e3ee71dc75c6765.9c5ae51d64866e513c873c1b7d5d085fa66d49ea9f407e8a7e3ee71dc75c6765.";

		protected override string ExpectedOAuthAuthorizationURLInProd => ExpectedOAuthAuthorizationURL + "PROD";

		protected override string ExpectedOAuthAuthorizationURLInTest => ExpectedOAuthAuthorizationURL + "TEST";

		public override void TestShouldShowCredentialsTab()
		{
			var eInvoiceCredentialsProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(CountryCode) as IEInvoiceCredentialsProvider;
			AssertNotNull(eInvoiceCredentialsProvider);

			var pastComplianceDate = ZDateTime.Today.AddDays(-1).ToDateTime();
			AssertShouldShowCredentialsTab(eInvoiceCredentialsProvider, pastComplianceDate, shouldShow: true);

			var todayIsComplianceDate = ZDateTime.Today.ToDateTime();
			AssertShouldShowCredentialsTab(eInvoiceCredentialsProvider, todayIsComplianceDate, shouldShow: true);

			var futureComplianceDate = ZDateTime.Today.AddDays(1).ToDateTime();
			AssertShouldShowCredentialsTab(eInvoiceCredentialsProvider, futureComplianceDate, shouldShow: false);
		}

		void AssertShouldShowCredentialsTab(IEInvoiceCredentialsProvider eInvoiceCredentialsProvider, DateTime complianceDate, bool shouldShow)
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, branchPk: Guid.Empty, departmentPk: Guid.Empty, temporaryValue: complianceDate))
			{
				AssertEquals(shouldShow, eInvoiceCredentialsProvider.ShouldShowCredentialsTab(Env.CurrentCompany));
			}
		}

		public override void TestGetAuthorizationURL()
		{
			AccountingMasterFilesRegistry.Instance.IsraelEInvoicingITAOAuthWebURL.SetValue(companyPK: Guid.Empty, branchPK: Guid.Empty, departmentPK: Guid.Empty, "https://example.com/");
			AccountingMasterFilesRegistry.Instance.IsraelEInvoicingWTCCallbackSiteWebURL.SetValue(companyPK: Guid.Empty, branchPK: Guid.Empty, departmentPK: Guid.Empty, "https://www.wisetechglobal.com/callback");
			AccountingMasterFilesRegistry.Instance.IsraelEInvoicingCredentials.SetValue(companyPK: Guid.Empty, branchPK: Guid.Empty, departmentPK: Guid.Empty, new EInvoicingCredentials
			{
				ClientId = "0819a6689265e8db236e6e37c6237e8a7e3ee71dc75c6765",
				ClientSecret = "9c5ae51d64866e513c873c1b7d5d085fa66d49ea9f407e8a7e3ee71dc75c6765"
			});

			base.TestGetAuthorizationURL();
		}

		#endregion
	}
}
