using System;
using CargoWise.Data;
using Enterprise.TransportBookings.ServiceTasks;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	DtbBookingQueueServiceTask.ServiceTaskCode,
	DtbBookingQueueServiceTask.QueueName,
	typeof(DtbBookingQueueProvider))]
namespace Enterprise.TransportBookings.ServiceTasks
{
	class DtbBookingQueueProvider : IHostedServiceQueueProvider
	{
		public QueueResult QueueResult
		{
			get
			{
				var result = QueueResult.Error;
				Db.Connection.ExecuteReader("SELECT COUNT(*), ISNULL(MAX(DATEDIFF(second, KMQ_SystemLastEditTimeUtc, GETUTCDATE())), 0) FROM dbo.DtbBookingQueue", reader =>
				{
					result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1)));
				});
				return result;
			}
		}
	}
}
