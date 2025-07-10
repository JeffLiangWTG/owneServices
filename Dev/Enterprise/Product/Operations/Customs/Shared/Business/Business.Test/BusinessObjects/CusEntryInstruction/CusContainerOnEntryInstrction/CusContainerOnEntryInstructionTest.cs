using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainerOnEntryInstruction))]
	sealed class CusContainerOnEntryInstructionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNotAddedToFactoryCache()
		{
			var nPCusContainer = GetNewBusinessObject();
			AssertEquals("Data should not be cached in the factory", 0, Factory.GetBizOsForPK(nPCusContainer.PK.ToGuid()).Length);
		}

		public void TestContainerNumber()
		{
			var container = (CusContainerOnEntryInstruction)GetNewBusinessObject();
			container.Container = Factory.New<BaseCusContainer>();
			container.Container.CO_ContainerNumber = "123456";
			AssertEquals("123456", container.ContainerNumber);
		}

		public void TestIsForEntry()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var container = declaration.CusContainers.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var result = new CusContainerOnEntryInstruction(instruction);
			result.Container = container;
			result.IsForEntry = true;

			AssertNotNull("IsForEntry is true", result.Pivot);

			result.IsForEntry = false;
			AssertNull("IsForEntry change to false", result.Pivot);
		}

		public void TestPivot()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var container = declaration.CusContainers.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var result = new CusContainerOnEntryInstruction(instruction);
			result.Container = container;
			result.IsForEntry = true;

			AssertEquals("Check Entry Instruction PK", instruction.PK, result.Pivot.CEP_CEI_EntryInstruction);
			AssertEquals("Check Container PK", container.PK, result.Pivot.CEP_CO_Container);
		}

		protected override BusinessObject GetNewBusinessObject()
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
