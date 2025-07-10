using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Common.ErrorReporting;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner
{
	public class ProcessorRunner
	{
		public ProcessorRunner(ILogHelper logHelper, IStagingRepository stage, IErrorReportingWrapper errorReportingWrapper)
		{
			Argument.NotNull(logHelper, nameof(logHelper));
			Argument.NotNull(stage, nameof(stage));
			Argument.NotNull(errorReportingWrapper, nameof(errorReportingWrapper));

			this.logHelper = logHelper;
			this.stage = stage;
			this.errorReportingWrapper = errorReportingWrapper;
		}

		public void Run(ProcessorInfo info, Func<Tuple<int, bool>> action)
		{
			Argument.NotNull(info, nameof(info));
			Argument.NotNull(action, nameof(action));

			var result = Tuple.Create(0, true);
			var processId = Guid.NewGuid();

			logHelper.LogInfo($"ProcessStart Id: {processId}. ");
			try
			{
				result = action();
				logHelper.LogInfo($"ProcessEnd Id: {processId}. EffectedRecords: {result.Item1}.");
			}
			catch (Exception ex)
			{
				errorReportingWrapper.PostCrashReport(ex, null, null);
				logHelper.LogError(ex.ToString());
				result = Tuple.Create(result.Item1, false);
				throw;
			}
			finally
			{
				var status = GetOrCreate(info);
				status.PRC_Status = StatusProvider.GetERRStatus();
				if (result.Item2)
				{
					status.PRC_LastSuccessRunTime = status.PRC_LastRunTime;
					status.PRC_Status = StatusProvider.GetPRSStatus();
				}
				if (result.Item1 > 0)
				{
					status.PRC_LastSuccessRecordUpdatedCount = result.Item1;
					status.PRC_LastDataSetUpdatedTime = status.PRC_LastRunTime;
				}
				stage.SaveChanges();
			}
		}

		ProcessorStatus GetOrCreate(ProcessorInfo info)
		{
			Argument.NotNull(info, nameof(info));
			var result = stage.Get<ProcessorStatus>().FirstOrDefault(x => x.PRC_SchedName == info.SchedName
			&& x.PRC_JobGroup == info.JobGroup && x.PRC_JobName == info.JobName);
			if (result == null)
			{
				result = new ProcessorStatus
				{
					PRC_PK = Guid.NewGuid(),
					PRC_JobName = info.JobName,
					PRC_JobGroup = info.JobGroup,
					PRC_SchedName = info.SchedName
				};
				stage.Add(result);
			}
			result.PRC_LastRunTime = DateTime.UtcNow;
			return result;
		}

		readonly ILogHelper logHelper;
		readonly IStagingRepository stage;
		readonly IErrorReportingWrapper errorReportingWrapper;
	}
}
