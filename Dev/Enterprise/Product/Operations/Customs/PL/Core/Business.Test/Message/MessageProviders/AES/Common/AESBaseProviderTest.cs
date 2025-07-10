using System;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.PL.Business.Testing;

abstract class AESBaseProviderTest<TDataProvider> : Customs.Business.Testing.DataProviderTestCase<TDataProvider>
	where TDataProvider : class, IAESBase
{
	public abstract void TestConstructor();

	public void TestMessageSender()
	{
		const string companyCountry = CountryCodes.Australia;
		const string branchCountry = CountryCodes.NewZealand;

		var testCases = new[]
		{
			new { description = "Empty MessageSender", companyRegNo = "", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = string.Empty },
			new { description = "Empty MessageSender - without branch", companyRegNo = "", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = true, expectedRegNo = string.Empty },
			new { description = "Empty MessageSender - without company", companyRegNo = "", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = true, branchIsEmpty = false, expectedRegNo = string.Empty },
			new { description = "Empty MessageSender - without branch and company", companyRegNo = "", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = true, branchIsEmpty = true, expectedRegNo = string.Empty },

			new { description = "GlbCompany MessageSender", companyRegNo = "111", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = companyCountry + "111" },
			new { description = "GlbCompany MessageSender - without branch", companyRegNo = "222", branchRegNo = "", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = true, expectedRegNo = companyCountry + "222" },

			new { description = "GlbBranch MessageSender", companyRegNo = "", branchRegNo = "333", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = branchCountry + "333" },
			new { description = "GlbBranch MessageSender - without company", companyRegNo = "", branchRegNo = "444", countryCode = string.Empty, companyIsEmpty = true, branchIsEmpty = false, expectedRegNo = branchCountry + "444" },
			new { description = "GlbBranch MessageSender - ignore company", companyRegNo = "666", branchRegNo = "555", countryCode = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = branchCountry + "555" },

			new { description = "Company PL MessageSender", companyRegNo = "777", branchRegNo = "", countryCode = CountryCodes.Poland, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = CountryCodes.Poland + "777" },
			new { description = "Branch PL MessageSender", companyRegNo = "", branchRegNo = "888", countryCode = CountryCodes.Poland, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = CountryCodes.Poland + "888" },
			new { description = "Branch DE MessageSender", companyRegNo = "", branchRegNo = "999", countryCode = CountryCodes.Germany, companyIsEmpty = false, branchIsEmpty = false, expectedRegNo = CountryCodes.Germany + "999" },
		};

		var branch = CreateNewGlbBranchAndCompanyWithSeparateOrgHeaders(companyCountry, branchCountry);

		string GetCountryCode(string code, string fallbackCode) => !string.IsNullOrEmpty(code) ? code : fallbackCode;

		const string codeEORI = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		CombineAssertions(() =>
		{
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
						GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(codeEORI, testCase.companyRegNo, GetCountryCode(testCase.countryCode, companyCountry));
					}

					GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();
					if (testCase.branchIsEmpty)
					{
						GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
					}
					else if (!string.IsNullOrEmpty(testCase.branchRegNo))
					{
						GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(codeEORI, testCase.branchRegNo, GetCountryCode(testCase.countryCode, branchCountry));
					}

					var actualRegNo = GetProvider().MessageSender;

					AssertEquals(testCase.description, testCase.expectedRegNo, actualRegNo);
				}
			}
		});
	}

	public void TestOfficeIdentifier()
	{
		var testCases = new[]
		{
			new { descripton = "Empty OfficeIdentifier", companyOfficeId = string.Empty, branchOfficeId = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedOfficeId = string.Empty },
			new { descripton = "Empty OfficeIdentifier - without company", companyOfficeId = string.Empty, branchOfficeId = string.Empty, companyIsEmpty = true, branchIsEmpty = false, expectedOfficeId = string.Empty },
			new { descripton = "Empty OfficeIdentifier - without branch", companyOfficeId = string.Empty, branchOfficeId = string.Empty, companyIsEmpty = false, branchIsEmpty = true, expectedOfficeId = string.Empty },
			new { descripton = "Empty OfficeIdentifier - without company and branch", companyOfficeId = string.Empty, branchOfficeId = string.Empty, companyIsEmpty = true, branchIsEmpty = true, expectedOfficeId = string.Empty },

			new { descripton = "GlbCompany OfficeIdentifier", companyOfficeId = "11", branchOfficeId = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedOfficeId = "11" },
			new { descripton = "GlbCompany OfficeIdentifier - without branch", companyOfficeId = "22", branchOfficeId = string.Empty, companyIsEmpty = false, branchIsEmpty = true, expectedOfficeId = "22" },
			new { descripton = "GlbBranch OfficeIdentifier", companyOfficeId = string.Empty, branchOfficeId = "99", companyIsEmpty = false, branchIsEmpty = false, expectedOfficeId = "99" },
			new { descripton = "GlbBranch OfficeIdentifier - without company", companyOfficeId = string.Empty, branchOfficeId = "88", companyIsEmpty = true, branchIsEmpty = false, expectedOfficeId = "88" },
			new { descripton = "GlbBranch OfficeIdentifier - ignore company", companyOfficeId = "33", branchOfficeId = "77", companyIsEmpty = false, branchIsEmpty = false, expectedOfficeId = "77" },

			new { descripton = "GlbCompany OfficeIdentifier with length < 2", companyOfficeId = "1", branchOfficeId = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedOfficeId = string.Empty },
			new { descripton = "GlbBranch OfficeIdentifier with length < 2", companyOfficeId = string.Empty, branchOfficeId = "9", companyIsEmpty = false, branchIsEmpty = false, expectedOfficeId = string.Empty },
			new { descripton = "GlbBranch OfficeIdentifier with length < 2 - ignore company", companyOfficeId = "33", branchOfficeId = "7", companyIsEmpty = false, branchIsEmpty = false, expectedOfficeId = string.Empty },

			new { descripton = "GlbCompany OfficeIdentifier with length > 2", companyOfficeId = "111", branchOfficeId = string.Empty, companyIsEmpty = false, branchIsEmpty = false, expectedOfficeId = string.Empty },
			new { descripton = "GlbBranch OfficeIdentifier with length > 2", companyOfficeId = string.Empty, branchOfficeId = "999", companyIsEmpty = false, branchIsEmpty = false, expectedOfficeId = string.Empty },
			new { descripton = "GlbBranch OfficeIdentifier with length > 2 - ignore company", companyOfficeId = "33", branchOfficeId = "777", companyIsEmpty = false, branchIsEmpty = false, expectedOfficeId = string.Empty },
		};

		const string plCode = CountryCodes.Poland;
		const string codeEDI = OrgCusCode.CodeTypes.EDISiteID;

		var branch = CreateNewGlbBranchAndCompanyWithSeparateOrgHeaders();

		CombineAssertions(() =>
		{
			foreach (var testCase in testCases)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
					if (testCase.companyIsEmpty)
					{
						GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
					}
					else if (!string.IsNullOrEmpty(testCase.companyOfficeId))
					{
						GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(codeEDI, testCase.companyOfficeId, plCode);
					}

					GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();
					if (testCase.branchIsEmpty)
					{
						GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
					}
					else if (!string.IsNullOrEmpty(testCase.branchOfficeId))
					{
						GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(codeEDI, testCase.branchOfficeId, plCode);
					}

					var actualOfficeId = GetProvider().OfficeIdentifier;

					AssertEquals(testCase.descripton, testCase.expectedOfficeId, actualOfficeId);
				}
			}
		});
	}

	public void TestPreparationDateAndTime() => AssertNotEquals(DateTime.Today, GetProvider().PreparationDateAndTime);

	public void TestMessageRecipient() => AssertEquals(Constants.AESMessageProvidersConstants.MessageRecipient, GetProvider().MessageRecipient);

	public void TestMessageIdentification() => AssertEquals(EDIMessage.PLMessageNumberPlaceHolder, GetProvider().MessageIdentification);

	public void TestCorrelationIdentifier() => AssertNull(GetProvider().CorrelationIdentifier);

	public void TestOperatorEmail()
	{
		var communicationChannel = GlbStaff.CurrentUser.Factory.New<CommunicationChannel>();
		communicationChannel.GP_PasswordType = PasswordTypesList.Codes.PLC;
		communicationChannel.GP_Name = "CommunicationChannel";
		communicationChannel.GP_GS = GlbStaff.CurrentUser.PK;
		communicationChannel.GP_GC = GlbCompany.CurrentCompany.PK;

		CombineAssertions(() =>
		{
			AssertNull("Not configured", GetProvider().OperatorEmail);

			using (PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test"))
			{
				AssertEquals("Empty staff communication channel, should use Registry data", "Test", GetProvider().OperatorEmail);

				communicationChannel.GP_MailBoxID = "something@email.com";

				AssertEquals("Staff Communication Channel have priority over Registry data", "something@email.com", GetProvider().OperatorEmail);
			}
		});
	}

	public void TestMessageType() => AssertEquals(ExpectedMessageType, GetProvider().MessageType);

	protected abstract string ExpectedMessageType { get; }

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		entryHeader = declaration.CustomsEntryHeaders.Single();

		sendingObject = new BaseMessageSendingObject(entryHeader);
	}
	protected CusEntryHeader entryHeader;
	protected JobDeclaration declaration;
	protected CusEntryInstruction instruction;
	protected BaseMessageSendingObject sendingObject;
	protected BaseMessageSendingObjectParent sendingObjectParent;

	GlbBranch CreateNewGlbBranchAndCompanyWithSeparateOrgHeaders(string companyCountry = CountryCodes.Poland, string branchCountry = CountryCodes.Poland)
	{
		ZGuid CreateOrgHeader(string code)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = code;
			orgHeader.Addresses.AddNew().OA_Address1 = "Test address";
			return orgHeader.PK;
		}

		var company = Factory.New<GlbCompany>();
		company.GC_RN_NKCountryCode = companyCountry;
		company.GC_OH_OrgProxy = CreateOrgHeader("Test1");

		var branch = company.Branches.AddNew();
		branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, branchCountry)).Code;
		branch.GB_OH_OrgProxy = CreateOrgHeader("Test2");

		Factory.Save();
		return branch;
	}
}
