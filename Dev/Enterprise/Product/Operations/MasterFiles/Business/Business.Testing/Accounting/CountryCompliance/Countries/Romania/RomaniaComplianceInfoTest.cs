using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(RomaniaComplianceInfo))]
	sealed class RomaniaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Romania;

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Romania;

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate => IsProductionSystem ? new ZDate(2024, 04, 01) : new ZDate(2024, 01, 01);

		#region IEInvoiceCredentialsProvider

		protected override string ExpectedOAuthEncryptionKey => AccountingMasterFilesRegistry.Instance.RomaniaEncryptionKey.Value;

		protected override bool ExpectedOAuthShouldShowCredentialsTab => true;

		protected override string ExpectedOAuthAuthorizationURLInProd
			=> "https://example.com/?response_type=code&client_id=DemoProdId&redirect_uri=https://www.wisetechglobal.com/prod/callback&token_content_type=jwt&state=EDIABCDAT.DemoProdId.DemoProdSecret";

		protected override string ExpectedOAuthAuthorizationURLInTest => ExpectedOAuthAuthorizationURLInProd;

		protected override ICompany ExpectedOAuthCompany()
		{
			var mockCompany = new Mock<ICompany>();
			mockCompany.Setup(x => x.Code).Returns("ABC");
			mockCompany.Setup(x => x.PK).Returns(GlbCompany.CurrentCompany.PK.ToGuid());

			return mockCompany.Object;
		}

		public override void TestGetAuthorizationURL()
		{
			InitializeRegistry();
			base.TestGetAuthorizationURL();
		}

		public void TestGetCallbackServerURL()
		{
			var romaniaComplianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(CountryCode) as RomaniaComplianceInfo;
			InitializeRegistry();
			using (SetUpIProductRegistration(isUAT: false, isDeveloper: false))
			{
				AssertEquals("https://www.wisetechglobal.com/prod/callback", romaniaComplianceInfo.GetOAuthCallbackServerURL());
			}
			using (SetUpIProductRegistration(isUAT: true, isDeveloper: false))
			{
				AssertEquals("https://www.wisetechglobal.com/test/callback", romaniaComplianceInfo.GetOAuthCallbackServerURL());
			}
			using (SetUpIProductRegistration(isUAT: false, isDeveloper: true))
			{
				AssertEquals("https://www.wisetechglobal.com/test/callback", romaniaComplianceInfo.GetOAuthCallbackServerURL());
			}
		}

		public void TestGetEInvoicingCredentialsRegistryItem()
		{
			var romaniaComplianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(CountryCode) as RomaniaComplianceInfo;
			InitializeRegistry();
			using (SetUpIProductRegistration(isUAT: false, isDeveloper: false))
			{
				AssertRegistryItem("DemoProdId", "DemoProdSecret", "Accounting -> E-Reporting and E-Invoicing Configurations -> Romania (RO) -> Production E-Invoicing Credentials (CargoWiseOne Support Only)", romaniaComplianceInfo.GetEInvoicingCredentialsRegistryItem());
			}
			using (SetUpIProductRegistration(isUAT: true, isDeveloper: false))
			{
				AssertRegistryItem("DemoTestId", "DemoTestSecret", "Accounting -> E-Reporting and E-Invoicing Configurations -> Romania (RO) -> Test E-Invoicing Credentials (CargoWiseOne Support Only)", romaniaComplianceInfo.GetEInvoicingCredentialsRegistryItem());
			}
			using (SetUpIProductRegistration(isUAT: false, isDeveloper: true))
			{
				AssertRegistryItem("DemoTestId", "DemoTestSecret", "Accounting -> E-Reporting and E-Invoicing Configurations -> Romania (RO) -> Test E-Invoicing Credentials (CargoWiseOne Support Only)", romaniaComplianceInfo.GetEInvoicingCredentialsRegistryItem());
			}
		}

		void AssertRegistryItem(string clientId, string clientSecret, string location, EInvoicingCredentialsRegistryItem registryItem)
		{
			AssertEquals(clientId, registryItem.Value.ClientId);
			AssertEquals(clientSecret, registryItem.Value.ClientSecret);
			AssertEquals(location, registryItem.Location());
		}

		void InitializeRegistry()
		{
			AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingANAFOauthWebURL.SetValue(companyPK: Guid.Empty, branchPK: Guid.Empty, departmentPK: Guid.Empty, "https://example.com/");
			AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingTestWTCCallbackWebSiteURL.SetValue(companyPK: Guid.Empty, branchPK: Guid.Empty, departmentPK: Guid.Empty, "https://www.wisetechglobal.com/test/callback");
			AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingProductionWTCCallbackWebSiteURL.SetValue(companyPK: Guid.Empty, branchPK: Guid.Empty, departmentPK: Guid.Empty, "https://www.wisetechglobal.com/prod/callback");
			AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingTestCredentials.SetValue(companyPK: Guid.Empty, branchPK: Guid.Empty, departmentPK: Guid.Empty, new EInvoicingCredentials
			{
				ClientId = "DemoTestId",
				ClientSecret = "DemoTestSecret"
			});
			AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingProductionCredentials.SetValue(companyPK: Guid.Empty, branchPK: Guid.Empty, departmentPK: Guid.Empty, new EInvoicingCredentials
			{
				ClientId = "DemoProdId",
				ClientSecret = "DemoProdSecret"
			});
		}

		IDisposable SetUpIProductRegistration(bool isUAT, bool isDeveloper)
		{
			var productRegistrationKey = new Mock<IProductRegistrationKey>();
			productRegistrationKey
				.Setup(x => x.EnterpriseCode)
				.Returns("RRO");
			productRegistrationKey
				.Setup(x => x.ServerCode)
				.Returns("AAA");

			var productRegistration = new Mock<IProductRegistration>();
			productRegistration
				.Setup(x => x.Key)
				.Returns(productRegistrationKey.Object);
			productRegistration
				.Setup(x => x.IsWiseTechGlobalInternalUATSystem())
				.Returns(isUAT);
			productRegistration
				.Setup(x => x.IsWiseTechGlobalInternalDeveloperSystem())
				.Returns(isDeveloper);
			return ObjectFactory.Substitute<IProductRegistration>(productRegistration.Object);
		}

		#endregion
	}
}
