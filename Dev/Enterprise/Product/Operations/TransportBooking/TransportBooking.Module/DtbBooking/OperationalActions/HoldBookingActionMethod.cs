using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.TransportBookings.Module
{
	public class HoldBookingActionMethod : OperationalActionMethod
	{
		public HoldBookingActionMethod()
			: base(new ZGuid("484b32ab-11f1-453c-93ed-637aa9982a75"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new HoldBookingChangeActionMethodApplicator();
		}

		public override string Name
		{
			get { return Res.GetString("e1e7cb32-6238-4f2e-b626-53a990037e54", "Hold Booking"); }
		}

		public override string Description
		{
			get { return Res.GetString("e1e7cb32-6238-4f2e-b626-53a990037e54", "Hold Booking"); }
		}
	}
}
