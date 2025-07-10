using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportBookings.Module.OperationalActions
{
	public class CreatePortTransportJobsOperationalActionMethodApplicator : DtbBookingOperationalActionMethodApplicator
	{
		public CreatePortTransportJobsOperationalActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			base.ApplyCore(log, targets);
			if (SomeTargetsAreLockedDown)
			{
				SomeTargetsAreLockedDown = false;
				return;
			}

			var buffer = new NotificationBuffer();
			var processingManager = new BookingToTransportJobCommonCreator(buffer, AutoCreatorTargetModules.Codes.PortTransport);

			if (targets.Any())
			{
				processingManager.TryCreateTransportJobsFromTransportBookings(targets);
				foreach (var error in buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error))
				{
					log.Notify(OperationalActionLogErrorLevel.Error, error.Message);
				}

				foreach (var warning in buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning))
				{
					log.Notify(OperationalActionLogErrorLevel.Informational, warning.Message);
				}
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("1F3A1315-05D3-4A31-8E58-40414F5B0873", "Selected Transport Bookings need to exist in database."));
			}
		}

		public CreatePortTransportJobsOperationalActionMethodApplicatorValidation Validation
		{
			get { return new CreatePortTransportJobsOperationalActionMethodApplicatorValidation(this); }
		}
	}

	public class CreatePortTransportJobsOperationalActionMethodApplicatorValidation : ZValidation
	{
		public CreatePortTransportJobsOperationalActionMethodApplicatorValidation(CreatePortTransportJobsOperationalActionMethodApplicator applicator)
			: base(applicator)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(CreatePortTransportJobsOperationalActionMethodApplicator); }
		}

		public override void ValidateAll()
		{
		}
	}
}
