using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivotCollectionForTesting))]
	sealed class CusStorageDocPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusStorageDocPivotCollectionForTesting(Factory.New<CusEntryInstructionAsTypeSupporter>());
		}

		public void TestSetDefaultsForNewChild()
		{
			var instruction = Factory.New<CusEntryInstructionAsTypeSupporter>();
			var testCollection = new CusStorageDocPivotCollectionForTesting(instruction);
			var newItem = testCollection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CSD_ParentTableCode", "CEI", newItem.CSD_ParentTableCode);
				AssertEquals("CSD_ParentID", instruction.PK, newItem.CSD_ParentID);
			});
		}
	}
}
