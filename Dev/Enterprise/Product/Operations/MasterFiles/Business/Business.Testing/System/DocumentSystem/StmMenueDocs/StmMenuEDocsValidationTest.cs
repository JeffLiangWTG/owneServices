using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StmMenuEDocsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPrintCopyType()
		{
			var stmMenuEdocs = Factory.NewWithValidTestData<StmMenuEDocs>();
			AssertEquals("PrintCopyTypeList.Count", 5, stmMenuEdocs.PrintCopyTypeList.Count);

			AssertEquals("PrintCopyTypeList[0].Code", "EML", stmMenuEdocs.PrintCopyTypeList[0].Code);
			AssertEquals("PrintCopyTypeList[1].Code", "FAX", stmMenuEdocs.PrintCopyTypeList[1].Code);
			AssertEquals("PrintCopyTypeList[2].Code", "PRN", stmMenuEdocs.PrintCopyTypeList[2].Code);
			AssertEquals("PrintCopyTypeList[3].Code", "EPR", stmMenuEdocs.PrintCopyTypeList[3].Code);
			AssertEquals("PrintCopyTypeList[4].Code", "ALL", stmMenuEdocs.PrintCopyTypeList[4].Code);

			AssertEquals("HasErrors", false, stmMenuEdocs.SX_PrintCopyTypeInfo.HasErrors());

			stmMenuEdocs.SX_PrintCopyType = nameof(PrintCopyType.ALL);
			AssertEquals("HasErrors", false, stmMenuEdocs.SX_PrintCopyTypeInfo.HasErrors());

			stmMenuEdocs.SX_PrintCopyType = "ZZZ";
			AssertHasError("Should have validation error if invalid value input", stmMenuEdocs.SX_PrintCopyTypeInfo, "Invalid type entered. Please select one from the list.");

			stmMenuEdocs.SX_PrintCopyType = nameof(PrintCopyType.EML);
			AssertEquals("HasErrors", false, stmMenuEdocs.SX_PrintCopyTypeInfo.HasErrors());
		}

		public void TestValidateSX_PrintCopyType()
		{
			var eDocs = Factory.New<StmMenuEDocs>();
			AssertEquals("HasErrors", false, eDocs.SX_PrintCopyTypeInfo.HasErrors());

			eDocs.SX_PrintCopyType = "";
			AssertHasError(eDocs.SX_PrintCopyTypeInfo, "Please enter a value.");
		}
	}
}
