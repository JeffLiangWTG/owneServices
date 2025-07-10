using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Shared.Testing
{
	public interface ITransportBookingTestHelper
	{
		IDtbBookingConsolidation CreateConsolidation();
		IDtbBooking CreateBooking(IDtbBookingConsolidation consolidation);
		IDtbBooking CreateBooking(OrgHeader transportCo);
		IDtbBooking CreateBooking();

		IDtbBookingInstruction CreateInstruction(IDtbBooking booking, ZString instructionType);
		IDtbBookingInstruction CreateInstruction(IDtbBooking booking, ZString instructionType, ZString orgType, OrgAddress address);
		IDtbBookingInstruction CreateInstruction(IDtbBooking booking, ZString instructionType, ZString orgType, OrgAddress address, IPkgPackage package, ZDateTime estimated);

		IDtbBookingConfirmation CreateConfirmation(IDtbBookingInstruction instruction, ZString confirmationTypeCode);

		OrgHeader CreateOrganisation(ZString code);
	}
}
