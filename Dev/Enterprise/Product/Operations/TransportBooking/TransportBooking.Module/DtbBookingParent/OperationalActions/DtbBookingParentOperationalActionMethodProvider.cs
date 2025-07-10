using System;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingParentOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			if (typeof(IDtbBookingParent).IsAssignableFrom(actionSupporter.RootType))
			{
				return new OperationalActionMethod[] {
					new CreateDtbBookingsFromDtbBookingParentsActionMethod(),
					new CreateMasterAndSubDtbBookingsFromDtbBookingParentsActionMethod(),
				};
			}

			return Array.Empty<OperationalActionMethod>();
		}
	}
}
