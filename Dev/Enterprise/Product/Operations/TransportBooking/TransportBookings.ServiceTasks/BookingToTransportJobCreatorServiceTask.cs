using System;
using System.Threading;
using Enterprise.TransportCommon.Registry;
using ServiceManager.Integration.ServiceTasks.CW;

// Category DOM = Land Transport
[assembly: HostedService(
	"TBC", 
	"Transport Booking to Transport Jobs", 
	"DOM", 
	typeof(Enterprise.TransportBookings.ServiceTasks.BookingToTransportJobCreatorServiceTask), 
	MinimumPeriod = "60Seconds", 
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "30minutes"
	)]

namespace Enterprise.TransportBookings.ServiceTasks
{
	public class BookingToTransportJobCreatorServiceTask : ServiceProviderImpl
	{
		[HostedServiceRequirement]
		public static string CheckEffectiveDateIsSet() => HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate, DateTime.MinValue);

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			var processingManager = new BookingToTransportJobCreatorProcessingManager(ServiceLogger);
			processingManager.CreateTransportJobsFromTransportBookings();
		}
	}
}
