using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.TransportBookings.Business
{
	public class InstructionToSelectFromForPrinting : NonPersistentBusinessObject
	{
		public InstructionToSelectFromForPrinting(DtbBookingInstruction instruction)
			: base(instruction.Factory)
		{
			Argument.NotNull(instruction, nameof(instruction));
			Instruction = instruction;
		}

		public DtbBookingInstruction Instruction { get; private set; }

		protected ZBool kn_Calc_PrintDocumentForInstruction;
		public ZBool KN_Calc_PrintDocumentForInstruction
		{
			get { return kn_Calc_PrintDocumentForInstruction; }
			set
			{
				kn_Calc_PrintDocumentForInstruction = value;
				KN_Calc_PrintDocumentForInstructionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo KN_Calc_PrintDocumentForInstructionInfo
		{
			get { return GetZPropertyInfo(nameof(KN_Calc_PrintDocumentForInstruction)); }
		}
	}
}
