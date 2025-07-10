using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Module
{
	public sealed class CreateDtbBookingsFromDtbBookingParentsApplicator : BaseCreateDtbBookingsFromDtbBookingParentsApplicator
	{
		public CreateDtbBookingsFromDtbBookingParentsApplicator(BusinessObjectFactory factory) : base(nameof(CreateDtbBookingsFromDtbBookingParentsApplicator), factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var validDirection = Enum.TryParse<DtbBookingDirection>(Direction, true, out var selectedDirection);
			if (!validDirection)
			{
				var processInfo = Res.GetString("21e3d46b-3dbb-4f47-b784-b553b31c7513", "Invalid Transport Booking direction chosen '{0}', cannot create Transport Bookings", Direction);
				log.Notify(OperationalActionLogErrorLevel.Error, processInfo);
				return;
			}

			var parents = targets.Cast<IDtbBookingParent>().ToArray();
			var logWrapper = new OperationalActionSectionLogLoggerWrapper(log);
			var manager = new DtbDeliveryManager(Factory, parents, selectedDirection, combineContainers: true, logger: logWrapper, errorManager: null, suppressDialogsAndUserInteractivity: true, selectAllContainers: true);
			manager.CreateTransportBookings(BookingTemplate);
		}
	}
}
