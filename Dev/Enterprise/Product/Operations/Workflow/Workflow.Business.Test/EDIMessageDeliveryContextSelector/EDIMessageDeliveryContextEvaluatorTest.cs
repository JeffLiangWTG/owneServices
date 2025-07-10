using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Workflow.Integration;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	public class EDIMessageDeliveryContextEvaluatroTest : TestCaseWithFactory
	{
		class EDIMessageDeliveryContextProviderForTest : IEDIMessageDeliveryContextProvider
		{
			public EDIMessageDeliveryContextProviderForTest(ZGuid selectorPK, BusinessObject[] roots)
			{
				EDIMessageDeliveryContextSelectorPK = selectorPK;
				this.roots = roots;
			}

			public EDIMessageDeliveryContextProviderForTest(ProcessTaskNotification action)
			{
				roots = (action as IDynamicRootProvider).Roots;
				EDIMessageDeliveryContextSelectorPK = action.PQ_ECS_MessageDeliveryContextSelector;
			}

			public ZGuid EDIMessageDeliveryContextSelectorPK { get; set; }

			public BusinessObject[] GetRoots() => roots;

			readonly BusinessObject[] roots;
		}

		EDIMessageDeliveryContextSelector MakeContextSelector(string code, string processType, string description = "blah")
		{
			var selector = Factory.New<EDIMessageDeliveryContextSelector>();
			selector.ECS_Code = code;
			selector.ECS_Description = description;
			selector.ECS_ProcessType = processType;
			return selector;
		}

		EDIMessageDeliveryContextLine AddLine(EDIMessageDeliveryContextSelector selector, string type, string value, string description = "")
		{
			var line = selector.Lines.AddNew();
			line.ECL_ContextType = type;
			line.ECL_Value = value;
			line.ECL_Description = description;
			return line;
		}

		[TestDate(2020, 1, 1)]
		public void TestContextApplicator()
		{
			var selector = MakeContextSelector("ABC", "ABC");
			AddLine(selector, "EASY", "PEASY");
			AddLine(selector, "Description", "<Z0_Description>", "I am a description");
			AddLine(selector, "InvalidGarbage", "<GHDKJFKDHFKDJ>");
			AddLine(selector, "HalfInvalidGarbage", "The value is <GHDKJFKDHFKDJ>");
			AddLine(selector, "DATE", "<Now>");
			AddLine(selector, "ADDITION", "<Add(\"<Z0_Number>\",\"1\")>");
			AddLine(selector, "TASK", "<WorkflowItems.FirstOrDefault(\"<P9_Description>\" == \"TASK DESCRIPTION\").P9_Description>");

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Description = "BLAH";
			dummy.Z0_Number = 10;
			var task = dummy.AddNewTask();
			task.P9_Description = "TASK DESCRIPTION";
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_ECS_MessageDeliveryContextSelector = selector.PK;
			var provider = new EDIMessageDeliveryContextProviderForTest(action);

			var applicator = new EDIMessageDeliveryContextEvaluator();
			var logger = new NotificationsForTest();
			var results = applicator.GetValues(Factory, provider, logger).ToArray();
			AssertEquals(7, results.Length);
			AssertResult(results[0], "EASY", "PEASY");
			AssertResult(results[1], "Description", "BLAH", "I am a description");
			AssertResult(results[2], "InvalidGarbage", "");
			AssertResult(results[3], "HalfInvalidGarbage", "The value is ");
			AssertResult(results[4], "DATE", "Wednesday, 01 January 2020 00:00:00");
			AssertResult(results[5], "ADDITION", "11");
			AssertResult(results[6], "TASK", "TASK DESCRIPTION");

			var expectedLogs = @"Error evaluating Additional Context macro: <GHDKJFKDHFKDJ>
Field <GHDKJFKDHFKDJ> not found on any of the DataSource Types: [DummyWithWorkflow], [DummyProcessTask], [ProcessTaskNotification].
Error evaluating Additional Context macro: The value is <GHDKJFKDHFKDJ>
Field <GHDKJFKDHFKDJ> not found on any of the DataSource Types: [DummyWithWorkflow], [DummyProcessTask], [ProcessTaskNotification].
";
			AssertEquals(expectedLogs, logger.ToString());
		}

		[TestDate(2020, 1, 1)]
		public void TestDeletedContextSelector()
		{
			var provider = new EDIMessageDeliveryContextProviderForTest(Guid.NewGuid(), new BusinessObject[2]);//Reference to a row that doesn't exist in the database
			var applicator = new EDIMessageDeliveryContextEvaluator();
			var results = applicator.GetValues(Factory, provider, new NotificationsForTest()).ToArray();
			AssertEquals(0, results.Length);
		}

		void AssertResult(IEDIMessageDeliveryContextResult result, string type, string value, string desc = "")
		{
			AssertEquals(type, result.ContextType);
			AssertEquals(value, result.ContextValue);
			AssertEquals(desc, result.Description);
		}
	}
}
