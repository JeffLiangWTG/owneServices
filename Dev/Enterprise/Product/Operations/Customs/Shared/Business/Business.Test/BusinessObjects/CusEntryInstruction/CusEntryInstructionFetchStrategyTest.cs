using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class CusEntryInstructionFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			return new CusEntryInstructionCollection<CusEntryInstruction>(factory.New<BaseJobDeclaration>());
		}

		public void TestFetchForLoadChildEditableObjects()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			var fetchStrategy = new CusEntryInstructionFetchStrategy(entryInstruction);
			fetchStrategy.FetchForLoadChildEditableObjects();
			var tableHits = new ZStringBuilder(Factory.GetAllFetchHintedTableNames().OrderBy(x => x)).ToStringWithNewLineBetweenAppends();
			AssertContains("All Table Hits", "JobDocAddress", tableHits);
		}
	}
}
