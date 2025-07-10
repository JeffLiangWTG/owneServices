using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(EDIInterchangeTypeList.Codes.EHubRegistryUpdate,
	Enterprise.Customs.ServiceTasks.EHubRegistryUpdateServiceTask.Constants.Description,
	Enterprise.Customs.ServiceTasks.EHubRegistryUpdateServiceTask.Constants.Category,
	typeof(Enterprise.Customs.ServiceTasks.EHubRegistryUpdateServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(EDIInterchangeTypeList.Codes.EHubRegistryUpdate,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + EDIInterchangeTypeList.Codes.EHubRegistryUpdate,
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.eHub },
	null)]

[assembly: HostedServiceBusinessObjectBinding(EDIInterchangeTypeList.Codes.EHubRegistryUpdate,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + EDIInterchangeTypeList.Codes.Configuration,
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.eHub },
	null)]

namespace Enterprise.Customs.ServiceTasks
{
	public class EHubRegistryUpdateServiceTask : CustomsServiceTask
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "service task")]
		public static class Constants
		{
			public const string Category = "ESV";
			public const string Description = "eHub Registry/Password Update";
		}

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branchPK in GetBranchPKsHavingDataToProcess())
			{
				using (DisposableEnvironment.ForBranch(branchPK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() => new EHubRegistryUpdateInterchangeProcessor(Logger).ExecuteBatch(token));
				}
			}
		}

		ZGuid[] GetBranchPKsHavingDataToProcess()
		{
			var sqlText = string.Format(Culture.Invariant, @"SELECT DISTINCT {0}
FROM {1}
WHERE {2} = @Status
AND {3} = @Direction
AND {4} = 1
AND {5} IN ({7})
AND {6} = @ApplicationCode"
	, EDIInterchange.Schema.EI_GB // {0}
	, EDIInterchange.Schema.TableName // {1}
	, EDIInterchange.Schema.EI_Status // {2}
	, EDIInterchange.Schema.EI_ReceiveTransmit // {3}
	, EDIInterchange.Schema.EI_IsActive // {4}
	, EDIInterchange.Schema.EI_InterchangeType // {5}
	, EDIInterchange.Schema.EI_ApplicationCode // {6}
	, string.Join(", ", interchangeTypesToProcess.Select(x => "'" + x + "'")) // {7}
	);
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@Status", EDIInterchange.Status.Queued, EDIInterchangeSchema.EI_Status);
			parameters.Add("@Direction", EDIInterchange.Direction.Receive, EDIInterchangeSchema.EI_ReceiveTransmit);
			parameters.Add("@ApplicationCode", EDIInterchange.ApplicationCodes.eHub, EDIInterchangeSchema.EI_ApplicationCode);

			var branchPKCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			branchPKCollection.Load(sqlText, parameters);
			return branchPKCollection.Select(x => new ZGuid(x[EDIInterchangeSchema.EI_GB])).ToArray();
		}

		readonly string[] interchangeTypesToProcess = new[] { EDIInterchangeTypeList.Codes.EHubRegistryUpdate, EDIInterchangeTypeList.Codes.Configuration };
	}
}
