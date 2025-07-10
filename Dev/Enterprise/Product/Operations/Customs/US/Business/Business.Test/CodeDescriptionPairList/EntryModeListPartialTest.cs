using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryModeListTest : TestCaseWithFactory
	{
		public void TestGetRelevantListFor()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;

			var list = EntryModeList.GetRelevantListFor(declaration.US_EntryDate);
			AssertEquals(2, list.Count);
			AssertEquals(true, list.ContainsCode(EntryModeList.Codes.Paired));
			AssertEquals(true, list.ContainsCode(EntryModeList.Codes.RLF));

			declaration.US_EntryDate = new ZDateTime(2011, 01, 27);
			list = EntryModeList.GetRelevantListFor(declaration.US_EntryDate);
			AssertEquals(true, list.ContainsCode(EntryModeList.Codes.Paired));
			AssertEquals(true, list.ContainsCode(EntryModeList.Codes.RLF));

			declaration.US_EntryDate = USConstants.PairedPortProgramEndDate;
			list = EntryModeList.GetRelevantListFor(declaration.US_EntryDate);
			AssertEquals(1, list.Count);
			AssertEquals(false, list.ContainsCode(EntryModeList.Codes.Paired));
			AssertEquals(true, list.ContainsCode(EntryModeList.Codes.RLF));
		}

		public void TestGetCodeDescriptionPairList()
		{
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider list = new EntryModeList();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}
	}
}
