#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using CargoWiseDefinedBusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.MasterFiles.Business
{
	class CriticalValidationService : ICriticalValidationService, IAfterSaveInTransactionService
	{
		public CriticalValidationService()
		{
#if DEBUG
			if (ForcedErrorType_Static_ForTestOnly.HasValue)
			{
				ForcedErrorTypeForTestOnly = ForcedErrorType_Static_ForTestOnly.Value;
			}
			if (ForcedExceptionType_Static_ForTestOnly != null)
			{
				ForcedExceptionTypeForTestOnly = ForcedExceptionType_Static_ForTestOnly;
			}
#endif
		}

		#region IAfterSaveInTransactionService Members

		public void DoFinalCheckBeforeCommit(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			if (businessObjectsInOnSavingOrder == null || !businessObjectsInOnSavingOrder.Any())
			{
				return;
			}

			try
			{
				if (exceptionToReportLater != null)
				{
					throw exceptionToReportLater;
				}

				foreach (var bizo in businessObjectsInOnSavingOrder)
				{
					if (bizo is ISupportCriticalValidation criticalValidationSupporter)
					{
						criticalValidationSupporter.CriticalValidation.RunAfterSavingCheck();
					}
				}
			}
			catch (OnSavingCriticalCheckException e)
			{
				lastException = e;
				throw;
			}
		}

		#endregion

		#region IAfterOnSavingBOProcessingService Members

		public void ProcessBusinessObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			if (businessObjectsInOnSavingOrder == null || !businessObjectsInOnSavingOrder.Any())
			{
				return;
			}

			if (((IBusinessObjectFactoryInternals)businessObjectsInOnSavingOrder.First().Factory).LastSavingRollbackHadException)
			{
				var userMessage = Globals.IsUserInteractive ?
						Res.GetString("3514D9BE-D130-4840-A8FE-F45DE9EB3277", "A critical saving error has occurred. Please close the form, then try again.") :
						Res.GetString("3EEB0C0A-D287-407B-B85C-E088C3B6769A", "A critical saving error has occurred. Please wait for the next cycle of service task.");
				throw new CannotSaveAfterCriticalErrorException(userMessage);
			}

			if (lastException != null && !businessObjectsInOnSavingOrder.First().Factory.HasContext(BusinessContext.SavingAsIncomplete))
			{
				var userMessage = Globals.IsUserInteractive ?
					Res.GetString("CriticalValidation|CannotSaveAfterCriticalValidationError", "A critical validation error has occurred. Please close the form, then try again.") :
					Res.GetString("ae21c8ba-759b-4559-a697-1222cfdf506e", "A critical validation error has occurred. Please wait for the next cycle of service task.");
				throw new CannotSaveAfterCriticalErrorException(userMessage);
			}

			if (businessObjectsInOnSavingOrder.Any(x => x is ISupportCriticalValidation && x.HasContext(BusinessContext.ConflictWithCriticalFields)))
			{
				var userMessage = Globals.IsUserInteractive ?
									Res.GetString("CriticalValidation|CannotSaveAfterStrictConcurrencyError", "Critical change(s) on accounting object(s) has occurred. Please close the form, then try again.") :
									Res.GetString("80acc64d-dbc0-4da8-af4e-8b87018502ec", "Critical change(s) on accounting object(s) has occurred. Please wait for the next cycle of service task.");
				throw new CannotSaveAfterCriticalErrorException(userMessage);
			}

			try
			{
				var decider = ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>();
				if (!businessObjectsInOnSavingOrder.Any(x => decider.HasSkippedDataRefreshBusUpdate(x)))
				{
					RunCriticalValidation(businessObjectsInOnSavingOrder);
				}
			}
			finally
			{
				businessObjectsInOnSavingOrder.First().Factory.Saved += Factory_Saved;
			}
		}

		#endregion

		void RunCriticalValidation(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			var clearCacheDelegates = new Dictionary<Type, ClearCacheDelegate>();
			try
			{
				foreach (var bizo in businessObjectsInOnSavingOrder)
				{
					if (bizo is ISupportCriticalValidation criticalValidationSupporter)
					{
						var criticalValidation = criticalValidationSupporter.CriticalValidation;
						if (bizo.IsDeleted)
						{
							criticalValidation.RunDeletedObjectOnSavingCheck();
						}
						else
						{
							if (criticalValidation is IClearCacheProvider clearCacheProvider)
							{
								var criticalValidationType = criticalValidation.GetType();
								if (!clearCacheDelegates.TryGetValue(criticalValidationType, out var _))
								{
									clearCacheDelegates[criticalValidationType] = clearCacheProvider.GetClearCacheDelegate(); //We do not want to keep all instances of criticalValidation in memory, thus GetClearCacheDelegate must return reference to static clear cache method
								}
							}
							criticalValidation.RunOnSavingCheck();
						}
					}
				}

#if DEBUG
				Testing.CriticalValidationServiceTestOnlyExtensions.ThrowCriticalValidationErrorIfRequiredForTestOnly(this);
				Testing.CriticalValidationServiceTestOnlyExtensions.ThrowExceptionIfRequiredForTestOnly(this);
#endif
			}
			catch (OnSavingCriticalCheckException e)
			{
				var errorTypesToThrowDirectly = new[] {
				nameof(CriticalValidationErrorType.BranchOfJobShouldNotBeNull),
				nameof(CriticalValidationErrorType.DepartmentOfJobShouldNotBeNull)
				};

				if (AccountingMasterFilesRegistry.Instance.EnableReportCriticalValidationErrorsAfterDBSaving.Value && !errorTypesToThrowDirectly.Contains(e.ErrorType))
				{
					exceptionToReportLater = e;
				}
				else
				{
					lastException = e;
					throw;
				}
			}
			finally
			{
				var factory = businessObjectsInOnSavingOrder.First().Factory;
				clearCacheDelegates.Values.ForEach(clearCacheDelegate => clearCacheDelegate(factory));
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= Factory_Saved;
			exceptionToReportLater = null;

			if (!savedSuccessfully && lastException == null)
			{
				factory.SetContext(BusinessContext.SavingFailedDueToDBError);
			}
			else
			{
				CriticalValidationInfoCollectorService.GetService(factory)?.ClearServiceCache();
			}

			if (savedSuccessfully)
			{
				factory.RemoveContext(BusinessContext.SavingFailedDueToDBError);
			}

			if (lastException == null)
			{
				return;
			}

			var issueKey = "AccountingCriticalVallidation_" + lastException.ErrorType;

#if DEBUG
			LastReportedIssueKeyForTestOnly = issueKey;
#endif
			var errorTypesNotReport = new[] {
				nameof(CriticalValidationErrorType.JobInvoiceNumberExceedTheMaximumNumber),
				nameof(CriticalValidationErrorType.CreateTransactionRestriction),
				nameof(CriticalValidationErrorType.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_DeletedOrUnlinkedFromConsolCost),
				nameof(CriticalValidationErrorType.TransactionLineWithoutGLAccountAndChargeCode_2),
				nameof(CriticalValidationErrorType.AbortOnSavingProcess),
				nameof(CriticalValidationErrorType.RemittanceReferenceNumberExceedMaxLength),
				nameof(CriticalValidationErrorType.TransactionHeaderAmountExceedMaximumAllowedAmount),
				nameof(CriticalValidationErrorType.TransactionLineAmountExceedMaximumAllowedAmount),
				nameof(CriticalValidationErrorType.JobChargeAmountExceedMaximumAllowedAmount),
				nameof(CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnesWithoutChanges),
				nameof(CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsWithoutChanges),
				nameof(CriticalValidationErrorType.InvalidReverseDate),
				nameof(CriticalValidationErrorType.InvalidInvoiceDate),
				nameof(CriticalValidationErrorType.JobChargeLinkedToClosedJobWhenCreatingProfitShareCharges),
				nameof(CriticalValidationErrorType.JobChargeOSSellExRateIsNegative),
				nameof(CriticalValidationErrorType.JobChargeNegativeRevenueIsNotPermitted),
				nameof(CriticalValidationErrorType.TransactionLineNegativeAmountOnAccountReceivableTransactionsIsNotAllowed),
				nameof(CriticalValidationErrorType.GLJournalEntriesNumberHasBeenAssigned)
			};

			if (errorTypesNotReport.Contains(lastException.ErrorType))
			{
				// do not report error
			}
			else
			{
				var reportErrorSilently = factory.HasContext(CargoWiseDefinedBusinessContext.JCLServiceTask); /* We are reporting it silently so that JCS service task does not get kicked out by the process controller after several occurances of critical validation exception.
																											   We are considering a critical validation exception as something that is specific to a job, nothing to do with the service task itself.
																											   Therefore, process controller should not stop JCS service task because of a critical validation exception. It will allow JCS service task to move on to the next job for processing. */

				if (reportErrorSilently)
				{
					ErrorReporter.Instance.ReportDeveloperExceptionOrHandleSilently(issueKey, lastException.DeveloperErrorMessage, null);
				}
				else
				{
					ErrorReporter.Instance.Report(issueKey, lastException.DeveloperErrorMessage, null);
				}

				/* I will implement this ina future workitem */
				//Logging critical validation 
				//if (lastException.BusinessEntity is ISupportAccProcessLogging parent)
				//{
				//	parent.LogException(lastException);
				//}
			}

			factory.SetContext(BusinessContext.CriticalValidation);
		}

		OnSavingCriticalCheckException lastException;
		OnSavingCriticalCheckException exceptionToReportLater;
#if DEBUG
		internal string LastReportedIssueKeyForTestOnly
		{
			get;
			set;
		}

		internal CriticalValidationErrorType ForcedErrorTypeForTestOnly = CriticalValidationErrorType.NoError;
		internal Exception ForcedExceptionTypeForTestOnly;

		internal void ResetLastExceptionForTestOnly()
		{
			lastException = null;
		}

		[ThreadStatic]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		static CriticalValidationErrorType? ForcedErrorType_Static_ForTestOnly;

		internal static void SetForcedErrorType_Static_ForTestOnly(CriticalValidationErrorType forcedErrorType_Static_ForTestOnly)
		{
			ForcedErrorType_Static_ForTestOnly = forcedErrorType_Static_ForTestOnly;
		}
		internal static void ResetForcedErrorType_Static_ForTestOnly()
		{
			ForcedErrorType_Static_ForTestOnly = null;
		}

		[ThreadStatic]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		static Exception ForcedExceptionType_Static_ForTestOnly;

		internal static void SetForcedExceptionType_Static_ForTestOnly(Exception forcedeExceptionType_Static_ForTestOnly)
		{
			ForcedExceptionType_Static_ForTestOnly = forcedeExceptionType_Static_ForTestOnly;
		}
		internal static void ResetForcedExceptionType_Static_ForTestOnly()
		{
			ForcedExceptionType_Static_ForTestOnly = null;
		}

#endif
	}
}
