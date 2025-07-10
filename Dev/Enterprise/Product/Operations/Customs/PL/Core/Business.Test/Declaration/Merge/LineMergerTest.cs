using System;
using System.Data;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class LineMergerTest : EU.Business.Testing.LineMergerTest
{
	public void TestPerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entry = Factory.New<DummyCusEntryHeader>();
		entry.TotalAmountPayableReturns = new ZDecimal(2000m);

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var detail = entryInstruction.Guarantees.AddNew();
		declaration.CustomsEntryHeaders.Add(entry);
		entry.CH_CEI_Instruction = entryInstruction.PK;

		var entryLine = entry.MergedLines.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;

		AssertEquals("Initial total amount payable", 0m, entry.CH_TotalPaid);
		AssertEquals("Total amount payable", 2000m, entry.TotalAmountPayable);

		var merger = new LineMerger(declaration);
		merger.DoMerge();

		AssertEquals("Entry Lines count", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		AssertEquals("Total amount payable after merge", 2000m, entry.CH_TotalPaid);
		AssertEquals("Total amount payable", 2000m, entry.TotalAmountPayable);
		AssertEquals("Guarantee amount", 2000m, detail.PW_BondAmount);
	}

	public override void TestLineMergerCreateOneEntryHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		var invoiceLine3 = invoice2.InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.JE_MessageType = "IMP";
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Tariff = "80";
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "81";
		invoiceLine3.JI_CEI = entryInstruction.PK;
		invoiceLine3.JI_Tariff = "82";
		invoice1.JZ_IncoTerm = "EXW";
		invoice2.JZ_IncoTerm = "EXW";
		invoice1.JZ_ValuationCode = "20";
		invoice2.JZ_ValuationCode = "20";
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();

		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		AssertEquals("Number of CusEntryLines should be", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);
	}

	public void TestLineNumberAssigner()
	{
		var declaration1 = Factory.New<JobDeclaration>();
		declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration1.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

		var inst1 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		inst1.CEI_SubStyle = "X";
		var invHeader1 = declaration1.Invoices.AddNew();
		var invLine1 = invHeader1.InvoiceLines.AddNew();
		invLine1.JI_CEI = inst1.PK;

		var declaration2 = Factory.New<JobDeclaration>();
		declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

		var inst2 = declaration2.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		inst2.CEI_SubStyle = "B";
		var invHeader2 = declaration2.Invoices.AddNew();
		var invLine2 = invHeader2.InvoiceLines.AddNew();
		invLine2.JI_CEI = inst2.PK;

		SetIsSupplementary(declaration1, true);
		SetIsSupplementary(declaration2, false);

		declaration1.DoMerge();
		declaration2.DoMerge();

		var lineMerger1 = new LineMerger_Exposed(declaration1);
		var lineMerger2 = new LineMerger_Exposed(declaration2);

		CombineAssertions(() =>
		{
			AssertEquals("Expected supplementary declaration to use the Custom LineNumberAssigner implementation.",
				true, lineMerger1.GetLineNumberAssigner_Exposed(invLine1.CusEntryLine.Header) is LineNumberAssigner);
			AssertEquals("Custom LineNumberAssigner for supplementary declaration must still be a subtype of Customs.Business.LineNumberAssigner.",
				true, lineMerger1.GetLineNumberAssigner_Exposed(invLine1.CusEntryLine.Header) is Customs.Business.LineNumberAssigner);

			AssertEquals("Expected non-supplementary declaration to NOT use the custom LineNumberAssigner.",
				false, lineMerger2.GetLineNumberAssigner_Exposed(invLine2.CusEntryLine.Header) is LineNumberAssigner);
			AssertEquals("Expected non-supplementary declaration to fall back to the default Customs.Business.LineNumberAssigner.",
				true, lineMerger2.GetLineNumberAssigner_Exposed(invLine2.CusEntryLine.Header) is Customs.Business.LineNumberAssigner);
		});
	}

	void SetIsSupplementary(JobDeclaration declaration, bool value)
	{
		var field = typeof(JobDeclaration).GetField("isSupplementaryDeclaration", BindingFlags.NonPublic | BindingFlags.Instance);
		field!.SetValue(declaration, value);
	}

	public void TestGetNewDutyCalculatorStrategy() => AssertType<DutyCalculatorStrategy>(new LineMerger_Exposed(Factory.New<JobDeclaration>()).GetNewDutyCalculatorStrategyExposed());

	protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

	protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(ImportEntryCreationStrategy) };

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);
}

class LineMerger_Exposed : LineMerger
{
	public LineMerger_Exposed(JobDeclaration declaration) : base(declaration)
	{
	}

	public ILineNumberAssigner GetLineNumberAssigner_Exposed(Customs.Business.CusEntryHeader entryHeader)
	{
		return base.GetLineNumberAssigner(entryHeader);
	}

	public IDutyCalculatorStrategy GetNewDutyCalculatorStrategyExposed() => (DutyCalculatorStrategy)base.GetNewDutyCalculatorStrategy();
}

sealed class DummyCusEntryHeader : CusEntryHeader
{
	public DummyCusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public ZDecimal TotalAmountPayableReturns { get; set; } = 0.0d;

	public override ZDecimal TotalAmountPayable => TotalAmountPayableReturns;
}
