
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public class AmalgamatedStatusListTest : TestCaseWithFactory
	{
		public void TestListHasAllFormalEntryStatuses()
		{
			AmalgamatedStatusList amalgamatedList = new AmalgamatedStatusList();
			FormalEntryStatusList formalList = new FormalEntryStatusList();
			foreach (CodeDescriptionPair formalPair in formalList)
			{
				Assert("List didn't contain '" + formalPair.Code + "', this is a PROBLEM.", amalgamatedList.ContainsCode(formalPair.Code));
			}
		}

		public void TestListHasAllECIWriteOffStatuses()
		{
			AmalgamatedStatusList amalgamatedList = new AmalgamatedStatusList();
			var eciList = new LowValueConsignmentStatusList();
			foreach (CodeDescriptionPair eciPair in eciList)
			{
				Assert("List didn't contain '" + eciPair.Code + "', this is a PROBLEM.", amalgamatedList.ContainsCode(eciPair.Code));
			}
		}

		public void TestAllListEntriesArePresentOnTheFormalOrECIList()
		{
			AmalgamatedStatusList amalgamatedList = new AmalgamatedStatusList();
			var eciList = new LowValueConsignmentStatusList();
			FormalEntryStatusList formalList = new FormalEntryStatusList();
			foreach (CodeDescriptionPair amalgamatedPair in amalgamatedList)
			{
				Assert("Neither list contained '" + amalgamatedPair.Code + "', this is a PROBLEM.", eciList.ContainsCode(amalgamatedPair.Code) || formalList.ContainsCode(amalgamatedPair.Code));
			}
		}
	}
}
