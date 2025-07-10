using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Business.Testing;
using Enterprise.ProductionRules.ServiceTasks;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[GuiTest]
	[UseSnapshotProtection]
	public class WhsItemCycleCountTaskCreationEndToEndTests : TestCase
	{
		public void TestCycleCountAutomation_NoScheduledRules()
		{
			var warehouse = Helper.CreateWarehouse("WHS");

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1), scheduleRule: false);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(string.Empty, serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_NoAvailableLocations()
		{
			var warehouse = Helper.CreateWarehouse("WHS");

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			RunConsumerAndAssertLogs($"Information|No cycle count tasks were created in this run.");

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_NoCycleCountTasks()
		{
			var (warehouse, location) = CreateWarehouseAndLocation();
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			RunConsumerAndAssertCreatedOne(location.PK);
		}

		public void TestCycleCountAutomation_WithOpenTask()
		{
			var (warehouse, location) = CreateWarehouseAndLocation();
			Factory.Save();

			var cycleCount = Helper.CreateCycleCountLocation(location, CycleCountLocationStatuses.Codes.NotStarted, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);
			Assert("Precondition: Cycle Count is not completed.", cycleCount.WIC_EndTime.IsEmpty);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			RunConsumerAndAssertLogs("Information|No cycle count tasks were created in this run.");

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery());
			AssertEquals("Should found one tasks.", 1, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_WithCompletedTask()
		{
			var (warehouse, location) = CreateWarehouseAndLocation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateCycleCountLocation(location, CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "TTT");
			Assert("Precondition: Cycle Count is completed.", !cycleCount.WIC_EndTime.IsEmpty);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			RunConsumerAndAssertLogs("Information|Created Cycle Count Task for Location: A-1-1.");

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery(WhsItemCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK)).Single();
			AssertEquals("Should have correct location.", location.PK, cycleCountTask.WIC_WL_Location);
		}

		public void TestCycleCountAutomation_WithOpenVariance()
		{
			var (warehouse, location) = CreateWarehouseAndLocation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateCycleCountLocation(location, CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "TTT");
			var locationVariance = Helper.CreateCycleCountLocationVariance(cycleCount, varianceQty: 1);
			AssertEquals("Cycle Count is finalised", false, cycleCount.WIC_EndTime.IsEmpty);
			AssertEquals("Cycle Count Location Variance should be open", CycleCountVarianceStatuses.Codes.Open, locationVariance.WIV_Status);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			RunConsumerAndAssertLogs("Information|No cycle count tasks were created in this run.");

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery(WhsItemCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK));
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_WithFinalisedVariance_Approved()
			=> TestCycleCountAutomation_WithFinalisedVariance(CycleCountVarianceStatuses.Codes.Approved);

		public void TestCycleCountAutomation_WithFinalisedVariance_Rejected()
			=> TestCycleCountAutomation_WithFinalisedVariance(CycleCountVarianceStatuses.Codes.Rejected);

		void TestCycleCountAutomation_WithFinalisedVariance(string varianceStatus)
		{
			var (warehouse, location) = CreateWarehouseAndLocation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateCycleCountLocation(location, CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "TTT");
			var locationVariance = Helper.CreateCycleCountLocationVariance(cycleCount, status: varianceStatus);
			AssertEquals("Cycle Count is finalised", false, cycleCount.WIC_EndTime.IsEmpty);
			AssertEquals("Cycle Count Location Variance should not be open", varianceStatus, locationVariance.WIV_Status);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			RunConsumerAndAssertLogs("Information|Created Cycle Count Task for Location: A-1-1.");

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery(WhsItemCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK)).Single();
			AssertEquals("Should have correct location.", location.PK, cycleCountTask.WIC_WL_Location);
		}

		#region Filter

		public void TestCycleCountAutomation_FiltersDockDoors()
		{
			var (warehouse, location) = CreateWarehouseAndLocation();
			var location1 = Helper.CreateLocation(warehouse, "DockDoor01");
			location1.WLV_WLT_LocationType = Factory.Load<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC")).First().PK;
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			var rule = CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			RunConsumerAndAssertCreatedOne(location.PK, "A-1-1");
		}

		public void TestCycleCountAutomation_OrgFilter_Consignor()
		{
			var (warehouse, location, _, consignor, _, _) = CreateTestDataForOrgFilter();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			var rule = CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1), consignor: consignor.PK);
			Factory.Save();

			RunConsumerAndAssertCreatedOne(location.PK, "A-1-1");
		}

		public void TestCycleCountAutomation_OrgFilter_Consignee()
		{
			var (warehouse, location, consignee, _, _, _) = CreateTestDataForOrgFilter();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			var rule = CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1), consignee: consignee.PK);
			Factory.Save();

			RunConsumerAndAssertCreatedOne(location.PK, "A-1-1");
		}

		public void TestCycleCountAutomation_OrgFilter_BookingParty()
		{
			var (warehouse, location, _, _, bookingParty, _) = CreateTestDataForOrgFilter();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			var rule = CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1), bookingParty: bookingParty.PK);
			Factory.Save();

			RunConsumerAndAssertCreatedOne(location.PK, "A-1-1");
		}

		public void TestCycleCountAutomation_OrgFilter_BillToParty()
		{
			var (warehouse, location, _, _, _, billToParty) = CreateTestDataForOrgFilter();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			var rule = CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1), billToParty: billToParty.PK);
			Factory.Save();

			RunConsumerAndAssertCreatedOne(location.PK, "A-1-1");
		}

		(WhsWarehouse warehouse, WhsLocation location, OrgHeader consignee, OrgHeader consignor, OrgHeader bookingParty, OrgHeader billToParty) CreateTestDataForOrgFilter()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var location1 = Helper.CreateLocation(warehouse, "A-1-1");
			var location2 = Helper.CreateLocation(warehouse, "B-1-1");

			var consignee = Helper.CreateClient("Org1");
			var consignor = Helper.CreateClient("Org2");
			var bookingParty = Helper.CreateClient("Org3");
			var billToParty = Helper.CreateClient("Org4");

			var rcn1 = helper.CreateReceiveConsignment("RCN1", warehouse.PK, bookingParty, consignor, consignee);
			helper.CreateJobDocAddressFromAddress(rcn1, DocAddressTypes.Codes.ClientRequestedBillingParty, billToParty.MainAddress);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location1.PK);
			helper.CreatePackageState(rcn1, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Putaway, receiveUnit: rtu1, location: location1);

			var rcn2 = helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location2.PK);
			helper.CreatePackageState(rcn2, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Putaway, receiveUnit: rtu2, location: location2);

			Factory.Save();
			return (warehouse, location1, consignee, consignor, bookingParty, billToParty);
		}

		#endregion

		#region MultipLocations

		public void TestCycleCountAutomation_MultipleLocations()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 3);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			RunConsumerAndAssertCreatedMultiple(["A-1-1", "A-1-2", "A-1-3", "A-2-1", "A-2-2", "A-2-3"]);
		}

		public void TestCycleCountAutomation_MultipleLocations_MaxTasks()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 3);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1), maximumTasksPerRun: 4);
			Factory.Save();

			RunConsumerAndAssertCreatedMultiple(["A-1-1", "A-1-2", "A-1-3", "A-2-1"]);
		}

		public void TestCycleCountAutomation_MultipleScheduledRules()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(warehouse, "A", 3, 3);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Cycle Count Task 1", ZDateTime.Today.AddDays(-1), maximumTasksPerRun: 3);
			CreateRule(ruleSet, "Create Cycle Count Task 2", ZDateTime.Today.AddDays(-1), maximumTasksPerRun: 3, rulePriority: 2);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Cycle Count Task 1, Create Cycle Count Task 2.
