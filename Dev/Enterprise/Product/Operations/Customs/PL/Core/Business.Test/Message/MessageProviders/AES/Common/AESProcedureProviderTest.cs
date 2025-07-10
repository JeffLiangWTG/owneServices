using System;
using System.Linq;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESProcedureProviderTest : Customs.Business.Testing.DataProviderTestCase<AESProcedureProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null InvoiceLine", "Value cannot be null.\r\nParameter name: invoiceLine", () => new AESProcedureProvider(null));
	}

	public void TestRequestedProcedure() => CombineAssertions(() =>
	{
		AssertEquals("Empty declaration", string.Empty, GetProvider().RequestedProcedure);
		instruction.CEI_Procedure = "51";
		AssertEquals("CEI_Procedure is 51", "51", GetProvider().RequestedProcedure);
	});

	public void TestPreviousProcedure() => CombineAssertions(() =>
	{
		AssertEquals("Empty declaration", string.Empty, GetProvider().PreviousProcedure);
		invoiceLine.PreviousProcedureCode = "11";
		AssertEquals("PreviousProcedureCode is 11", "11", GetProvider().PreviousProcedure);
	});

	public void TestAdditionalProcedures() => CombineAssertions(() =>
	{
		AssertEquals("Empty declaration", 0, GetProvider().AdditionalProcedures.Count);
		var concession1 = invoiceLine.AdditionalProcedureCodes.AddNew();
		var concession2 = invoiceLine.AdditionalProcedureCodes.AddNew();
		concession2.CY_Code = "21";
		concession1.CY_Code = "11";
		AssertEquals("2 AdditionalProcedureCodes", 2, GetProvider().AdditionalProcedures.Count);
	});

	public void TestAdditionalProceduresEuCodesBeforeNational() =>
		AesRuleTestHelper.TestItemsAreOrderedByRuleR0093E(
			AesRuleHelper.RuleR0093E.Patterns.n1an2,
			new[] { invoiceLine.AdditionalProcedureCodes.AddNew(), invoiceLine.AdditionalProcedureCodes.AddNew() },
			(item, value) => item.CY_Code = value,
			() => GetProvider().AdditionalProcedures.Select(x => x.AdditionalProcedure));

	protected override AESProcedureProvider GetProvider() => new AESProcedureProvider(invoiceLine);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		instruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.JI_Description = "invoice line 1";

		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice;
	CusEntryInstruction instruction;
}
