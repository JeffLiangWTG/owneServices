using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.TransportBookings.ServiceTasks;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	DtbBookingQueueServiceTask.ServiceTaskCode,
	"Transport Job Generator",
	"DOM",
	typeof(DtbBookingQueueServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	ServiceTaskCode = DtbBookingQueueServiceTask.ServiceTaskCode,
	Table = DtbBookingQueueSchema.Constants.TableName,
	Predicates = new string[] { },
	QueueName = null)]
namespace Enterprise.TransportBookings.ServiceTasks
{
	public class DtbBookingQueueServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Log(LogType.Information, QueueName + " service task started.");
			var runner = ObjectFactory.Get<IDtbBookingQueueRunner>();

			try
			{
				runner.Run(youMustReactToThisToken, ServiceLogger);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Log(LogType.Error, FormattableString.Invariant($"{QueueName} service task ended abruptly.\r\nException: {ex.GetType()}\r\nException Message: {ex.Message}.\r\nStack Trace: {ex.StackTrace}."));
			}

			ServiceLogger.Log(LogType.Information, QueueName + " service task ended.");
		}

		public const string ServiceTaskCode = "KMQ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Queue name")]
		public const string QueueName = "Transport Job Generator";
	}
}
