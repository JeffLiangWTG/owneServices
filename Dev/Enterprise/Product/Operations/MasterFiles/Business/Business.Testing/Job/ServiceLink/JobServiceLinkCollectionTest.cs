using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobServiceLinkCollection))]
	sealed class JobServiceLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<JobServiceLinkCollection>
	{
		public void TestGetJobServiceLinkByServiceCollection()
		{
			var jobservice = Factory.NewWithValidTestData<JobService>();
			jobservice.ES_ServiceCode = "FUM";
			var jobServiceLink1 = Factory.NewWithValidTestData<JobServiceLink>();
			jobServiceLink1.ESL_ES_JobService = jobservice.PK;
			jobServiceLink1.ESL_Quantity = 1;
			jobServiceLink1.ESL_ParentTableCode = "KP";
			var jobServiceLink2 = Factory.NewWithValidTestData<JobServiceLink>();
			jobServiceLink2.ESL_ES_JobService = jobservice.PK;
			jobServiceLink2.ESL_Quantity = 2;
			jobServiceLink2.ESL_ParentTableCode = "KP";
			Factory.Save();

			var links = new JobServiceLinkCollection(jobservice);
			AssertNotNull(links);
			AssertEquals("Should have 2 links", 2, links.Count);

			AssertEquals("Method should match", 1, links.Single(s => s.PK == jobServiceLink1.PK).ESL_Quantity);
			AssertEquals("Method should match", 2, links.Single(s => s.PK == jobServiceLink2.PK).ESL_Quantity);
		}

		protected override JobServiceLinkCollection GetCollectionToTest()
		{
			return new JobServiceLinkCollection(Factory.NewWithValidTestData<JobService>());
		}
	}
}
