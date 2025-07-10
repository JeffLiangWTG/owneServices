using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.GUI;

namespace Enterprise.TransportBookings.Module
{
	public abstract class BaseCreateDtbBookingsFromDtbBookingParentsActionMethod : OperationalActionMethod
	{
		protected BaseCreateDtbBookingsFromDtbBookingParentsActionMethod(ZGuid methodID) : base(methodID)
		{
		}

		public sealed override string Name => ActionMethodName;

		public sealed override string Description => ActionMethodName;

		protected abstract string ActionMethodName { get; }

		public sealed override bool HasControl => true;

		public sealed override IComponent NewGuiControl() => new CreateDtbBookingsFromDtbBookingParentsUserControl();
	}
}
