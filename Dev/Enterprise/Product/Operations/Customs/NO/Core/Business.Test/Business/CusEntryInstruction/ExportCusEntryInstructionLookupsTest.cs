using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ExportCusEntryInstructionLookups))]
sealed class ExportCusEntryInstructionLookupsTest : CusEntryInstructionLookupsAbstractTest
{
	protected override string MessageType => JobMessageTypeList.Codes.Export;

	public void TestStyleList()
	{
		AssertEquals("Style Codes for Export", "1, 2, 3, 8, 0", lookups.StyleList.CodesAsString);
	}

	public void TestProcedureList()
	{
		RefCusProcedureHelper.CreateRefCusProcedureList(Factory);
		instruction.CEI_Style = "1";
		AssertContainsExactElementsInAnyOrder("ProcedureList", new[] { "1000", "1010", "1110", "1111" }, lookups.ProcedureList.GetAllCodes());
	}

	public void TestDeclarationSubTypesList()
	{
		AssertType<ExportDeclarationSubTypes>(lookups.EntrySubStyleList);
	}
}
