using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class TriggerConditionsViewModelLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Event Types

		public void TestMilestoneEventTypes_ForMilestone()
		{
			AssertEquals("Arrival is a milestone", true, lookups.MilestoneEventTypes.ContainsCode(Events.Arrival.Code));
			AssertEquals("Record Added is a milestone", true, lookups.MilestoneEventTypes.ContainsCode(Events.AddedARecordToTheSystem.Code));
			AssertEquals("Set to Active change log is a milestone", true, lookups.MilestoneEventTypes.ContainsCode(Events.SetToActive.Code));
			AssertEquals("Set to Inactive change log is a milestone", true, lookups.MilestoneEventTypes.ContainsCode(Events.SetToInactive.Code));
			AssertEquals("Any other change log is not a milestone", false, lookups.MilestoneEventTypes.ContainsCode(Events.EditedARecord.Code));
			AssertEquals("Should not contain WTE event", false, lookups.MilestoneEventTypes.ContainsCode(Events.WorkflowTriggerEvent.Code));
			AssertEquals("Exception Raised for milestone", true, lookups.MilestoneEventTypes.ContainsCode(Events.ExceptionRaised.Code));
			AssertEquals("Read Related Note for milestone", true, lookups.MilestoneEventTypes.ContainsCode(Events.ReadRelatedNotes.Code));
		}

		public void TestMilestoneEventTypes_ForTrigger()
		{
			processTask.IsWorkflowTrigger = true;

			AssertEquals("Arrival trigger", true, lookups.MilestoneEventTypes.ContainsCode(Events.Arrival.Code));
			AssertEquals("Record Added trigger", true, lookups.MilestoneEventTypes.ContainsCode(Events.AddedARecordToTheSystem.Code));
			AssertEquals("Record Edited trigger", true, lookups.MilestoneEventTypes.ContainsCode(Events.EditedARecord.Code));
			AssertEquals("Set to Active change log is a trigger", true, lookups.MilestoneEventTypes.ContainsCode(Events.SetToActive.Code));
			AssertEquals("Set to Inactive change log is a trigger", true, lookups.MilestoneEventTypes.ContainsCode(Events.SetToInactive.Code));
			AssertEquals("Should not contain WTE event", false, lookups.MilestoneEventTypes.ContainsCode(Events.WorkflowTriggerEvent.Code));
			AssertEquals("Exception Raised trigger", true, lookups.MilestoneEventTypes.ContainsCode(Events.ExceptionRaised.Code));
			AssertEquals("Read Related Note is a trigger", true, lookups.MilestoneEventTypes.ContainsCode(Events.ReadRelatedNotes.Code));
		}

		public void TestMilestoneEventTypes_DoesNotContainInactiveEvents()
		{
			foreach (var inactiveEvent in Events.InactiveEvents)
			{
				Assert(!lookups.MilestoneEventTypes.ContainsCode(inactiveEvent.Code));
			}
		}

		public void TestMilestoneEventTypes_ContainsInactiveEventCodeWhenProcessTaskIsAlreadyCompleted()
		{
			var inactiveEvent = Events.InactiveEvents.FirstOrDefault();
			AssertNotNull("Pre-requisite: expecting to have at least one inactive event", inactiveEvent);

			processTask.TriggerConditions.TriggerEventCode = inactiveEvent.Code;
			processTask.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Assert(!lookups.MilestoneEventTypes.ContainsCode(inactiveEvent.Code));

			processTask.SetMilestoneActualDateForTest(ZDateTime.Now);
			Assert(lookups.MilestoneEventTypes.ContainsCode(inactiveEvent.Code));
		}

		public void TestExceptionEventTypes_ContainsInactiveEventCodeWhenProcessTaskIsAlreadyCompleted()
		{
			var inactiveEvent = Events.InactiveEvents.FirstOrDefault();
			AssertNotNull("Pre-requisite: expecting to have at least one inactive event", inactiveEvent);

			processTask.IsException = true;
			processTask.TriggerConditions.TriggerEventCode = inactiveEvent.Code;
			processTask.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Assert(!lookups.MilestoneEventTypes.ContainsCode(inactiveEvent.Code));

			processTask.SetMilestoneActualDateForTest(ZDateTime.Now);
			Assert(lookups.MilestoneEventTypes.ContainsCode(inactiveEvent.Code));
		}

		#endregion

		#region Condition List

		public void TestTrigerConditionList()
		{
			processTask.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			var actualList = processTask.TriggerConditions.Lookups.TriggerConditionList.ToArray();
			var expectedList = new EventReferenceConditionList().ToArray();
			AssertContainsExactElementsInAnyOrder("Trigger Condition List", expectedList, actualList);

			processTask.TriggerConditions.TriggerEventCode = Events.ExceptionRaisedCode;

			actualList = processTask.TriggerConditions.Lookups.TriggerConditionList.ToArray();
			expectedList = expectedList.Concat(new ExceptionActionConditionList().Cast<ICodeDescription>()).ToArray();
			AssertContainsExactElementsInAnyOrder("Trigger Condition List", expectedList, actualList);
		}

		public void TestMilestoneEventTypes_DoesNotContainDuplicateEventCodes()
		{
			var triggerProcessTask = job.WorkflowItems.Triggers.AddNew();
			var triggerProcessTaskViewModel = new TriggerConditionsViewModel(triggerProcessTask);
			var triggerProcessTaskLookups = triggerProcessTaskViewModel.Lookups;

			var eventCodesList = triggerProcessTaskLookups.MilestoneEventTypes.GetAllCodes();
			var duplicates = eventCodesList.GroupBy(eventCode => eventCode).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
			var duplicatesString = string.Join(",", duplicates);

			AssertEquals($"MilestoneEventTypes should not have any duplicate event codes. The current event codes that are duplicated are '{duplicatesString}'.",
				0, duplicates.Count);
		}

		#endregion

		#region Implementation

		IWorkflowProvider job;
		ProcessTask processTask;
		TriggerConditionsViewModelLookups lookups;

		protected override void SetUp()
		{
			base.SetUp();

			job = Factory.New<OrgHeader>();
			processTask = job.WorkflowItems.Milestones.AddNew();
			var viewModel = new TriggerConditionsViewModel(processTask);
			lookups = viewModel.Lookups;
		}

		#endregion
	}
}
