using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(InstructionToSelectFromForPrinting))]
	sealed class InstructionToSelectFromForPrintingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestChangingKN_Calc_PrintDocumentForInstructionDoesNotMarkBOsForSave()
		{
			var instruction = Factory.NewWithValidTestData<DtbBookingInstruction>();
			Factory.Save();

			Assert("Factory.Save has just occurred, no changes should be recognized", !instruction.HasChanges);
			var instructionForPrinting = new InstructionToSelectFromForPrinting(instruction);
			instructionForPrinting.KN_Calc_PrintDocumentForInstruction = !instructionForPrinting.KN_Calc_PrintDocumentForInstruction;
			Assert("Changing KN_Calc_PrintDocumentForInstruction should not be recognized as a change to the related business objects", !instruction.HasChanges);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var instruction = Factory.New<DtbBookingInstruction>();
			return new InstructionToSelectFromForPrinting(instruction);
		}
	}
}
