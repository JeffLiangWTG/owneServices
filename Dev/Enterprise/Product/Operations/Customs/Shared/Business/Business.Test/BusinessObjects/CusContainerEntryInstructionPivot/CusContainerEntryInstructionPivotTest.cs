using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainerEntryInstructionPivot))]
	sealed class CusContainerEntryInstructionPivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(Factory);
		}

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var declaration = factory.New<BaseJobDeclarationWithEntryInstructions>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var container = declaration.CusContainers.AddNew();
			factory.Save();

			var pivot = Factory.New<CusContainerEntryInstructionPivot>();
			pivot.CEP_CEI_EntryInstruction = entryInstruction.PK;
			pivot.CEP_CO_Container = container.PK;

			return pivot;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject(factory);
		}

		public void TestContainer()
		{
			var container = Factory.New<BaseCusContainer>();

			var pivot = Factory.New<CusContainerEntryInstructionPivot>();
			pivot.CEP_CO_Container = container.PK;
			AssertEquals("Container should be linked to CusContainerEntryInstructionPivot", pivot.CEP_CO_Container, pivot.Container.PK);
		}

		public void TestEntryInstruction()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclarationWithEntryInstructions>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";

			var pivot = Factory.New<CusContainerEntryInstructionPivot>();
			pivot.CEP_CEI_EntryInstruction = instruction.PK;
			AssertEquals("EntryInstruction should be linked to CusContainerEntryInstructionPivot", pivot.CEP_CEI_EntryInstruction, pivot.EntryInstruction.PK);
		}
	}
}
