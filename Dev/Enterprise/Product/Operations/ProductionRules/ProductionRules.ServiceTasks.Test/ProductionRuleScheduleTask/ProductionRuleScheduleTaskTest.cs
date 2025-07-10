using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Business.Testing;
using Enterprise.Scheduler.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProductionRules.ServiceTasks.Testing
{
	[TestedType(typeof(ProductionRuleScheduleTask))]
	class ProductionRuleScheduleTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			var productionRuleScheduleTask = Factory.New<ProductionRuleScheduleTask>();
			var scheduleRow = ((IBusinessObjectInternals)productionRuleScheduleTask).Row;

			var newFactory = new BusinessObjectFactory();
			var typeDecider = new StmScheduleTaskTypeDecider();
			AssertEquals(typeof(ProductionRuleScheduleTask), typeDecider.GetTypeForLoad(scheduleRow, newFactory));
		}

		public void TestSetDefaultValues()
		{
			var productionRuleScheduleTask = Factory.New<ProductionRuleScheduleTask>();
			AssertEquals(ProductionRuleSchema.Constants.Prefix, productionRuleScheduleTask.S5_ParentTableCode);
		}

		public void TestDescriptionForLog()
		{
			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			AssertEquals("Queue Scheduled Rule, Context: , RuleSet: , Rule: ", scheduleTask.DescriptionForLog);
			AssertEquals("Queue Scheduled Rule, Context: , RuleSet: , Rule: ", scheduleTask.HumanReadableName);

			var rule = Factory.New<ProductionRule>();
			rule.PRL_Name = "Some rule!";
			scheduleTask.S5_ParentID = rule.PK;
			AssertEquals("Queue Scheduled Rule, Context: , RuleSet: , Rule: Some rule!", scheduleTask.DescriptionForLog);
			AssertEquals("Queue Scheduled Rule, Context: , RuleSet: , Rule: Some rule!", scheduleTask.HumanReadableName);

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "Some ruleset!";
			rule.PRL_PRS_RuleSet = ruleSet.PK;
			AssertEquals("Queue Scheduled Rule, Context: PWW, RuleSet: Some ruleset!, Rule: Some rule!", scheduleTask.DescriptionForLog);
			AssertEquals("Queue Scheduled Rule, Context: PWW, RuleSet: Some ruleset!, Rule: Some rule!", scheduleTask.HumanReadableName);
		}

		public void TestUtcOffsetOverride()
		{
			TestDateAttribute.UseUNLOCO = true;

			var branch = Factory.New<GlbBranch>();
			branch.GB_RL_NKHomePort = "GBLON";

			var warehouse = Factory.New<IWhsWarehouse>();
			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "Some ruleset!";
			ruleSet.PRS_WW_Warehouse = warehouse.PK;

			var rule = Factory.New<ProductionRule>();
			rule.PRL_Name = "Some rule!";
			rule.PRL_PRS_RuleSet = ruleSet.PK;

			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_ParentID = rule.PK;
			AssertEquals(false, scheduleTask.UtcOffsetOverride.HasValue);
		}

		public void TestS5_GB()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_RL_NKHomePort = "GBLON";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_RL_NKHomePort = "INBLR";

			var branch3 = Factory.New<GlbBranch>();
			branch3.GB_RL_NKHomePort = "AUSYD";

			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_GB = branch1.PK;
			AssertEquals("Should use the S5_GB value.", branch1.PK, scheduleTask.S5_GB);

			var rule = Factory.New<ProductionRule>();
			rule.PRL_Name = "Some rule!";
			scheduleTask.S5_ParentID = rule.PK;
			AssertEquals("Should use the S5_GB value.", branch1.PK, scheduleTask.S5_GB);

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "Some ruleset!";
			rule.PRL_PRS_RuleSet = ruleSet.PK;
			AssertEquals("Should use the S5_GB value.", branch1.PK, scheduleTask.S5_GB);

			var warehouse = Factory.New<IWhsWarehouse>();
			warehouse.WW_GB_RelatedCompanyBranch = branch3.PK;
			ruleSet.PRS_WW_Warehouse = warehouse.PK;
			AssertEquals("Should use the warehouse's branch.", branch3.PK, scheduleTask.S5_GB);

			scheduleTask.S5_GB = branch2.PK;
			AssertEquals("Should use the warehouse's branch.", branch3.PK, scheduleTask.S5_GB);

			ruleSet.PRS_WW_Warehouse = ZGuid.Empty;
			AssertEquals("Should use the S5_GB value.", branch2.PK, scheduleTask.S5_GB);

			warehouse.WW_GB_RelatedCompanyBranch = branch1.PK;
			ruleSet.PRS_WW_Warehouse = warehouse.PK;
			AssertEquals("Should use the warehouse's branch.", branch1.PK, scheduleTask.S5_GB);
		}

		public void TestPriority()
		{
			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			AssertEquals("Should default to 0 when there is no rule.", 0, scheduleTask.Priority);

			var rule = Factory.New<ProductionRule>();
			rule.PRL_Name = "Some rule!";
			rule.PRL_Priority = 0;
			scheduleTask.S5_ParentID = rule.PK;
			AssertEquals("Should default to 0 when there is no priority set.", 0, scheduleTask.Priority);

			rule.PRL_Priority = 1;
			AssertEquals("Should return an appropriate priority for sorting in descending order.", int.MaxValue - 1, scheduleTask.Priority);

			rule.PRL_Priority = 10;
			AssertEquals("Should return an appropriate priority for sorting in descending order.", int.MaxValue - 10, scheduleTask.Priority);
		}

		[TestDate(2022, 1, 3)]
		public void TestRun()
		{
			var ruleSets = Factory.Load<ProductionRuleSet>(new ZQuery());

			foreach (var existingRuleSet in ruleSets)
			{
				existingRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Helper.CreateRuleSet("A", "A", context: "PWP");

			var rule = Helper.CreateRule(ruleSet, "Rule1", "Desc", 42);
			rule.PRL_RuleDefinition = "DEFINITION";
			Factory.Save();

			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_ParentID = rule.PK;

			var notifications = new NotificationBuffer();
			scheduleTask.Run(notifications);
			AssertEquals("Should *not* have warnings.", false, notifications.HasWarnings);
			AssertEquals("Should *not* have errors.", false, notifications.HasErrors);
			AssertEquals(string.Empty, notifications.AsString);
			AssertEquals("Should have bumped next run time.", new ZDateTime(2022, 1, 4), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("Should have bumped run count.", 1, scheduleTask.S5_ScheduleActualRunCount);

			var queue = Factory.LoadTop1<ProductionRuleScheduleQueue>(new ZQuery(ProductionRuleScheduleQueueSchema.PRQ_PRL_Rule, rule.PK));
			AssertNotNull("Should have populated queue.", queue);
			AssertEquals("Should have zero retries.", (byte)0, queue.PRQ_RetryCount);
		}

		[TestDate(2022, 09, 24)]
		public void TestRun_TimeZones()
		{
			// 24th is a Saturday, next schedule time should be the Tuesday at 9am.
			var expectedNextRunTimeSydneyLocal = new DateTime(2022, 09, 27, 09, 00, 00);
			TestRun_TimeZones(expectedNextRunTimeSydneyLocal);
		}

		[TestDate(2024, 10, 05)]
		public void TestRun_TimeZones_DST_ClockForward()
		{
			// 5th is a Saturday right before DST cutover, next schedule time should be the Tuesday at 9am.
			var expectedNextRunTimeSydneyLocal = new DateTime(2024, 10, 08, 09, 00, 00);
			TestRun_TimeZones(expectedNextRunTimeSydneyLocal);
		}

		[TestDate(2024, 04, 06)]
		public void TestRun_TimeZones_DST_ClockBackward()
		{
			// 6th is a Saturday right before DST cutover, next schedule time should be the Tuesday at 9am.
			var expectedNextRunTimeSydneyLocal = new DateTime(2024, 04, 09, 09, 00, 00);
			TestRun_TimeZones(expectedNextRunTimeSydneyLocal);
		}

		void TestRun_TimeZones(DateTime expectedNextRunTimeSydneyLocal)
		{
			TestDateAttribute.UseUNLOCO = true;

			var ruleSets = Factory.Load<ProductionRuleSet>(new ZQuery());

			foreach (var existingRuleSet in ruleSets)
			{
				existingRuleSet.PRS_IsLive = false;
			}

			var bangaloreBranch = Factory.NewWithValidTestData<GlbBranch>(); // Anything that is not Sydney or near UTC+0
			bangaloreBranch.GB_RL_NKHomePort = "INBLR";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, bangaloreBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var ruleSet = Helper.CreateRuleSet("A", "A", context: "PWP");

				var warehouse = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
				ruleSet.PRS_WW_Warehouse = warehouse.PK;

				var sydneyBranch = Factory.NewWithValidTestData<GlbBranch>();
				sydneyBranch.GB_RL_NKHomePort = "AUSYD";
				warehouse.WW_GB_RelatedCompanyBranch = sydneyBranch.PK;

				var rule = Helper.CreateRule(ruleSet, "Rule1", "Desc", 42);
				rule.PRL_RuleDefinition = "DEFINITION";
				Factory.Save();

				var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
				scheduleTask.S5_DayList = "NNYYYYN";
				scheduleTask.S5_ScheduleType = "W";
				scheduleTask.S5_DailyStartTime = new DateTime(0001, 01, 01, 09, 00, 00);
				scheduleTask.S5_ParentID = rule.PK;
				Factory.Save();

				// Clear S5_GB in the database, to approximate it coming from Glow empty
				((IDbConnected)Factory).Connection.ExecuteNonQuery($"UPDATE StmScheduleTask SET S5_GB = null WHERE S5_PK = '{scheduleTask.PK}'");

				var expectedNextRunTimeUtc = sydneyBranch.HomePort.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(expectedNextRunTimeSydneyLocal);

				var notifications = new NotificationBuffer();
				scheduleTask.RunSafe(notifications); // Run safe also sets the branch context, as is done in the service task
				AssertEquals("Should *not* have warnings.", false, notifications.HasWarnings);
				AssertEquals("Should *not* have errors.", false, notifications.HasErrors);
				AssertEquals(string.Empty, notifications.AsString);
				AssertEquals("Should have bumped next run time.", expectedNextRunTimeUtc, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
				AssertEquals("Should have bumped run count.", 1, scheduleTask.S5_ScheduleActualRunCount);

				var queue = Factory.LoadTop1<ProductionRuleScheduleQueue>(new ZQuery(ProductionRuleScheduleQueueSchema.PRQ_PRL_Rule, rule.PK));
				AssertNotNull("Should have populated queue.", queue);
				AssertEquals("Should have zero retries.", (byte)0, queue.PRQ_RetryCount);
			}
		}

		[TestDate(2022, 1, 3)]
		public void TestRun_QueueAlreadyPopulated()
		{
			var ruleSets = Factory.Load<ProductionRuleSet>(new ZQuery());

			foreach (var existingRuleSet in ruleSets)
			{
				existingRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Helper.CreateRuleSet("A", "A", context: "PWP");

			var rule = Helper.CreateRule(ruleSet, "Rule1", "Desc", 42);
			rule.PRL_RuleDefinition = "DEFINITION";

			var queue = Factory.New<ProductionRuleScheduleQueue>();
			queue.PRQ_PRL_Rule = rule.PK;

			Factory.Save();

			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_ParentID = rule.PK;

			var notifications = new NotificationBuffer();
			AssertNoExceptionThrown(() => scheduleTask.Run(notifications));
			AssertEquals("Should have warnings.", true, notifications.HasWarnings);
			AssertEquals("Should *not* have errors.", false, notifications.HasErrors);
			AssertEquals(@"Skipping Production Rule as it has already been queued for processing: Queue Scheduled Rule, Context: PWP, RuleSet: A, Rule: Rule1
", notifications.AsString);
			AssertEquals("Should have bumped next run time.", new ZDateTime(2022, 1, 4), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("Should have bumped run count.", 1, scheduleTask.S5_ScheduleActualRunCount);

			var queues = Factory.Load<ProductionRuleScheduleQueue>(new ZQuery(ProductionRuleScheduleQueueSchema.PRQ_PRL_Rule, rule.PK));
			AssertEquals("Should not have re-populated queue.", 1, queues.Length);
		}

		[TestDate(2022, 1, 3)]
		public void TestRun_RuleCannotBeLoaded()
		{
			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_ParentID = scheduleTask.PK;

			var notifications = new NotificationBuffer();
			scheduleTask.Run(notifications);
			AssertEquals("Should have warnings.", true, notifications.HasWarnings);
			AssertEquals("Should *not* have errors.", false, notifications.HasErrors);
			AssertEquals($@"Production Rule not found. Rule PK = {scheduleTask.S5_ParentID}
", notifications.AsString);
			AssertEquals("Should have bumped next run time.", new ZDateTime(2022, 1, 4), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("Should have bumped run count.", 1, scheduleTask.S5_ScheduleActualRunCount);

			var queue = Factory.LoadTop1<ProductionRuleScheduleQueue>(new ZQuery());
			AssertNull("Should *not* have populated queue.", queue);
		}

		[TestDate(2022, 1, 3)]
		public void TestRun_RuleSetCannotBeLoaded()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A", context: "PWP");
			ruleSet.PRS_IsLive = false;

			var rule = Helper.CreateRule(ruleSet, "Rule1", "Desc", 42);
			rule.PRL_RuleDefinition = "DEFINITION";
			Factory.Save();

			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_ParentID = rule.PK;

			// Change the fks strategically such that we can save but loading during Run fails
			Factory.Saving += (s) => rule.PRL_PRS_RuleSet = ruleSet.PK;
			Factory.Saved += (s, e) => rule.PRL_PRS_RuleSet = scheduleTask.PK;

			var notifications = new NotificationBuffer();
			scheduleTask.Run(notifications);
			AssertEquals("Should have warnings.", true, notifications.HasWarnings);
			AssertEquals("Should *not* have errors.", false, notifications.HasErrors);
			AssertEquals($@"Production Ruleset not found. Ruleset PK = {scheduleTask.PK}
", notifications.AsString);
			AssertEquals("Should have bumped next run time.", new ZDateTime(2022, 1, 4), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("Should have bumped run count.", 1, scheduleTask.S5_ScheduleActualRunCount);

			var queue = Factory.LoadTop1<ProductionRuleScheduleQueue>(new ZQuery(ProductionRuleScheduleQueueSchema.PRQ_PRL_Rule, rule.PK));
			AssertNull("Should *not* have populated queue.", queue);
		}

		[TestDate(2022, 1, 3)]
		public void TestRun_RuleSetIsNotLive()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A", context: "PWP");
			ruleSet.PRS_IsLive = false;

			var rule = Helper.CreateRule(ruleSet, "Rule1", "Desc", 42);
			rule.PRL_RuleDefinition = "DEFINITION";
			Factory.Save();

			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_ParentID = rule.PK;

			var notifications = new NotificationBuffer();
			scheduleTask.Run(notifications);
			AssertEquals("Should have warnings.", true, notifications.HasWarnings);
			AssertEquals("Should *not* have errors.", false, notifications.HasErrors);
			AssertEquals(@"Production Ruleset is not live: Queue Scheduled Rule, Context: PWP, RuleSet: A, Rule: Rule1
", notifications.AsString);
			AssertEquals("Should have bumped next run time.", new ZDateTime(2022, 1, 4), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("Should have bumped run count.", 1, scheduleTask.S5_ScheduleActualRunCount);

			var queue = Factory.LoadTop1<ProductionRuleScheduleQueue>(new ZQuery(ProductionRuleScheduleQueueSchema.PRQ_PRL_Rule, rule.PK));
			AssertNull("Should *not* have populated queue.", queue);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());

			var ruleSet = Helper.CreateRuleSet("TEST", "TEST", warehousePK: warehouse.PK);
			var rule = Helper.CreateRule(ruleSet, "TEST", "TEST");
			rule.PRL_RuleDefinition = $@"{{
    ""conditions"": [
    ],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			var task = Factory.New<ProductionRuleScheduleTask>();
			task.S5_ParentID = rule.PK;
			return task;
		}

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;
	}
}
