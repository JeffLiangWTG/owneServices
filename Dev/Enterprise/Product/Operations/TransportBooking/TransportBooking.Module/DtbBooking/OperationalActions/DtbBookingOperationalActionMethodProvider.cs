using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Module.OperationalActions;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new MakeBookingAvailableActionMethod(),
				new HoldBookingActionMethod(),
				new RemoveBookingHeldStatusActionMethod(),
				new CreateTransportJobsOperationalActionMethod(),
				new CreateLandTransportConsignmentsOperationalActionMethod(),
				new CreatePortTransportJobsOperationalActionMethod(),
				new MakeBookingStatusIncompleteActionMethod(),
				new CarrierBookingAgentChangeActionMethod()
			};
		}
	}
}
