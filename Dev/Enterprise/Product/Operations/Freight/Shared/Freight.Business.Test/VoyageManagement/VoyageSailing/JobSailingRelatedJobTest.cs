using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobSailingRelatedJobTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CommonConsol job = Factory.New<CommonConsol>();
			JobSailing sailing = Factory.New<JobSailing>();

			JobSailingRelatedJob sailingRelatedJob = new JobSailingRelatedJob(job, sailing, "JobNumber", DummyControllerIDs.Dummy);
			AssertEquals("Job", job, sailingRelatedJob.Job);
			AssertEquals("Sailing", sailing, sailingRelatedJob.Sailing);
			AssertEquals("JobNumber", sailingRelatedJob.JobNumber);
			AssertEquals("ControllerID", DummyControllerIDs.Dummy, sailingRelatedJob.ControllerID);
		}

		public void TestJobUrl()
		{
			CommonConsol job = Factory.New<CommonConsol>();
			JobSailing sailing = Factory.New<JobSailing>();

			JobSailingRelatedJob sailingRelatedJob = new JobSailingRelatedJob(job, sailing, "JobNumber", DummyControllerIDs.Dummy);
			AssertEquals("JobUrl", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(DummyControllerIDs.Dummy, job.PK.ToGuid()), sailingRelatedJob.JobUrl);
		}
	}
}
