using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Test
{
	[TestedType(typeof(CarrierContractForUtilizationSimulationFetchStrategy))]
	internal class CarrierContractForUtilizationSimulationFetchStrategyTest : TestCaseWithFactory
	{
		public void TestNamedAccountPivotFetchHints()
		{
			var factory = new BusinessObjectFactory();
			var contractsList = new List<RatingContract>();

			foreach (var n in Enumerable.Range(0, 3))
			{
				var contract = factory.NewWithValidTestData<RatingContract>();
				var namedAccount = factory.NewWithValidTestData<OrgHeader>();
				contract.NamedAccountPivots.AddChild(namedAccount);
				contractsList.Add(contract);
			}

			factory.Save();

			var newFactory = new BusinessObjectFactory();

			foreach (var contract in contractsList)
			{
				newFactory.Load<CarrierContractForUtilizationSimulation>(contract.PK);
			}

			AssertEquals("Should not have db hit", 0, newFactory.GetTableHitCount(RatingContractNamedAccountPivotSchema.Constants.TableName));

			foreach (var contract in contractsList)
			{
				var query = new ZQuery(RatingContractNamedAccountPivotSchema.RNP_ParentID, contract.PK);
				query.AddToFilter(new ZQuery(RatingContractNamedAccountPivotSchema.RNP_ParentTableCode, contract.TablePrefix));
				newFactory.Load<RatingContractNamedAccountPivot>(query);
			}

			AssertEquals("Should not have extra db hit", 1, newFactory.GetTableHitCount(RatingContractNamedAccountPivotSchema.Constants.TableName));
		}

		public void TestAllocationLineFetchHints()
		{
			var factory = new BusinessObjectFactory();
			var contract = factory.NewWithValidTestData<RatingContract>();

			foreach (var n in Enumerable.Range(0, 3))
			{
				var allocationLine = factory.NewWithValidTestData<RatingContractAllocationLine>();
				contract.Allocations.Add(allocationLine);
			}

			factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.Load<CarrierContractForUtilizationSimulation>(contract.PK);

			AssertEquals("Should not have db hit", 0, newFactory.GetTableHitCount(RatingContractAllocationLineSchema.Constants.TableName));

			foreach (var allocation in contract.Allocations)
			{
				newFactory.Load<RatingContractAllocationLine>(allocation.PK);
			}

			AssertEquals("Should not have extra db hit", 1, newFactory.GetTableHitCount(RatingContractAllocationLineSchema.Constants.TableName));
		}

		public void TestViewRatingContractSummaryFetchHints()
		{
			var factory = new BusinessObjectFactory();
			var contractsList = new List<RatingContract>();

			foreach (var n in Enumerable.Range(0, 3))
			{
				var contract = factory.NewWithValidTestData<RatingContract>();
				var allocationLine = factory.NewWithValidTestData<RatingContractAllocationLine>();
				contract.Allocations.Add(allocationLine);

				var consol = factory.New<CommonConsol>();
				var container = consol.Containers.AddNew();
				container.JC_RCA_AllocationLine = allocationLine.PK;
				contractsList.Add(contract);
			}

			factory.Save();

			var newFactory = new BusinessObjectFactory();
			foreach (var contract in contractsList)
			{
				newFactory.Load<CarrierContractForUtilizationSimulation>(contract.PK);
			}

			AssertEquals("Should not have db hit", 0, newFactory.GetTableHitCount(ViewRatingContractSummarySchema.Constants.TableName));

			foreach (var contract in contractsList)
			{
				var res = newFactory.Load<ViewRatingContractSummary>(contract.PK);
			}

			AssertEquals("Should not have extra db hit", 1, newFactory.GetTableHitCount(ViewRatingContractSummarySchema.Constants.TableName));
		}
	}
}
