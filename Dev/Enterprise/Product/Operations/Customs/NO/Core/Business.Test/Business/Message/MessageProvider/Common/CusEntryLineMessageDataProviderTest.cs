using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryLineMessageDataProvider))]
sealed class CusEntryLineMessageDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When entryLine is null", () => new CusEntryLineMessageDataProvider(null));
	});

	public void TestGetGoodsDescriptions_ShouldIncludeFirstFiveGoodsDescriptions() => CombineAssertions(() =>
	{
		ZString apples = "apples";
		ZString oranges = "oranges";
		ZString bananas = "bananas";
		ZString pears = "pears";
		ZString pineapples = "pineapples";
		ZString coconuts = "coconuts";

		invoiceLine.JI_Description = apples;
		AssertContainsExactElementsInAnyOrder("GetGoodsDescriptions(), when one merged invoice line", [apples], GetNewDataProvider().GetGoodsDescriptions());

		AddNewInvoiceLine().JI_Description = oranges;
		AssertContainsExactElementsInAnyOrder("GetGoodsDescriptions(), when two merged invoice lines", [apples, oranges], GetNewDataProvider().GetGoodsDescriptions());

		AddNewInvoiceLine().JI_Description = bananas;
		AddNewInvoiceLine().JI_Description = pears;
		AddNewInvoiceLine().JI_Description = pineapples;
		AddNewInvoiceLine().JI_Description = coconuts;
		AssertContainsExactElementsInAnyOrder("GetGoodsDescriptions(), when six merged invoice lines", [apples, oranges, bananas, pears, pineapples], GetNewDataProvider().GetGoodsDescriptions());
	});

	public void TestGetGoodsDescriptions_ShouldOnlyKeepFirst31CharsOfGoodsDescription()
	{
		const string longGoodsDescription = "goods description with 31 chars";

		invoiceLine.JI_Description = $"{longGoodsDescription} and then some";
		AssertContainsExactElementsInAnyOrder("GetGoodsDescriptions(), when goods description is more than 31 chars", [longGoodsDescription], GetNewDataProvider().GetGoodsDescriptions());
	}

	public void TestGetGoodsDescriptions_ShouldOrderGoodsDescriptionsByLineNo() => CombineAssertions(() =>
	{
		ZString apples = "apples";
		ZString oranges = "oranges";

		invoiceLine.JI_Description = apples;
		invoiceLine.JI_LineNo = 1;
		var invoiceLine2 = AddNewInvoiceLine();
		invoiceLine2.JI_Description = oranges;
		invoiceLine2.JI_LineNo = 2;
		AssertContainsExactElementsInExactOrder("GetGoodsDescriptions(), when apples on LineNo 1 and oranges on LineNo 2", [apples, oranges], GetNewDataProvider().GetGoodsDescriptions());

		invoiceLine.JI_LineNo = 2;
		invoiceLine2.JI_LineNo = 1;
		AssertContainsExactElementsInExactOrder("GetGoodsDescriptions(), when apples on LineNo 2 and oranges on LineNo 1", [oranges, apples], GetNewDataProvider().GetGoodsDescriptions());
	});

	public void TestGetGoodsDescriptions_ShouldSkipDuplicates() => CombineAssertions(() =>
	{
		ZString apples = "apples";
		ZString longGoodsDescription = "goods description with 31 chars";
		var invoiceLine2 = AddNewInvoiceLine();

		invoiceLine.JI_Description = apples;
		invoiceLine2.JI_Description = apples;
		AssertContainsExactElementsInAnyOrder("GetGoodsDescriptions(), when same goods description", [apples], GetNewDataProvider().GetGoodsDescriptions());

		invoiceLine.JI_Description = longGoodsDescription;
		invoiceLine2.JI_Description = $"{longGoodsDescription} and then some";
		AssertContainsExactElementsInAnyOrder("GetGoodsDescriptions(), when first 31 chars of goods description is same", [longGoodsDescription], GetNewDataProvider().GetGoodsDescriptions());
	});

	public void TestGetTotalGrossWeight() => CombineAssertions(() =>
	{
		invoiceLine.JI_Weight = 110m;
		var details1 = GetNewDataProvider();
		AssertEquals("GrossWeight, when one merged invoice line", 110m, details1.GetTotalGrossWeight().Amount);

		invoiceLine.JI_Weight = 110.42m;
		var details2 = GetNewDataProvider();
		AssertEquals("GrossWeight, when one merged invoice line that has decimals", 110.42m, details2.GetTotalGrossWeight().Amount);

		var invoiceLine2 = AddNewInvoiceLine();
		invoiceLine.JI_Weight = 110m;
		invoiceLine2.JI_Weight = 3000m;
		var details3 = GetNewDataProvider();
		AssertEquals("GrossWeight, when two merged invoice lines", 3110m, details3.GetTotalGrossWeight().Amount);
	});

	public void TestGetTotalNetWeight()
	{
		invoiceLine.JI_NetWeight = 72m;
		var details1 = GetNewDataProvider();
		AssertEquals("NetWeight, when one merged invoice line", 72m, details1.GetTotalNetWeight().Amount);

		invoiceLine.JI_NetWeight = 72.42m;
		var details2 = GetNewDataProvider();
		AssertEquals("NetWeight, when one merged invoice line that has decimals", 72.42m, details2.GetTotalNetWeight().Amount);

		var invoiceLine2 = AddNewInvoiceLine();
		invoiceLine.JI_NetWeight = 72m;
		invoiceLine2.JI_NetWeight = 3000m;
		var details3 = GetNewDataProvider();
		AssertEquals("NetWeight, when two merged invoice lines", 3072m, details3.GetTotalNetWeight().Amount);
	}

	public void TestGetValuationCodeOrMethod() => CombineAssertions(() =>
	{
		invoiceLine.JI_ValuationCode = ZString.Empty;
		invoiceHeader.JZ_ValuationMethod = ZString.Empty;
		AssertEquals("When InvLine ValuationCode and Header.JZ_ValuationMethod is empty", ZString.Empty, GetNewDataProvider().GetValuationCodeOrMethod());

		invoiceLine.JI_ValuationCode = "VD";
		AssertEquals("When InvLine ValuationCode is not empty", "VD", GetNewDataProvider().GetValuationCodeOrMethod());

		invoiceLine.JI_ValuationCode = ZString.Empty;
		invoiceHeader.JZ_ValuationMethod = "1";
		AssertEquals("When InvLine ValuationCode is empty, fetch from linked JobComInvoiceHeader.JZ_ValuationMethod", "1", GetNewDataProvider().GetValuationCodeOrMethod());
	});

	public void TestGetProcedureCode() => CombineAssertions(() =>
	{
		AssertProcedureCode("when procedure code not found in invoice line or entry instruction", ZString.Empty);

		invoiceLine.JI_Procedure = "1111";
		AssertProcedureCode("when procedure code found in invoice line", "1111");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_Procedure = "2222";
		AssertProcedureCode("when procedure code found in both invoice line and entry instruction, pick invoice line", "1111");

		invoiceLine.JI_Procedure = ZString.Empty;
		AssertProcedureCode("when procedure code found in entry instruction", "2222");

		void AssertProcedureCode(string message, ZString expected)
		{
			var details = GetNewDataProvider();
			AssertEquals($"ProcedureCode, {message}", expected, details.GetProcedureCode());
		}
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = AddNewInvoiceLine();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;

	JobComInvoiceLine AddNewInvoiceLine()
	{
		var newInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		newInvoiceLine.JI_CL = entryLine.PK;
		entryLine.InvoiceLines.Add(newInvoiceLine);
		return newInvoiceLine;
	}

	CusEntryLineMessageDataProvider GetNewDataProvider() => new(entryLine);
}
