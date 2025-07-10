using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportBookings.Module.OperationalActions
{
	public class CreateTransportJobsOperationalActionMethodApplicator : DtbBookingOperationalActionMethodApplicator
	{
		public CreateTransportJobsOperationalActionMethodApplicator(string name, BusinessObjectFactory factory)
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
			var processingManager = new BookingToTransportJobCommonCreator(buffer);

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
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("DA584189-6277-4C7D-A239-784F619D4604", "Selected Transport Bookings need to exist in database."));
			}
		}

		public CreateTransportJobsOperationalActionMethodApplicatorValidation Validation
		{
			get { return new CreateTransportJobsOperationalActionMethodApplicatorValidation(this); }
		}
	}

	public class CreateTransportJobsOperationalActionMethodApplicatorValidation : ZValidation
	{
		public CreateTransportJobsOperationalActionMethodApplicatorValidation(CreateTransportJobsOperationalActionMethodApplicator applicator)
			: base(applicator)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(CreateTransportJobsOperationalActionMethodApplicator); }
		}

		public override void ValidateAll()
		{
		}
	}
}
