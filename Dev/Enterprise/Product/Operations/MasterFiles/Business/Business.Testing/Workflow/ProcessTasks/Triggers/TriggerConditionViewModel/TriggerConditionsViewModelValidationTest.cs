using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TriggerConditionsViewModelValidationTest : BusinessObjectValidationTestCase
	{
		#region Inactive events

		public void TestTriggerEventCode_InactiveEvent_NoActualDate()
		{
			var inactiveEvent = Events.InactiveEvents.OrderBy(c => c.Code).FirstOrDefault();
			AssertNotNull("Pre-requisite: expecting to have at least one inactive event", inactiveEvent);

			Trigger.SetMilestoneActualDateForTest(ZDateTime.Empty);
			ViewModel.TriggerEventCode = inactiveEvent.Code;
			AssertHasError(ViewModel.TriggerEventCodeInfo, "This event is inactive and should not be used. Replaced by DHR Event");
		}

		public void TestTriggerEventCode_InactiveEvent_HasActualDate()
		{
			var inactiveEvent = Events.InactiveEvents.FirstOrDefault();

			ViewModel.TriggerEventCode = inactiveEvent.Code;
			Trigger.Parent.Logs.AddNew(inactiveEvent);
			ViewModel.Validation.ValidateTriggerEventCode();

			AssertNoErrors(ViewModel.TriggerEventCodeInfo);
		}

		#endregion

		#region Shipment Template Validation

		public void TestTriggerEventCode()
		{
			Trigger.TemplateConditions.TemplateCondition1 = JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg;
			ViewModel.TriggerEventCode = Events.Attached.Code;
			AssertHasError(ViewModel.TriggerEventCodeInfo, "Only the Arrival event is applicable on a Consol Discharge Transport Leg.");

			ViewModel.TriggerEventCode = Events.Arrival.Code;
			AssertNoErrors(ViewModel.TriggerEventCodeInfo);
		}

		public void TestTriggerField()
		{
			Trigger.TemplateConditions.TemplateCondition1 = JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg;
			ViewModel.TriggerFieldName = JobShipmentSchema.JS_HouseBill.Name;
			AssertHasError(ViewModel.TriggerFieldNameInfo, "Only Transport Leg Fields are applicable on a Consol Discharge Transport Leg.");

			ViewModel.TriggerFieldName = JobConsolTransportSchema.JW_ETA.Name;
			AssertNoError(ViewModel.TriggerFieldNameInfo, "Only Transport Leg Fields are applicable on a Consol Discharge Transport Leg.");
		}

		#endregion

		#region TriggerFiredCountdown

		public void TestTriggerFiredCountdown()
		{
			AssertTriggerFiredCountdown("MIL");
			AssertTriggerFiredCountdown("TRG");

			var viewModel = new TriggerConditionsViewModel(Factory.New<IUniversalTemplateTrigger>());
			AssertNoWarning(viewModel.TriggerFiredCountdownInfo, "This trigger has fired too many times and will not fire any more unless this value is increased. It may be part of an infinite loop.");
			viewModel.TriggerFiredCountdown = 0;
			AssertNoWarning(viewModel.TriggerFiredCountdownInfo, "This trigger has fired too many times and will not fire any more unless this value is increased. It may be part of an infinite loop.");
			viewModel.TriggerFiredCountdown = 100;
			AssertNoWarning(viewModel.TriggerFiredCountdownInfo, "This trigger has fired too many times and will not fire any more unless this value is increased. It may be part of an infinite loop.");
		}

		void AssertTriggerFiredCountdown(string taskType)
		{
			var triggerable = Factory.NewWithValidTestData<ProcessTask>();
			triggerable.P9_Type = taskType;
			triggerable.TriggerConditions.TriggerFiredCountdown = 1;
			AssertNoWarning(triggerable.TriggerConditions.TriggerFiredCountdownInfo, "This trigger has fired too many times and will not fire any more unless this value is increased. It may be part of an infinite loop.");
			triggerable.TriggerConditions.TriggerFiredCountdown = 0;
			AssertHasWarning(triggerable.TriggerConditions.TriggerFiredCountdownInfo, "This trigger has fired too many times and will not fire any more unless this value is increased. It may be part of an infinite loop.");
			triggerable.TriggerConditions.TriggerFiredCountdown = 100;
			AssertNoWarning(triggerable.TriggerConditions.TriggerFiredCountdownInfo, "This trigger has fired too many times and will not fire any more unless this value is increased. It may be part of an infinite loop.");
		}

		#endregion

		#region TriggerConditionValue

		public void TestP9_TriggerConditionValueValidation_REF()
		{
			Trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;

			ViewModel.TriggerConditionValue = "AAA == 111 && BBB == 222";
			AssertNoErrors("Should no have errors as condition is not MCR/UDF", ViewModel.TriggerConditionValueInfo);

			ViewModel.TriggerConditionValue = "AAA == 111 &&& BBB = 222 ||";
			AssertNoErrors("Should no have errors as condition is not MCR/UDF", ViewModel.TriggerConditionValueInfo);
		}

		public void TestP9_TriggerConditionValueValidation_MCR()
		{
			ViewModel.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;

			ViewModel.TriggerConditionValue = "AAA == 111 && BBB == 222";
			AssertNoErrors("Should no have errors because... there are no errors", ViewModel.TriggerConditionValueInfo);

			ViewModel.TriggerConditionValue = "AAA == 111 &&& BBB = 222 ||";
			AssertHasErrors("Should have errors as syntax is incorrect", ViewModel.TriggerConditionValueInfo);
		}

		public void TestP9_TriggerConditionValueValidation_UDFWithoutValueProviders()
		{
			ViewModel.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;

			ViewModel.TriggerConditionValue = "\"AAA\" == \"111\" && \"BBB\" == \"222\"";
			AssertNoErrors("Should no have errors because... there are no errors", ViewModel.TriggerConditionValueInfo);

			ViewModel.TriggerConditionValue = "";
			AssertHasErrors("Should have errors as value is empty", ViewModel.TriggerConditionValueInfo);
		}

		public void TestP9_TriggerConditionValueValidation_UDFWithValueProviders()
		{
			ViewModel.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;

			ViewModel.TriggerConditionValue = "\"<DateTimeAsString('<NOW>', 'dddd')>\" == \"Thursday\"";
			AssertNoErrors("Should no have errors because <NOW> and <DateTimeAsString()> are valid value providers", ViewModel.TriggerConditionValueInfo);
		}

		public void TestGenerateReferenceForNewLog_AllowDuplicateParameters_Triggers_Ignore()
		{
			ViewModel.TriggerEventCode = Events.CustomisableEvent00Code;
			ViewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			ViewModel.TriggerConditionValue = "|NEW=1|NEW=2";

			AssertNoWarnings(ViewModel.TriggerConditionValueInfo);
		}

		public void TestGenerateReferenceForNewLog_AllowDuplicateParameters_Milestone()
		{
			var milestone = Factory.NewWithValidTestData<ProcessTask>();
			milestone.IsMilestone = true;
			var viewModel = milestone.TriggerConditions;
			viewModel.TriggerEventCode = Events.CustomisableEvent00Code;
			viewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			viewModel.TriggerConditionValue = "|NEW=1|NEW=2";

			AssertHasWarning(viewModel.TriggerConditionValueInfo, "This trigger condition contains parameters with identical keys (|NEW=1, |NEW=2). The Reference of the [Z00] Event created from this property will ignore the duplicate parameters.");
		}

		public void TestGenerateReferenceForNewLog_AllowDuplicateParameters_TemplateTask_Milestone_AvoidEventReferenceWeirdness()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "CON";
			var milestone = template.WorkflowItems.Milestones.AddNew();
			var viewModel = milestone.TriggerConditions;
			viewModel.TriggerEventCode = Events.CustomisableEvent00Code;

			foreach (var triggerCondition in new EventReferenceConditionList().GetAllCodes())
			{
				viewModel.TriggerCondition = triggerCondition;
				AssertNoExceptionThrown(() => viewModel.TriggerConditionValue = "|NEW=1|NEW=" + triggerCondition);
			}
		}

		public void TestGenerateReferenceForNewRFPLog_IgnoreDuplicateParametersOnTriggers()
		{
			ViewModel.TriggerEventCode = Events.CustomisableEvent00Code;
			ViewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			ViewModel.TriggerConditionValue = "NEW=1,NEW=2";
			AssertNoWarnings(ViewModel.TriggerConditionValueInfo);
		}

		public void TestGenerateReferenceForNewRFPLog_WarnDuplicateParametersOnMilestone()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var viewModel = milestone.TriggerConditions;
			viewModel.TriggerEventCode = Events.CustomisableEvent00Code;
			viewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			viewModel.TriggerConditionValue = "NEW=1,NEW=2";
			AssertHasWarning(viewModel.TriggerConditionValueInfo, "This trigger condition contains parameters with identical keys (|NEW=1, |NEW=2). The Reference of the [Z00] Event created from this property will ignore the duplicate parameters.");
		}

		public void TestGenerateReferenceForNewRFPLog_WarnMultipleDuplicateParametersOnMilestone()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var viewModel = milestone.TriggerConditions;
			viewModel.TriggerEventCode = Events.CustomisableEvent00Code;
			viewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			viewModel.TriggerConditionValue = "NEW=1,LOC=AUMEL,NEW=2,LOC=AUSYD,OLD=YES,NEW=NO";
			AssertHasWarning(viewModel.TriggerConditionValueInfo, "This trigger condition contains parameters with identical keys (|LOC=AUMEL, |LOC=AUSYD, |NEW=1, |NEW=2, |NEW=NO). The Reference of the [Z00] Event created from this property will ignore the duplicate parameters.");
		}

		public void TestGenerateReferenceForNewRFPLog_WarnDuplicateParameters_TemplateTask_Milestone()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "CON";
			var milestone = template.WorkflowItems.Milestones.AddNew();
			var viewModel = milestone.TriggerConditions;
			viewModel.TriggerEventCode = Events.CustomisableEvent00Code;
			viewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			viewModel.TriggerConditionValue = "NEW=1,NEW=2";
			AssertHasWarning(viewModel.TriggerConditionValueInfo, "This trigger condition contains parameters with identical keys (|NEW=1, |NEW=2). The Reference of the [Z00] Event created from this property will ignore the duplicate parameters.");
		}

		public void TestGenerateReferenceForNewRFPLog_IgnoreDuplicateParameters_TemplateTask_Trigger()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "CON";
			var trigger = template.WorkflowItems.Triggers.AddNew();
			var viewModel = trigger.TriggerConditions;
			viewModel.TriggerEventCode = Events.CustomisableEvent00Code;
			viewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			ViewModel.TriggerConditionValue = "NEW=1,NEW=2";
			AssertNoWarnings(viewModel.TriggerConditionValueInfo);
		}

		public void TestConditionValue_NoUnicodeSupport()
		{
			var viewModel = new TriggerConditionsViewModel(Factory.New<IUniversalTemplateTrigger>());
			viewModel.TriggerConditionValue = "Rubber baby buggy bumpers. M○○♣rty.";

			AssertEquals("Rubber baby buggy bumpers. M○○♣rty.", viewModel.TriggerConditionValue);
			AssertHasError(viewModel.TriggerConditionValueInfo, "Trigger Condition Value only accepts Western European languages characters.");
		}

		public void TestConditionValue_RFP_WarnOnInvalid_Pipe()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var viewModel = milestone.TriggerConditions;
			viewModel.TriggerEventCode = Events.CustomisableEvent00Code;
			viewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			viewModel.TriggerConditionValue = "|BUG=LUG,|BIG=PIG";
			AssertHasWarning(viewModel.TriggerConditionValueInfo,
   "Condition [|BUG=LUG,|BIG=PIG] for Event Reference with Parameters contains invalid parameters. This field should never contain the '|' character.");
		}

		public void TestConditionValue_RFP_WarnOnInvalid_KeyTooLong()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var viewModel = milestone.TriggerConditions;
			viewModel.TriggerEventCode = Events.CustomisableEvent00Code;
			viewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			viewModel.TriggerConditionValue = "POTATO=WINDOWS,GARAGE=SCREWB";
			AssertHasWarning(viewModel.TriggerConditionValueInfo,
 "Condition [POTATO=WINDOWS,GARAGE=SCREWB] for Event Reference with Parameters contains invalid parameters. Parameter keys (e.g. KEY=VALUE) should be one to three characters long.");
		}

		public void TestConditionValue_RFP_LengthValid()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var viewModel = milestone.TriggerConditions;
			viewModel.TriggerEventCode = Events.CustomisableEvent00Code;
			viewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			viewModel.TriggerConditionValue = "T=WINDOWS,TO=SCREWB,TOD=SCREWB";
			AssertNoWarnings(viewModel.TriggerConditionValueInfo);
		}

		#endregion

		#region Milestone Validation

		public void TestTriggerEventCode_MandatoryValidation()
		{
			Trigger.P9_Type = Constants.Workflow.UndefinedTaskType;
			Trigger.TriggerConditions.TriggerEventCode = ZString.Empty;

			AssertMandatoryValidationError(ViewModel.TriggerEventCodeInfo, false);

			Trigger.IsMilestone = true;
			Trigger.TriggerConditions.TriggerEventCode = "XXX";
			Trigger.TriggerConditions.TriggerEventCode = ZString.Empty;

			AssertMandatoryValidationError(ViewModel.TriggerEventCodeInfo, true);
		}

		public void TestTriggerEventCode_MustBeUniqueWithinJobAndReferencedID()
		{
			var job1 = Factory.New<DummyWithWorkflow>();
			var job2 = Factory.New<DummyWithWorkflow>();

			var milestone1 = job1.WorkflowItems.Milestones.AddNew();
			var milestone2 = job1.WorkflowItems.Milestones.AddNew();
			var milestone3 = job2.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone3.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			milestone1.TriggerConditions.Validation.ValidateAll();
			milestone2.TriggerConditions.Validation.ValidateAll();
			milestone3.TriggerConditions.Validation.ValidateAll();

			AssertHasWarning(milestone1.TriggerConditions.TriggerEventCodeInfo, "There is more than one milestone for this event.");
			AssertHasWarning(milestone2.TriggerConditions.TriggerEventCodeInfo, "There is more than one milestone for this event.");
			AssertNoNotifications(milestone3.TriggerConditions.TriggerEventCodeInfo);

			milestone2.P9_ReferencedID = ZGuid.NewZGuid();
			milestone2.TriggerConditions.Validation.ValidateAll();
			AssertNoNotifications(milestone2.TriggerConditions.TriggerEventCodeInfo);
		}

		public void TestP9_SE_NKMilestoneEvent_MustBeUnique_ExceptForDepartureWithDifferentConditions()
		{
			TestP9_SE_NKMilestoneEvent_MustBeUnique_ExceptForArrivalOrDeparturesWithDifferentConditions(Events.Departure);
		}

		public void TestP9_SE_NKMilestoneEvent_MustBeUnique_ExceptForArrivalWithDifferentConditions()
		{
			TestP9_SE_NKMilestoneEvent_MustBeUnique_ExceptForArrivalOrDeparturesWithDifferentConditions(Events.Arrival);
		}

		public void TestP9_SE_NKMilestoneEvent_MustBeUnique_ExceptForDifferentEventReferenceConditions()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone1 = job.WorkflowItems.Milestones.AddNew();
			var milestone2 = job.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestone1.TriggerConditions.TriggerConditionValue = "Ref 1";
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestone2.TriggerConditions.TriggerConditionValue = "Ref 2";

			milestone2.TriggerConditions.Validation.ValidateAll();
			AssertNoWarnings("Event Reference conditions are different", milestone2.TriggerConditions.TriggerEventCodeInfo);

			milestone2.TriggerConditions.TriggerConditionValue = "Ref 1";
			milestone2.TriggerConditions.Validation.ValidateAll();
			AssertHasWarning(milestone2.TriggerConditions.TriggerEventCodeInfo, "There is more than one milestone for this event.");
		}

		void TestP9_SE_NKMilestoneEvent_MustBeUnique_ExceptForArrivalOrDeparturesWithDifferentConditions(Event eventType)
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone1 = job.WorkflowItems.Milestones.AddNew();
			var milestone2 = job.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = eventType.Code;
			milestone2.TriggerConditions.TriggerEventCode = eventType.Code;
			AssertHasWarnings("When P9_SE_NKMilestoneEvent is duplicated with no condition", milestone2.TriggerConditions.TriggerEventCodeInfo);

			milestone1.TriggerConditions.TriggerEventCode = eventType.Code;
			milestone2.TriggerConditions.TriggerEventCode = eventType.Code;
			milestone2.TemplateConditions.TemplateCondition1 = "XXX";
			milestone2.TriggerConditions.Validation.ValidateAll();
			AssertNoWarnings("When conditions are different", milestone2.TriggerConditions.TriggerEventCodeInfo);
		}

		#endregion

		#region Trigger Validation

		public void TestTriggerFieldName_Or_TriggerEventCode_OneIsMandatory()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);
			Trigger.TriggerConditions.TriggerFieldName = "XXX";
			Trigger.TriggerConditions.TriggerFieldName = "";
			Trigger.TriggerConditions.TriggerEventCode = "XXX";
			Trigger.TriggerConditions.TriggerEventCode = "";
			AssertHasErrors("Error when neither trigger field or event is specified", Trigger.TriggerConditions.TriggerFieldNameInfo);
			AssertHasErrors("Error when neither trigger field or event is specified", Trigger.TriggerConditions.TriggerEventCodeInfo);
			Trigger.TriggerConditions.TriggerEventCode = "XXX";

			Trigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			Trigger.TriggerConditions.TriggerEventCode = "";
			AssertNoErrors("No error when trigger field specified", Trigger.TriggerConditions.TriggerFieldNameInfo);
			AssertNoErrors("No error when trigger field specified", Trigger.TriggerConditions.TriggerEventCodeInfo);

			Trigger.TriggerConditions.TriggerEventCode = Events.Booked.Code;
			Trigger.TriggerConditions.TriggerFieldName = "";
			AssertNoErrors("No error when event specified", Trigger.TriggerConditions.TriggerFieldNameInfo);
			AssertNoErrors("No error when event specified", Trigger.TriggerConditions.TriggerEventCodeInfo);
		}

		public void TestTriggerFieldName_OughtToExist()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);
			Trigger.TriggerConditions.TriggerFieldName = "SNOGGO";
			// Take note: It is possible to use fields that are not in the list.
			AssertHasWarnings("This is clearly not a real field...", Trigger.TriggerConditions.TriggerFieldNameInfo);
		}

		public void TestTriggerEventCode_ForEditEvent()
		{
			Trigger.TriggerConditions.TriggerEventCode = Events.CustomsCommenced.Code;
			AssertNoWarnings("No warnings initially", Trigger.TriggerConditions.TriggerEventCodeInfo);

			Trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			AssertHasWarning("Warning when triggering on edit event", Trigger.TriggerConditions.TriggerEventCodeInfo, "An 'Edit' trigger action will only occur when the edit happens on the module's form by a user. A data import or sailing schedule change will not cause the trigger action to occur.");

			Trigger.IsMilestone = true;
			Trigger.TriggerConditions.Validation.ValidateAll();
			AssertNoWarnings("Warning does not apply to milestones", Trigger.TriggerConditions.TriggerEventCodeInfo);
		}

		public void TestTriggerCondition_RFP()
		{
			Trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			Trigger.TriggerConditions.TriggerConditionValue = "SNOGGO";
			AssertHasWarning(Trigger.TriggerConditions.TriggerConditionValueInfo, "Condition [SNOGGO] for Event Reference with Parameters contains invalid parameters. Parameters should always contain '=' and multiple parameters should be separated by ',' characters. (e.g. LOC=BIL,NAM=VIN).");
		}

		public void TestTriggerCondition_RFP_Template()
		{
			var trigger = Factory.NewWithValidTestData<ProcessTaskTemplate>().WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger.TriggerConditions.TriggerConditionValue = "SNOGGO";
			AssertHasError(trigger.TriggerConditions.TriggerConditionValueInfo, "Condition [SNOGGO] for Event Reference with Parameters contains invalid parameters. Parameters should always contain '=' and multiple parameters should be separated by ',' characters. (e.g. LOC=BIL,NAM=VIN).");
		}

		public void TestTriggerCondition_RFP_Multi()
		{
			var trigger = Factory.NewWithValidTestData<ProcessTaskTemplate>().WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger.TriggerConditions.TriggerConditionValue = "SNOGGO,BIG=BOG";
			AssertHasError(trigger.TriggerConditions.TriggerConditionValueInfo, "Condition [SNOGGO,BIG=BOG] for Event Reference with Parameters contains invalid parameters. Parameters should always contain '=' and multiple parameters should be separated by ',' characters. (e.g. LOC=BIL,NAM=VIN).");
		}

		#endregion

		#region Task Validation

		public void TestTriggerEventAndField_ForTask_ShouldNotBeMandatory()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			var standaloneTask = Factory.New<ProcessTask>();

			task.TriggerConditions.Validation.ValidateAll();
			standaloneTask.TriggerConditions.Validation.ValidateAll();

			AssertNoErrors(task.TriggerConditions.TriggerEventCodeInfo);
			AssertNoErrors(task.TriggerConditions.TriggerFieldNameInfo);

			AssertNoErrors(standaloneTask.TriggerConditions.TriggerEventCodeInfo);
			AssertNoErrors(standaloneTask.TriggerConditions.TriggerFieldNameInfo);
		}

		#endregion

		#region Exception Validation

		public void TestExceptionsDontHaveCodes()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = dummy.WorkflowItems.Triggers.AddNew();
			var viewModel = exception.TriggerConditions;

			viewModel.Validation.ValidateAll();
			AssertHasError(viewModel.TriggerEventCodeInfo, "You must specify a Trigger Event Code or Trigger Field Name.");
			exception.P9_Type = "EXC";
			viewModel.Validation.ValidateAll();
			AssertNoError(viewModel.TriggerEventCodeInfo, "You must specify a Trigger Event Code or Trigger Field Name.");
		}

		#endregion

		#region TriggerContextCode

		public void TestValidateTriggerContextCode()
		{
			Trigger.TriggerConditions.TriggerContextCode = "NEH";
			AssertListValidationInvalidCodeError(Trigger.TriggerConditions.TriggerContextCodeInfo, true);

			Trigger.TriggerConditions.TriggerContextCode = "";
			AssertMandatoryValidationError(Trigger.TriggerConditions.TriggerContextCodeInfo, true);

			Trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Default;
			AssertNoErrors(Trigger.TriggerConditions.TriggerContextCodeInfo);
		}

		public void TestValidateTriggerContextCode_TriggerField()
		{
			Trigger.TriggerConditions.TriggerFieldName = "GEHK";
			Trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Event;
			AssertHasError(Trigger.TriggerConditions.TriggerContextCodeInfo, "Cannot use EVT when Trigger Field (GEHK) is set.");
			Trigger.TriggerConditions.TriggerFieldName = "";
			Trigger.TriggerConditions.Validation.ValidateTriggerContextCode();
			AssertNoErrors(Trigger.TriggerConditions.TriggerContextCodeInfo);
		}

		public void TestValidateTriggerField_WithEVTContextCode()
		{
			Trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Event;
			Trigger.TriggerConditions.TriggerFieldName = "GEHK";
			AssertHasError(Trigger.TriggerConditions.TriggerFieldNameInfo, "Cannot use EVT when Trigger Field (GEHK) is set.");
			Trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Default;
			Trigger.TriggerConditions.Validation.ValidateTriggerFieldName();
			AssertNoErrors(Trigger.TriggerConditions.TriggerFieldNameInfo);
		}

		#endregion

		#region TriggerStaffCode

		public void TestValidateTriggerStaffCode()
		{
			Trigger.TriggerConditions.TriggerStaffCode = "GEH";
			AssertListValidationInvalidCodeError(Trigger.TriggerConditions.TriggerStaffCodeInfo, true);
			Trigger.TriggerConditions.TriggerStaffCode = "";
			AssertListValidationInvalidCodeError(Trigger.TriggerConditions.TriggerStaffCodeInfo, false);
		}

		#endregion

		#region TriggerBranch

		public void TestValidateTriggerBranch()
		{
			Trigger.TriggerConditions.TriggerBranch = ZGuid.BrettsGuid;
			AssertListValidationInvalidPKError(Trigger.TriggerConditions.TriggerBranchInfo, true);
			Trigger.TriggerConditions.TriggerBranch = ZGuid.Empty;
			AssertListValidationInvalidPKError(Trigger.TriggerConditions.TriggerBranchInfo, false);
		}

		#endregion

		#region TriggerCompany

		public void TestValidateTriggerCompany()
		{
			Trigger.TriggerConditions.TriggerCompany = ZGuid.BrettsGuid;
			AssertListValidationInvalidPKError(Trigger.TriggerConditions.TriggerCompanyInfo, true);
			AssertMandatoryValidationError(Trigger.TriggerConditions.TriggerCompanyInfo, false);
			Trigger.TriggerConditions.TriggerCompany = ZGuid.Empty;
			AssertListValidationInvalidPKError(Trigger.TriggerConditions.TriggerCompanyInfo, false);
			AssertMandatoryValidationError(Trigger.TriggerConditions.TriggerCompanyInfo, true);

			Trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Event;
			AssertListValidationInvalidPKError(Trigger.TriggerConditions.TriggerCompanyInfo, false);
			AssertMandatoryValidationError(Trigger.TriggerConditions.TriggerCompanyInfo, false);
		}

		public void TestTriggerCompany_ForTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCompany = ZGuid.BrettsGuid;
			AssertMandatoryValidationError(trigger.TriggerConditions.TriggerCompanyInfo, false);
		}

		#endregion

		#region TriggerDepartment

		public void TestValidateTriggerDepartment()
		{
			Trigger.TriggerConditions.TriggerDepartment = ZGuid.BrettsGuid;
			AssertListValidationInvalidPKError(Trigger.TriggerConditions.TriggerDepartmentInfo, true);
			Trigger.TriggerConditions.TriggerDepartment = ZGuid.Empty;
			AssertListValidationInvalidPKError(Trigger.TriggerConditions.TriggerDepartmentInfo, false);
		}

		#endregion

		#region Implementation

		ProcessTask Trigger;
		TriggerConditionsViewModel ViewModel;

		protected override void SetUp()
		{
			base.SetUp();

			var job = Factory.New<DummyWithWorkflow>();
			Trigger = job.WorkflowItems.Triggers.AddNew();
			ViewModel = Trigger.TriggerConditions;
		}

		#endregion
	}
}
