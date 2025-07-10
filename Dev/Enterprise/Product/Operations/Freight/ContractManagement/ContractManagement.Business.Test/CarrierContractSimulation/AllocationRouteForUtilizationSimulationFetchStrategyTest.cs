using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Test
{
	[TestedType(typeof(AllocationRouteForUtilizationSimulationFetchStrategy))]
	internal class AllocationRouteForUtilizationSimulationFetchStrategyTest : TestCaseWithFactory
	{
		public void TestNamedAccountPivotFetchHints()
		{
			var factory = new BusinessObjectFactory();
			var contract = factory.NewWithValidTestData<RatingContract>();

			foreach (var n in Enumerable.Range(0, 3))
			{
				var allocationLine = factory.NewWithValidTestData<RatingContractAllocationLine>();
				var namedAccount = factory.NewWithValidTestData<OrgHeader>();
				allocationLine.NamedAccountPivots.AddChild(namedAccount);
				contract.Allocations.Add(allocationLine);
			}

			factory.Save();

			var newFactory = new BusinessObjectFactory();

			foreach (var allocation in contract.Allocations)
			{
				newFactory.Load<AllocationRouteForUtilizationSimulation>(allocation.PK);
			}

			AssertEquals("Should not have db hit", 0, newFactory.GetTableHitCount(RatingContractNamedAccountPivotSchema.Constants.TableName));

			foreach (var allocation in contract.Allocations)
			{
				var query = new ZQuery(RatingContractNamedAccountPivotSchema.RNP_ParentID, allocation.PK);
				query.AddToFilter(new ZQuery(RatingContractNamedAccountPivotSchema.RNP_ParentTableCode, allocation.TablePrefix));
				newFactory.Load<RatingContractNamedAccountPivot>(query);
			}

			AssertEquals("Should not have extra db hit", 1, newFactory.GetTableHitCount(RatingContractNamedAccountPivotSchema.Constants.TableName));
		}

		public void TestJobSailingFetchHints()
		{
			var factory = new BusinessObjectFactory();
			var contract = factory.NewWithValidTestData<RatingContract>();

			foreach (var n in Enumerable.Range(0, 3))
			{
				var allocationLine = factory.NewWithValidTestData<RatingContractAllocationLine>();
				var sailing = factory.NewWithValidTestData<JobSailing>();
				allocationLine.RCA_JX_SailingSchedule = sailing.PK;
				contract.Allocations.Add(allocationLine);
			}

			factory.Save();

			var newFactory = new BusinessObjectFactory();

			foreach (var allocation in contract.Allocations)
			{
				newFactory.Load<AllocationRouteForUtilizationSimulation>(allocation.PK);
			}

			AssertEquals("Should not have db hit", 0, newFactory.GetTableHitCount(JobSailingSchema.Constants.TableName));

			foreach (var allocation in contract.Allocations)
			{
				newFactory.Load<JobSailing>(allocation.RCA_JX_SailingSchedule);
			}

			AssertEquals("Should not have extra db hit", 1, newFactory.GetTableHitCount(JobSailingSchema.Constants.TableName));
		}

		public void TestViewRatingContractAllocationLineSummaryFetchHints()
		{
			var factory = new BusinessObjectFactory();
			var contract = factory.NewWithValidTestData<RatingContract>();

			foreach (var n in Enumerable.Range(0, 3))
			{
				var allocationLine = factory.NewWithValidTestData<RatingContractAllocationLine>();
				contract.Allocations.Add(allocationLine);

				var consol = factory.New<CommonConsol>();
				var container = consol.Containers.AddNew();
				container.JC_RCA_AllocationLine = allocationLine.PK;
			}

			factory.Save();

			var newFactory = new BusinessObjectFactory();

			foreach (var allocation in contract.Allocations)
			{
				newFactory.Load<AllocationRouteForUtilizationSimulation>(allocation.PK);
			}

			AssertEquals("Should not have db hit", 0, newFactory.GetTableHitCount(ViewRatingContractAllocationLineSummarySchema.Constants.TableName));

			foreach (var allocation in contract.Allocations)
			{
				newFactory.Load<ViewRatingContractAllocationLineSummary>(allocation.PK);
			}

			AssertEquals("Should not have extra db hit", 1, newFactory.GetTableHitCount(ViewRatingContractAllocationLineSummarySchema.Constants.TableName));
		}
	}
}
