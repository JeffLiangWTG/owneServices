using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks
{
	public abstract class BranchMessageProcessorService : BranchCustomsMessagingService
	{
		protected BranchMessageProcessorService() { }

		protected sealed override Guid[] GetBranchPKsHavingDataToProcess()
		{
			var applicationCodes = GetApplicationCodes();

			if (string.IsNullOrEmpty(applicationCodes))
			{
				return Array.Empty<Guid>();
			}

			var messageTypes = GetMessageTypes();
			var sqlText = string.Format(Culture.Invariant, @"SELECT DISTINCT {0}
FROM {1}
WHERE {2} IN (@QueuedStatus, @PreProcessedOKStatus)
AND {3} = @Direction
AND {4} = 1
AND {5} IN ({6})
{7}"
				, EDIMessage.Schema.EM_GB // {0}
				, EDIMessage.Schema.TableName // {1}
				, EDIMessage.Schema.EM_Status // {2}
				, EDIMessage.Schema.EM_ReceiveTransmit // {3}
				, EDIMessage.Schema.EM_IsActive // {4}
				, EDIMessage.Schema.EM_ApplicationCode // {5}
				, applicationCodes // {6}
				, string.IsNullOrEmpty(messageTypes) ? "" : $"AND {EDIMessage.Schema.EM_MessageType} IN ({messageTypes})" // {7}
				);
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@QueuedStatus", EDIMessage.Status.Queued, EDIMessageSchema.EM_Status);
			parameters.Add("@PreProcessedOKStatus", EDIMessage.Status.PreProcessedOK, EDIMessageSchema.EM_Status);
			parameters.Add("@Direction", EDIMessage.Direction.Receive, EDIMessageSchema.EM_ReceiveTransmit);

			var branchPKCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			branchPKCollection.Load(sqlText, parameters);
			return branchPKCollection.Select(x => ((ZGuid)x[EDIMessageSchema.EM_GB]).ToGuid()).ToArray();
		}

		string GetMessageTypes()
		{
			return new ZStringBuilder(MessageTypes.Select(x => "'" + x + "'")).ToStringWithDelimiterBetweenAppends(", ");
		}

		protected abstract IEnumerable<ZString> MessageTypes { get; }

		string GetApplicationCodes()
		{
			return new ZStringBuilder(ApplicationCodes.Select(x => "'" + x + "'")).ToStringWithDelimiterBetweenAppends(", ");
		}

		protected abstract IEnumerable<ZString> ApplicationCodes { get; }
		protected abstract BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor();

		protected sealed override ICustomsServiceTaskProcess GetNewProcess() => GetNewBranchCustomsMessageProcessor();
	}
}
