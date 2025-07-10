using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Integration.CFS;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(VoyageRelatedJob))]
	sealed class VoyageRelatedJobTest : BusinessObjectBaseTestCase
	{
		public void TestCanBeEditedByCurrentCompany()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var voyOrigin = voyage.Origins.AddNew();
			var voyDestination = voyage.Destinations.AddNew();

			voyOrigin.FillWithValidTestData();
			voyOrigin.JA_RL_NKPortOfLoading = "AUBNE";

			voyDestination.FillWithValidTestData();
			voyDestination.JB_RL_NKPortOfDischarge = "DEHAM";

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_JX = sailing.PK;
			shipment.JS_IsCFSRegistered = true;

			var portTransportForCurrentCompany = (BusinessObject)Factory.New<LocalCartage.Integration.ICommonCartage>();
			portTransportForCurrentCompany.FillWithValidTestData();
			portTransportForCurrentCompany["JJ_JX_Sailing"] = sailing.PK;
			portTransportForCurrentCompany["JJ_GB"] = GlbBranch.CurrentBranch.PK;

			var portTransportForDifferentCompany = (BusinessObject)Factory.New<LocalCartage.Integration.ICommonCartage>();
			portTransportForDifferentCompany.FillWithValidTestData();
			portTransportForDifferentCompany["JJ_JX_Sailing"] = sailing.PK;
			portTransportForDifferentCompany["JJ_GB"] = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;

			Factory.Save();

			var shipmentJob = (VoyageRelatedJob)voyage.RelatedJobs.FindByPK(shipment.PK);
			var portTransportJobForCurrentCompany = (VoyageRelatedJob)voyage.RelatedJobs.FindByPK(portTransportForCurrentCompany.PK);
			var portTransportJobForDifferentCompany = (VoyageRelatedJob)voyage.RelatedJobs.FindByPK(portTransportForDifferentCompany.PK);

			Assert("Can edit jobs with no company specified", shipmentJob.CanBeEditedByCurrentCompany);
			Assert("Can edit jobs with current company", portTransportJobForCurrentCompany.CanBeEditedByCurrentCompany);
			Assert("Can't edit jobs with another company", !portTransportJobForDifferentCompany.CanBeEditedByCurrentCompany);
		}

		public void TestInvalidVoyageRelatedJobTypeKeyIsReported()
		{
			var job = Factory.New<VoyageRelatedJob>();
			job.VJV_JobType = "XXX";
			AssertNull("Job Type should be null", job.JobType);
			ErrorReporter.Clear();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var voyOrigin = voyage.Origins.AddNew();
			var voyDestination = voyage.Destinations.AddNew();

			voyOrigin.FillWithValidTestData();
			voyOrigin.JA_RL_NKPortOfLoading = "AUBNE";

			voyDestination.FillWithValidTestData();
			voyDestination.JB_RL_NKPortOfDischarge = "DEHAM";

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			var shipment = (CommonShipment)Factory.New<ICFSShipment>();
			shipment.FillWithValidTestData();
			shipment.JS_JX = sailing.PK;

			Factory.Save();

			var collection = new VoyageRelatedJobCollection(voyage);
			collection.Load();
			return collection[0];
		}
	}
}
