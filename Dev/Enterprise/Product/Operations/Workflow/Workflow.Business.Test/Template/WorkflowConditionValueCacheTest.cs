using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	public class WorkflowConditionValueCacheTest : TestCaseWithFactory
	{
		const string SampleUdfCondition1 = "\"<Consignee.OH_Code>\"==\"123\"";
		const string SampleUdfCondition2 = "\"<Consignee.OH_Code>\"==\"321\"";
		const string SampleMcrCondition3 = "Consignee.OH_Code==\"123\"";
		const string SampleMcrCondition4 = "Consignee.OH_Code==\"321\"";
		const string SampleCompositeUdfCondition = SampleUdfCondition1 + " && " + SampleUdfCondition2;

		ProcessTaskTemplate NewTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			return template;
		}

		ProcessTask AddNewTriggerItemToTemplate(ProcessTaskTemplate template, string udfConditionValue)
		{
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			trigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger.TemplateConditions.TemplateCondition2Value = udfConditionValue;

			return trigger;
		}

		ProcessTask AddNewTriggerItemToTemplateWithMCRCondition(ProcessTaskTemplate template, string mcrConditionValue)
		{
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			trigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			trigger.TemplateConditions.TemplateCondition2Value = mcrConditionValue;

			return trigger;
		}

		[TestDateIncremental(minutes: 1)]
		public void TestGetConditionValue_ReturnsTheCorrectValueForProcessTask()
		{
			var cache = new WorkflowConditionValueParentCache();

			var template = NewTemplate();
			var task1 = AddNewTriggerItemToTemplate(template, SampleUdfCondition1);
			var task2 = AddNewTriggerItemToTemplate(template, SampleUdfCondition2);
			var task3 = AddNewTriggerItemToTemplateWithMCRCondition(template, SampleMcrCondition3);
			var task4 = AddNewTriggerItemToTemplateWithMCRCondition(template, SampleMcrCondition4);
			Factory.Save();

			AssertEquals("Value retrieved from cache should be equivalent to the saved value", SampleUdfCondition1, cache.GetConditionValue(task1));
			AssertEquals("Value retrieved from cache should be equivalent to the saved value", SampleUdfCondition2, cache.GetConditionValue(task2));
			AssertEquals("Value retrieved from cache should be equivalent to the saved value", SampleMcrCondition3, cache.GetConditionValue(task3));
			AssertEquals("Value retrieved from cache should be equivalent to the saved value", SampleMcrCondition4, cache.GetConditionValue(task4));
		}

		[TestDateIncremental(minutes: 1)]
		public void TestGetPossiblyCachedCondition2Value_ReturnsTheCorrectValueForProcessTask()
		{
			var template = NewTemplate();
			var task1 = AddNewTriggerItemToTemplate(template, SampleUdfCondition1);
			Factory.Save();

			AssertEquals("Value retrieved from cache should be equivalent to the saved value", SampleUdfCondition1, new UserDefinedEvaluatableConditionValue().GetPossiblyCachedCondition2Value(task1).AsString());
		}

		[TestDateIncremental(minutes: 1)]
		public void TestGetPossiblyCachedCondition2ValueForMCR_ReturnsTheCorrectValueForProcessTask()
		{
			var template = NewTemplate();
			var task1 = AddNewTriggerItemToTemplateWithMCRCondition(template, SampleMcrCondition3);
			Factory.Save();

			AssertEquals("Value retrieved from cache should be equivalent to the saved value", SampleMcrCondition3, new MacroEvaluatableConditionValue().GetPossiblyCachedCondition2Value(task1).AsString());
		}

		[TestDateIncremental(minutes: 1)]
		public void TestGetConditionValue_ReturnsTheCorrectValueForProcessTaskAfterUpdate()
		{
			var cache = new WorkflowConditionValueParentCache();

			var template = NewTemplate();
			var task = AddNewTriggerItemToTemplate(template, SampleUdfCondition1);
			Factory.Save();

			cache.GetConditionValue(task);

			task.TemplateConditions.TemplateCondition2Value = SampleUdfCondition2;
			Factory.Save();

			AssertEquals("Value retrieved from cache should be equivalent to the saved value", SampleUdfCondition2, cache.GetConditionValue(task));
		}

		[TestDateIncremental(minutes: 1)]
		public void TestGetConditionValue_OnlyHitsDbForTheFirstProcessTaskBelongingToTheSameParent()
		{
			var cache = new WorkflowConditionValueParentCache();

			var template = NewTemplate();
			var task1 = AddNewTriggerItemToTemplate(template, SampleUdfCondition1);
			var task2 = AddNewTriggerItemToTemplate(template, SampleUdfCondition2);
			Factory.Save();

			int dbHits = 0;
			SqlEventTracker.Instance.SqlCommandExecutedEvent += args => dbHits++;
			cache.GetConditionValue(task1);

			AssertEquals("Only one DB hit for first loading into cache", 1, dbHits);

			cache.GetConditionValue(task1);
			cache.GetConditionValue(task2);

			AssertEquals("No DB hit after loading into cache", 1, dbHits);
		}

		[TestDateIncremental(minutes: 1)]
		public void TestGetConditionValue_HitsDbWhenCacheIsOutdated()
		{
			var cache = new WorkflowConditionValueParentCache();

			var template = NewTemplate();
			var task = AddNewTriggerItemToTemplate(template, SampleUdfCondition1);
			Factory.Save();
			cache.GetConditionValue(task);

			task.TemplateConditions.TemplateCondition2Value = SampleUdfCondition2;
			Factory.Save();

			int dbHits = 0;
			SqlEventTracker.Instance.SqlCommandExecutedEvent += args => dbHits++;

			cache.GetConditionValue(task);

			AssertEquals("One DB hit after ProcessTask gets modified", 1, dbHits);
		}

		[TestDateIncremental(minutes: 1)]
		public void TestGetMcrConditionValue_ReturnsTheCorrectValueForProcessTaskAfterUpdate()
		{
			var cache = new WorkflowConditionValueParentCache();

			var template = NewTemplate();
			var task = AddNewTriggerItemToTemplateWithMCRCondition(template, SampleMcrCondition3);
			Factory.Save();

			cache.GetConditionValue(task);

			task.TemplateConditions.TemplateCondition2Value = SampleMcrCondition4;
			Factory.Save();

			AssertEquals("Value retrieved from cache should be equivalent to the saved value", SampleMcrCondition4, cache.GetConditionValue(task));
		}

		[TestDateIncremental(minutes: 1)]
		public void TestGetMcrConditionValue_OnlyHitsDbForTheFirstProcessTaskBelongingToTheSameParent()
		{
			var cache = new WorkflowConditionValueParentCache();

			var template = NewTemplate();
			var task3 = AddNewTriggerItemToTemplateWithMCRCondition(template, SampleMcrCondition3);
			var task4 = AddNewTriggerItemToTemplateWithMCRCondition(template, SampleMcrCondition4);
			Factory.Save();

			int dbHits = 0;
			SqlEventTracker.Instance.SqlCommandExecutedEvent += args => dbHits++;
			cache.GetConditionValue(task3);

			AssertEquals("Only one DB hit for first loading into cache", 1, dbHits);

			cache.GetConditionValue(task3);
			cache.GetConditionValue(task4);

			AssertEquals("No DB hit after loading into cache", 1, dbHits);
		}

		[TestDateIncremental(minutes: 1)]
		public void TestGetMcrConditionValue_HitsDbWhenCacheIsOutdated()
		{
			var cache = new WorkflowConditionValueParentCache();

			var template = NewTemplate();
			var task = AddNewTriggerItemToTemplateWithMCRCondition(template, SampleMcrCondition3);
			Factory.Save();
			cache.GetConditionValue(task);

			task.TemplateConditions.TemplateCondition2Value = SampleMcrCondition4;
			Factory.Save();

			int dbHits = 0;
			SqlEventTracker.Instance.SqlCommandExecutedEvent += args => dbHits++;

			cache.GetConditionValue(task);

			AssertEquals("One DB hit after ProcessTask gets modified", 1, dbHits);
		}

		public void TestGetFromObjectFactoryReturnsWorkflowConditionValueBooleanExpressionClauseCache()
		{
			var cache = ObjectFactory.Get<IWorkflowConditionValueBooleanExpressionClauseCache>();

			AssertNotNull("ObjectFactory.Get<IWorkflowConditionValueBooleanExpressionClauseCache>() shoud not be null", cache);
			AssertEquals("Call to ObjectFactory.Get<IWorkflowConditionValueBooleanExpressionClauseCache>() an instance of UdfConditionValueCahce", cache.GetType(), typeof(WorkflowConditionValueBooleanExpressionClauseCache));
		}

		public void TestGetFromObjectFactoryReturnsWorkflowConditionValueParentCache()
		{
			var cache = ObjectFactory.Get<IWorkflowConditionValueParentCache>();

			AssertNotNull("ObjectFactory.Get<IWorkflowConditionValueParentCache>() shoud not be null", cache);
			AssertEquals("Call to ObjectFactory.Get<IWorkflowConditionValueParentCache>() an instance of UdfConditionValueCahce", cache.GetType(), typeof(WorkflowConditionValueParentCache));
		}

		public void TestGetSplitConditionValueFromZStringReturnsCorrectSplitResult()
		{
			var cache = new UserDefinedEvaluatableConditionValue();

			var conditions = cache.GetBooleanExpressionClause(SampleCompositeUdfCondition);
			var splitterOutput = new UserDefinedConditionSplitter().Split(SampleCompositeUdfCondition);

			AssertEquals("Output of cache.GetSplitConditionValueUdf() should be equal to UserDefinedConditionSplitter.Split() - IsConjunction", conditions.IsConjunction, conditions.IsConjunction);
			AssertEquals("Output of cache.GetSplitConditionValueUdf() should be equal to UserDefinedConditionSplitter.Split() - AsString()", conditions.AsString(), conditions.AsString());
			AssertContainsExactElementsInExactOrder("Output of cache.GetSplitConditionValueUdf() should be equal to UserDefinedConditionSplitter.Split() - Children", conditions.Children.Select(c => c.AsString()), conditions.Children.Select(c => c.AsString()));
		}

		IMacroBooleanExpressionClause CreateSimpleMockClause(ZString clause)
		{
			var mockClause = new Mock<IMacroBooleanExpressionClause>();
			mockClause.Setup(c => c.AsString()).Returns(clause);

			return mockClause.Object;
		}

		IMacroBooleanExpressionClause CreateCompositeMockClause(params IMacroBooleanExpressionClause[] conditions)
		{
			var mockClause = new Mock<IMacroBooleanExpressionClause>();
			mockClause.Setup(c => c.Children).Returns(conditions);

			return mockClause.Object;
		}

		public void TestGetSplitConditionValueActuallyHitsTheCacheForString()
		{
			var cache = ObjectFactory.Get<IWorkflowConditionValueBooleanExpressionClauseCache>();

			var mockCache = new Mock<IWorkflowConditionValueBooleanExpressionClauseCache>();
			var mockSplitResult = CreateCompositeMockClause(CreateSimpleMockClause("A"), CreateSimpleMockClause("B"), CreateSimpleMockClause("C"));

			mockCache.Setup(c => c.GetBooleanExpressionClauseCached(It.IsAny<Guid>(), It.IsAny<ZString>(), It.IsAny<Func<ZString, IMacroBooleanExpressionClause>>())).Returns(mockSplitResult);

			ObjectFactory.Substitute(mockCache.Object);

			cache = ObjectFactory.Get<IWorkflowConditionValueBooleanExpressionClauseCache>();

			var evaluatableCondition = SampleUdfCondition1.ToUdfEvaluatableConditionValue();

			AssertContainsExactElementsInExactOrder("EvaluatableCondition Should Return the Cached Split Result If Available", new ZString[] { "A", "B", "C", }, evaluatableCondition.Children.Select(c => c.AsString()));
		}

		[TestDateIncremental(minutes: 1)]
		public void TestOldTriggersCanHaveDuplicateStmNotes()
		{
			// This case is not possibe today due to the SQL Trigger: TG_CheckDuplicateNote
			// This is only to handle old data that can exist in the DB

			var template = NewTemplate();
			var task1 = AddNewTriggerItemToTemplate(template, SampleUdfCondition1);
			var task2 = AddNewTriggerItemToTemplate(template, SampleUdfCondition2);
			Factory.Save();

			Db.Connection.ExecuteNonQuery($"UPDATE {StmNoteSchema.Constants.SqlSchemaName}.{StmNoteSchema.Constants.TableName} SET {StmNoteSchema.ST_ParentID.Name} = '{task1.PK}' WHERE {StmNoteSchema.ST_ParentID.Name} = '{task2.PK}'");

			AssertEquals("Precondition: task1 has 2 stm notes", 2, new BusinessObjectFactory().Load<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, task1.PK)).Length);

			AssertEquals("Value retrieved from cache should be equivalent to the saved value", (task1 as ITemplateConditional).TemplateCondition2Value, new WorkflowConditionValueParentCache().GetConditionValue(task1));
		}
	}
}
