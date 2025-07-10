using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(PortTransportFormExceptionHandler))]
	class PortTransportFormExceptionHandlerTest : TestCaseWithFactory
	{
		public void TestHandleSaveExceptionForTriggers_ForDuplicateLegTriggerZSaveException_ReturnsTrueAndDisplaysMessage()
		{
			var triggerException = new ZSaveException(new ZDataException(new Exception(RawTriggerMessage), ((INeedRow)Factory.New<DummyBusinessObject>()).Row, TestConnection), Factory);
			var exceptionDescription = FormattableString.Invariant($"ZSaveException with inner-most exception message '{PortTransportFormExceptionHandler.JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTriggerID}'");
			var expectedErrorMessage = FormattableString.Invariant($"Attempted to insert duplicate suffix on cartage job leg(s). Try to close and re-open the form.");
			HandleSaveExceptionForTriggersCoreTest(
				triggerException,
				true,
				exceptionDescription,
				expectedErrorMessage);
		}

		public void TestHandleSaveExceptionForTriggers_ForNonDuplicateLegTriggerZSaveException_ReturnsFalseAndDisplaysNoMessage()
		{
			var nonTriggerException = new ZSaveException(new ZDataException(new Exception("Some other message"), ((INeedRow)Factory.New<DummyBusinessObject>()).Row, TestConnection), Factory);
			var exceptionDescription = FormattableString.Invariant($"ZSaveException with InnerException with InnerException with different message to '{PortTransportFormExceptionHandler.JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTriggerID}'");
			HandleSaveExceptionForTriggersCoreTest(
				nonTriggerException,
				false,
				exceptionDescription);
		}

		public void TestHandleSaveExceptionForTriggers_ForNonZSaveException_ReturnsFalseAndDisplaysNoMessage()
		{
			var nonTriggerException = new Exception("Some other message");
			var exceptionDescription = "non-ZSaveException";
			HandleSaveExceptionForTriggersCoreTest(
				nonTriggerException,
				false,
				exceptionDescription);
		}

		void HandleSaveExceptionForTriggersCoreTest(Exception ex, bool expectedReturnValue, string exceptionDescription, string expectedMessageDisplayed = null)
		{
			UnitTestUserNotification.Instance.ClearMessages();

			CombineAssertions(
				FormattableString.Invariant($"Check that PortTransportFormExceptionHandler.HandleSaveExceptionForTriggers() works correctly for {exceptionDescription}"),
				() =>
				{
					AssertEquals(FormattableString.Invariant($"HandleSaveExceptionForTriggers() should return {expectedReturnValue} for {exceptionDescription}"), expectedReturnValue, PortTransportFormExceptionHandler.HandleSaveExceptionForTriggers(ex));
					var notificationsAssertMessage = expectedReturnValue ? FormattableString.Invariant($"Correct error message '{expectedMessageDisplayed}' was displayed.") : "No error message was displayed";
					AssertEquals(notificationsAssertMessage, expectedMessageDisplayed, UnitTestUserNotification.Instance.LastMessage.Text);
				});
		}

		const string RawTriggerMessage = "TriggerLikelyConcurrencyError: Attempted to insert duplicate suffix on cartage job leg(s).";
	}
}
