using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Integration.Agency;
using static Enterprise.Freight.Integration.CFS;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SailingRelatedJob))]
	sealed class SailingRelatedJobTest : BusinessObjectBaseTestCase
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

			var portTransportForCurrentCompany = (BusinessObject)Factory.New<ICommonCartage>();
			portTransportForCurrentCompany.FillWithValidTestData();
			portTransportForCurrentCompany["JJ_JX_Sailing"] = sailing.PK;
			portTransportForCurrentCompany["JJ_GB"] = GlbBranch.CurrentBranch.PK;

			var portTransportForDifferentCompany = (BusinessObject)Factory.New<ICommonCartage>();
			portTransportForDifferentCompany.FillWithValidTestData();
			portTransportForDifferentCompany["JJ_JX_Sailing"] = sailing.PK;
			portTransportForDifferentCompany["JJ_GB"] = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;

			Factory.Save();

			var shipmentJob = (SailingRelatedJob)sailing.RelatedJobs.FindByPK(shipment.PK);
			var portTransportJobForCurrentCompany = (SailingRelatedJob)sailing.RelatedJobs.FindByPK(portTransportForCurrentCompany.PK);
			var portTransportJobForDifferentCompany = (SailingRelatedJob)sailing.RelatedJobs.FindByPK(portTransportForDifferentCompany.PK);

			Assert("Can edit jobs with no company specified", shipmentJob.CanBeEditedByCurrentCompany);
			Assert("Can edit jobs with current company", portTransportJobForCurrentCompany.CanBeEditedByCurrentCompany);
			Assert("Can't edit jobs with another company", !portTransportJobForDifferentCompany.CanBeEditedByCurrentCompany);
		}

		public void TestLoadBizObj()
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

			var shipment = (CommonShipment)Factory.New<IAgencyBooking>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;

			var mainTrasnport = shipment.Transports.AddNew();
			mainTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			mainTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			mainTrasnport.JW_IsLinked = false;

			var otherTrasnport = shipment.Transports.AddNew();
			otherTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			otherTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			otherTrasnport.JW_IsLinked = true;
			otherTrasnport.JW_JX = sailing.PK;

			Factory.Save();

			var shipmentJob = sailing.RelatedJobs[0];

			AssertEquals(shipment.PK, shipmentJob.BizObj.PK);
		}

		public void TestInvalidVoyageRelatedJobTypeKeyIsReported()
		{
			var job = Factory.New<SailingRelatedJob>();
			job.VJX_JobType = "XXX";
			AssertNull("Job Type should be null", job.JobType);
			ErrorReporter.Clear();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var shipment = (CommonShipment)Factory.New<ICFSShipment>();
			shipment.FillWithValidTestData();
			shipment.JS_JX = sailing.PK;

			Factory.Save();

			var collection = new SailingRelatedJobCollection(sailing);
			collection.Load();
			return collection[0];
		}
	}
}
