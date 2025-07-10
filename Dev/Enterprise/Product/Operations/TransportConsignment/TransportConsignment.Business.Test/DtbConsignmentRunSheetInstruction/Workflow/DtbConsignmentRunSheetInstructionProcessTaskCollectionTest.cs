using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetInstructionProcessTaskCollection))]
	sealed class DtbConsignmentRunSheetInstructionProcessTaskCollectionTest : ProcessTaskCollectionTest<DtbConsignmentRunSheetInstructionProcessTaskCollection>
	{
		protected override DtbConsignmentRunSheetInstructionProcessTaskCollection GetCollectionToTestCore()
		{
			return new DtbConsignmentRunSheetInstructionProcessTaskCollection(Instruction);
		}

		DtbConsignmentRunSheetInstruction Instruction
		{
			get
			{
				if (instruction == null)
				{
					var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
					instruction = runSheet.RunSheetInstructions.AddNew();
				}
				return instruction;
			}
		}
		DtbConsignmentRunSheetInstruction instruction;
	}
}
