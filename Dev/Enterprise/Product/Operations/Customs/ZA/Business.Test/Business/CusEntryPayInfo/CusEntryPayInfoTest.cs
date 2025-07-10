using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryPayInfo))]
	sealed class CusEntryPayInfoTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var entryPayInfo = (CusEntryPayInfo)GetNewBusinessObject();
			entryPayInfo.EntryHeader.CH_BGMReference = "00505655GRB20170111003761";

			AssertEquals("Proof of Payment 00505655GRB20170111003761", entryPayInfo.HumanReadableName);
		}

		public void TestPropertyAccess()
		{
			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_ParentTable = "CusEntryHeader";
			entryNum.CE_ParentID = header.PK;
			entryNum.CE_EntryNum = mRNumber;
			entryNum.CE_EntryType = "MRN";
			entryNum.CE_Category = "CUS";
			entryNum.CE_IssueDate = ZDateTime.Today;
			entryNum.CE_EntryIsSystemGenerated = true;

			var payInfo = Factory.New<CusEntryPayInfo>();
			payInfo.C9_CH = header.PK;
			Factory.Save();

			AssertEquals("Job Number", declaration.JE_DeclarationReference, payInfo.JobNumber);
			AssertEquals("Customs Office", customsOffice, payInfo.CustomsOffice);
			AssertEquals("Financial Account Number", fANumber, payInfo.FANumber);
			AssertEquals("Importer", importerCode, payInfo.Importer.OH_Code);
			AssertEquals("Local Reference Number", lRNumber, payInfo.LRNumber);
			AssertEquals("Movement Reference Number", mRNumber, payInfo.MRNumber);

			declaration.JE_AgentsReference = "123";
			AssertEquals("Agents Reference", declaration.JE_AgentsReference, payInfo.AgentsReference);
		}

		public void TestTotalVat()
		{
			var payInfo1 = Factory.New<CusEntryPayInfo>();
			payInfo1.C9_CH = header.PK;
			payInfo1.C9_TransactionType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			payInfo1.C9_PaymentAmount = new ZDecimal(12.1);
			var payInfo2 = Factory.New<CusEntryPayInfo>();
			payInfo2.C9_CH = header.PK;
			payInfo2.C9_TransactionType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			payInfo2.C9_PaymentAmount = new ZDecimal(7.3);
			Factory.Save();

			Assert("Receipt Number 1 Check", payInfo1.C9_PaymentReference.IsEmpty);
			Assert("Receipt Number 2 Check", payInfo2.C9_PaymentReference.IsEmpty);
			AssertEquals("Total VAT 1 Check", ZDecimal.Zero, payInfo1.TotalVat);
			AssertEquals("Total VAT 2 Check", ZDecimal.Zero, payInfo2.TotalVat);

			payInfo1.C9_PaymentReference = "B9272091-FE93-4ABA-B";
			payInfo2.C9_PaymentReference = "B9272091-FE93-4ABA-B";
			Factory.Save();

			var totalVat = new ZDecimal(19.4);
			AssertEquals("Total VAT 1 Check", totalVat, payInfo1.TotalVat);
			AssertEquals("Total VAT 2 Check", totalVat, payInfo2.TotalVat);
		}

		public void TestRelatedARInvoices()
		{
			var zaUniversalReferenceTestDataHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaUniversalReferenceTestDataHelper.CreateCustomsOfficeCusCodeEntry("JHB");

			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
			testCreditor.OH_IsCreditor = true;
			var testCreditor2 = Factory.NewWithValidTestData<OrgHeader>();
			testCreditor2.OH_IsCreditor = true;
			Factory.Save();

			var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
			var mapping1 = agentOfficeToCreditorMappings.AddNew();
			mapping1.OrganizationPK = testAgent.PK;
			mapping1.CustomsOfficeCode = "JHB";
			mapping1.FinancialAccountNumber = "1234567890";
			mapping1.CreditorPK = testCreditor.PK;
			mapping1.ImporterPays = false;
			mapping1.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings);

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_OH_AgentOverride = testAgent.PK;
			dec.JE_CustomsOffice = "JHB";

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var accHeader1 = Factory.NewWithValidTestData<ARInvoice>();
			var accHeader2 = Factory.NewWithValidTestData<ARInvoice>();
			var accHeader3 = Factory.NewWithValidTestData<ARInvoice>();
			var accHeader4 = Factory.NewWithValidTestData<ARInvoice>();
			accHeader1.AH_TransactionNum = "INV0001";
			accHeader2.AH_TransactionNum = "INV0002";
			accHeader3.AH_TransactionNum = "INV0003";
			accHeader4.AH_TransactionNum = "INV0004";

			var accLines1 = Factory.NewWithValidTestData<ARInvoiceLine>();
			var accLines2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			var accLines3 = Factory.NewWithValidTestData<ARInvoiceLine>();
			var accLines4 = Factory.NewWithValidTestData<ARInvoiceLine>();
			accLines1.AL_AH = accHeader1.PK;
			accLines2.AL_AH = accHeader2.PK;
			accLines3.AL_AH = accHeader3.PK;
			accLines4.AL_AH = accHeader4.PK;
			accLines1.AL_AG = glHeader.PK;
			accLines2.AL_AG = glHeader.PK;
			accLines3.AL_AG = glHeader.PK;
			accLines4.AL_AG = glHeader.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = dec.PK;

			var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			var jobCharge2 = Factory.NewWithValidTestData<JobCharge>();
			var jobCharge3 = Factory.NewWithValidTestData<JobCharge>();
			var jobCharge4 = Factory.NewWithValidTestData<JobCharge>();
			var jobCharge5 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge1.JR_JH = jobHeader.PK;
			jobCharge2.JR_JH = jobHeader.PK;
			jobCharge3.JR_JH = jobHeader.PK;
			jobCharge4.JR_JH = jobHeader.PK;
			jobCharge5.JR_JH = jobHeader.PK;

			jobCharge2.JR_AL_ARLine = accLines1.PK;
			jobCharge3.JR_AL_ARLine = accLines2.PK;
			jobCharge4.JR_AL_ARLine = accLines3.PK;
			jobCharge5.JR_AL_ARLine = accLines4.PK;

			jobCharge1.JR_OH_CostAccount = testCreditor.PK;
			jobCharge3.JR_OH_CostAccount = testCreditor.PK;
			jobCharge4.JR_OH_CostAccount = testCreditor.PK;
			jobCharge5.JR_OH_CostAccount = testCreditor2.PK;

			var entry = dec.ActiveEntryHeaders.AddNew();

			Factory.Save();
			var tester = entry.EntryPayInfos.AddNew();

			// note: assigned AH_TransactionNum is overriden in TransactionHeader.OnSavingCore
			string[] expectedInvoiceNumbers = new string[] { accHeader2.AH_TransactionNum, accHeader3.AH_TransactionNum };

			AssertEquals(string.Join(System.Environment.NewLine, expectedInvoiceNumbers.OrderBy(s => s)), tester.RelatedARInvoiceNumbers);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var zaUniversalReferenceTestDataHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaUniversalReferenceTestDataHelper.CreateCustomsOfficeCusCodeEntry(customsOffice);

			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "51234501", Core.Constants.CountryCodes.SouthAfrica);
			testAgent.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "TST", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping = collection.AddNew();
			mapping.OrganizationPK = testAgent.PK;
			mapping.CustomsOfficeCode = customsOffice;
			mapping.FinancialAccountNumber = fANumber;
			mapping.ImporterPays = true;
			mapping.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.OH_Code = importerCode;
			Factory.Save();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_CustomsOffice = customsOffice;
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + UniversalReferenceConstants.ProcedureCodes._00;

			new LineMerger(declaration).DoMerge();
			header = declaration.ActiveEntryHeaders[0];
			header.CH_BGMReference = lRNumber;
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var payInfo = factory.New<CusEntryPayInfo>();
			payInfo.C9_CH = header.PK;
			return payInfo;
		}

		JobDeclaration declaration;
		CusEntryHeader header;

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + UniversalReferenceConstants.ProcedureCodes._00;

			new LineMerger(declaration).DoMerge();
			var header = declaration.ActiveEntryHeaders[0];

			var payInfo = Factory.New<CusEntryPayInfo>();
			payInfo.C9_CH = header.PK;
			return payInfo;
		}

		readonly ZString customsOffice = "KFN";
		readonly ZString fANumber = "3234002346";
		readonly ZString importerCode = "IMPC001";
		readonly ZString lRNumber = "51234501BBR20160822000206";
		readonly ZString mRNumber = "BBR201607115000007";
	}
}
