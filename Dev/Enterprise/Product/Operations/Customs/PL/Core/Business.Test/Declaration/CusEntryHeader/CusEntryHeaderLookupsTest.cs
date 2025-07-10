using Enterprise.Customs.PL.Business.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class CusEntryHeaderLookupsTest : EU.Business.Declaration.Testing.CusEntryHeaderLookupsTest
{
	public void TestParent()
	{
		var parent = Factory.New<CusEntryHeader>();
		AssertEquals(parent.Validation.Parent, parent);
	}

	public void TestList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = Factory.New<CusEntryHeader>();
		var list = entryHeader.Lookups.CH_EntryStatusList;
		CombineAssertions(() =>
		{
			list.AssertContainsExactCodes("CodesAsString", new [] { "REQ", "CAN", "ECE", "ECO", "ECX", "ERE", "EXP", "ICO", "IMA", "IMF", "MRN", "NPP", "REL", "UPO" });
			AssertEquals("Cached", list, declaration.Lookups.EntryStatusList);
		});
	}
}
