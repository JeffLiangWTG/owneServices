#if DEBUG

using System;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public static class JobRelatedExtensions_ForTestOnly
	{
		public static T NewJobForTesting<T>(this BusinessObjectFactory factory) where T : JobHeader
		{
			try
			{
				factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
				var bizo = factory.New<T>();
				bizo.CreateMockJobSavingWithoutMutexErrorReporter();

				return bizo;
			}
			finally
			{
				factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			}
		}

		public static T NewJobWithPrimaryKeyForTesting<T>(this BusinessObjectFactory factory, Guid initialisingPk) where T : JobHeader
		{
			try
			{
				factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
				var bizo = factory.NewWithPrimaryKey<T>(initialisingPk);
				bizo.CreateMockJobSavingWithoutMutexErrorReporter();

				return bizo;
			}
			finally
			{
				factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			}
		}

		public static T NewJobWithValidTestDataForTesting<T>(this BusinessObjectFactory factory) where T : JobHeader
		{
			try
			{
				factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
				var bizo = factory.NewWithValidTestData<T>();
				bizo.CreateMockJobSavingWithoutMutexErrorReporter();

				return bizo;
			}
			finally
			{
				factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			}
		}

		public static T NewJobWithValidTestDataForTesting<T>(this BusinessObjectFactory factory, TestBusinessObjectKind kind) where T : JobHeader
		{
			try
			{
				factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
				var bizo = factory.NewWithValidTestData<T>(kind);
				bizo.CreateMockJobSavingWithoutMutexErrorReporter();

				return bizo;
			}
			finally
			{
				factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			}
		}

		public static void CreateMockJobSavingWithoutMutexErrorReporter(this JobHeader jobHeader)
		{
			if (jobHeader != null)
			{
				var jobSavingWithoutMutexErrorReporterMock = new Moq.Mock<IJobSavingWithoutMutexErrorReporter>();
				jobHeader.SubstituteJobSavingWithoutMutexErrorReporter_ForTestOnly(jobSavingWithoutMutexErrorReporterMock.Object);
			}
		}

		public static JobHeader TryLoadOrCreateWithoutMutexForTestOnly(this JobHeader.Loader loader)
		{
			var job = loader.TryLoadOrCreate();
			job.CreateMockJobSavingWithoutMutexErrorReporter();

			return job;
		}

		public static JobHeader TryLoadOrCreateWithoutMutexForTestOnly(this JobHeader.Loader loader, GlbBranch branch)
		{
			var job = loader.TryLoadOrCreate(branch);
			job.CreateMockJobSavingWithoutMutexErrorReporter();

			return job;
		}

		public static JobHeader TryCreateWithoutMutexForTestOnly(this JobHeader.Loader loader)
		{
			var job = loader.TryCreate();
			job.CreateMockJobSavingWithoutMutexErrorReporter();

			return job;
		}

		public static JobHeader TryCreateWithoutMutexForTestOnly(this JobHeader.Loader loader, GlbBranch branch)
		{
			var job = loader.TryCreate(branch);
			job.CreateMockJobSavingWithoutMutexErrorReporter();

			return job;
		}
	}
}
#endif