Information|Created Cycle Count Task for Location: A-1-1.
Information|Created Cycle Count Task for Location: A-1-2.
Information|Created Cycle Count Task for Location: A-1-3.
Information|Created Cycle Count Task for Location: A-2-1.
Information|Created Cycle Count Task for Location: A-2-2.
Information|Created Cycle Count Task for Location: A-2-3.
Information|Succesfully processed and saved RuleSet: Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Cycle Count Task 1, Create Cycle Count Task 2.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created 6 tasks.", 6, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_MultipleScheduledRules_MultipleRuleSets()
		{
			var warehouse1 = Helper.CreateWarehouse("WH1");
			var whs1RowA = Helper.CreateRowAndGenerateLocations(warehouse1, "A", 1, 1);

			var warehouse2 = Helper.CreateWarehouse("WH2");
			var whs2RowA = Helper.CreateRowAndGenerateLocations(warehouse2, "A", 1, 1);
			Factory.Save();

			var ruleSet1 = ProductionRuleHelper.CreateRuleSet("Cycle Counting 1", "Cycle Counting", true, context: "TWC", warehousePK: warehouse1.PK);
			CreateRule(ruleSet1, "Create Cycle Count Task", ZDateTime.Today.AddDays(-2), priority: 1);

			var ruleSet2 = ProductionRuleHelper.CreateRuleSet("Cycle Counting 2", "Cycle Counting", true, context: "TWC", warehousePK: warehouse2.PK);
			CreateRule(ruleSet2, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), priority: 2);

			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting 1 for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A.
