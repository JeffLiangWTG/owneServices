using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class UCKHostedServiceQueue : UCMHostedServiceQueue
	{
		public UCKHostedServiceQueue(string applicationCode)
			: base(UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen, $"{UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen}-{applicationCode} Queued Service Task", applicationCode, GetQueueResult)
		{
		}

		static QueueResult GetQueueResult(string applicationCode)
		{
			var filter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			filter.AddToFilter(BaseMessageProcessor.ValidTransmitDateMessageFilterUsingUtcTime);
			filter.AddFilterAndZSQLParameterCollection($"NOT EXISTS(SELECT NULL FROM dbo.EDIMessageQueueState WHERE EQS_ApplicationCode = {EDIMessageQueueStateFactory.ApplicationCodeParam} AND EQS_EM = EM_PK)",
				new ZSqlParameterCollection(ZSqlParameter.New(EDIMessageQueueStateFactory.ApplicationCodeParam, applicationCode, EDIMessageQueueStateSchema.EQS_ApplicationCode)));

			var connectionInfo = new ZSqlConnectionInfo(Db.Connection, string.Empty);
			var query = new ZCountDataQuery(connectionInfo, EDIMessageSchema.Constants.TableName, filter);
			using (var cmd = connectionInfo.GetNewDbCommandForSelect(query.ParameterisedQueryText.Replace("COUNT(*)", "COUNT(*),ISNULL(MAX(DATEDIFF(second, EM_SystemCreateTimeUtc, GETUTCDATE())), 0)"), query.Parameters))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					var count = reader.GetInt32(0);
					var age = TimeSpan.FromSeconds(reader.GetInt32(1));
					return new QueueResult(count, age);
				}
			}
			return QueueResult.Zero;
		}
	}
}
