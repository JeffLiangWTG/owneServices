using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	class ProcessTemplateTriggerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSequence()
		{
			var trigger = WorkflowTestCase.CreateTrigger(Factory);

			AssertEquals(1, trigger.P9T_Sequence);

			trigger.P9T_Sequence = 0;
			AssertHasError(trigger.P9T_SequenceInfo, "Please enter a 'Sequence' greater than or equal to 1.");

			trigger.P9T_Sequence = -1;
			AssertHasError(trigger.P9T_SequenceInfo, "Please enter a 'Sequence' greater than or equal to 1.");

			trigger.P9T_Sequence = 1;
			AssertNoErrors(trigger.P9T_SequenceInfo);
		}

		public void TestDescription()
		{
			var template = WorkflowTestCase.CreateTemplate(Factory);
			Factory.Save();

			var trigger = WorkflowTestCase.CreateTrigger(template);
			trigger.P9T_Description = "";

			AssertExceptionThrown<ZSaveException>(Factory.Save);
			AssertMandatoryValidationError(trigger.P9T_DescriptionInfo, true);

			trigger.P9T_Description = "Uh oh!";
			AssertMandatoryValidationError(trigger.P9T_DescriptionInfo, false);

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestTriggerFiredCountdown()
		{
			var template = WorkflowTestCase.CreateTemplate(Factory);
			Factory.Save();

			var trigger = WorkflowTestCase.CreateTrigger(template);
			trigger.P9T_Description = "Test Desc";
			Factory.Save();

			AssertNoWarnings(trigger.P9T_TriggerFiredCountdownInfo);

			trigger.P9T_TriggerFiredCountdown = 0;
			AssertHasWarning(trigger.P9T_TriggerFiredCountdownInfo, "This trigger has fired too many times and will not fire any more unless this value is increased. It may be part of an infinite loop.");

			trigger.P9T_TriggerFiredCountdown = 1;
			AssertNoWarnings(trigger.P9T_TriggerFiredCountdownInfo);
		}

		public void TestP9T_P0_Template_ShouldNotHaveError_WhenAddTriggerToUnactiveUniversalTemplate()
		{
			var template = WorkflowTestCase.CreateTemplate(Factory);
			template.P0_IsUniversal = true;
			template.P0_IsActive = false;
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			Factory.Save();
			var newFactory = NewFactory();
			var savedTemplate = newFactory.Load<ProcessTaskTemplate>(template.PK);

			var trigger = WorkflowTestCase.CreateTrigger(savedTemplate);
			var messageError = "should be warning not error, showWarningIfCancelled is not set properly.";
			AssertNoErrors(messageError, trigger.P9T_P0_TemplateInfo);
		}

		[TestDate(2022, 06, 07)]
		public void TestIsEstimate_ShouldNotBeAllowedToUntick_WhenThereAreCompletionTriggerActionsWithNegativeOffset()
		{
			var template = WorkflowTestCase.CreateTemplate(Factory);
			Factory.Save();

			var trigger = WorkflowTestCase.CreateTrigger(template);
			trigger.P9T_Description = "Test trigger";

			var action = WorkflowTestCase.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);

			var zeroOffset = new ZDateTime(2022, 1, 1);
			var positiveOffset = zeroOffset.AddHours(1);
			var negativeOffset = zeroOffset.AddHours(-1);

			action.PQ_Offset = positiveOffset;

			trigger.P9T_IsEstimate = false;

			string error = "The trigger cannot respond to actual events as there are DLY completion trigger actions set with a negative offset. Negative offsets are allowed for triggers responding to estimate events only.";
			AssertNoError(trigger.P9T_IsEstimateInfo, error);

			trigger.P9T_IsEstimate = true;
			AssertNoError(trigger.P9T_IsEstimateInfo, error);

			action.PQ_Offset = zeroOffset;

			trigger.P9T_IsEstimate = false;
			AssertNoError(trigger.P9T_IsEstimateInfo, error);

			trigger.P9T_IsEstimate = true;
			AssertNoError(trigger.P9T_IsEstimateInfo, error);

			action.PQ_Offset = negativeOffset;

			trigger.P9T_IsEstimate = false;
			AssertHasError(trigger.P9T_IsEstimateInfo, error);

			trigger.P9T_IsEstimate = true;
			AssertNoError(trigger.P9T_IsEstimateInfo, error);

			action.PQ_Offset = positiveOffset;
			AssertNoError(trigger.P9T_IsEstimateInfo, error);

			trigger.P9T_IsEstimate = false;
			AssertNoError(trigger.P9T_IsEstimateInfo, error);

			action.PQ_Offset = negativeOffset;
			AssertHasError(trigger.P9T_IsEstimateInfo, error);

			Factory.Save();
		}

		public void TestSuppressDuplicates_CannotBeSetWithoutDelayDuration()
		{
			var error = "You cannot set 'Suppress Duplicates' when 'Delay Duration' is 0.";

			var template = WorkflowTestCase.CreateTemplate(Factory);
			var trigger = WorkflowTestCase.CreateTrigger(template);
			trigger.P9T_Description = "Test trigger";
			trigger.P9T_DelayDuration = TimeSpan.FromSeconds(0);

			AssertNoError(trigger.P9T_SuppressDuplicatesInfo, error);

			trigger.P9T_SuppressDuplicates = true;
			AssertHasError(trigger.P9T_SuppressDuplicatesInfo, error);

			trigger.P9T_DelayDuration = TimeSpan.FromSeconds(10);
			trigger.RunPreSaveValidation();
			AssertNoError(trigger.P9T_SuppressDuplicatesInfo, error);
		}
	}
}
