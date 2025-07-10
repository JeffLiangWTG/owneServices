using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(RatingContractAllocationLine))]
	internal class RatingContractAllocationLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniversalCopyTemplate_JobConsolsAndJobContainersAreIgnored()
		{
			var elementType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(RatingContractAllocationLine), true);
			var componentType = typeof(RatingContractAllocationLine);
			var copyTemplateTree = new CopyTemplateTree(elementType, componentType, BusinessObjectCopyManager.CopyTreeConfiguration);
			var innerNode = copyTemplateTree.InnerNode as EntityCopyTemplateNode;

			var jobConsolsInnerNode = innerNode.Nodes.Find(node => node.Name == "JobConsols");
			AssertNull(jobConsolsInnerNode);
			var jobContainersInnerNode = innerNode.Nodes.Find(node => node.Name == "JobContainers");
			AssertNull(jobContainersInnerNode);
		}

		#region Implementation

		public void TestParentContractNumber()
		{
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "CON1";

			var allocation = contract.Allocations.AddNew();

			AssertEquals("Contract Number is CON1", "CON1", allocation.ParentContractNumber);
		}

		public void TestNamedAccountsFormatted()
		{
			var allocationLine = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var namedAccount1 = Factory.NewWithValidTestData<OrgHeader>();
			var namedAccount2 = Factory.NewWithValidTestData<OrgHeader>();
			var namedAccount3 = Factory.NewWithValidTestData<OrgHeader>();

			namedAccount1.OH_Code = "AAA";
			namedAccount2.OH_Code = "BBB";
			namedAccount3.OH_Code = "CCC";

			var namedAccountPivot1 = allocationLine.NamedAccountPivots.AddNew() as RatingContractNamedAccountPivot;
			var namedAccountPivot2 = allocationLine.NamedAccountPivots.AddNew() as RatingContractNamedAccountPivot;
			var namedAccountPivot3 = allocationLine.NamedAccountPivots.AddNew() as RatingContractNamedAccountPivot;

			namedAccountPivot1.RNP_OH_NamedAccount = namedAccount1.PK;
			namedAccountPivot2.RNP_OH_NamedAccount = namedAccount2.PK;
			namedAccountPivot3.RNP_OH_NamedAccount = namedAccount3.PK;

			AssertEquals("NamedAccountsFormatted shows all codes", "AAA, BBB, CCC", allocationLine.NamedAccountsFormatted);
		}

		public void TestUtilization()
		{
			var allocationLine = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.Containers;

			var viewRatingContractAllocationLineSummary = Factory.NewWithValidTestData<ViewRatingContractAllocationLineSummary>();
			viewRatingContractAllocationLineSummary.RAV_ContainerCount = 1;
			viewRatingContractAllocationLineSummary.RAV_TEUCount = 2;
			viewRatingContractAllocationLineSummary.RAV_RCA_AllocationLine = allocationLine.PK;

			AssertEquals("Utilization is the summary container count", allocationLine.Utilization, (ZDecimal)viewRatingContractAllocationLineSummary.RAV_ContainerCount);

			allocationLine.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;

			AssertEquals("Utilization is the summary TEU count", allocationLine.Utilization, viewRatingContractAllocationLineSummary.RAV_TEUCount);
		}

		public void TestCapacityWithVariance()
		{
			var allocationLine = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine.RCA_AllocatedQuantity = 5;
			allocationLine.RCA_BookingVariance = 20;

			// Capacity with Variance formula = Allocated Quantity * ( 1 + Booking Variance / 100 )
			AssertEquals("Capacity with Variance formula is calculated correctly", allocationLine.CapacityWithVariance, new ZDecimal(6.0));
		}

		public void TestOutstandingCommitted()
		{
			var allocationLine = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.Containers;
			allocationLine.RCA_AllocatedQuantity = 5;

			var viewRatingContractAllocationLineSummary = Factory.NewWithValidTestData<ViewRatingContractAllocationLineSummary>();
			viewRatingContractAllocationLineSummary.RAV_ContainerCount = 1;
			viewRatingContractAllocationLineSummary.RAV_TEUCount = 2;
			viewRatingContractAllocationLineSummary.RAV_RCA_AllocationLine = allocationLine.PK;

			AssertEquals("Outstanding Committed is calculated correctly", allocationLine.OutstandingCommitted, new ZDecimal(4.0));
		}

		public void TestOutstandingWithVariance()
		{
			var allocationLine = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.Containers;
			allocationLine.RCA_AllocatedQuantity = 5;
			allocationLine.RCA_BookingVariance = 20;

			var viewRatingContractAllocationLineSummary = Factory.NewWithValidTestData<ViewRatingContractAllocationLineSummary>();
			viewRatingContractAllocationLineSummary.RAV_ContainerCount = 1;
			viewRatingContractAllocationLineSummary.RAV_TEUCount = 2;
			viewRatingContractAllocationLineSummary.RAV_RCA_AllocationLine = allocationLine.PK;

			AssertEquals("Outstanding with Variance is calculated correctly", allocationLine.OutstandingWithVariance, new ZDecimal(5.0));
		}

		public void TestLinkedScheduleETDUpdated()
		{
			var allocationLine = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var jobSailing = Factory.New<IJobSailing>();
			var voyageOrigin = Factory.New<IVoyageOrigin>();
			jobSailing.JX_JA = voyageOrigin.PK;
			allocationLine.RCA_JX_SailingSchedule = jobSailing.PK;

			voyageOrigin.JA_E_DEP = new ZDateTime(2020, 12, 20);
			voyageOrigin.JA_S_DEP = new ZDateTime(2020, 12, 20);
			AssertEquals("Linked Schedule ETD is not updated", allocationLine.LinkedScheduleETDUpdated, ZBool.False);

			voyageOrigin.JA_E_DEP = new ZDateTime(2020, 12, 20);
			voyageOrigin.JA_S_DEP = new ZDateTime(2020, 12, 21);
			AssertEquals("Linked Schedule ETD is updated", allocationLine.LinkedScheduleETDUpdated, ZBool.True);
		}

		public void TestValuesAreProxiedWhenScheduleIsLinked()
		{
			var allocationLine = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var jobSailing = Factory.New<IJobSailing>();
			var voyageOrigin = Factory.New<IVoyageOrigin>();
			var voyageDestination = Factory.New<IVoyageDestination>();
			var jobVoyageFromOrigin = Factory.New<IJobVoyage>();

			voyageOrigin.JA_JV = jobVoyageFromOrigin.PK;
			jobSailing.JX_JA = voyageOrigin.PK;
			jobSailing.JX_JB = voyageDestination.PK;

			allocationLine.RCA_LoadLocation = "GSOKO";
			allocationLine.RCA_DischargeLocation = "HRUKE";
			allocationLine.RCA_RV_NKVessel = "Orient Express";
			allocationLine.RCA_VoyageNumber = "99999";
			allocationLine.RCA_ServiceLoop = "Choo Choo";

			voyageOrigin.JA_RL_NKPortOfLoading = "AUSYD";
			voyageDestination.JB_RL_NKPortOfDischarge = "GBWCI";
			jobVoyageFromOrigin.JV_RV_NKVessel = "Titanic 2";
			jobVoyageFromOrigin.JV_VoyageFlight = "11111";
			jobSailing.JX_ServiceString = "hello I am a boat";

			AssertEquals("Load Location from Allocation Route", allocationLine.RCA_Calc_LoadLocation, allocationLine.RCA_LoadLocation);
			AssertEquals("Discharge Location from Allocation Route", allocationLine.RCA_Calc_DischargeLocation, allocationLine.RCA_DischargeLocation);
			AssertEquals("Vessel Name from Allocation Route", allocationLine.RCA_Calc_VesselName, allocationLine.RCA_RV_NKVessel);
			AssertEquals("Voyage Number from Allocation Route", allocationLine.RCA_Calc_VoyageNumber, allocationLine.RCA_VoyageNumber);
			AssertEquals("Service String from Allocation Route", allocationLine.RCA_Calc_ServiceString, allocationLine.RCA_ServiceLoop);

			allocationLine.RCA_JX_SailingSchedule = jobSailing.PK;
			AssertEquals("Load Location from linked Schedule", allocationLine.RCA_Calc_LoadLocation, voyageOrigin.JA_RL_NKPortOfLoading);
			AssertEquals("Discharge Location from linked Schedule", allocationLine.RCA_Calc_DischargeLocation, voyageDestination.JB_RL_NKPortOfDischarge);
			AssertEquals("Vessel Name from linked Schedule", allocationLine.RCA_Calc_VesselName, jobVoyageFromOrigin.JV_RV_NKVessel);
			AssertEquals("Voyage Number from linked Schedule", allocationLine.RCA_Calc_VoyageNumber, jobVoyageFromOrigin.JV_VoyageFlight);
			AssertEquals("Service String from linked Schedule", allocationLine.RCA_Calc_ServiceString, jobSailing.JX_ServiceString);
		}

		public void TestTradeLaneCalculatedProperties()
		{
			var allocationLine = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var tradeLane = Factory.New<IJobTradeLane>();

			tradeLane.EJ_Code = "MACE";
			tradeLane.EJ_Description = "WHOA";
			allocationLine.RCA_EJ_TradeLane = tradeLane.PK;

			AssertEquals("Calculated Trade Lane Code", tradeLane.EJ_Code, allocationLine.RCA_Calc_TradeLaneCode);
			AssertEquals("Calculated Trade Lane Description", tradeLane.EJ_Description, allocationLine.RCA_Calc_TradeLaneDescription);
		}

		public void TestParentAllocationRoute()
		{
			var allocationLine = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var parentAllocation = Factory.New<RatingContractAllocationLine>();

			allocationLine.RCA_RCA_ParentAllocationRoute = parentAllocation.PK;

			AssertNotNull(allocationLine.ParentAllocationRoute);
			AssertEquals("Parent Allocation", allocationLine.ParentAllocationRoute.PK, allocationLine.RCA_RCA_ParentAllocationRoute);
		}

		public void TestAllocationDistributions()
		{
			var allocationLine = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var childAllocation = allocationLine.AllocationDistributions.AddNew();
			childAllocation.RCA_RCA_ParentAllocationRoute = allocationLine.PK;

			AssertEquals("Allocation Distribution Count", 1, allocationLine.AllocationDistributions.Count);
			AssertEquals("Allocation Distribution Relationship", allocationLine.PK, childAllocation.RCA_RCA_ParentAllocationRoute);
		}

		#endregion
	}
}
