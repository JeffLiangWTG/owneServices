using CargoWise.Macros;

namespace Enterprise.TransportBookings.Business
{
	public interface IInstructionSelector
	{
		Either<string, DtbBookingInstruction> SelectInstruction(DtbBookingInstruction[] instructions);
	}
}
