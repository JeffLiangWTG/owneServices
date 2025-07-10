using System.Threading;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.ServiceTask.Deduplication;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	DeduplicationQueueProcessTask.Code,
	DeduplicationQueueProcessTask.FriendlyName,
	"SYS",
	typeof(DeduplicationQueueProcessTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "30minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	DeduplicationQueueProcessTask.Code,
	PatternMatchingResultSchema.Constants.TableName,
	new[]
	{
		PatternMatchingResultSchema.Constants.PMT_Status + "=" + PatternMatchingResult.StatusCodes.Queued
	},
	DeduplicationQueueProcessTask.FriendlyName)]

namespace Enterprise.MasterData.ServiceTask.Deduplication
{
	public sealed class DeduplicationQueueProcessTask : ServiceProviderImpl
	{
		public const string Code = "DQP";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task FriendlyName")]
		public const string FriendlyName = "De-duplication Queue Processing Service";
		readonly IDuplicateDetectorProvider duplicateDetectorProvider;

		public DeduplicationQueueProcessTask()
			: this(new DuplicateDetectorProviderWithThresholdOverrides())
		{
		}

		internal DeduplicationQueueProcessTask(IDuplicateDetectorProvider duplicateDetectorProvider)
		{
			this.duplicateDetectorProvider = duplicateDetectorProvider;
		}

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			DeduplicationQueuePreProcessor.Process(ServiceLogger);

			var branch = GlbBranch.GetFirstActiveBranch();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var orgProcessor = new DeduplicationQueueProcessBatcher<OrgHeader>(duplicateDetectorProvider, duplicateDetectorProvider.DetectDuplicates, OrgHeaderSchema.PK, OrgHeaderSchema.Constants.Prefix);
				orgProcessor.Process(ServiceLogger.GetTaskNotificationSubscriber(), youMustReactToThisToken);

				var personProcessor = new DeduplicationQueueProcessBatcher<GlbPerson>(duplicateDetectorProvider, duplicateDetectorProvider.DetectDuplicates, GlbPersonSchema.PK, GlbPersonSchema.Constants.Prefix);
				personProcessor.Process(ServiceLogger.GetTaskNotificationSubscriber(), youMustReactToThisToken);
			}
		}
	}
}
