using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test.UniversalTriggers
{
	class UniversalTriggers_TriggerConditions_MCR : UniversalTriggers_TriggerConditionsBaseTest
	{
		protected internal override string TriggerCondition => EventReferenceConditionList.Codes.ConditionWithMacros;

		protected override string TriggerConditionValue => "Source.Z0_Code == \"ZAP\"";

		protected override void AddNonMatchingEvent(DummyWithWorkflow job)
		{
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);
		}

		protected override void AddMatchingEvent(DummyWithWorkflow job)
		{
			job.Z0_Code = "ZAP";
			Factory.Save();

			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);
		}
	}

	class UniversalTriggers_TriggerConditions_REF : UniversalTriggers_TriggerConditionsBaseTest
	{
		protected internal override string TriggerCondition => EventReferenceConditionList.Codes.EventReference;

		protected override string TriggerConditionValue => "STAHP";

		protected override void AddNonMatchingEvent(DummyWithWorkflow job)
		{
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved, "ZZZAP");
		}

		protected override void AddMatchingEvent(DummyWithWorkflow job)
		{
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved, "STAHP");
		}
	}

	class UniversalTriggers_TriggerConditions_RFP : UniversalTriggers_TriggerConditionsBaseTest
	{
		protected internal override string TriggerCondition => EventReferenceConditionList.Codes.EventReferenceParameters;

		protected override string TriggerConditionValue => "GRP=DEA,ARG=GRR";

		protected override void AddNonMatchingEvent(DummyWithWorkflow job)
		{
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved, new KeyValuePair<string, string>("GRP", "DEA"));
		}

		protected override void AddMatchingEvent(DummyWithWorkflow job)
		{
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved, new KeyValuePair<string, string>("GRP", "DEA"), new KeyValuePair<string, string>("ARG", "GRR"));
		}
	}

	class UniversalTriggers_TriggerConditions_RFR : UniversalTriggers_TriggerConditionsBaseTest
	{
		protected internal override string TriggerCondition => EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions;

		protected override string TriggerConditionValue => @"S..HP \d";

		protected override void AddNonMatchingEvent(DummyWithWorkflow job)
		{
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved, "STAHP ME");
		}

		protected override void AddMatchingEvent(DummyWithWorkflow job)
		{
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved, "STAHP 3");
		}
	}

	class UniversalTriggers_TriggerConditions_RFW : UniversalTriggers_TriggerConditionsBaseTest
	{
		protected internal override string TriggerCondition => EventReferenceConditionList.Codes.EventReferenceWithWildcards;

		protected override string TriggerConditionValue => @"Y*S";

		protected override void AddNonMatchingEvent(DummyWithWorkflow job)
		{
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved, "YAAAAAAH");
		}

		protected override void AddMatchingEvent(DummyWithWorkflow job)
		{
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved, "YAAASS");
		}
	}

	class UniversalTriggers_TriggerConditions_UDF : UniversalTriggers_TriggerConditionsBaseTest
	{
		protected internal override string TriggerCondition => EventReferenceConditionList.Codes.UserDefined;

		protected override string TriggerConditionValue => "\"<Z0_Code>\" == \"ZAP\"";

		protected override void AddNonMatchingEvent(DummyWithWorkflow job)
		{
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);
		}

		protected override void AddMatchingEvent(DummyWithWorkflow job)
		{
			job.Z0_Code = "ZAP";
			Factory.Save();

			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);
		}
	}

	abstract class UniversalTriggers_TriggerConditionsBaseTest : WorkflowTestCase
	{
		protected internal abstract string TriggerCondition { get; }
		protected abstract string TriggerConditionValue { get; }

		protected abstract void AddNonMatchingEvent(DummyWithWorkflow job);
		protected abstract void AddMatchingEvent(DummyWithWorkflow job);

		[TestDate(2015, 7, 14)]
		public void TestFireEventOnJob_WithTriggerConditions_MCR()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);

			trigger.TriggerConditions.TriggerCondition = TriggerCondition;
			trigger.TriggerConditions.TriggerConditionValue = TriggerConditionValue;

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);

			var ghostedTrigger = (ProcessTask)job.WorkflowItems.TriggersIncludingRelated.Single();

			AssertEquals(TriggerCondition, ghostedTrigger.TriggerConditions.TriggerCondition);
			AssertEquals(TriggerConditionValue, ghostedTrigger.TriggerConditions.TriggerConditionValue);
			AssertEquals(TriggerUserContextList.Codes.Event, ghostedTrigger.TriggerConditions.TriggerContextCode);

			AddNonMatchingEvent(job);
			Factory.Save();

			AssertEquals("Trigger condition does not match so should not be fired", ZDateTime.Empty, ghostedTrigger.P9_ActualDate.ToZDateTime());

			var newFactory = Factory.CreateNewFactory();
			var loadedTriggerLink = FindJobTriggerLink(job, trigger, newFactory);

			AssertNull(loadedTriggerLink);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			AddMatchingEvent(job);
			Factory.Save();

			AssertEquals("Trigger condition now matches so should be fired", new ZDateTime(2015, 7, 15), ghostedTrigger.P9_ActualDate.ToZDateTime());

			newFactory = Factory.CreateNewFactory();
			loadedTriggerLink = FindJobTriggerLink(job, trigger, newFactory);

			AssertNotNull(loadedTriggerLink);
			AssertEquals(ghostedTrigger.P9_ActualDate.ToZDateTime(), ((IWorkflowTrigger)loadedTriggerLink).LastFiredTime.ToZDateTime());
		}
	}

	class UniversalTriggers_TriggerConditionsTestCoverageTest : WorkflowTestCase
	{
		public void TestAllTriggerConditionTypesAreTested()
		{
			var types = new Dictionary<string, Type>
			{
				{ EventReferenceConditionList.Codes.ConditionWithMacros, typeof(UniversalTriggers_TriggerConditions_MCR) },
				{ EventReferenceConditionList.Codes.EventReference, typeof(UniversalTriggers_TriggerConditions_REF) },
				{ EventReferenceConditionList.Codes.EventReferenceParameters, typeof(UniversalTriggers_TriggerConditions_RFP) },
				{ EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions, typeof(UniversalTriggers_TriggerConditions_RFR) },
				{ EventReferenceConditionList.Codes.EventReferenceWithWildcards, typeof(UniversalTriggers_TriggerConditions_RFW) },
				{ EventReferenceConditionList.Codes.UserDefined, typeof(UniversalTriggers_TriggerConditions_UDF) },
			};

			CombineAssertions("All types of trigger conditions should be tested here to ensure Universal Triggers can support them", () =>
			{
				foreach (ICodeDescription pair in new EventReferenceConditionList())
				{
					var type = types.ContainsKey(pair.Code) ? types[pair.Code] : null;
					AssertNotNull($"{pair.Code}, {pair.Description}", type);

					if (type != null)
					{
						var instance = (UniversalTriggers_TriggerConditionsBaseTest)Activator.CreateInstance(type);

						AssertEquals(pair.Code, instance.TriggerCondition);
					}
				}
			});
		}
	}
}
