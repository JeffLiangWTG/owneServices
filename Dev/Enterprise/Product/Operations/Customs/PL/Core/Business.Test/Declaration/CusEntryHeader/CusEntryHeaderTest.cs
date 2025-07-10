using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryHeader))]
sealed class CusEntryHeaderTest : EU.Business.Declaration.Testing.CusEntryHeaderTest<CusEntryHeader>
{
	public override void TestOfficeOfExit()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entry = declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsOffices.RemoveAndDeleteAll();
		declaration.JE_CustomsOffice = "LV001000";
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "LV002000");
		AssertEquals("LV002000", declaration.OfficeOfExit);
		AssertEquals("LV002000", entry.OfficeOfExit);
	}

	public override void TestOfficeOfEntry()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsOffices.RemoveAndDeleteAll();
		declaration.JE_CustomsOffice = "LV001000";
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
		AssertEquals("LV002000", declaration.OfficeOfEntry);
		AssertEquals("LV002000", entry.OfficeOfEntry);
	}

	public void TestMessages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.Messages.AddNew();
		AssertType<EDIMessageCollection>(entryHeader.Messages);
	}

	public override BaseJobDeclaration GetNewBaseJobDeclaration() => GetNewDeclaration();

	public override void TestIsIndirectExport()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var entry = declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsOffices.RemoveAndDeleteAll();
		Factory.Save();
		var cusOffice = declaration.CustomsOffices.AddNew();
		cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;

		declaration.JE_MessageType = "EXP";
		cusOffice.CY_Data = "GB00001";
		Assert(entry.IsIndirectExport);

		cusOffice.CY_Data = "PL00001";
		Assert(!entry.IsIndirectExport);

		cusOffice.CY_Data = "";
		Assert(!entry.IsIndirectExport);

		declaration.JE_MessageType = "IMP";
		cusOffice = declaration.CustomsOffices.AddNew();
		cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
		cusOffice.CY_Data = "GB00001";
		Assert(!entry.IsIndirectExport);
	}

	public void TestAllEntryLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertType<AllCusEntryLineCollection<CusEntryLine>>(entryHeader.AllEntryLines);
	}

	public void TestMergedLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertType<CusEntryLineCollection<CusEntryLine>>(entryHeader.MergedLines);
	}

	public new void TestWorkflowSupportableBusinessObject()
	{
		Assert("We no longer support workflow on CusEntryHeader", true);
	}

	public void TestLookups()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertType<CusEntryHeaderLookups>(entryHeader.Lookups);
	}

	public void TestValidationType() => AssertType<CusEntryHeaderValidation>(Factory.New<CusEntryHeader>().Validation);

	public void TestHasAdditionalInfoCode()
	{
		var (declaration, invoiceHeader, invoiceLine, entryHeader) = GetNewTestObjects();

		var declarationDocument = declaration.AdditionalInfos.AddNew();
		var invoiceDocumentt = invoiceHeader.AdditionalInfos.AddNew();
		var invoiceLineDocument = invoiceLine.AdditionalInfos.AddNew();

		var searchedCode = "01";
		CombineAssertions(() =>
		{
			AssertEquals("defaults", false, entryHeader.HasAdditionalInfoCode(searchedCode));

			declarationDocument.CSI_Code = searchedCode;
			AssertEquals("SearchedCode on declaration", true, entryHeader.HasAdditionalInfoCode(searchedCode));

			declarationDocument.CSI_Code = "02";
			invoiceDocumentt.CSI_Code = searchedCode;
			AssertEquals("SearchedCode on invoice", true, entryHeader.HasAdditionalInfoCode(searchedCode));

			invoiceDocumentt.CSI_Code = "03";
			invoiceLineDocument.CSI_Code = searchedCode;
			AssertEquals("SearchedCode on invoice line", true, entryHeader.HasAdditionalInfoCode(searchedCode));

			invoiceLineDocument.CSI_Code = "04";
			AssertEquals("No searchedCode on any level", false, entryHeader.HasAdditionalInfoCode(searchedCode));
		});
	}

	public void TestHasPreviousDocumentCode()
	{
		var (declaration, invoiceHeader, invoiceLine, entryHeader) = GetNewTestObjects();

		var declarationDocument = declaration.PreviousDocuments.AddNew();
		var invoiceDocumentt = invoiceHeader.PreviousDocuments.AddNew();
		var invoiceLineDocument = invoiceLine.PreviousDocuments.AddNew();

		var searchedCode = "01";
		CombineAssertions(() =>
		{
			AssertEquals("defaults", false, entryHeader.HasPreviousDocumentCode(searchedCode));

			declarationDocument.CSI_Code = searchedCode;
			AssertEquals("SearchedCode on declaration", true, entryHeader.HasPreviousDocumentCode(searchedCode));

			declarationDocument.CSI_Code = "02";
			invoiceDocumentt.CSI_Code = searchedCode;
			AssertEquals("SearchedCode on invoice", true, entryHeader.HasPreviousDocumentCode(searchedCode));

			invoiceDocumentt.CSI_Code = "03";
			invoiceLineDocument.CSI_Code = searchedCode;
			AssertEquals("SearchedCode on invoice line", true, entryHeader.HasPreviousDocumentCode(searchedCode));

			invoiceLineDocument.CSI_Code = "04";
			AssertEquals("No searchedCode on any level", false, entryHeader.HasPreviousDocumentCode(searchedCode));
		});
	}

	public void TestHasSupportingDocumentCode()
	{
		var (declaration, invoiceHeader, invoiceLine, entryHeader) = GetNewTestObjects();

		var declarationDocument = declaration.SupportingDocuments.AddNew();
		var invoiceDocumentt = invoiceHeader.SupportingDocuments.AddNew();
		var invoiceLineDocument = invoiceLine.SupportingDocuments.AddNew();

		var searchedCode = "01";
		CombineAssertions(() =>
		{
			AssertEquals("defaults", false, entryHeader.HasSupportingDocumentCode(searchedCode));

			declarationDocument.CSI_Code = searchedCode;
			AssertEquals("SearchedCode on declaration", true, entryHeader.HasSupportingDocumentCode(searchedCode));

			declarationDocument.CSI_Code = "02";
			invoiceDocumentt.CSI_Code = searchedCode;
			AssertEquals("SearchedCode on invoice", true, entryHeader.HasSupportingDocumentCode(searchedCode));

			invoiceDocumentt.CSI_Code = "03";
			invoiceLineDocument.CSI_Code = searchedCode;
			AssertEquals("SearchedCode on invoice line", true, entryHeader.HasSupportingDocumentCode(searchedCode));

			invoiceLineDocument.CSI_Code = "04";
			AssertEquals("No searchedCode on any level", false, entryHeader.HasSupportingDocumentCode(searchedCode));
		});
	}

	public void TestHasConcessionCode()
	{
		var (_, _, invoiceLine, entryHeader) = GetNewTestObjects();

		var concession = invoiceLine.AdditionalProcedureCodes.AddNew();

		var searchedCode = "01";
		CombineAssertions(() =>
		{
			AssertEquals("defaults", false, entryHeader.HasConcessionCode(searchedCode));

			concession.CY_Code = searchedCode;
			AssertEquals("SearchedCode exists", true, entryHeader.HasConcessionCode(searchedCode));

			concession.CY_Code = "02";
			AssertEquals("No searchedCode", false, entryHeader.HasConcessionCode(searchedCode));
		});
	}

	(JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine, CusEntryHeader) GetNewTestObjects()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.First();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders.Single();

		return (declaration, invoiceHeader, invoiceLine, entryHeader);
	}

	protected override BaseJobDeclaration GetNewDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.CustomsEntryInstructions.AddNew();

		return declaration;
	}

	protected override ZBool IgnoreTestsThatRequireEntryShouldSetUCRinBGMReferenceNumber => true;

	protected override (ZString OverseasFreightChargeCode, ZString OverseasInsuranceChargeCode, ZString NotIncludedChargeCode) GetChargeCodesForTotalTAndI()
		=> (PLCustomsChargeTypeList.Codes.AK, PLCustomsChargeTypeList.Codes.AK, CustomsChargeTypeList.Codes.PackingCost);

	protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new CurrencyTestSetup();

	protected override BaseJobDeclaration ImportJobDeclaration
	{
		get
		{
			var declaration = base.ImportJobDeclaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.CustomsEntryInstructions.AddNew();

			return declaration;
		}
	}

	sealed class CurrencyTestSetup : IChargesCurrencyTestSetup
	{
		void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
		{
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.AutoCreateChargesBasedOnIncoTerm = false;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Weight = 1;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			var nonDutiableCharge = invoiceHeader.Charges.AddNew();
			nonDutiableCharge.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight;
			nonDutiableCharge.J7_Amount = 200m;
			nonDutiableCharge.J7_IsDutiable = false;
			nonDutiableCharge.J7_IsIncludedInITOT = true;

			var oft = invoiceHeader.Charges.AddNew();
			oft.J7_ChargeType = PLCustomsChargeTypeList.Codes.AK;
			oft.J7_Amount = 500m;
			oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
		}

		ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10500m;
		ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 11000m;
	}
}
