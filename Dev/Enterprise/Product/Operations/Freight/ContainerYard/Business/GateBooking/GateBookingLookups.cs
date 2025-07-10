using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateBookingLookups : AutoGateBookingLookups
	{
		public GateBookingLookups(AutoGateBooking parent)
			: base(parent)
		{
		}

		public RefPackTypeCollection GTD_F3_NKPackType_List
		{
			get { return new RefPackTypeCollection(Factory); }
		}
	}
}
