using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ImportCusEntryInstructionLookups))]
sealed class ImportCusEntryInstructionLookupsTest : CusEntryInstructionLookupsAbstractTest
{
	protected override string MessageType => JobMessageTypeList.Codes.Import;

	public void TestStyleList()
	{
		AssertEquals("Style Codes for Import", "4, 5, 6, 7", lookups.StyleList.CodesAsString);
	}

	public void TestProcedureList()
	{
		RefCusProcedureHelper.CreateRefCusProcedureList(Factory);
		instruction.CEI_Style = "4";
		AssertContainsExactElementsInAnyOrder("ProcedureList", new[] { "4050", "4052", "4000", "4010", "4110", "4111" }, lookups.ProcedureList.GetAllCodes());
	}

	public void TestDeclarationSubTypesList()
	{
		AssertType<ImportDeclarationSubTypes>(lookups.EntrySubStyleList);
	}
}
