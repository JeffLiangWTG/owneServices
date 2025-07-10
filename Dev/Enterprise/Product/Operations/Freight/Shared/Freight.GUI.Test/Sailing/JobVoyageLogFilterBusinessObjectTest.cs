using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(JobVoyageLogFilterBusinessObject))]
	sealed class JobVoyageLogFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestAdditionalEventsOriginsAreAdded()
		{
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var jobVoyageLogFilterBusinessObject = new JobVoyageLogFilterBusinessObject(jobVoyage);
			var showForFilter = jobVoyageLogFilterBusinessObject
				.ModuleFilters
				.FirstOrDefault(filter => filter.Description.Equals("Show for")) as ModuleTextFilter;

			AssertNotNull("Precondition: \"Show for\" filter should exist.", showForFilter);

			var filterOptions = showForFilter.List as CodeDescriptionPairList;
			AssertNotNull("Precondition: \"Show for\" filter should have a list of options.", filterOptions);

			var relatedJobsOptionDescription = filterOptions.GetDescriptionFromCode(RelatedJobsShowForCode);
			AssertEquals("An option for Related Jobs should exist with the correct description.", $"Jobs related to this {jobVoyage.HumanReadableName}", relatedJobsOptionDescription);
		}

		public void TestGetBusinessObjectsWithRelatedEventsQuery()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2069, 4, 20);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2069, 4, 21);

			voyage.GenerateSailings();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var consolLog = AddNewDEPLogToEnterpriseBizo(consol);

			var transport = consol.Transports[0];
			transport.JW_JX = voyage.Sailings[0].PK;
			var transportLog = AddNewDEPLogToEnterpriseBizo(transport);

			Factory.Save();

			var filterBusinessObject = new TestJobVoyageLogFilterBusinessObject(voyage);
			var query = filterBusinessObject.GetRelatedEventsQuery(RelatedJobsShowForCode);
			var relatedJobStmALogs = Factory.Load<StmALog>(query);

			AssertCollectionContains("Related job logs should contain the consol's DEP log.", relatedJobStmALogs, stmALog => stmALog.PK == consolLog.PK);
			AssertCollectionContains("Related job logs should contain the transport's DEP log.", relatedJobStmALogs, stmALog => stmALog.PK == transportLog.PK);
		}

		StmALog AddNewDEPLogToEnterpriseBizo(EnterpriseBusinessObject bizo)
		{
			var result = bizo.Logs.AddNew();
			using (result.LockForUpdatingKeyFieldsForTesting())
			{
				result.SL_SE_NKEvent = AutoEvents.DepartureCode;
			}

			return result;
		}

		#region Implementation

		const string RelatedJobsShowForCode = "Related jobs";

		class TestJobVoyageLogFilterBusinessObject : JobVoyageLogFilterBusinessObject
		{
			public TestJobVoyageLogFilterBusinessObject(JobVoyage jobVoyage) : base(jobVoyage)
			{
			}

			public ZQuery GetRelatedEventsQuery(ZString showForCode) => GetBusinessObjectsWithRelatedEventsQuery(showForCode);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var jobVoyage = Factory.New<JobVoyage>();
			return new JobVoyageLogFilterBusinessObject(jobVoyage);
		}

		#endregion
	}
}
