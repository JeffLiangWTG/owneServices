using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GenericJobExtensionTest : TestCaseWithFactory
	{
		JobHeader Job1;
		JobHeader Job2;

		protected override void SetUp()
		{
			base.SetUp();

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001";
			Job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Job1.JH_ParentID = shipment.PK;
			Job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingConsol)));
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C00001";
			Job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Job2.JH_ParentID = consol.PK;
			Job2.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();
		}

		public void TestAddGenericJobQueryHintSeparatesForDifferentTables()
		{
			var factory = new BusinessObjectFactory();
			factory.AddGenericJobQueryHint(Job1.JH_ParentID, Job1.JH_ParentTableCode);
			factory.AddGenericJobQueryHint(Job2.JH_ParentID, Job2.JH_ParentTableCode);
			factory.AddGenericJobQueryHint(ZGuid.NewZGuid(), Job2.JH_ParentTableCode);

			AssertEquals(0, factory.DatabaseLoadCount);
			AssertEquals(3, factory.ActiveFetchHintsForTable(ViewGenericJobSchema.Constants.TableName));

			factory.LoadGenericJob(ZGuid.NewZGuid(), "JS");
			AssertEquals(3, factory.DatabaseLoadCount);
		}

		public void TestLoadGenericJobWithJobHeader()
		{
			var genericJobFound = Factory.LoadGenericJob(Job1);
			AssertNotNull("Job1 should be found", genericJobFound);
			AssertEquals("Job1 should be found", "S00001", genericJobFound.Consumer.JobNumber);
			AssertEquals("Job1 should be found", Job1.JH_ParentID, genericJobFound.Consumer.PK);

			genericJobFound = Factory.LoadGenericJob(Job2);
			AssertNotNull("Job2 should be found", genericJobFound);
			AssertEquals("Job2 should be found", "C00001", genericJobFound.Consumer.JobNumber);
			AssertEquals("Job2 should be found", Job2.JH_ParentID, genericJobFound.Consumer.PK);
		}

		public void TestLoadGenericJobFromJobHeader()
		{
			var genericJobFound = Job1.LoadGenericJob();
			AssertNotNull("Job1 should be found", genericJobFound);
			AssertEquals("Job1 should be found", "S00001", genericJobFound.Consumer.JobNumber);
			AssertEquals("Job1 should be found", Job1.JH_ParentID, genericJobFound.Consumer.PK);

			genericJobFound = Job2.LoadGenericJob();
			AssertNotNull("Job2 should be found", genericJobFound);
			AssertEquals("Job2 should be found", "C00001", genericJobFound.Consumer.JobNumber);
			AssertEquals("Job2 should be found", Job2.JH_ParentID, genericJobFound.Consumer.PK);
		}

		public void TestLoadGenericJobWithPKAndTableCode()
		{
			var genericJobFound = Factory.LoadGenericJob(Job1.JH_ParentID, JobShipmentSchema.Constants.Prefix);
			AssertNotNull("should be found", genericJobFound);
			AssertEquals("Job1 should be found", "S00001", genericJobFound.Consumer.JobNumber);
			AssertEquals("Job1 should be found", Job1.JH_ParentID, genericJobFound.Consumer.PK);

			genericJobFound = Factory.LoadGenericJob(Job1.JH_ParentID, JobConsolSchema.Constants.Prefix);
			AssertNull("should not be found", genericJobFound);

			genericJobFound = Factory.LoadGenericJob(Job2.JH_ParentID, JobConsolSchema.Constants.Prefix);
			AssertNotNull("should be found", genericJobFound);
			AssertEquals("Job2 should be found", "C00001", genericJobFound.Consumer.JobNumber);
			AssertEquals("Job2 should be found", Job2.JH_ParentID, genericJobFound.Consumer.PK);

			genericJobFound = Factory.LoadGenericJob(Job2.JH_ParentID, JobShipmentSchema.Constants.Prefix);
			AssertNull("should not be found", genericJobFound);
		}

		public void TestLoadGenericJobWithEmptyParameters()
		{
			var genericJob = Factory.LoadGenericJob(ZGuid.Empty, string.Empty);
			AssertNull(genericJob);
			AssertNull("Should be no error reported", ErrorReporter.LastExceptionReported);

			genericJob = Factory.LoadGenericJob(ZGuid.Empty, Job1.JH_ParentTableCode);
			AssertNull(genericJob);
			AssertNull("Should be no error reported", ErrorReporter.LastExceptionReported);

			genericJob = Factory.LoadGenericJob(Job1.JH_ParentID, Job1.JH_ParentTableCode);
			AssertNotNull(genericJob);
			AssertNull("Should be no error reported", ErrorReporter.LastExceptionReported);

			genericJob = Factory.LoadGenericJob(Job1.JH_ParentID, "");
			AssertNull(genericJob);
			AssertContains("ViewGenericJob should not be queried by empty parent table code.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			genericJob = Factory.LoadGenericJob(null);
			AssertNull(genericJob);
			AssertNull("Should be no error reported", ErrorReporter.LastExceptionReported);
		}
	}
}
