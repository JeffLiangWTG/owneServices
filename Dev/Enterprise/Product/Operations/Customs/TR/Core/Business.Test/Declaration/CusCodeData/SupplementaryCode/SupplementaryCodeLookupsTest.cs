using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Testing;
sealed class SupplementaryCodeLookupsTest : TestCaseWithFactory
{
	public void TestCY_CodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var proc4000 = helper.CreateRefCusProcedure("TR", "EXM", "40", "00", "", "4000 desc", "IMP");
		var proc4010 = helper.CreateRefCusProcedure("TR", "EXM", "40", "10", "", "4010 desc", "EXP");

		var proc4020 = helper.CreateRefCusProcedure("IT", "EXM", "40", "20", "", "4020 desc", "EXP");
		var proc4030 = helper.CreateRefCusProcedure("IT", "EXM", "40", "30", "", "4030 desc", "IMP");

		var proc4040 = helper.CreateRefCusProcedure("TR", "IM", "40", "40", "", "4040 desc", "EXP");
		var proc4050 = helper.CreateRefCusProcedure("TR", "PRO", "40", "50", "", "4040 desc", "IMP");

		var proc4060 = helper.CreateRefCusProcedure("TR", "EXM", "40", "60", "", "4040 desc", "IMP,EXP");

		var lookups = new SupplementaryCodeLookups(supplementaryCode);

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var cyCodeList = lookups.CY_CodeList;
		CombineAssertions("Import Message Type", () =>
		{
			AssertEquals(2, cyCodeList.Count);
			AssertEquals("proc4000 is valid", true, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4000.ZZ6_ProcedureCode, proc4000.ZZ6_PreviousProcedureCode), proc4000.ZZ6_Description)));
			AssertEquals("proc4010 is invalid", false, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4010.ZZ6_ProcedureCode, proc4010.ZZ6_PreviousProcedureCode), proc4010.ZZ6_Description)));
			AssertEquals("proc4020 is invalid", false, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4020.ZZ6_ProcedureCode, proc4020.ZZ6_PreviousProcedureCode), proc4020.ZZ6_Description)));
			AssertEquals("proc4030 is invalid", false, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4030.ZZ6_ProcedureCode, proc4030.ZZ6_PreviousProcedureCode), proc4030.ZZ6_Description)));
			AssertEquals("proc4040 is invalid", false, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4040.ZZ6_ProcedureCode, proc4040.ZZ6_PreviousProcedureCode), proc4040.ZZ6_Description)));
			AssertEquals("proc4050 is invalid", false, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4050.ZZ6_ProcedureCode, proc4050.ZZ6_PreviousProcedureCode), proc4050.ZZ6_Description)));
			AssertEquals("proc4060 is valid", true, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4060.ZZ6_ProcedureCode, proc4060.ZZ6_PreviousProcedureCode), proc4060.ZZ6_Description)));
		});

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		cyCodeList = lookups.CY_CodeList;
		CombineAssertions("Export Message Type", () =>
		{
			AssertEquals(2, cyCodeList.Count);
			AssertEquals("proc4000 is invalid", false, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4000.ZZ6_ProcedureCode, proc4000.ZZ6_PreviousProcedureCode), proc4000.ZZ6_Description)));
			AssertEquals("proc4010 is valid", true, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4010.ZZ6_ProcedureCode, proc4010.ZZ6_PreviousProcedureCode), proc4010.ZZ6_Description)));
			AssertEquals("proc4020 is invalid", false, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4020.ZZ6_ProcedureCode, proc4020.ZZ6_PreviousProcedureCode), proc4020.ZZ6_Description)));
			AssertEquals("proc4030 is invalid", false, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4030.ZZ6_ProcedureCode, proc4030.ZZ6_PreviousProcedureCode), proc4030.ZZ6_Description)));
			AssertEquals("proc4040 is invalid", false, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4040.ZZ6_ProcedureCode, proc4040.ZZ6_PreviousProcedureCode), proc4040.ZZ6_Description)));
			AssertEquals("proc4050 is invalid", false, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4050.ZZ6_ProcedureCode, proc4050.ZZ6_PreviousProcedureCode), proc4050.ZZ6_Description)));
			AssertEquals("proc4060 is valid", true, cyCodeList.Contains(new CodeDescriptionPair(string.Concat(proc4060.ZZ6_ProcedureCode, proc4060.ZZ6_PreviousProcedureCode), proc4060.ZZ6_Description)));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	SupplementaryCode supplementaryCode;
}
