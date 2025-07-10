using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class CC507RootProviderTest : DataProviderTestCase<CC507RootProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var expectedMessage = string.Empty;

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitReport";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitReport')";
#endif

		AssertExceptionThrown<ArgumentNullException>("Null CusExitReport", expectedMessage,
			() => new CC507RootProvider(null));
	});

	public void TestMessageType() => AssertEquals(Constants.MessageType.ExitControl.CC507C, Provider.MessageType);

	public void TestExportOperation() => AssertNotNull(Provider.ExportOperation);

	public void TestAuthorisationNumbers() => AssertEquals(0, Provider.AuthorisationNumbers.Count);

	public void TestCustomsOfficeOfExitActualReferenceNumber() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("empty", GetProvider().CustomsOfficeOfExitActualReferenceNumber);

		cusExitReport.CER_OfficeOfExit = "PL1234";
		AssertEquals("not empty", "PL1234", GetProvider().CustomsOfficeOfExitActualReferenceNumber);
	});

	public void TestGoodsShipment() => AssertNotNull(Provider.GoodsShipment);

	public void TestExporter() => AssertNull(Provider.Exporter);

	public void TestDeclarant() => AssertNull(Provider.Declarant);

	public void TestRepresentative() => AssertNull(Provider.Representative);

	public void TestMessageSender() => CombineAssertions(() =>
	{
		var provider = new CC507RootProvider(jobDeclarationExitHeader.CusExitReports.AddNew());
		AssertEquals("Part of JobDeclaration", Constants.AESMessageProvidersConstants.MessageRecipient, provider.MessageSender);

		TestMessageSenderMessageRecipient(() => GetProvider().MessageSender);
	});

	public void TestMessageRecipient() => CombineAssertions(() =>
	{
		AssertEquals("Part of JobDeclaration", Constants.AESMessageProvidersConstants.MessageRecipient, new CC507RootProvider(jobDeclarationExitHeader.CusExitReports.AddNew()).MessageRecipient);

		TestMessageSenderMessageRecipient(() => GetProvider().MessageRecipient);
	});

	void TestMessageSenderMessageRecipient(Func<string> propertyGetter)
	{
		var testCases = new[]
		{
			new { description = "Empty MessageSender", companyRegNo = "", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = string.Empty },
			new { description = "Empty MessageSender - without branch", companyRegNo = "", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = true, expectedRegNo = string.Empty },
			new { description = "Empty MessageSender - without company", companyRegNo = "", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = true, branchIsEmpty = false, expectedRegNo = string.Empty },
			new { description = "Empty MessageSender - without branch and company", companyRegNo = "", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = true, branchIsEmpty = true, expectedRegNo = string.Empty },

			new { description = "GlbCompany MessageSender", companyRegNo = "111", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = defaultCompanyCountry + "111" },
			new { description = "GlbCompany MessageSender - without branch", companyRegNo = "222", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = true, expectedRegNo = defaultCompanyCountry + "222" },

			new { description = "GlbBranch MessageSender", companyRegNo = "", branchRegNo = "333", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = defaultBranchCountry + "333" },
			new { description = "GlbBranch MessageSender - without company", companyRegNo = "", branchRegNo = "444", countryCode = string.Empty, companyIsEmpty = true, branchIsEmpty = false, expectedRegNo = defaultBranchCountry + "444" },
			new { description = "GlbBranch MessageSender - ignore company", companyRegNo = "666", branchRegNo = "555", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = defaultBranchCountry + "555" },

			new { description = "Company PL MessageSender", companyRegNo = "777", branchRegNo = "", countryCode = CountryCodes.Poland, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = CountryCodes.Poland + "777" },
			new { description = "Branch PL MessageSender", companyRegNo = "", branchRegNo = "888", countryCode = CountryCodes.Poland, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = CountryCodes.Poland + "888" },
			new { description = "Branch DE MessageSender", companyRegNo = "", branchRegNo = "999", countryCode = CountryCodes.Germany, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = CountryCodes.Germany + "999" },
		};
		const string codeEORI = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;

		foreach (var testCase in testCases)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
				if (testCase.companyIsEmpty)
				{
					GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				}
				else if (!string.IsNullOrEmpty(testCase.companyRegNo))
				{
					GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(codeEORI, testCase.companyRegNo, GetCountryCode(testCase.countryCode, defaultCompanyCountry));
				}

				GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();
				if (testCase.branchIsEmpty)
				{
					GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				}
				else if (!string.IsNullOrEmpty(testCase.branchRegNo))
				{
					GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(codeEORI, testCase.branchRegNo, GetCountryCode(testCase.countryCode, defaultBranchCountry));
				}

				var actualRegNo = propertyGetter.Invoke();

				AssertEquals(testCase.description, testCase.expectedRegNo, actualRegNo);
			}
		}

		string GetCountryCode(string code, string fallbackCode) => !string.IsNullOrEmpty(code) ? code : fallbackCode;
	}

	[TestDate(2024, 10, 1, 1, 30, 0)]
	public void TestPreparationDateAndTime() => AssertEquals(ZDateTime.UtcNow.ToDateTime(), Provider.PreparationDateAndTime);

	public void TestOperatorEmail() => AssertNull(Provider.OperatorEmail);

	public void TestOfficeIdentifier() => AssertNull(Provider.OfficeIdentifier);

	public void TestMessageIdentification() => AssertEquals(BaseEDIMessage.PLMessageNumberPlaceHolder, Provider.MessageIdentification);

	public void TestCorrelationIdentifier() => AssertNull(Provider.CorrelationIdentifier);

	protected override CC507RootProvider GetProvider() => new CC507RootProvider(cusExitReport);

	protected override void SetUp()
	{
		base.SetUp();

		branch = CreateNewGlbBranchAndCompanyWithSeparateOrgHeaders(defaultCompanyCountry, defaultBranchCountry);

		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclarationExitHeader = Factory.New<CusExitHeader>();
		Factory.Save();
		jobDeclarationExitHeader.Parent = jobDeclaration;

		cusExitHeader = Factory.New<CusExitHeader>();
		cusExitConsignment = cusExitHeader.CusExitConsignments.AddNew();
		cusExitReport = cusExitHeader.CusExitReports.AddNew();
		cusExitReport.CER_CXC_Consignment = cusExitConsignment.PK;
	}

	CusExitHeader cusExitHeader;
	CusExitConsignment cusExitConsignment;
	CusExitReport cusExitReport;
	CusExitHeader jobDeclarationExitHeader;
	GlbBranch branch;
	const string defaultCompanyCountry = CountryCodes.Australia;
	const string defaultBranchCountry = CountryCodes.NewZealand;

	GlbBranch CreateNewGlbBranchAndCompanyWithSeparateOrgHeaders(string companyCountry = CountryCodes.Poland, string branchCountry = CountryCodes.Poland)
	{
		var company = Factory.New<GlbCompany>();
		company.GC_RN_NKCountryCode = companyCountry;
		company.GC_OH_OrgProxy = CreateOrgHeader("Test1");

		var branch = company.Branches.AddNew();
		branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, branchCountry)).Code;
		branch.GB_OH_OrgProxy = CreateOrgHeader("Test2");

		Factory.Save();
		return branch;

		ZGuid CreateOrgHeader(string code)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = code;
			orgHeader.Addresses.AddNew().OA_Address1 = "Test address";
			return orgHeader.PK;
		}
	}
}
