using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCusAccount))]
	sealed class OrgCusAccountTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCodeList()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var codeList = orgCusAccount.Lookups.CodeList.OfType<CodeDescriptionPair>().ToArray();
			AssertEquals("Empty Lookups.CodeList", 0, codeList.Length);
		}

		public void TestIssuerList()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var issuerList = orgCusAccount.Lookups.IssuerList.OfType<CodeDescriptionPair>().ToArray();
			AssertEquals("Empty Lookups.IssuerList", 0, issuerList.Length);
		}

		public void TestReportingPeriodList()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var reportingPeriod = orgCusAccount.Lookups.ReportingPeriodList.OfType<CodeDescriptionPair>().ToArray();
			AssertEquals("Empty Lookups.IssuerList", 0, reportingPeriod.Length);
		}

		public void TestProviderIsCorrectType()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			AssertType<OrgCusAccountProvider>(orgCusAccount.Provider);
		}

		public void TestProviderIsCached()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			AssertSame(orgCusAccount.Provider, orgCusAccount.Provider);
		}

		public void TestProviderInstanceIsChangedWhenCountryChanges()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var eritreaProvider = orgCusAccount.Provider;
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Vanuatu;
			var vanuatuProvider = orgCusAccount.Provider;
			AssertEquals("Country change causes provider to create new instance", false, ReferenceEquals(eritreaProvider, vanuatuProvider));
		}

		public void TestLookupsNotCached()
		{
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = "XXX";//A generic Provider is used
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;//DE generic Provider is used
			orgCusAccount.CZ_Code = "XXX";//This is invalid in DE. A new instance of Lookups should have been created
			AssertHasErrorContaining(orgCusAccount.CZ_CodeInfo, ListValidation.InvalidCodeError);
		}
	}
}
