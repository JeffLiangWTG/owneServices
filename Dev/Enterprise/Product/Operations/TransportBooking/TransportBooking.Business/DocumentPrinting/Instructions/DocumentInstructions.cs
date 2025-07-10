using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business
{
	public class DocumentInstructions : NonPersistentBusinessObject
	{
		public DocumentInstructions(InstructionToSelectFromForPrintingCollection instructionsToSelectFrom)
			: base(instructionsToSelectFrom.Factory)
		{
			this.instructionsToSelectFrom = instructionsToSelectFrom;
		}

		public InstructionToSelectFromForPrintingCollection InstructionsToSelectFrom
		{
			get { return instructionsToSelectFrom; }
		}
		readonly InstructionToSelectFromForPrintingCollection instructionsToSelectFrom;

		public DtbBookingInstruction InstructionToPrint
		{
			get
			{
				return InstructionsToSelectFrom
					.Where(i => i.KN_Calc_PrintDocumentForInstruction)
					.Select(i => i.Instruction)
					.OfType<DtbBookingInstruction>()
					.FirstOrDefault();
			}
		}

		public static new string TableName
		{
			get { return "TransportBookingInstruction"; }
		}
	}
}
