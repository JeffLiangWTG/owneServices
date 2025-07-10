using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>))]
	sealed class CusContainerOnEntryInstructionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowRemove);
		}

		public override void TestAddNew()
		{
			base.TestAddNew();

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var container = declaration.CusContainers.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var collection = new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(instruction);
			var result = collection.AddNew(container);
			AssertType<CusContainerOnEntryInstruction>(result);
			AssertEquals(container, result.Container);
		}

		public void TestOnloading()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.SupportContainerEntryInstructionPivotCoreForTesting = true;
			var container = declaration.CusContainers.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var collection = new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(instruction);
			CombineAssertions(() =>
			{
				AssertEquals("Loading all of the container", 1, collection.Count);
				AssertEquals(container, collection[0].Container);

				declaration.CusContainers.AddNew();
				collection = new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(instruction);
				AssertEquals("Loading all of the container", 2, collection.Count);

				declaration.SupportContainerEntryInstructionPivotCoreForTesting = false;
				collection = new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(instruction);
				AssertEquals("Not call loading when SupportContainerEntryInstructionPivot is false", 0, collection.Count);
			});
		}

		public void TestOnContainerChanged()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.SupportContainerEntryInstructionPivotCoreForTesting = true;
			var container1 = declaration.CusContainers.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var collection = new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(instruction);

			CombineAssertions(() =>
			{
				AssertEquals("1 container", 1, collection.Count);
				AssertEquals(container1, collection[0].Container);

				var container2 = declaration.CusContainers.AddNew();
				AssertEquals("Add new container", 2, collection.Count);

				container2.Delete();
				AssertEquals("Delete a container", 1, collection.Count);
				AssertEquals(container1, collection[0].Container);

				declaration.SupportContainerEntryInstructionPivotCoreForTesting = false;
				collection = new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(instruction);
				declaration.CusContainers.AddNew();
				AssertEquals("Not call event when SupportContainerEntryInstructionPivot is false", 0, collection.Count);
			});
		}

		public void TestAllLinkedContainers()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.SupportContainerEntryInstructionPivotCoreForTesting = true;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TEST1";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var result1 = new CusContainerOnEntryInstruction(instruction);
			result1.Container = container;
			result1.IsForEntry = true;

			container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TEST2";
			var result2 = new CusContainerOnEntryInstruction(instruction);
			result2.Container = container;
			result2.IsForEntry = false;

			var collection = new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(instruction);
			var containerNumbers = collection.AllLinkedContainers.ToList().Select(c => c.ContainerNumber);
			AssertEquals(1, collection.AllLinkedContainers.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "TEST1" }, containerNumbers);

			result2.IsForEntry = true;
			containerNumbers = collection.AllLinkedContainers.ToList().Select(c => c.ContainerNumber);
			AssertEquals(2, collection.AllLinkedContainers.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "TEST1", "TEST2" }, containerNumbers);

			declaration.SupportContainerEntryInstructionPivotCoreForTesting = false;
			collection = new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(instruction);
			AssertEquals("SupportContainerEntryInstructionPivotCoreForTesting equal to false", 0, collection.AllLinkedContainers.Length);
		}

		protected override CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction> GetCollectionToTest()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(instruction);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var container = declaration.CusContainers.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var result = new CusContainerOnEntryInstruction(instruction);
			result.Container = container;
			result.IsForEntry = true;
			return result;
		}
	}
}
