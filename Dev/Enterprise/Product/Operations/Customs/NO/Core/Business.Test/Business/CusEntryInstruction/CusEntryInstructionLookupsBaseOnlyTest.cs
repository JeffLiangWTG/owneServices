using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business.Testing;

sealed class CusEntryInstructionLookupsBaseOnlyTest : CusEntryInstructionLookupsAbstractTest
{
	protected override string MessageType => JobMessageTypeList.Codes.Import;

	public void TestCopyStatusList() => CombineAssertions(() =>
	{
		var codeList = lookups.CopyStatusList;
		AssertSame("Cached", lookups.CopyStatusList, codeList);
		AssertType<NODeclarationCopyStatus>("Type", codeList);
	});

	public void TestProcedureList()
	{
		var codeList = lookups.ProcedureList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.ProcedureList, codeList);
			AssertType<CodeDescriptionPairList>("Type", codeList);
		});
	}
}
