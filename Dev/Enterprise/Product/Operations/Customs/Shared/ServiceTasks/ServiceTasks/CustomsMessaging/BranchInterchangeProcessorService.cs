using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks
{
	public abstract class BranchInterchangeProcessorService : BranchCustomsMessagingService
	{
		protected BranchInterchangeProcessorService() { }

		protected sealed override Guid[] GetBranchPKsHavingDataToProcess()
		{
			var sqlText = FormattableString.Invariant($@"SELECT DISTINCT {EDIInterchange.Schema.EI_GB}
FROM {EDIInterchangeSchema.Constants.SqlSchemaName}.{EDIInterchangeSchema.Constants.TableName}
WHERE {EDIInterchange.Schema.EI_Status} = '{EDIInterchange.Status.Queued}'
AND {EDIInterchange.Schema.EI_ReceiveTransmit} = '{EDIInterchange.Direction.Receive}'
AND {EDIInterchange.Schema.EI_IsActive} = 1
AND {EDIInterchange.Schema.EI_ApplicationCode} IN ({ApplicationCodesAsSQL})");

			var branchPKCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			branchPKCollection.Load(sqlText);
			return branchPKCollection.Select(x => ((ZGuid)x[EDIInterchangeSchema.EI_GB]).ToGuid()).ToArray();
		}

		string ApplicationCodesAsSQL => new ZStringBuilder(ApplicationCodes.Select(x => "'" + x + "'")).ToStringWithDelimiterBetweenAppends(", ");

		protected abstract IEnumerable<string> ApplicationCodes { get; }

		protected abstract BranchInboundInterchangeProcessor GetNewBranchInboundInterchangeProcessor();

		protected sealed override ICustomsServiceTaskProcess GetNewProcess() => GetNewBranchInboundInterchangeProcessor();
	}
}
