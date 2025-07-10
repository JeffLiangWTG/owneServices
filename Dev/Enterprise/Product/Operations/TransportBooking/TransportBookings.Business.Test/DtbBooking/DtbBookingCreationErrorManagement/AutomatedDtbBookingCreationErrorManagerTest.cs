using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.TransportBookings.Business.Test
{
	public class AutomatedDtbBookingCreationErrorManagerTest : TestCaseWithFactory
	{
		public void TestAddZSaveConcurrencyError()
		{
			TestAddErrorCore(DtbBookingCreationErrorType.ZSaveConcurrencyError, "A ZSaveConcurrency error occurred", true, false);
		}

		public void TestAddZCannotSaveError()
		{
			TestAddErrorCore(DtbBookingCreationErrorType.ZCannotSaveError, "A ZCannotSave error occurred", true, false);
		}

		public void TestAddServiceHasCommencedError()
		{
			TestAddErrorCore(DtbBookingCreationErrorType.ServiceHasCommencedError, "Cannot create a transport booking for a consolidation where service has already commenced", false, false);
		}

		public void TestConsolidationCannotBeOverriddenAgainError()
		{
			TestAddErrorCore(DtbBookingCreationErrorType.ConsolidationCannotBeOverriddenAgainError, "Consolidation has already been overridden and cannot be overridden again", false, false);
		}

		public void TestAddCancelledParentError()
		{
			TestAddErrorCore(DtbBookingCreationErrorType.CancelledParentError, "Cannot create a Transport Booking for a cancelled parent job", false, false);
		}

		public void TestAddInvalidParentError()
		{
			var exception = new InvalidOperationException("Cannot create a Transport Booking for an invalid parent job");
			TestAddErrorCore(DtbBookingCreationErrorType.InvalidParentError, exception, false, false);
		}

		public void TestAddUniversalResultError()
		{
			TestAddErrorCore(DtbBookingCreationErrorType.UniversalResultError, "An error occurred during Universal Xml processing", false, false);
		}

		public void TestAddUniversalEventDeliveryFailureError()
		{
			TestAddErrorCore(DtbBookingCreationErrorType.UniversalEventDeliveryFailure, "A universal event delivery failure occurred", false, false);
		}

		public void TestAddDuplicateBookingConsolidationError()
		{
			TestAddErrorCore(DtbBookingCreationErrorType.DuplicateBookingConsolidationError, "Cannot insert duplicate key row in object 'DtbBookingConsolidation' with unique index 'NR_UX__KB_ParentID_KB_ParentTableCode_KB_JobDirection'", true, false);
		}

		public void TestAddExistingBookingIsSubError()
		{
			TestAddErrorCore(DtbBookingCreationErrorType.ExistingBookingIsSubError, "Booking is a sub booking to Master Transport Booking TB00000001 and cannot be overwritten.", false, false);
		}

		public void TestAddUnknownError()
		{
			var exception = new Exception("An unknown error occurred");
			TestAddErrorCore(DtbBookingCreationErrorType.UnknownError, exception, true, true);
		}

		public void TestClearErrors()
		{
			AddAnError();
			AssertEquals("Precondition: ErrorList is not empty", true, manager.ErrorList.Any());

			manager.ClearErrors();
			AssertEquals("ClearErrors() should clear ErrorList", false, manager.ErrorList.Any());
		}

		public void TestRetryNoExceptionsReturnsFalse()
		{
			AssertEquals("Precondition: Reorder starts with no errors in ErrorList", 0, manager.ErrorList.Count);

			AssertEquals("Retry should return false for case with 0 errors in ErrorList", false, manager.Retry);
		}

		public void TestRetryOneExceptionWithRetryFalseReturnsFalse()
		{
			AddRetryFalseError();
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Manager has been set up with one exception", 1, manager.ErrorList.Count);
				AssertEquals("Precondition: Manager has been set up with one exception with Retry == false", false, manager.ErrorList.Single().Retry);
			});

			AssertEquals("Retry should return false for case with 1 exception in ErrorList, first exception has Retry == false", false, manager.Retry);
		}

		public void TestRetryOneExceptionWithRetryTrueReturnsTrue()
		{
			AddRetryTrueError();
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Manager has been set up with one exception", 1, manager.ErrorList.Count);
				AssertEquals("Precondition: Manager has been set up with one exception with Retry == true", true, manager.ErrorList.Single().Retry);
			});

			AssertEquals("Retry should return true for case with 1 exception in ErrorList, first exception has Retry == true", true, manager.Retry);
		}

		public void TestRetryTwoExceptionsWithRetryTrueAndFalseReturnsFalse()
		{
			AddRetryTrueError();
			AddRetryFalseError();
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Manager has been set up with two exceptions", 2, manager.ErrorList.Count);
				AssertContainsExactElementsInAnyOrder("Precondition: Record has been set up with exceptions one with Retry == false and one with Retry == true", new bool[] { false, true }, manager.ErrorList.Select(e => e.Retry));
			});

			AssertEquals("Retry should return false for case with 2 exceptions, one with Retry == false and one with Retry == true", false, manager.Retry);
		}

		public void TestRetryTwoExceptionsWithRetryTrueAndTrueReturnsTrue()
		{
			AddRetryTrueError();
			AddRetryTrueError();
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Manager has been set up with two exceptions", 2, manager.ErrorList.Count);
				AssertContainsExactElementsInAnyOrder("Precondition: Record has been set up with exceptions both with Retry == true", new bool[] { true, true }, manager.ErrorList.Select(e => e.Retry));
			});

			AssertEquals("Retry should return false for case with 2 exceptions both with Retry == true", true, manager.Retry);
		}

		public void TestRetryTwoExceptionsWithRetryFalseAndFalseReturnsFalse()
		{
			AddRetryFalseError();
			AddRetryFalseError();
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Manager has been set up with two exceptions", 2, manager.ErrorList.Count);
				AssertContainsExactElementsInAnyOrder("Precondition: Record has been set up with exceptions both with Retry == false", new bool[] { false, false }, manager.ErrorList.Select(e => e.Retry));
			});

			AssertEquals("Retry should return false for case with 2 exceptions both with Retry == false", false, manager.Retry);
		}

		public void TestSendToErrorReporterNoExceptionsReturnsFalse()
		{
			AssertEquals("Precondition: Manager starts with no errors in ErrorList", 0, manager.ErrorList.Count);

			CombineAssertions(() =>
			{
				AssertEquals("SendToErrorReporter should return false for case with 0 errors in ErrorList", false, manager.SendToErrorReporter);
				AssertEquals("MessageToSendToErrorReporter should return empty string for case with 0 errors in ErrorList", string.Empty, manager.MessageToSendToErrorReporter);
				AssertEquals("MessageToLog should return empty string as there are no errors", string.Empty, manager.MessageToLog);
			});
		}

		public void TestSendToErrorReporterOneExceptionWithSendToErrorReporterFalseReturnsFalse()
		{
			AddSendToErrorReporterFalseError();
			CombineAssertions("Preconditions",
				() =>
				{
					AssertEquals("Precondition: Manager has been set up with one exception", 1, manager.ErrorList.Count);
					AssertEquals("Precondition: Manager has been set up with one exception with SendToErrorReporter == false", false, manager.ErrorList.First().SendToErrorReporter);
				});

			CombineAssertions(() =>
			{
				AssertEquals("SendToErrorReporter should return false for case with 1 exception in ErrorList, first exception has SendToErrorReporter == false", false, manager.SendToErrorReporter);
				AssertEquals("MessageToSendToErrorReporter should return empty string for case with 1 exception in ErrorList, first exception has SendToErrorReporter == false", string.Empty, manager.MessageToSendToErrorReporter);
				AssertEquals("MessageToLog should return the message from the only member of the ErrorList", "Some cannot save error", manager.MessageToLog);
			});
		}

		public void TestSendToErrorReporterOneExceptionWithSendToErrorReporterTrueReturnsTrue()
		{
			AddSendToErrorReporterTrueError();
			CombineAssertions("Preconditions",
				() =>
				{
					AssertEquals("Precondition: Manager has been set up with one exception", 1, manager.ErrorList.Count);
					AssertEquals("Precondition: Manager has been set up with one exception with SendToErrorReporter == true", true, manager.ErrorList.First().SendToErrorReporter);
				});

			CombineAssertions(() =>
			{
				AssertEquals("SendToErrorReporter should return true for case with 1 exception in ErrorList, first exception has SendToErrorReporter == true", true, manager.SendToErrorReporter);
				AssertEquals("MessageToSendToErrorReporter should return exception message for case where 1 errors in ErrorList with SendToErrorReporter == true", "Some unknown error", manager.MessageToSendToErrorReporter);
				AssertEquals("MessageToLog should return the message from the only member of the ErrorList", "Some unknown error", manager.MessageToLog);
				AssertNotNull("Should store exception", manager.LastException);
			});
		}

		public void TestSendToErrorReporterTwoExceptionsWithSendToErrorReporterTrueAndFalseReturnsTrue()
		{
			AddSendToErrorReporterTrueError();
			AddSendToErrorReporterFalseError();
			CombineAssertions("Preconditions",
				() =>
				{
					AssertEquals("Precondition: Manager has been set up with two exceptions", 2, manager.ErrorList.Count);
					AssertContainsExactElementsInAnyOrder("Precondition: Record has been set up with exceptions one with SendToErrorReporter == false and one with SendToErrorReporter == true", new bool[] { false, true }, manager.ErrorList.Select(e => e.SendToErrorReporter));
				});

			CombineAssertions(() =>
			{
				AssertEquals("SendToErrorReporter should return true for case with 2 exceptions, one with SendToErrorReporter == false and one with SendToErrorReporter == true", true, manager.SendToErrorReporter);
				AssertEquals("MessageToSendToErrorReporter should return exception message for case where 2 errors in ErrorList, one with SendToErrorReporter == false and one with SendToErrorReporter == true", "Some unknown error", manager.MessageToSendToErrorReporter);
				AssertEquals("MessageToLog should return all messages concatenated with a new line for more than one error in ErrorList", "Some unknown error" + System.Environment.NewLine + "Some cannot save error", manager.MessageToLog);
				AssertNotNull("Should store exception", manager.LastException);
			});
		}

		public void TestSendToErrorReporterTwoExceptionsWithSendToErrorReporterTrueAndTrueReturnsTrue()
		{
			AddSendToErrorReporterTrueError();
			AddSendToErrorReporterTrueError();
			CombineAssertions("Preconditions",
				() =>
				{
					AssertEquals("Precondition: Manager has been set up with two exceptions", 2, manager.ErrorList.Count);
					AssertContainsExactElementsInAnyOrder("Precondition: Record has been set up with exceptions both with SendToErrorReporter == true", new bool[] { true, true }, manager.ErrorList.Select(e => e.SendToErrorReporter));
				});

			CombineAssertions(() =>
			{
				AssertEquals("SendToErrorReporter should return false for case with 2 exceptions both with SendToErrorReporter == true", true, manager.SendToErrorReporter);
				AssertEquals("MessageToSendToErrorReporter should return messages concatenated with a new line for case where 2 errors in ErrorList both with SendToErrorReporter == true", "Some unknown error" + System.Environment.NewLine + "Some unknown error", manager.MessageToSendToErrorReporter);
				AssertEquals("MessageToLog should return all messages concatenated with a new line for more than one error in ErrorList", "Some unknown error" + System.Environment.NewLine + "Some unknown error", manager.MessageToLog);
				// Even though both errors have an exception, only return the last one. In reality, the manager will probably only have one error that has an exception.
				AssertNotNull("Should store exception", manager.LastException);
			});
		}

		public void TestSendToErrorReporterTwoExceptionsWithSendToErrorReporterFalseAndFalseReturnsFalse()
		{
			AddSendToErrorReporterFalseError();
			AddSendToErrorReporterFalseError();
			CombineAssertions("Preconditions",
				() =>
				{
					AssertEquals("Precondition: Manager has been set up with two exceptions", 2, manager.ErrorList.Count);
					AssertContainsExactElementsInAnyOrder("Precondition: Record has been set up with exceptions both with SendToErrorReporter == false", new bool[] { false, false }, manager.ErrorList.Select(e => e.SendToErrorReporter));
				});

			CombineAssertions(() =>
			{
				AssertEquals("SendToErrorReporter should return false for case with 2 exceptions both with SendToErrorReporter == false", false, manager.SendToErrorReporter);
				AssertEquals("MessageToSendToErrorReporter should return empty string for case where 2 errors in ErrorList both with SendToErrorReporter == false", string.Empty, manager.MessageToSendToErrorReporter);
				AssertEquals("MessageToLog should return all messages concatenated with a new line for more than one error in ErrorList", "Some cannot save error" + System.Environment.NewLine + "Some cannot save error", manager.MessageToLog);
				AssertNull("No exception to store", manager.LastException);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			manager = new AutomatedDtbBookingCreationErrorManager();
		}

		protected override void TearDown()
		{
			base.TearDown();

			manager = null;
		}

		void TestAddErrorCore(DtbBookingCreationErrorType errorType, string errorMessage, bool expectedRetry, bool expectedSendToErrorReporter)
		{
			manager.ClearErrors();
			manager.AddError(errorType, errorMessage);
			var error = manager.ErrorList.Single();
			CombineAssertions(() =>
			{
				AssertEquals("error.Message should be correct", errorMessage, error.Message);
				AssertEquals("error.Retry should be correct", expectedRetry, error.Retry);
				AssertEquals("error.SendToErrorReporter should be correct", expectedSendToErrorReporter, error.SendToErrorReporter);
				AssertNull("error.Exception should be null", error.Exception);
				AssertNull("manager.LastException should be null", manager.LastException);
			});
		}

		void TestAddErrorCore(DtbBookingCreationErrorType errorType, Exception exception, bool expectedRetry, bool expectedSendToErrorReporter)
		{
			manager.ClearErrors();
			manager.AddError(errorType, exception);
			var error = manager.ErrorList.Single();
			CombineAssertions(() =>
			{
				AssertEquals("error.Message should be correct", exception.Message, error.Message);
				AssertEquals("error.Retry should be correct", expectedRetry, error.Retry);
				AssertEquals("error.SendToErrorReporter should be correct", expectedSendToErrorReporter, error.SendToErrorReporter);
				AssertEquals("error.Exception should be correct", exception, error.Exception);
				AssertEquals("manager.LastException should be correct", exception, manager.LastException);
			});
		}

		void AddAnError()
		{
			manager.AddError(DtbBookingCreationErrorType.ServiceHasCommencedError, "Service has commenced");
		}

		void AddRetryTrueError()
		{
			manager.AddError(DtbBookingCreationErrorType.ZSaveConcurrencyError, "Some concurrency error");
		}

		void AddRetryFalseError()
		{
			manager.AddError(DtbBookingCreationErrorType.UniversalResultError, "Some xml processing error");
		}

		void AddSendToErrorReporterTrueError()
		{
			var exception = new Exception("Some unknown error");
			manager.AddError(DtbBookingCreationErrorType.UnknownError, exception);
		}

		void AddSendToErrorReporterFalseError()
		{
			manager.AddError(DtbBookingCreationErrorType.ZCannotSaveError, "Some cannot save error");
		}

		AutomatedDtbBookingCreationErrorManager manager;
	}
}