Information|Succesfully processed and saved RuleSet: Cycle Counting 1 for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Cycle Count Task.
Information|Processing RuleSet: Cycle Counting 2 for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A.
Information|Succesfully processed and saved RuleSet: Cycle Counting 2 for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created 2 tasks.", 2, cycleCountTasks.Length);

			var whs1Task = cycleCountTasks.Single(cct => cct.WIC_WL_Location == whs1RowA.Locations.Single().PK);
			AssertEquals("Should have correct priority.", (byte)1, whs1Task.WIC_Priority);

			var whs2Task = cycleCountTasks.Single(cct => cct.WIC_WL_Location == whs2RowA.Locations.Single().PK);
			AssertEquals("Should have correct priority.", (byte)2, whs2Task.WIC_Priority);
		}

		#endregion

		public void TestCycleCountAutomation_WithUserDefinedProperties()
		{
			var (warehouse, location, consignee, _, _, _) = CreateTestDataForOrgFilter();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateCycleCountLocation(location, CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "TTT");
			Assert("Precondition: Cycle Count is finalised.", !cycleCount.WIC_EndTime.IsEmpty);
			Factory.Save();

			ProductionRuleHelper.CreateUserDefinedProperty("BOO", "CreateTask", factUniqueKey: "TCC");
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			var rule1 = productionHelper.CreateRule(ruleSet, "Set Property", "Set Property", 1);
			rule1.PRL_RuleDefinition = $@"
{{
  ""conditions"": [{{
			""fieldPath"":""Consignee"",
			""operation"":""equals"",
			""value"":""{consignee.PK}""
	}}],
  ""action"": {{
    ""$type"": ""SetPropertyActionState"",
    ""propertyPath"": ""CreateTask"",
    ""value"": true
  }}
}}"
			.TrimStart();

			var rule2 = productionHelper.CreateRule(ruleSet, "Create Transit Cycle Count Task", "Create Cycle Count Task", 10);
			rule2.PRL_RuleDefinition = @"
{
	""conditions"": [{
			""fieldPath"":""CreateTask"",
			""operation"":""equals"",
			""value"":true
	}],
	""action"":	{
		""$type"": ""CreateTransitCycleCountTaskActionState"",
		""sortByCriteria"": [],
		""priority"": 1,
		""maximumTasksPerRun"": 0
	}
}".TrimStart();
			ProductionRuleHelper.CreateScheduledRuleQueue(rule2, ZDateTime.Now.AddDays(-2));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
	@"Information|Processing RuleSet: Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Transit Cycle Count Task, Set Property.
Information|Created Cycle Count Task for Location: A-1-1.
Information|Succesfully processed and saved RuleSet: Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Transit Cycle Count Task, Set Property.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery(WhsItemCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK)).Single();
			AssertEquals("Should have correct location.", location.PK, cycleCountTask.WIC_WL_Location);
		}

		public void TestCycleCountAutomation_Error()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1);
			var location = rowA.Locations.Single();
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			var rule = CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var recipient = Factory.NewWithValidTestData<GlbStaff>();
			recipient.GS_EmailAddress = "testingEmail@email.com.au";
			recipient.GS_Code = "ABC";

			Factory.Save();

			var creatorMock = new Mock<IWhsItemCycleCountLocationCreator>();
			creatorMock
				.Setup(cc => cc.CreateCycleCountLocations(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<WhsItemCycleCountLocationInfo>>()))
				.Throws(new Exception("ERROR!"));

			var serviceLogger = new TestServiceLogger();
			using (ObjectFactory.Substitute(creatorMock.Object))
			using (WarehouseDataRegistry.Instance.CycleCountingAutomationFailureNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.AllPK))
			using (SystemDataRegistry.Instance.MaxNumberOfAttemptsForRuleProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				RunProductionRuleQueueConsumer(serviceLogger);
			}

			AssertEquals("An email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var errorEmail1 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContainsExactElementsInAnyOrder(new[] { "testingEmail@email.com.au" }, errorEmail1.Recipients.ToStringCollection());
			AssertEquals("Transit Warehouse Cycle Count Automation Rule Set Failure", errorEmail1.Subject);
			AssertEquals($@"Production Rule Set Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation disabled the following rule(s) due to repeated failure to process:
Create Transit Cycle Count Task

Error Logs can be viewed on the Scheduled Production Rule Consumer (SPC) service task.

To include disabled rules in future runs, re-enable the rules via the Production Rules Management Portal.


You have received this email because you are a member of the staff group defined at System Registry: Warehouse -> Inventory Accuracy Management -> Cycle Count Task Creation Failure Notification Group.", errorEmail1.Body);

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery());
			AssertEquals("Should not have created any cycle counting tasks.", 0, cycleCountTasks.Length);

			var scheduleTask = newFactory.Load<ProductionRuleScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ParentID, rule.PK)).Single();
			AssertEquals("Scheduled task is disabled.", false, scheduleTask.S5_IsActive);

			AssertEquals("Should have logged service tasks.",
	@"Information|Processing RuleSet: Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Transit Cycle Count Task.
Error|Unexpected error occurred: ERROR!
Information|Processing RuleSet: Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Transit Cycle Count Task.
Error|Unexpected error occurred: ERROR!
", serviceLogger.ToString());

			ErrorReporter.Clear();
		}

		public void TestCycleCountAutomation_TaskCreatedConcurrently()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1);
			var location = rowA.Locations.Single();
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Transit Warehouse Cycle Count Automation", "Transit Warehouse Cycle Count Automation", true, context: "TWC", warehousePK: warehouse.PK);
			var rule = CreateRule(ruleSet, "Create Transit Cycle Count Task", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var concreteCreator = new WhsItemCycleCountLocationCreator();

			WhsItemCycleCountLocation cycleCount = null;
			var creatorMock = new Mock<IWhsItemCycleCountLocationCreator>();
			creatorMock
				.Setup(cc => cc.CreateCycleCountLocations(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<WhsItemCycleCountLocationInfo>>()))
				.Returns<BusinessObjectFactory, IEnumerable<WhsItemCycleCountLocationInfo>>(
					(f, infos) =>
					{
						var newHelper = new WhsTransitTestHelper(f);
						cycleCount = newHelper.CreateCycleCountLocation(location, CycleCountLocationStatuses.Codes.NotStarted, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "TTT");
						Assert("Precondition: Cycle Count is not finalised.", cycleCount.WIC_EndTime.IsEmpty);
						f.Save();

						return concreteCreator.CreateCycleCountLocations(f, infos);
					});

			var serviceLogger = new TestServiceLogger();
			using (ObjectFactory.Substitute(creatorMock.Object))
			{
				RunProductionRuleQueueConsumer(serviceLogger);
			}

			AssertNotNull("Precondition: Delegate hit.", cycleCount);

			AssertEquals(
	@"Information|Processing RuleSet: Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Transit Cycle Count Task.
Warning|Skipped creating Cycle Count Task for Location: A as one already exists.
Information|Succesfully processed and saved RuleSet: Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Transit Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery(WhsItemCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK));
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}
		#region Implementation

		void RunConsumerAndAssertLogs(string informationLogs)
		{
			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
	@$"Information|Processing RuleSet: Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Transit Cycle Count Task.
{informationLogs}
Information|Succesfully processed and saved RuleSet: Transit Warehouse Cycle Count Automation for Context: TransitWarehouseCycleCountAutomation, Scheduled Rules: Create Transit Cycle Count Task.
", serviceLogger.ToString());
		}

		(WhsWarehouse warehouse, WhsLocation location) CreateWarehouseAndLocation()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateLocation(warehouse, "A-1-1");
			return (warehouse, location);
		}

		void RunConsumerAndAssertCreatedOne(ZGuid expactedPK, string locationString = "A-1-1")
		{
			RunConsumerAndAssertLogs($"Information|Created Cycle Count Task for Location: {locationString}.");

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery()).Single();
			AssertEquals("Should have correct location.", expactedPK, cycleCountTask.WIC_WL_Location);
		}

		void RunConsumerAndAssertCreatedMultiple(string[] locationStrings)
		{
			var logs = new StringBuilder();
			foreach (var locationString in locationStrings)
			{
				logs.AppendLine($"Information|Created Cycle Count Task for Location: {locationString}.");
			}
			RunConsumerAndAssertLogs(logs.ToString().Trim());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsItemCycleCountLocation>(new ZQuery());
			var length = locationStrings.Length;
			AssertEquals($"Should have created {length} tasks.", length, cycleCountTasks.Length);
		}

		void RunProductionRuleQueueConsumer(TestServiceLogger serviceLogger)
		{
			using (EnvProxy.Instance.TemporaryServiceTaskContext("SPC", canRunInAnyBranch: true))
			{
				var queueConsumer = ObjectFactory.Get<IScheduledProductionRuleQueueConsumer>();
				queueConsumer.ProcessQueue(serviceLogger.GetTaskNotificationSubscriber(), It.IsAny<CancellationToken>());
			}
		}

		ProductionRule CreateRule(
			ProductionRuleSet ruleSet,
			string ruleName,
			ZDateTime queueCreateTime,
			int priority = 1,
			int maximumTasksPerRun = 0,
			bool scheduleRule = true,
			short rulePriority = 1,
			ZGuid? consignor = null,
			ZGuid? consignee = null,
			ZGuid? bookingParty = null,
			ZGuid? billToParty = null)
		{
			var rule = ProductionRuleHelper.CreateRule(ruleSet, ruleName, ruleName, rulePriority);
			var conditions = "[";
			if (consignor != null)
			{
				conditions += $@"
					{{
						""fieldPath"":""Consignor"",
						""operation"":""equals"",
						""value"":""{consignor}""
					}}";
			}

			if (consignee != null)
			{
				conditions += $@"
					{{
						""fieldPath"":""Consignee"",
						""operation"":""equals"",
						""value"":""{consignee}""
					}}";
			}

			if (bookingParty != null)
			{
				conditions += $@"
					{{
						""fieldPath"":""BookingParty"",
						""operation"":""equals"",
						""value"":""{bookingParty}""
					}}";
			}

			if (billToParty != null)
			{
				conditions += $@"
					{{
						""fieldPath"":""BillToParty"",
						""operation"":""equals"",
						""value"":""{billToParty}""
					}}";
			}
			conditions += "]";
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"":{conditions},
	""action"":	{{
		""$type"": ""CreateTransitCycleCountTaskActionState"",
		""sortByCriteria"": [],
		""priority"": {priority},
		""maximumTasksPerRun"": {maximumTasksPerRun}
	}}
}}".TrimStart();

			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_ParentID = rule.PK;
			scheduleTask.S5_ScheduleDescription = rule.PRL_Name;
			scheduleTask.S5_DayList = "NNYYYYN";
			scheduleTask.S5_ScheduleType = "W";
			scheduleTask.S5_DailyStartTime = ZDateTime.Today;
			scheduleTask.S5_ParentID = rule.PK;

			if (scheduleRule)
			{
				ProductionRuleHelper.CreateScheduledRuleQueue(rule, queueCreateTime);
			}

			return rule;
		}

		Helper ProductionRuleHelper => productionHelper ?? (productionHelper = new Helper(Factory));
		Helper productionHelper;

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory(testCaseDbConnection));
		BusinessObjectFactory factory;

		DbConnection testCaseDbConnection;

		protected override void SetUp()
		{
			testCaseDbConnection = Db.NewExtraConnectionToMainDb();
		}

		protected override void TearDown()
		{
			testCaseDbConnection.Dispose();
			testCaseDbConnection = null;
		}

		#endregion
	}
}
