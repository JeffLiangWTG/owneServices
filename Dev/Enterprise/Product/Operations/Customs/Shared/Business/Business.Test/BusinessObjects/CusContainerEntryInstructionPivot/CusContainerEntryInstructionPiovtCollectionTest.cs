using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainerEntryInstructionPiovtCollection))]
	sealed class CusContainerEntryInstructionPiovtCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddAndDeleteCusContainerEntryInstructionPivot()
		{
			var cusContainer = Factory.New<BaseCusContainer>();
			var instruction = Factory.New<CusEntryInstruction>();
			var collection = new CusContainerEntryInstructionPiovtCollection(instruction);
			collection.AddPivotFor(cusContainer);
			AssertEquals("Container has been added to collection", 1, collection.Count);
			AssertEquals("Should have returned true if container exists in pivot.", cusContainer.PK, collection.Cast<CusContainerEntryInstructionPivot>().First().CEP_CO_Container);
			var piovt = collection.GetRelatedPivot(cusContainer);
			AssertEquals(instruction.PK, piovt.CEP_CEI_EntryInstruction);
			AssertEquals(cusContainer.PK, piovt.CEP_CO_Container);
			collection.AddPivotFor(cusContainer);
			AssertEquals("There is no new element when add an existed container", 1, collection.Count);
			collection.DeletePivotFor(cusContainer);
			Assert("Piovt has been deleted", piovt.IsDeleted);
			AssertEquals("Container should have been deleted at this point.", 0, collection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return new CusContainerEntryInstructionPiovtCollection(instruction);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var pivot = Factory.New<CusContainerEntryInstructionPivot>();
			return pivot;
		}
	}
}
