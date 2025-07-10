using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business.GPS;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
		Enterprise.Freight.LocalCartage.Service.GPSUpdateCartageTask.Code,
		"GPS Update Port Transport Task",
		"GPS",
		typeof(Enterprise.Freight.LocalCartage.Service.GPSUpdateCartageTask),
		MinimumPeriod = "5minutes",
		// GPSCartageLegUpdater converts UTC to local time, which is affected by the current branch. This needs to be fixed before CanRunInAnyBranch can be made true.
		CanRunInAnyBranch = false,
		DefaultScheduleRunEvery = "15minutes"
		)]

[assembly: HostedServiceBusinessObjectBinding(
		Enterprise.Freight.LocalCartage.Service.GPSUpdateCartageTask.Code,
		LocalCartageVehicleActivitySchema.Constants.TableName,
		new[] { LocalCartageVehicleActivitySchema.Constants.EN_EventType + "!=CUS" }, null)]

[assembly: HostedServiceBusinessObjectBinding(
		Enterprise.Freight.LocalCartage.Service.GPSUpdateCartageTask.Code,
		GlbDeviceLocationSchema.Constants.TableName,
		new[] { GlbDeviceLocationSchema.Constants.V2_SpeedLimitState + "=M" }, null)]

namespace Enterprise.Freight.LocalCartage.Service
{
	public class GPSUpdateCartageTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			new GPSCartageLegUpdater(new BusinessObjectFactory()).Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}

		public const string Code = "UCT";
	}
}
