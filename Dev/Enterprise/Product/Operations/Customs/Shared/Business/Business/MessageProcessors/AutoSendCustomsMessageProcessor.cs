using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.Business
{
	public abstract class AutoSendCustomsMessageProcessor : IProcessor
	{
		protected AutoSendCustomsMessageProcessor(BaseJobDeclaration declaration)
		{
			this._declaration = declaration;
		}
		readonly BaseJobDeclaration _declaration;

		protected BaseJobDeclaration Declaration
		{
			get { return _declaration; }
		}

		protected abstract ZString MessageDescription { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "text in service tasks")]
		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			using (DisposableEnvironment.ForBranch(_declaration.RegistryBranchPK))
			{
				if (Declaration.ShowSubmitMenuItem)
				{
					notifications.AddWarning(ZString.Format("System cannot send the {0} message for Job:{1}, because this job is configured to submit through a designated service provider interface.", MessageDescription, Declaration.JE_DeclarationReference));
					return;
				}

				var entryHeadersToSend = GetEntryHeadersToSend();
				if (entryHeadersToSend == null || !entryHeadersToSend.Any())
				{
					entryHeadersToSend = MergeAndGetEntryHeadersToSend(notifications);
				}

				if (Declaration.HasChanges)
				{
					if (Declaration.HasErrors)
					{
						notifications.AddError(LogErrorsOnDeclaration);
						return;
					}

					try
					{
						Declaration.Factory.Save();
					}
					catch (ZSaveException e)
					{
						LogSystemError(notifications, e.Message);
					}
				}

				if (entryHeadersToSend != null && entryHeadersToSend.Any())
				{
					var hasValidationRunOnDeclaration = false;
					var hasMessageBeenGenerated = false;
					var hasErrorsOnDeclaration = false;
					foreach (var entryHeaderToSend in entryHeadersToSend)
					{
						var entryHeaderMutex = GetEntryHeaderMutexToSendMessage(entryHeaderToSend);
						try
						{
							if (entryHeaderMutex.Lock())
							{
								if (CanSendEntryHeader(entryHeaderToSend))
								{
									if (!hasValidationRunOnDeclaration)
									{
										hasErrorsOnDeclaration = ValidateDeclarationBeforeSendingMessage(notifications, entryHeaderToSend);
										hasValidationRunOnDeclaration = true;
									}

									if (!hasErrorsOnDeclaration)
									{
										var processorType = GetType().FullName;
										using (new FactorySaveAlerter(participants =>
										{
											if (participants.Contains(Declaration.Factory))
											{
												ExceptionReporter.Instance.ReportDeveloperException(
													$"Unexpected Factory.Save in [{processorType}]",
													(NoResString)"Factory.Save() should not be called by message processor.", null);
											}
										}))
										{
											hasMessageBeenGenerated |= SendCustomsMessageCore(notifications, entryHeaderToSend);
										}
									}
								}
								else if (entryHeaderToSend.IsWaitingForResponse)
								{
									notifications.AddWarning(ZString.Format((NoResString)"System cannot send the {0} message for Job:{1}, please check whether {0}({2}) is waiting for response from customs.", MessageDescription, Declaration.JE_DeclarationReference, GetEntryReferenceNumber(entryHeaderToSend)));
								}
								else
								{
									notifications.AddWarning(ZString.Format((NoResString)"System cannot send the {0}({1}) message for Job:{2}.", MessageDescription, GetEntryReferenceNumber(entryHeaderToSend), Declaration.JE_DeclarationReference));
								}
							}
							else
							{
								var mutexLockedByInfo = entryHeaderMutex.GetMutexLockByInfo();
								notifications.AddError(ZString.Format((NoResString)"System cannot send the {0}({1}) message for Job:{2}, as {3} is trying to send the same message for this entry. Please wait unitl the lock has been released before trying to send the message again.", MessageDescription, GetEntryReferenceNumber(entryHeaderToSend), Declaration.JE_DeclarationReference, mutexLockedByInfo));
							}
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							LogSystemError(notifications, e.Message);
						}
						finally
						{
							if (entryHeaderMutex != null && entryHeaderMutex.HasLock)
							{
								entryHeaderMutex.Unlock();
							}
						}
					}

					if (hasMessageBeenGenerated)
					{
						try
						{
							Declaration.LogCustomsCommencedIfNeeded();
							ZExceptionReporting.ProcessWithConcurrencyHandling(Declaration.Factory.Save, null);
							notifications.Add(NotificationType.Information, ZString.Format((NoResString)"{0} message has been sent to customs for Job:{1}", MessageDescription, Declaration.JE_DeclarationReference));
						}
						catch (ZSaveException e)
						{
							LogSystemError(notifications, e.Message);
							return;
						}
					}
				}
				else
				{
					notifications.AddError(ZString.Format((NoResString)"There is no entry found to send {0} message for Job:{1}.", MessageDescription, Declaration.JE_DeclarationReference));
				}
			}
		}

		IEnumerable<CusEntryHeader> GetEntryHeadersToSend()
		{
			return GetEntryHeadersToSendCore().Where(x => x != null && !x.IsDeleted);
		}

		protected abstract IEnumerable<CusEntryHeader> GetEntryHeadersToSendCore();

		IEnumerable<CusEntryHeader> MergeAndGetEntryHeadersToSend(INotifications notifications)
		{
			IEnumerable<CusEntryHeader> result = null;

			if (!Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge)
			{
				var notifier = new SendsMessagesToCustomsShutterUpperer(false);
				if (Declaration.DoMerge(notifier))
				{
					result = GetEntryHeadersToSend();
				}
				else
				{
					notifications.AddError(notifier.InvalidOperationText);
				}
			}

			return result;
		}

		protected virtual ZBool CanSendEntryHeader(CusEntryHeader entryHeader)
		{
			return !entryHeader.IsWaitingForResponse;
		}

		protected virtual ZString GetEntryReferenceNumber(CusEntryHeader entryHeader)
		{
			return entryHeader.EntryNumber;
		}

		ZBool ValidateDeclarationBeforeSendingMessage(INotifications notifications, CusEntryHeader entryHeader)
		{
			ValidateDeclarationCore(entryHeader);
			if (Declaration.HasErrors)
			{
				notifications.AddError(LogErrorsOnDeclaration);
				return true;
			}
			return false;
		}

		ZString LogErrorsOnDeclaration => ZString.Format((NoResString)"System cannot send {0} message because of following errors on Job:{1}, please fix all of them and try again.\r\n{2}", MessageDescription, Declaration.JE_DeclarationReference, Declaration.GetErrors().ToUniqueMessageListString());

		protected abstract ZBool SendCustomsMessageCore(INotifications notifications, CusEntryHeader entryHeader);

		protected virtual void ValidateDeclarationCore(CusEntryHeader entryHeader)
		{
			Declaration.RunPreSaveValidation();
		}

		protected void LogSystemError(INotifications notifications, ZString errorMessage)
		{
			notifications.AddError(ZString.Format((NoResString)"There was a system error while attempting sending {0} message. See below for more information.\r\n{1}\r\n", MessageDescription, errorMessage));
		}

		ZGlobalMutex GetEntryHeaderMutexToSendMessage(CusEntryHeader entryHeader)
		{
			return entryHeaderMutexToSendMessage ?? (entryHeaderMutexToSendMessage = new ZGlobalMutex(Enterprise.ZArchitecture.Modules.MutexIDs.SendCustomsMessage, entryHeader.PK.ToString()));
		}
		ZGlobalMutex entryHeaderMutexToSendMessage;
	}
}
