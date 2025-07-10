using CargoWise.Common;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public interface IJobSavingWithoutMutexErrorReporter
	{
		void ReportErrorWhenJobIsSavedWithoutMutex(JobHeader job, ZGlobalMutex mutex);
	}

	class JobSavingWithoutMutexErrorReporter : IJobSavingWithoutMutexErrorReporter
	{
		void IJobSavingWithoutMutexErrorReporter.ReportErrorWhenJobIsSavedWithoutMutex(JobHeader job, ZGlobalMutex mutex)
		{
			if (!job.IsInDatabase && job.Parent != null && job.Parent.IsInDatabase && !(mutex?.HasLock ?? false)
				&& !job.HasContext(BusinessContext.JobIsSavedSoonAfterDisposal_RecheckIfThisStillTrue))
			{
				var collectedInfo_JobDisposedBeforeSaving = CriticalValidationInfoCollectorService.GetService(job.Factory).GetInfoSafe(job.PK, CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving);

				ErrorReporter.ReportDeveloperExceptionOnce("JobIsBeenSavedWithoutMutexForSavedParent_3", $@"
{collectedInfo_JobDisposedBeforeSaving}

{job.GetAllPropertyValues()}

{job.ConstructorStackTrace}
"
, new DeveloperNotificationException($@"
There are 2 rules for job mutexes:
1. Job either should be deleted or saved, you do not need to call Dispose() in this case.
2. Dispose() have to be called only if you never going to save its factory, like cancelling form or going out of scope of its factory."));
			}
		}
	}
}
