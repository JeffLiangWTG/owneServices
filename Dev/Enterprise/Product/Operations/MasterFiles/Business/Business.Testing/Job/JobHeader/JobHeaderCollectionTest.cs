using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobHeaderCollection))]
	public class JobHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobHeaderCollection(Factory, new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			var result = base.GetNewElementToAddToTheCollection();
			Factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			return result;
		}

		public void TestAddNewShouldRaiseAnError()
		{
			var testCollection = new JobHeaderCollection(Factory);
			AssertExceptionThrown<NotSupportedException>("You cannot directly add to this collection. You need to use the JobHeader.Loader to create a new Job.",
				() => testCollection.AddNew());
		}

		public void TestExlcudeSpotQuoteJobs()
		{
			Collection.Load();
			AssertEquals("The collection should be empty as no jobs shoudld be in db", 0, Collection.Count);
			AddJobHeaderForQuote();
			Collection.RemoveAll();
			Collection.Load();
			AssertEquals("The collection should still be empty as header for quotes should not be included", 1, Collection.Count);
			AssertEquals("The one job header should be for shipments not rating", JobShipmentSchema.Constants.Prefix, ((JobHeader)Collection.ToArray()[0]).JH_ParentTableCode);
		}

		void AddJobHeaderForQuote()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();
		}

		public void TestCreateFilter()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader2.MarkAsInactive();
			Factory.Save();

			var testCollection = new JobHeaderCollection(Factory);
			testCollection.Load();
			AssertEquals("Collection should contain only 1 record", 1, testCollection.Count);
			Assert(testCollection.Contains(jobHeader));
		}
	}
}
