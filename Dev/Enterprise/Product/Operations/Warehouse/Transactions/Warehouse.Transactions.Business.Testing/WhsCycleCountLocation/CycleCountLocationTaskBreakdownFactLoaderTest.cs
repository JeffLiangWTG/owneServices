using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Facts;
using WTG.ProductionRules.Business.ProductWarehouseCycleCountTaskCreation;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class CycleCountLocationTaskBreakdownFactLoaderTest : CycleCountLocationFactLoaderTest<ICycleCountLocationTaskBreakdownFactLoader, CycleCountLocationTaskBreakdownFactLoader>
	{
		public void TestLoadInputFacts_TaskBreakdownSpecificFields()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");

			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount1.WCL_Priority = 0;
			cycleCount1.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount2.WCL_Priority = 7;
			cycleCount2.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITaskManagementCycleCountLocationFact>().ToArray();
			AssertEquals("There are 2 locations", 2, locationFacts.Length);
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.LocationPK == location2.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(ITaskManagementCycleCountLocationFact.Granularity), CycleCountGranularity.Codes.ProductWithAttributes, locationFact1.Granularity);
				AssertEquals(nameof(ITaskManagementCycleCountLocationFact.Priority), 21, locationFact1.Priority);
				AssertEquals(nameof(ICycleCountLocationFact.CycleCountTaskExists), true, ((ICycleCountLocationFact)locationFact1).CycleCountTaskExists);

				AssertEquals("Should set number of lines.", 1, locationFact1.Grouping.Fact.NumberOfLines);
				AssertEquals("Should *not* set other quantities.", 0, locationFact1.Grouping.Fact.NumberOfPacks);
				AssertEquals("Should *not* set other quantities.", 0, locationFact1.Grouping.Fact.NumberOfUnits);
				AssertEquals("Should *not* set other quantities.", 0m, locationFact1.Grouping.Fact.Weight);
				AssertEquals("Should *not* set other quantities.", 0m, locationFact1.Grouping.Fact.Volume);
				AssertEquals(nameof(CycleCountLocationCoreFact.EntityPK), cycleCount1.PK, ((CycleCountLocationCoreFact)locationFact1.Grouping.Fact).EntityPK);
				AssertEquals(nameof(ITaskManagemementTaskLink.PK), cycleCount1.PK, locationFact1.Grouping.Fact.PK);

				AssertEquals(nameof(ITaskManagementCycleCountLocationFact.Granularity), CycleCountGranularity.Codes.PalletIDOnly, locationFact2.Granularity);
				AssertEquals(nameof(ITaskManagementCycleCountLocationFact.Priority), 7, locationFact2.Priority);
				AssertEquals(nameof(ICycleCountLocationFact.CycleCountTaskExists), true, ((ICycleCountLocationFact)locationFact2).CycleCountTaskExists);
				AssertEquals("Should set number of lines.", 1, locationFact2.Grouping.Fact.NumberOfLines);
				AssertEquals("Should *not* set other quantities.", 0, locationFact2.Grouping.Fact.NumberOfPacks);
				AssertEquals("Should *not* set other quantities.", 0, locationFact2.Grouping.Fact.NumberOfUnits);
				AssertEquals("Should *not* set other quantities.", 0m, locationFact2.Grouping.Fact.Weight);
				AssertEquals("Should *not* set other quantities.", 0m, locationFact2.Grouping.Fact.Volume);
				AssertEquals(nameof(CycleCountLocationCoreFact.EntityPK), cycleCount2.PK, ((CycleCountLocationCoreFact)locationFact2.Grouping.Fact).EntityPK);
				AssertEquals(nameof(ITaskManagemementTaskLink.PK), cycleCount2.PK, locationFact2.Grouping.Fact.PK);
			});
		}
		public void TestLoadInputFacts_FiltersTasksThatAreNotReadyForPlanning_Empty()
			=> TestLoadInputFacts_FiltersTasksThatAreNotReadyForPlanning(taskPlanningStatus: string.Empty);

		public void TestLoadInputFacts_FiltersTasksThatAreNotReadyForPlanning_NotReady()
			=> TestLoadInputFacts_FiltersTasksThatAreNotReadyForPlanning(taskPlanningStatus: TaskPlanningStatus.Codes.NotReady);

		public void TestLoadInputFacts_FiltersTasksThatAreNotReadyForPlanning_Planned()
			=> TestLoadInputFacts_FiltersTasksThatAreNotReadyForPlanning(taskPlanningStatus: TaskPlanningStatus.Codes.Planned);

		public void TestLoadInputFacts_FiltersTasksThatAreNotReadyForPlanning_Error()
			=> TestLoadInputFacts_FiltersTasksThatAreNotReadyForPlanning(taskPlanningStatus: TaskPlanningStatus.Codes.Error);

		void TestLoadInputFacts_FiltersTasksThatAreNotReadyForPlanning(string taskPlanningStatus)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount.WCL_TaskPlanningStatus = taskPlanningStatus;
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("Should have 0 locations,", 0, locationFacts.Length);
		}

		public void TestLoadInputFacts_NoCycleCountTask()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("Should have 0 locations,", 0, locationFacts.Length);
		}

		protected override void SetupRow(WhsRow row)
		{
			base.SetupRow(row);

			foreach (var location in row.Locations)
			{
				var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes);
				cycleCount.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			}
		}
	}
}
