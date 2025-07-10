using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(Trip))]
	sealed class TripWorkflowProviderTest : WorkflowProviderTest<Trip, ProcessTaskCollection<eManifestProcessTask, Trip>>
	{
		public void TestGetTemplateSelectionCriteria()
		{
			var trip = Factory.New<Trip>();
			trip.BH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			trip.BH_OA_Importer = Factory.NewWithValidTestData<OrgAddress>().PK;

			var implementation = (IWorkflowProvider)trip;
			var ranker = (ColumnValueRanker)implementation.GetTemplateSelectionCriteria();
			AssertEquals("P0_GB", trip.BH_GB, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB)[0]);
			AssertEquals("P0_OH_Client", trip.Importer.OA_OH, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
		}

		protected override ZString ExpectedWorkflowType => JobInvoicingConsumerTypes.eManifest.Code;
	}
}
