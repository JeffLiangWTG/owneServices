using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DocumentInstructions))]
	sealed class DocumentInstructionsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new InstructionToSelectFromForPrintingCollection(Factory);
			return new DocumentInstructions(collection);
		}

		public void TestInstructionsToSelectFrom()
		{
			var instruction1 = new InstructionToSelectFromForPrinting(Factory.New<DtbBookingInstruction>());
			var instruction2 = new InstructionToSelectFromForPrinting(Factory.New<DtbBookingInstruction>());

			var instructionsCollection = new InstructionToSelectFromForPrintingCollection(Factory);
			instructionsCollection.Add(instruction1);
			instructionsCollection.Add(instruction2);

			var docInstructions = new DocumentInstructions(instructionsCollection);
			AssertNotNull("InstructionsToSelectFrom is not null", docInstructions.InstructionsToSelectFrom);
			AssertEquals("InstructionsToSelectFrom count", 2, docInstructions.InstructionsToSelectFrom.Count);
		}

		public void TestInstructionsToPrint()
		{
			var instruction1 = new InstructionToSelectFromForPrinting(Factory.New<DtbBookingInstruction>());
			instruction1.KN_Calc_PrintDocumentForInstruction = ZBool.False;
			var instruction2 = new InstructionToSelectFromForPrinting(Factory.New<DtbBookingInstruction>());
			instruction2.KN_Calc_PrintDocumentForInstruction = ZBool.False;

			var instructionsCollection = new InstructionToSelectFromForPrintingCollection(Factory);
			instructionsCollection.Add(instruction1);
			instructionsCollection.Add(instruction2);

			var docInstructions = new DocumentInstructions(instructionsCollection);
			AssertNull("InstructionToPrint is null", docInstructions.InstructionToPrint);

			instruction1.KN_Calc_PrintDocumentForInstruction = ZBool.True;

			docInstructions = new DocumentInstructions(instructionsCollection);
			AssertNotNull("InstructionToPrint is not null", docInstructions.InstructionToPrint);
			AssertEquals("InstructionToPrint equals to selected Instruction", instruction1.Instruction, docInstructions.InstructionToPrint);
		}
	}
}
