using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CriticalValidationServiceTest : TestCaseWithFactory
	{
		public void TestNextSavePreventedOnFailedSaveRollback()
		{
			((IBusinessObjectFactoryInternals)Factory).LastSavingRollbackHadException = false;
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);

			AssertNoExceptionThrown(() => Factory.Save());

			((IBusinessObjectFactoryInternals)Factory).LastSavingRollbackHadException = true;
			bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();

			var exception = AssertExceptionThrown<CannotSaveAfterCriticalErrorException>(() => Factory.Save());
			AssertEquals("A critical saving error has occurred. Please close the form, then try again.", exception.Message);

			Globals.IsUserInteractive = false;
			exception = AssertExceptionThrown<CannotSaveAfterCriticalErrorException>(() => Factory.Save());
			AssertEquals("A critical saving error has occurred. Please wait for the next cycle of service task.", exception.Message);
		}

		public void TestDoFinalCheckBeforeCommit()
		{
			var bizoWithoutCriticalError = Factory.New<DummyBusinessObject>();
			var deletedBizoWithoutCriticalError = Factory.New<DummyBusinessObject>();
			deletedBizoWithoutCriticalError.Delete();
			var bizoWithCriticalError = Factory.New<DummyCriticalValidationParent>();
			var deletedbizoWithCriticalError = Factory.New<DummyCriticalValidationParent>();
			deletedbizoWithCriticalError.Delete();

			var objects = new BusinessObject[] { bizoWithoutCriticalError, deletedBizoWithoutCriticalError, bizoWithCriticalError, deletedbizoWithCriticalError };

			var service = new CriticalValidationService();
			service.DoFinalCheckBeforeCommit(objects);

			AssertEquals("After Saving Critical Validation should be called", true, bizoWithCriticalError.IsAfterSavingValidationCalled);
			AssertEquals("After Saving Critical Validation should be called", true, deletedbizoWithCriticalError.IsAfterSavingValidationCalled);
		}

		public void TestDoFinalCheckBeforeCommitObjectsIsNullOrEmpty()
		{
			var service = new CriticalValidationService();
			AssertNoExceptionThrown(() => service.DoFinalCheckBeforeCommit(null));
			AssertNoExceptionThrown(() => service.DoFinalCheckBeforeCommit(Enumerable.Empty<BusinessObject>()));
		}

		public void TestProcessBusinessObjects()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			var bizoDeleted = Factory.New<DummyBusinessObject>();
			bizoDeleted.Delete();
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			var bizoWithCriticalValidationDeleted = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidationDeleted.Delete();
			var bizoWithCriticalValidation2 = Factory.New<DummyCriticalValidationParent>();

			var objects = new BusinessObject[] { bizo, bizoDeleted, bizoWithCriticalValidation, bizoWithCriticalValidationDeleted, bizoWithCriticalValidation2 };

			var service = new CriticalValidationService();

			service.ProcessBusinessObjects(objects);

			AssertEquals("Critical Validation should be called", true, bizoWithCriticalValidation.IsOnSavingValidationCalled);
			AssertEquals("Critical Validation should be called", true, bizoWithCriticalValidation2.IsOnSavingValidationCalled);
			AssertEquals("Critical Validation should not be called for deleted objects.", false, bizoWithCriticalValidationDeleted.IsOnSavingValidationCalled);

			AssertEquals("Deleted Object Critical Validation should not be called for not deleted object", false, bizoWithCriticalValidation.IsDeletedObjectValidationCalled);
			AssertEquals("Deleted Object Critical Validation should not be called for not deleted object", false, bizoWithCriticalValidation2.IsDeletedObjectValidationCalled);
			AssertEquals("Deleted Object Critical Validation should be called for deleted objects.", true, bizoWithCriticalValidationDeleted.IsDeletedObjectValidationCalled);
		}

		public void TestFactory_SavedWithJobInvoiceNumberExcceedTheMaximumNumber()
		{
			var service = new CriticalValidationService();
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.Factory.SetContext(BusinessContext.MaximumJobInvoiceNumberError);
			AssertNoExceptionThrown(() => { Factory.Save(); });
		}

		public void TestProcessBusinessObjectsIsNullOrEmpty()
		{
			var service = new CriticalValidationService();
			AssertNoExceptionThrown(() => service.ProcessBusinessObjects(null));
			AssertNoExceptionThrown(() => service.ProcessBusinessObjects(Enumerable.Empty<BusinessObject>()));
		}

		public void TestDeveloperExceptionReportedMoreThanOnce_ThrowErrorOnSaving()
		{
			AssertDeveloperExceptionReportedMoreThanOnce(true);
		}

		public void TestDeveloperExceptionReportedMoreThanOnce_ThrowErrorAfterSaving()
		{
			AssertDeveloperExceptionReportedMoreThanOnce(false);
		}

		void AssertDeveloperExceptionReportedMoreThanOnce(bool shouldThrowErrorOnSaving)
		{
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			var errorMessage = "After Saving Critical Validation should" + (shouldThrowErrorOnSaving ? " not" : "") + " be called";
			bizoWithCriticalValidation.ThrowErrorOnSaving = shouldThrowErrorOnSaving;
			bizoWithCriticalValidation.ThrowErrorAfterSaving = !shouldThrowErrorOnSaving;
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);

			try
			{
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException e)
			{
				AssertEquals("On Saving Critical Validation should be called", true, bizoWithCriticalValidation.IsOnSavingValidationCalled);
				AssertEquals(errorMessage, !shouldThrowErrorOnSaving, bizoWithCriticalValidation.IsAfterSavingValidationCalled);

				AssertContains($"Error Has Occurred {(shouldThrowErrorOnSaving ? "On Saving" : "After Saving")}", e.Message);
				AssertContains("E=MC2", e.DeveloperErrorMessage);
				AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.DummyErrorKeyForTest), e.ErrorType);
				AssertEquals("Exception count", 1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Issue Key", "AccountingCriticalVallidation_" + nameof(CriticalValidationErrorType.DummyErrorKeyForTest), service.LastReportedIssueKeyForTestOnly);
			}

			var newFactory = new BusinessObjectFactory();
			bizoWithCriticalValidation = newFactory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation.ThrowErrorOnSaving = shouldThrowErrorOnSaving;
			bizoWithCriticalValidation.ThrowErrorAfterSaving = !shouldThrowErrorOnSaving;
			service = new CriticalValidationService();
			newFactory.ServiceContainer.AddCriticalValidationService(service);
			newFactory.ServiceContainer.AddAfterSaveInTransactionService(service);
			try
			{
				newFactory.Save();
			}
			catch (OnSavingCriticalCheckException e)
			{
				AssertEquals("On Saving Critical Validation should be called in NewFactory", true, bizoWithCriticalValidation.IsOnSavingValidationCalled);
				AssertEquals(errorMessage, !shouldThrowErrorOnSaving, bizoWithCriticalValidation.IsAfterSavingValidationCalled);

				AssertContains($"Error Has Occurred {(shouldThrowErrorOnSaving ? "On Saving" : "After Saving")}", e.Message);
				AssertContains("E=MC2", e.DeveloperErrorMessage);
				AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.DummyErrorKeyForTest), e.ErrorType);
				AssertEquals("Exception count", 2, ExceptionReporterTestListener.Instance.Count);
				var exception = ExceptionReporterTestListener.Instance[1].InnerException;
				AssertEquals("Two identical Exceptions", ExceptionReporterTestListener.Instance[0].InnerException.Message, exception.Message);
				AssertEquals("Issue Key", "AccountingCriticalVallidation_" + nameof(CriticalValidationErrorType.DummyErrorKeyForTest), service.LastReportedIssueKeyForTestOnly);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestDoNotValidateAfterErrorFoundOnPreviousRun_ThrowErrorOnSaving()
		{
			AssertDoNotValidateAfterErrorFoundOnPreviousRun(true);
		}

		public void TestDoNotValidateAfterErrorFoundOnPreviousRun_ThrowErrorAfterSaving()
		{
			AssertDoNotValidateAfterErrorFoundOnPreviousRun(false);
		}

		void AssertDoNotValidateAfterErrorFoundOnPreviousRun(bool shouldThrowErrorOnSaving)
		{
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation.ThrowErrorOnSaving = shouldThrowErrorOnSaving;
			bizoWithCriticalValidation.ThrowErrorAfterSaving = !shouldThrowErrorOnSaving;
			var errorMessage = "After Saving Critical Validation should" + (shouldThrowErrorOnSaving ? " not" : "") + " be called";
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);

			try
			{
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException e)
			{
				AssertEquals("On Save Critical Validation should be called", true, bizoWithCriticalValidation.IsOnSavingValidationCalled);
				AssertEquals(errorMessage, !shouldThrowErrorOnSaving, bizoWithCriticalValidation.IsAfterSavingValidationCalled);

				AssertContains($"Error Has Occurred {(shouldThrowErrorOnSaving ? "On Saving" : "After Saving")}", e.Message);
				AssertContains("E=MC2", e.DeveloperErrorMessage);
				AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.DummyErrorKeyForTest), e.ErrorType);
				AssertEquals("Exception count", 1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Issue Key", "AccountingCriticalVallidation_" + nameof(CriticalValidationErrorType.DummyErrorKeyForTest), service.LastReportedIssueKeyForTestOnly);
			}

			bizoWithCriticalValidation.IsOnSavingValidationCalled = false;
			bizoWithCriticalValidation.IsAfterSavingValidationCalled = false;
			service.LastReportedIssueKeyForTestOnly = string.Empty;
			ErrorReporter.Clear();

			try
			{
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException e)
			{
				AssertEquals("On Save Critical Validation should not be called as previous run found an error", false, bizoWithCriticalValidation.IsOnSavingValidationCalled);
				AssertEquals("After Save Critical Validation should not be called as previous run found an error", false, bizoWithCriticalValidation.IsAfterSavingValidationCalled);

				AssertContains("A critical validation error has occurred. Please close the form, then try again.", e.Message);
				AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.CannotSaveAfterError), e.ErrorType);
				AssertEquals("Exception count", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("No issue was reported by CriticalValidationService", string.Empty, service.LastReportedIssueKeyForTestOnly);
			}

			ErrorReporter.Clear();
		}

		public void TestDoNotRunOnSavingValidationIfDataRefreshWasSkiped()
		{
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange);
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);

			try
			{
				bizoWithCriticalValidation.ThrowErrorOnSaving = true; //this error should not be validated and so exception should not be thrown
				bizoWithCriticalValidation.ThrowErrorAfterSaving = true; //this error should be validated and exception should be thrown 
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException e)
			{
				AssertEquals("On Save Critical Validation should not be called as it may cause unrelated errors", false, bizoWithCriticalValidation.IsOnSavingValidationCalled);
				AssertEquals("After Save Critical Validation should still be called as it protects us from case when expected concurrency error was not happened.", true, bizoWithCriticalValidation.IsAfterSavingValidationCalled);

				AssertContains("Error Has Occurred After Saving", e.Message);
				AssertEquals("Exception count", 1, ExceptionReporterTestListener.Instance.Count);
			}

			ErrorReporter.Clear();
		}

		public void TestIssueReportedForWIPMustHaveDebtorErrorTypeAndNoContext_ThrowErrorOnSaving()
		{
			AssertIssueReportedForErrorTypeAndContext(CriticalValidationErrorType.WIPMustHaveDebtor_1, true);
		}

		public void TestIssueReportedForWIPMustHaveDebtorErrorTypeAndNoContext_ThrowErrorAfterSaving()
		{
			AssertIssueReportedForErrorTypeAndContext(CriticalValidationErrorType.WIPMustHaveDebtor_1, false);
		}

		public void TestIssueReportedForDefaultErrorTypeAndNoContext_ThrowErrorOnSaving()
		{
			AssertIssueReportedForErrorTypeAndContext(CriticalValidationErrorType.DummyErrorKeyForTest, true);
		}

		public void TestIssueReportedForDefaultErrorTypeAndNoContext_ThrowErrorAfterSaving()
		{
			AssertIssueReportedForErrorTypeAndContext(CriticalValidationErrorType.DummyErrorKeyForTest, false);
		}

		public void TestIssueNotReportedForErrorTypeTransactionLineWithoutGLAccount_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.TransactionLineWithoutGLAccountAndChargeCode_2, true);
		}

		public void TestIssueNotReportedForErrorTypeTransactionLineWithoutGLAccount_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.TransactionLineWithoutGLAccountAndChargeCode_2, false);
		}

		public void TestIssueReportedForErrorTypeOverseasCostAmountNotEqualWithNewKey_ThrowErrorOnSaving()
		{
			AssertIssueReportedForErrorTypeAndContext(CriticalValidationErrorType.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmount_9, true);
		}

		public void TestIssueReportedForErrorTypeOverseasCostAmountNotEqualWithNewKey_ThrowErrorAfterSaving()
		{
			AssertIssueReportedForErrorTypeAndContext(CriticalValidationErrorType.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmount_9, false);
		}

		public void TestShouldThrowCriticalValidationExceptionForErrorTypeBranchOfJobShouldNotBeNullWithRegisterSetToFalse()
		{
			AssertShouldThrowCriticalValidationException(CriticalValidationErrorType.BranchOfJobShouldNotBeNull, false);
		}

		public void TestShouldThrowCriticalValidationExceptionForErrorTypeBranchOfJobShouldNotBeNullWithRegisterSetToTrue()
		{
			AssertShouldThrowCriticalValidationException(CriticalValidationErrorType.BranchOfJobShouldNotBeNull, true);
		}

		public void TestShouldThrowCriticalValidationExceptionForErrorTypeDepartmentOfJobShouldNotBeNullWithRegisterSetToFalse()
		{
			AssertShouldThrowCriticalValidationException(CriticalValidationErrorType.DepartmentOfJobShouldNotBeNull, false);
		}

		public void TestShouldThrowCriticalValidationExceptionForErrorTypeDepartmentOfJobShouldNotBeNullWithRegisterSetToTrue()
		{
			AssertShouldThrowCriticalValidationException(CriticalValidationErrorType.DepartmentOfJobShouldNotBeNull, true);
		}

		void AssertShouldThrowCriticalValidationException(CriticalValidationErrorType criticalValidationErrorType, object enableReportCriticalValidationErrorsAfterDBSavingValue)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableReportCriticalValidationErrorsAfterDBSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableReportCriticalValidationErrorsAfterDBSavingValue))
			{
				var bizo = SetupDummyCriticalValidationObject(ConcurrencyPolicy.Strict);
				bizo.ErrorType = criticalValidationErrorType;

				try
				{
					Factory.Save();
					Fail("Should not be here");
				}
				catch (OnSavingCriticalCheckException e)
				{
					AssertContains("Error Has Occurred", e.Message);
					AssertEquals("Critical Validation Type", criticalValidationErrorType.ToString(), e.ErrorType);
					Assert("Critical Validation Context Should be set", Factory.HasContext(BusinessContext.CriticalValidation));
					AssertEquals("Exception count", 1, ExceptionReporterTestListener.Instance.Count);
					ErrorReporter.Clear();
				}
				catch
				{
					Fail("Should not be here");
				}
			}
		}

		void AssertIssueReportedForErrorTypeAndContext(CriticalValidationErrorType errorType, bool shouldThrowErrorOnSaving)
		{
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation.ErrorType = errorType;
			bizoWithCriticalValidation.ThrowErrorOnSaving = shouldThrowErrorOnSaving;
			bizoWithCriticalValidation.ThrowErrorAfterSaving = !shouldThrowErrorOnSaving;
			var errorMessage = "After Saving Critical Validation should" + (shouldThrowErrorOnSaving ? " not" : "") + " be called";
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);

			try
			{
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException e)
			{
				AssertEquals("On Saving Critical Validation should be called", true, bizoWithCriticalValidation.IsOnSavingValidationCalled);
				AssertEquals(errorMessage, !shouldThrowErrorOnSaving, bizoWithCriticalValidation.IsAfterSavingValidationCalled);
				AssertContains($"Error Has Occurred {(shouldThrowErrorOnSaving ? "On Saving" : "After Saving")}", e.Message);
				AssertContains("E=MC2", e.DeveloperErrorMessage);
				AssertEquals("Critical Validation Type", errorType.ToString(), e.ErrorType);

				AssertEquals("Exception count", 1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Issue Key", "AccountingCriticalVallidation_" + errorType.ToString(), service.LastReportedIssueKeyForTestOnly);
			}
			ErrorReporter.Clear();
		}

		public void TestDoNotValidateAfterObjectHasBusinessContextConflictWithCriticalFields_ThrowErrorOnSaving()
		{
			AssertDoNotValidateAfterObjectHasBusinessContextConflictWithCriticalFields(true);
		}

		public void TestDoNotValidateAfterObjectHasBusinessContextConflictWithCriticalFields_ThrowErrorAfterSaving()
		{
			AssertDoNotValidateAfterObjectHasBusinessContextConflictWithCriticalFields(false);
		}

		void AssertDoNotValidateAfterObjectHasBusinessContextConflictWithCriticalFields(bool shouldThrowErrorOnSaving)
		{
			var header = Factory.New<AccTransactionHeader>();
			header.SetContext(BusinessContext.ConflictWithCriticalFields);
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation.ErrorType = CriticalValidationErrorType.CannotSaveAfterError;
			bizoWithCriticalValidation.ThrowErrorOnSaving = shouldThrowErrorOnSaving;
			bizoWithCriticalValidation.ThrowErrorAfterSaving = !shouldThrowErrorOnSaving;
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);

			Assert("Testing for interactive user", Globals.IsUserInteractive);
			try
			{
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException e)
			{
				AssertEquals("On Saving Critical Validation should not be called as it has found a strict concurrency error before", false, bizoWithCriticalValidation.IsOnSavingValidationCalled);
				AssertEquals("After Saving Critical Validation should not be called as OnSaving validation found a strict concurrency error before", false, bizoWithCriticalValidation.IsAfterSavingValidationCalled);

				AssertContains("Critical change(s) on accounting object(s) has occurred. Please close the form, then try again.", e.Message);
				AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.CannotSaveAfterError), e.ErrorType);
				AssertEquals("Exception count", 0, ExceptionReporterTestListener.Instance.Count);
				AssertNull("No issue was reported by CriticalValidationService", service.LastReportedIssueKeyForTestOnly);
			}
			ErrorReporter.Clear();

			Globals.IsUserInteractive = false;
			Assert("Testing for non-interactive user", !Globals.IsUserInteractive);
			try
			{
				Factory.Save();
			}
			catch (CannotSaveAfterCriticalErrorException e)
			{
				AssertEquals("On Saving Critical Validation should not be called as it has found a strict concurrency error before", false, bizoWithCriticalValidation.IsOnSavingValidationCalled);
				AssertEquals("After Saving Critical Validation should not be called as OnSaving validation found a strict concurrency error before", false, bizoWithCriticalValidation.IsAfterSavingValidationCalled);

				AssertContains("Critical change(s) on accounting object(s) has occurred. Please wait for the next cycle of service task.", e.Message);
				AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.CannotSaveAfterError), e.ErrorType);
				AssertEquals("Exception count", 0, ExceptionReporterTestListener.Instance.Count);
				AssertNull("No issue was reported by CriticalValidationService", service.LastReportedIssueKeyForTestOnly);
			}
			ErrorReporter.Clear();
		}

		public void TestContextHasBeenSet_ThrowErrorOnSaving()
		{
			AssertContextHasBeenSet(true);
		}

		public void TestContextHasBeenSet_ThrowErrorAfterSaving()
		{
			AssertContextHasBeenSet(false);
		}

		void AssertContextHasBeenSet(bool shouldThrowErrorOnSaving)
		{
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation.ThrowErrorOnSaving = shouldThrowErrorOnSaving;
			bizoWithCriticalValidation.ThrowErrorAfterSaving = !shouldThrowErrorOnSaving;
			var errorMessage = "After Saving Critical Validation should" + (shouldThrowErrorOnSaving ? " not" : "") + " be called";
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);

			try
			{
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException e)
			{
				AssertEquals("On Saving Critical Validation should be called", true, bizoWithCriticalValidation.IsOnSavingValidationCalled);
				AssertEquals(errorMessage, !shouldThrowErrorOnSaving, bizoWithCriticalValidation.IsAfterSavingValidationCalled);
				AssertContains($"Error Has Occurred {(shouldThrowErrorOnSaving ? "On Saving" : "After Saving")}", e.Message);
				AssertContains("E=MC2", e.DeveloperErrorMessage);
				Assert("Critical Validation Context Should be set", bizoWithCriticalValidation.Factory.HasContext(BusinessContext.CriticalValidation));
			}
			ErrorReporter.Clear();
		}

		#region Test CriticalValidationError with EnableReportCriticalValidationErrorsAfterDBSaving Registry

		public void TestCriticalValidationError_WithSavingStrictConcurrencyError_ThrowErrorOnFirstSaving()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableReportCriticalValidationErrorsAfterDBSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bizo = SetupDummyCriticalValidationObject(ConcurrencyPolicy.Strict);
				AssertCriticalValidationError_FistSavingWithError(true);
				AssertCriticalValidationError_WithSavingStrictConcurrencyError(true);
			}
		}

		public void TestCriticalValidationError_WithSavingStrictConcurrencyError_NotThrowErrorOnFirstSaving()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableReportCriticalValidationErrorsAfterDBSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bizo = SetupDummyCriticalValidationObject(ConcurrencyPolicy.Strict);
				AssertCriticalValidationError_FistSavingWithError(false);
				AssertCriticalValidationError_WithSavingStrictConcurrencyError(false);
			}
		}

		void AssertCriticalValidationError_WithSavingStrictConcurrencyError(bool throwErrorOnFirstSaving)
		{
			try
			{
				Factory.Save();
				Fail("Should not be here");
			}
			catch (CannotSaveAfterCriticalErrorException e)
			{
				string expectedMessage;
				if (throwErrorOnFirstSaving)
				{
					expectedMessage = "A critical validation error has occurred. Please close the form, then try again.";
				}
				else
				{
					expectedMessage = "Critical change(s) on accounting object(s) has occurred. Please close the form, then try again.";
					Assert("SavingFailedDueToDBError Should be set", Factory.HasContext(BusinessContext.SavingFailedDueToDBError));
					AssertEquals("Exception count", 0, ExceptionReporterTestListener.Instance.Count);
				}
				AssertContains(expectedMessage, e.Message);
				AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.CannotSaveAfterError), e.ErrorType);
			}

			ErrorReporter.Clear();
		}

		public void TestCriticalValidationError_WithSavingNonStrictConcurrencyError_SecondSavingError_ThrowErrorOnFirstSaving()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableReportCriticalValidationErrorsAfterDBSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bizo = SetupDummyCriticalValidationObject(ConcurrencyPolicy.Observe);
				AssertCriticalValidationError_FistSavingWithError(true);
				AssertCriticalValidationError_WithSavingNonStrictConcurrencyError_SecondSavingError(true);
			}
		}

		public void TestCriticalValidationError_WithSavingNonStrictConcurrencyError_SecondSavingError_NotThrowErrorOnFirstSaving()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableReportCriticalValidationErrorsAfterDBSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bizo = SetupDummyCriticalValidationObject(ConcurrencyPolicy.Observe);
				AssertCriticalValidationError_FistSavingWithError(false);
				AssertCriticalValidationError_WithSavingNonStrictConcurrencyError_SecondSavingError(false);
			}
		}

		void AssertCriticalValidationError_WithSavingNonStrictConcurrencyError_SecondSavingError(bool throwErrorOnFirstSaving)
		{
			try
			{
				Factory.Save();
				Fail("Should not be here");
			}
			catch (CannotSaveAfterCriticalErrorException e)
			{
				if (throwErrorOnFirstSaving)
				{
					AssertContains("A critical validation error has occurred. Please close the form, then try again.", e.Message);
					AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.CannotSaveAfterError), e.ErrorType);
				}
				else
				{
					Fail("CannotSaveAfterCriticalErrorException Should not be caught here");
				}
			}
			catch (OnSavingCriticalCheckException e)
			{
				if (throwErrorOnFirstSaving)
				{
					Fail("OnSavingCriticalCheckException Should not be caught here");
				}
				else
				{
					AssertContains("Error Has Occurred", e.Message);
					AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.DummyErrorKeyForTest), e.ErrorType);
					Assert("Critical Validation Context Should be set", Factory.HasContext(BusinessContext.CriticalValidation));
					AssertEquals("Exception count", 1, ExceptionReporterTestListener.Instance.Count);
				}
			}
			ErrorReporter.Clear();
		}

		public void TestCriticalValidationError_WithSavingNonStrictConcurrencyError_SecondSavingNoError_ThrowErrorOnFirstSaving()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableReportCriticalValidationErrorsAfterDBSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bizo = SetupDummyCriticalValidationObject(ConcurrencyPolicy.Observe);
				AssertCriticalValidationError_FistSavingWithError(true);
				bizo.ThrowErrorOnSaving = false;
				AssertCriticalValidationError_WithSavingNonStrictConcurrencyError_SecondSavingNoError(true);
			}
		}

		public void TestCriticalValidationError_WithSavingNonStrictConcurrencyError_SecondSavingNoError_NotThrowErrorOnFirstSaving()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableReportCriticalValidationErrorsAfterDBSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bizo = SetupDummyCriticalValidationObject(ConcurrencyPolicy.Observe);
				AssertCriticalValidationError_FistSavingWithError(false);
				bizo.ThrowErrorOnSaving = false;
				AssertCriticalValidationError_WithSavingNonStrictConcurrencyError_SecondSavingNoError(false);
			}
		}

		void AssertCriticalValidationError_WithSavingNonStrictConcurrencyError_SecondSavingNoError(bool throwErrorOnFirstSaving)
		{
			try
			{
				Factory.Save();
			}
			catch (CannotSaveAfterCriticalErrorException e)
			{
				if (throwErrorOnFirstSaving)
				{
					AssertContains("A critical validation error has occurred. Please close the form, then try again.", e.Message);
					AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.CannotSaveAfterError), e.ErrorType);
				}
				else
				{
					Fail("CannotSaveAfterCriticalErrorException Should not be caught here");
				}
			}
			catch (Exception)
			{
				Fail("Exception Should not be caught here");
			}
			finally
			{
				Assert("SavingFailedDueToDBError Should be reset", !Factory.HasContext(BusinessContext.SavingFailedDueToDBError));
			}
		}

		void AssertCriticalValidationError_FistSavingWithError(bool throwErrorOnFirstSaving)
		{
			try
			{
				Factory.Save();
				Fail("Should not be here");
			}
			catch (OnSavingCriticalCheckException e)
			{
				if (throwErrorOnFirstSaving)
				{
					AssertContains("Error Has Occurred", e.Message);
					AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.DummyErrorKeyForTest), e.ErrorType);
					Assert("Critical Validation Context Should be set", Factory.HasContext(BusinessContext.CriticalValidation));
					AssertEquals("Exception count", 1, ExceptionReporterTestListener.Instance.Count);

					ErrorReporter.Clear();
				}
				else
				{
					Fail("OnSavingCriticalCheckException Should not be caught here");
				}
			}
			catch (ZSaveConcurrencyException e)
			{
				if (throwErrorOnFirstSaving)
				{
					Fail("ZSaveConcurrencyException Should not be caught here");
				}
				else
				{
					AssertContains("concurrency error is caught here", "~ConcurrencyError~", e.Message);
					Assert("SavingFailedDueToDBError Should be set", Factory.HasContext(BusinessContext.SavingFailedDueToDBError));
					ZExceptionReporting.HandleZSaveConcurrencyException(e, NotificationHandler.Instance, false);
				}
			}
		}

		DummyCriticalValidationParent SetupDummyCriticalValidationObject(ConcurrencyPolicy concurrencyPolicy)
		{
			var bizo = Factory.New<DummyCriticalValidationParent>();
			bizo.Z0_Code = "123";
			ConcurrencyInfo.SetConcurrencyPolicy(bizo, nameof(DummyCriticalValidationParent.Z0_Code), concurrencyPolicy);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var newBizo = newFactory.Load<DummyCriticalValidationParent>(bizo.PK);
			newBizo.Z0_Code = "456";
			newFactory.Save();

			bizo.Z0_Code = "789";
			bizo.Z0_Description = "test";
			bizo.ThrowErrorOnSaving = true;
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);

			return bizo;
		}

		#endregion

		public void TestUserFriendlyMessageForGUIUserAndBatchProcessorWhenCannotSaveAfterCriticalErrorExceptionIsThrown_ThrowErrorOnSaving()
		{
			AssertUserFriendlyMessageForGUIUserAndBatchProcessorWhenCannotSaveAfterCriticalErrorExceptionIsThrown(true);
		}

		public void TestUserFriendlyMessageForGUIUserAndBatchProcessorWhenCannotSaveAfterCriticalErrorExceptionIsThrown_ThrowErrorAfterSaving()
		{
			AssertUserFriendlyMessageForGUIUserAndBatchProcessorWhenCannotSaveAfterCriticalErrorExceptionIsThrown(false);
		}

		void AssertUserFriendlyMessageForGUIUserAndBatchProcessorWhenCannotSaveAfterCriticalErrorExceptionIsThrown(bool shouldThrowErrorOnSaving)
		{
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation.ThrowErrorOnSaving = shouldThrowErrorOnSaving;
			bizoWithCriticalValidation.ThrowErrorAfterSaving = !shouldThrowErrorOnSaving;
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);

			try
			{
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException e)
			{
				AssertEquals($"Error Has Occurred {(shouldThrowErrorOnSaving ? "On Saving" : "After Saving")}", e.Message);
			}
			ErrorReporter.Clear();

			Assert("Testing for interactive user", Globals.IsUserInteractive);
			try
			{
				Factory.Save();
			}
			catch (CannotSaveAfterCriticalErrorException e)
			{
				AssertEquals("A critical validation error has occurred. Please close the form, then try again.", e.Message);
			}
			ErrorReporter.Clear();

			Globals.IsUserInteractive = false; //batch processor
			Assert("Testing for non-interactive user", !Globals.IsUserInteractive);
			try
			{
				Factory.Save();
			}
			catch (CannotSaveAfterCriticalErrorException e)
			{
				AssertEquals("A critical validation error has occurred. Please wait for the next cycle of service task.", e.Message);
			}
			ErrorReporter.Clear();
		}

		public void TestClearCriticalValidationInfoCollectorCacheWhenValidationFinished()
		{
			var collectService = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation.ThrowErrorOnSaving = true;
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);

			var collectorKey = CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalCostAmtNotEqualRelatedLineAmount;

			collectService.AddInfoWhenAllowed(bizoWithCriticalValidation.PK, collectorKey, () => "info", CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

			AssertEquals("Should have info", "\r\n" + collectorKey + ":\r\ninfo", collectService.GetInfo(bizoWithCriticalValidation.PK, collectorKey));

			try
			{
				Factory.Save();

				Fail("Should not be here");
			}
			catch (OnSavingCriticalCheckException)
			{
				AssertContains("Cache should be cleaned", "\r\nJobChargeLocalCostAmtNotEqualRelatedLineAmount: There is no data collected for this PK.", collectService.GetInfo(bizoWithCriticalValidation.PK, collectorKey));
			}
			catch (Exception)
			{
				Fail("Should not be here");
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestIssueNotReportedForErrorTypeJobInvoiceNumberExceedTheMaximumNumber_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.JobInvoiceNumberExceedTheMaximumNumber, true);
		}

		public void TestIssueNotReportedForErrorTypeJobInvoiceNumberExceedTheMaximumNumber_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.JobInvoiceNumberExceedTheMaximumNumber, false);
		}

		public void TestIssueNotReportedForErrorTypeCreateTransactionRestriction_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.CreateTransactionRestriction, true);
		}

		public void TestIssueNotReportedForErrorTypeCreateTransactionRestriction_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.CreateTransactionRestriction, false);
		}

		public void TestIssueNotReportedForErrorTypeJobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_DeletedOrUnlinkedFromConsolCost_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_DeletedOrUnlinkedFromConsolCost, true);
		}

		public void TestIssueNotReportedForErrorTypeJobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_DeletedOrUnlinkedFromConsolCost_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_DeletedOrUnlinkedFromConsolCost, false);
		}

		public void TestIssueNotReportedForErrorTypeTransactionHeaderAmountExceedMaximumAllowedAmount_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.TransactionHeaderAmountExceedMaximumAllowedAmount, true);
		}

		public void TestIssueNotReportedForErrorTypeTransactionHeaderAmountExceedMaximumAllowedAmount_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.TransactionHeaderAmountExceedMaximumAllowedAmount, false);
		}

		public void TestIssueNotReportedForErrorTypeTransactionLineAmountExceedMaximumAllowedAmount_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.TransactionLineAmountExceedMaximumAllowedAmount, true);
		}

		public void TestIssueNotReportedForErrorTypeTransactionLineAmountExceedMaximumAllowedAmount_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.TransactionLineAmountExceedMaximumAllowedAmount, false);
		}

		public void TestIssueNotReportedForErrorTypeJobChargeAmountExceedMaximumAllowedAmount_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.JobChargeAmountExceedMaximumAllowedAmount, true);
		}

		public void TestIssueNotReportedForErrorTypeJobChargeAmountExceedMaximumAllowedAmount_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.JobChargeAmountExceedMaximumAllowedAmount, false);
		}

		public void TestIssueNotReportedForErrorTypeRemittanceReferenceNumberExceedMaxLengtht_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.RemittanceReferenceNumberExceedMaxLength, true);
		}

		public void TestIssueNotReportedForErrorTypeRemittanceReferenceNumberExceedMaxLength_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.RemittanceReferenceNumberExceedMaxLength, false);
		}

		public void TestIssueNotReportedForErrorTypeJobChargeInvoiceDetailsNotEqualConsolCostOnesWithoutChanges_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnesWithoutChanges, true);
		}

		public void TestIssueNotReportedForErrorTypeJobChargeInvoiceDetailsNotEqualConsolCostOnesWithoutChanges_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnesWithoutChanges, false);
		}

		public void TestIssueNotReportedForErrorTypeApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsWithoutChanges_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsWithoutChanges, true);
		}

		public void TestIssueNotReportedForErrorTypeApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsWithoutChanges_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsWithoutChanges, false);
		}

		public void TestInvalidReverseDate_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.InvalidReverseDate, true);
		}

		public void TestInvalidReverseDate_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.InvalidReverseDate, false);
		}

		public void TestInvalidInvoiceDate_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.InvalidInvoiceDate, true);
		}

		public void TestInvalidInvoiceDate_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.InvalidInvoiceDate, false);
		}

		public void TestJobChargeNegativeRevenueIsNotPermitted_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.JobChargeNegativeRevenueIsNotPermitted, true);
		}

		public void TestJobChargeNegativeRevenueIsNotPermitted_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.JobChargeNegativeRevenueIsNotPermitted, false);
		}

		public void TestTransactionLineNegativeAmountOnAccountReceivableTransactionsIsNotAllowed_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.TransactionLineNegativeAmountOnAccountReceivableTransactionsIsNotAllowed, true);
		}

		public void TestTransactionLineNegativeAmountOnAccountReceivableTransactionsIsNotAllowed_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.TransactionLineNegativeAmountOnAccountReceivableTransactionsIsNotAllowed, false);
		}

		public void TestIssueNotReportedForErrorTypeGLJournalEntriesNumberHasBeenAssigned_ThrowErrorOnSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.GLJournalEntriesNumberHasBeenAssigned, true);
		}

		public void TestIssueNotReportedForErrorTypeGLJournalEntriesNumberHasBeenAssigned_ThrowErrorAfterSaving()
		{
			AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType.GLJournalEntriesNumberHasBeenAssigned, false);
		}

		void AssertIssueNotReportedForErrorTypeAndContext(CriticalValidationErrorType errorType, bool shouldThrowErrorOnSaving)
		{
			var bizoWithCriticalValidation = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation.ErrorType = errorType;
			bizoWithCriticalValidation.ThrowErrorOnSaving = shouldThrowErrorOnSaving;
			bizoWithCriticalValidation.ThrowErrorAfterSaving = !shouldThrowErrorOnSaving;
			var errorMessage = "After Saving Critical Validation should" + (shouldThrowErrorOnSaving ? " not" : "") + " be called";
			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);
			try
			{
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException e)
			{
				AssertEquals("On Saving Critical Validation should be called", true, bizoWithCriticalValidation.IsOnSavingValidationCalled);
				AssertEquals(errorMessage, !shouldThrowErrorOnSaving, bizoWithCriticalValidation.IsAfterSavingValidationCalled);
				AssertContains($"Error Has Occurred {(shouldThrowErrorOnSaving ? "On Saving" : "After Saving")}", e.Message);
				AssertContains("E=MC2", e.DeveloperErrorMessage);
				AssertEquals("Critical Validation Type", errorType.ToString(), e.ErrorType);
				AssertEquals("Exception count", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Issue Key", "AccountingCriticalVallidation_" + errorType.ToString(), service.LastReportedIssueKeyForTestOnly);
			}
			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		sealed class CriticalValidationServiceNonTransactionalTest : TestCase
		{
			public void TestDeveloperExceptionReportedViaEmail()
			{
				using (var command = Db.Connection.Command(string.Format("DELETE [{0}]", StmErrorReportSchema.Constants.TableName))) // Clear any user data when running test against test data
				{
					command.ExecuteNonQuery();
				}

				var participant = (ITransactionParticipant)factory;
				using (participant.BeginTransactionWithManager())
				{
					var staff = factory.NewWithValidTestData<GlbStaff>();
					factory.Save();

					using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
					using (var command = Db.Connection.Command(string.Format("SELECT COUNT(*) FROM [{0}] WHERE [{1}] = @code", StmErrorReportSchema.Constants.TableName, StmErrorReportSchema.Constants.QER_TransmitStatus)))
					{
						command.AddParameter("@code", SqlDbType.VarChar, StmErrorReportTransmitStatus.Codes.Queued);
						var service = new CriticalValidationService();

						try
						{
							var bizoWithCriticalValidation = factory.New<DummyCriticalValidationParent>();
							bizoWithCriticalValidation.ThrowErrorOnSaving = true;

							factory.ServiceContainer.AddCriticalValidationService(service);

							AssertEquals("Pre-condition: Should be no error report in the StmErrorReport table", 0, (int)command.ExecuteScalar());

							using (NUnit.Framework.TestingState.SuspendIsRunningTests()) // To avoid using UnitTestNotification which does not create error report
							{
								try
								{
									Globals.IsUserInteractive = false; // Do not show Exception form
									factory.Save();
								}
								finally
								{
									Globals.IsUserInteractive = true;
									AssertEquals("Critical Validation should be called", true, bizoWithCriticalValidation.IsOnSavingValidationCalled);
								}
							}
						}
						catch (OnSavingCriticalCheckException e)
						{
							AssertContains("Error Has Occurred On Saving", e.Message);
							AssertEquals("Critical Validation Type", nameof(CriticalValidationErrorType.DummyErrorKeyForTest), e.ErrorType);
							AssertEquals("Should be one error report in the StmErrorReport table", 1, (int)command.ExecuteScalar());
							AssertEquals("Issue Key", "AccountingCriticalVallidation_" + nameof(CriticalValidationErrorType.DummyErrorKeyForTest), service.LastReportedIssueKeyForTestOnly);
						}
						finally
						{
							ExceptionReporter.Instance.TotalReportCount = 0;
						}
					}
				}
			}

			BusinessObjectFactory factory;

			protected override void SetUp()
			{
				base.SetUp();
				factory = new BusinessObjectFactory();
			}
		}

		class DummyCriticalValidationParent : DummyBusinessObject, ISupportCriticalValidation
		{
			public DummyCriticalValidationParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool IsOnSavingValidationCalled;
			public bool IsDeletedObjectValidationCalled;
			public bool IsAfterSavingValidationCalled;
			public bool ThrowErrorOnSaving;
			public bool ThrowErrorAfterSaving;
			public CriticalValidationErrorType ErrorType = CriticalValidationErrorType.DummyErrorKeyForTest;

			#region ISupportCriticalValidation Members

			public ICriticalValidation CriticalValidation
			{
				get { return new DummyCriticalValidation(this); }
			}

			public void SetConflictWithCriticalFieldsBusinessContext()
			{
				CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(this);
			}

			#endregion
		}

		class DummyCriticalValidation : ICriticalValidation
		{
			public DummyCriticalValidation(DummyCriticalValidationParent parent)
			{
				this.parent = parent;
			}

			readonly DummyCriticalValidationParent parent;

			#region ICriticalValidation Members

			public void RegisterOnSavingCheck()
			{
				throw new NotImplementedException();
			}

			public void RunOnSavingCheck()
			{
				parent.IsOnSavingValidationCalled = true;

				if (parent.ThrowErrorOnSaving)
				{
					throw new OnSavingCriticalCheckException<DummyCriticalValidationParent>(parent, parent.ErrorType, "Error Has Occurred On Saving", "E=MC2");
				}
			}

			public void RunDeletedObjectOnSavingCheck()
			{
				parent.IsDeletedObjectValidationCalled = true;
			}

			public void RunAfterSavingCheck()
			{
				parent.IsAfterSavingValidationCalled = true;

				if (parent.ThrowErrorAfterSaving)
				{
					throw new OnSavingCriticalCheckException<DummyCriticalValidationParent>(parent, parent.ErrorType, "Error Has Occurred After Saving", "E=MC2");
				}
			}

			#endregion
		}
	}
}
