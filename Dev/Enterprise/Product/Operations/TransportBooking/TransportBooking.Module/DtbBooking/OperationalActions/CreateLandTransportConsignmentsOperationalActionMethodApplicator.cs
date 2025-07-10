using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportBookings.Module.OperationalActions
{
	public class CreateLandTransportConsignmentsOperationalActionMethodApplicator : DtbBookingOperationalActionMethodApplicator
	{
		public CreateLandTransportConsignmentsOperationalActionMethodApplicator(string name, BusinessObjectFactory factory)
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
			var processingManager = new BookingToTransportJobCommonCreator(buffer, AutoCreatorTargetModules.Codes.LandTransportConsignment);

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
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("DDEA5BBE-9A87-4FBC-B180-29D1C3178FF1", "Selected Transport Bookings need to exist in database."));
			}
		}

		public CreateLandTransportConsignmentsOperationalActionMethodApplicatorValidation Validation
		{
			get { return new CreateLandTransportConsignmentsOperationalActionMethodApplicatorValidation(this); }
		}
	}

	public class CreateLandTransportConsignmentsOperationalActionMethodApplicatorValidation : ZValidation
	{
		public CreateLandTransportConsignmentsOperationalActionMethodApplicatorValidation(CreateLandTransportConsignmentsOperationalActionMethodApplicator applicator)
			: base(applicator)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(CreateLandTransportConsignmentsOperationalActionMethodApplicator); }
		}

		public override void ValidateAll()
		{
		}
	}
}
