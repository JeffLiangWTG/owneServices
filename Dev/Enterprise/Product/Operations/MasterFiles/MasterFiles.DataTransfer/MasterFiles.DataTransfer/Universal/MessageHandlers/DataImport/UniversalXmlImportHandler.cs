using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public abstract class UniversalXmlImportHandler<T> : UniversalXmlMessageHandler<T> where T : TopLevelDataObject, new()
	{
		protected UniversalXmlImportHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig) : base(xmlSessionTracker, processingConfig)
		{
		}

		protected sealed override IHttpXmlProcessingResult ProcessDataObject(BusinessObjectFactory factory,
			T topLevelDataObject,
			IHttpXmlEDIMessage requestEdiMessage,
			ICodeMappingManager codeMapper = null,
			IHttpXmlMessageSaver messageSaver = null)
		{
			var result = new HttpXmlProcessingResult();

			var dataContext = topLevelDataObject.DataContext?.GetEnterpriseServerAndCompanyIDs();
			Exception exceptionWhileSaving = null;
			var saved = false;

			void HandleSaveExceptionBeforeRetrying(Exception e)
			{
				if (e is ZSaveException)
				{
					try
					{
						ZExceptionReporting.HandleSaveException(e, new NullNotificationHandler(), throwOnMergeFailure: true);
					}
					catch (Exception ex) when (ex.Find<ZSaveException>() != null)
					{
						// Handle by retrying
					}
				}
			}

			bool SaveAndCommit(IDelayedTransactionManager transaction)
			{
				try
				{
					factory.Save();
					messageSaver?.Save(result);
					transaction.CommitTransaction();
					saved = !transaction.IsRollingback;
					return true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					exceptionWhileSaving = ex;
					if ((Workflow.UniversalXmlWorkflowProcessor.IsRetryException(ex) || ex is ZSaveException) && (!messageSaver?.IsFinalProcessingAttempt ?? false))
					{
						return false;
					}
					using (result)
					{
						throw;
					}
				}
			}

			Exception retryException = null;

			var hasFinishedWithoutCriticalExceptions = false;

			try
			{
				using (var transaction = SetupDelayedTransaction(factory))
				{
					var isSuccessful = false;
					if (dataContext == null || ContextSwitching.IncomingContextMatchesRegistrationKey(dataContext, xmlSessionTracker))
					{
						var resultEvents = Workflow.UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(factory, null, topLevelDataObject, RequestMessageSubType, requestEdiMessage, xmlSessionTracker, codeMapper, transaction: transaction, shouldRetry: false);
						if (resultEvents != null && resultEvents.Length > 0)
						{
							var stream = requestEdiMessage.Factory.SubscribeForDispose(new CargoWise.IO.Shim.SubStreamableStream()); // This stream will not be saved by this factory.
							result.ShouldRetry = resultEvents.ShouldRetry;
							if (resultEvents.ShouldRetry)
							{
								retryException = resultEvents.FailureException;
							}
							result.ResponseMessageText = stream;
							new XmlWriter().WriteXML(resultEvents[0], result.ResponseMessageText, false);

							isSuccessful = resultEvents.FailureException is null;
						}
					}

					UpdateStatuses(isSuccessful, result, requestEdiMessage);

					if (!transaction.IsRollingback && !result.ShouldRetry)
					{
						using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
						{
							var saveSucceeded = SaveAndCommit(transaction);
							result.ShouldRetry = !saveSucceeded;
						}
					}
				}

				if (!saved && ((messageSaver?.IsFinalProcessingAttempt ?? false) || !result.ShouldRetry))
				{
					// This happens when a save inside processing handles a save exception but the transaction must be rolled back.
					// Here we should only be saving the request and response EDIMessages which should be updated with the correct data as the save exception has been handled.
					// It is incorrect if the factories saved here have been used for business logic. This should be done in MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName
					// We should make these messages use the same factory and get rid of the delayed transaction
					using (var transaction = SetupDelayedTransaction(factory))
					{
						SaveAndCommit(transaction);
					}
				}

				hasFinishedWithoutCriticalExceptions = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				hasFinishedWithoutCriticalExceptions = true;
				throw;
			}
			finally
			{
				if (hasFinishedWithoutCriticalExceptions)
				{
					factory.DeactivateActiveCollectionsAndCaches();
					if ((!saved || exceptionWhileSaving != null) && ((messageSaver?.IsFinalProcessingAttempt ?? false) || !result.ShouldRetry))
					{
						Workflow.UniversalXmlWorkflowProcessor.TryUseNewFactoryToFailMessage(requestEdiMessage, xmlSessionTracker, exceptionWhileSaving);
					}
					if (result.ShouldRetry && retryException != null)
					{
						HandleSaveExceptionBeforeRetrying(retryException);
					}
				}
			}

			return result;
		}

		IDelayedTransactionManager SetupDelayedTransaction(BusinessObjectFactory factory)
		{
			return eAdaptorRegistry.Instance.UniversalXMLExtendedTransactionProtectionEnabled.Value ?
				factory.DelayedTransaction() :
				factory.GetDummyDelayedTransaction();
		}

		class NullNotificationHandler : INotificationHandler
		{
			public void ReportInformation(string message, string caption)
			{
				// Continue handling exception
			}

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				// Continue handling exception
			}
		}
	}
}
