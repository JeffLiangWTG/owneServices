using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Macros;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	sealed class InstructionSelector : IInstructionSelector
	{
		public Either<string, DtbBookingInstruction> SelectInstruction(DtbBookingInstruction[] instructions)
		{
			if (instructions == null
				|| instructions.Length == 0
				|| instructions.Length == 1)
			{
				return string.Empty;
			}

			var instructionsToSelectFrom = new InstructionToSelectFromForPrintingCollection(instructions.First().Factory);

			var isUnselectingInstructions = false;

			void UnselectOtherInstructions(object sender, EventArgs e)
			{
				if (isUnselectingInstructions)
				{
					return;
				}

				if (sender is InstructionToSelectFromForPrinting clickedInstruction
					&& clickedInstruction.KN_Calc_PrintDocumentForInstruction)
				{
					isUnselectingInstructions = true;

					foreach (InstructionToSelectFromForPrinting instruction in instructionsToSelectFrom)
					{
						if (instruction != clickedInstruction)
						{
							instruction.KN_Calc_PrintDocumentForInstruction = false;
						}
					}
				}

				isUnselectingInstructions = false;
			}

			var isFirstInstruction = true;

			foreach (var instruction in instructions)
			{
				var instructionToSelect = new InstructionToSelectFromForPrinting(instruction);
				instructionsToSelectFrom.Add(instructionToSelect);

				instructionToSelect.KN_Calc_PrintDocumentForInstruction = isFirstInstruction;
				instructionToSelect.KN_Calc_PrintDocumentForInstructionInfo.ValueChanged += UnselectOtherInstructions;
				isFirstInstruction = false;
			}

			var docInstructionsBizObject = new DocumentInstructions(instructionsToSelectFrom);

			using (var form = new DocumentInstructionsForm(docInstructionsBizObject))
			{
				try
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.Yes)
					{
						return docInstructionsBizObject.InstructionToPrint;
					}

					return string.Empty;
				}
				finally
				{
					foreach (InstructionToSelectFromForPrinting instruction in instructionsToSelectFrom)
					{
						instruction.KN_Calc_PrintDocumentForInstructionInfo.ValueChanged -= UnselectOtherInstructions;
					}
				}
			}
		}
	}
}
