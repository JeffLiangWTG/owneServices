using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks
{
	public abstract class GMDCustomsMessagingService : BranchCustomsMessagingService
	{
		protected GMDCustomsMessagingService() { }

		protected sealed override Guid[] GetBranchPKsHavingDataToProcess()
		{
			var sqlText = FormattableString.Invariant($@"SELECT DISTINCT {EDIInterchange.Schema.EI_GB}
FROM {EDIInterchangeSchema.Constants.SqlSchemaName}.{EDIInterchangeSchema.Constants.TableName}
WHERE {EDIInterchange.Schema.EI_Status} = '{EDIInterchange.Status.Queued}'
AND {EDIInterchange.Schema.EI_ReceiveTransmit} = '{EDIInterchange.Direction.Receive}'
AND {EDIInterchange.Schema.EI_IsActive} = 1
AND {EDIInterchange.Schema.EI_InterchangeType} IN ({InterchangeTypesAsSQL})
AND {EDIInterchange.Schema.EI_ApplicationCode} = '{EDIInterchange.ApplicationCodes.GenericMessageDelivery}'");

			var branchPKCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			branchPKCollection.Load(sqlText);
			return branchPKCollection.Select(x => ((ZGuid)x[EDIInterchangeSchema.EI_GB]).ToGuid()).ToArray();
		}

		string InterchangeTypesAsSQL => new ZStringBuilder(InterchangeTypes.Select(x => "'" + x + "'")).ToStringWithDelimiterBetweenAppends(", ");

		protected abstract IEnumerable<ZString> InterchangeTypes { get; }

		protected abstract GMDInboundInterchangeProcessor GetNewGMDInboundInterchangeProcessor();

		protected sealed override ICustomsServiceTaskProcess GetNewProcess() => GetNewGMDInboundInterchangeProcessor();
	}
}
