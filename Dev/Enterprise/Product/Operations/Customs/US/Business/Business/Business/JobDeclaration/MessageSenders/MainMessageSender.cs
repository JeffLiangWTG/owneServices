using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class MainMessageSender : MessageSender
	{
		public delegate bool PrepareEventHandler(ImportMessageSendingActionCollection actions);

		public MainMessageSender(JobDeclaration declaration, ImportMessageSendingMessageType orgAmdWithdrawal, ImportMessageSendingMessageTypeAdditionalFilter additionalFilter = ImportMessageSendingMessageTypeAdditionalFilter.None)
			: base(declaration)
		{
			this.orgAmdWithdrawal = orgAmdWithdrawal;
			this.additionalFilter = additionalFilter;
		}
		readonly ImportMessageSendingMessageType orgAmdWithdrawal;
		readonly ImportMessageSendingMessageTypeAdditionalFilter additionalFilter;

		public event PrepareEventHandler OnPrepare;
		public Action AfterATHLogAdded;

		public void RestoreMPFAndDutyLastCalcDateIfNeeded()
		{
			if (lastDutyCalcDate.HasValue || lastMPFCalcDate.HasValue)
			{
				var entrySummaryEntry = Job.ActiveEntryHeaders.EntrySummaryEntry;
				if (entrySummaryEntry != null)
				{
					if (lastDutyCalcDate.HasValue && entrySummaryEntry.US_DutyCalcDate != lastDutyCalcDate.Value)
					{
						entrySummaryEntry.US_DutyCalcDate = lastDutyCalcDate.Value;
					}
					if (lastMPFCalcDate.HasValue && entrySummaryEntry.US_MPFCalcDate != lastMPFCalcDate.Value)
					{
						entrySummaryEntry.US_MPFCalcDate = lastMPFCalcDate.Value;
					}
				}
			}
		}

		ZDate? lastMPFCalcDate;
		ZDate? lastDutyCalcDate;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override bool Prepare()
		{
			bool isOKToSend = true;

			if (additionalFilter == ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease)
			{
				if (orgAmdWithdrawal == ImportMessageSendingMessageType.Original)
				{
					var simplifiedEntry = Job.ActiveEntryHeaders.SimplifiedEntry;
					if ((simplifiedEntry == null || !simplifiedEntry.HasBeenLodgedAtCustoms) && !Job.US_EnableCRL)
					{
						Job.MessageInitiator.WarnUserAboutSomething("In order to send an Original Cargo Release message, Enable Cargo Release must be ticked. Please tick the Enable Cargo Release tick box on the Declaration tab in order to send an Original Cargo Release message.", "Send Messages");
						return false;
					}
				}
				else if (!Job.IsCargoReleaseValidationMode)
				{
					Job.MessageInitiator.WarnUserAboutSomething("In order to send a Cargo Release message, please tick either Enable Cargo Release or Certify Cargo Rel from Sum on the Declaration tab.", "Send Message");
					return false;
				}
			}

			if (Job.ShouldRefreshExchangeRatesExposed)
			{
				Job.RefreshExchangeRates();
			}

			IDisposable forceMPFDateRecalculation = null;
			lastMPFCalcDate = null;
			lastDutyCalcDate = null;
			CusEntryHeader entrySummaryEntryForMPFAndDutyCheck = Job.ActiveEntryHeaders.EntrySummaryEntry;
			var shouldStoreMPFAndDutyDates = additionalFilter == ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary && (orgAmdWithdrawal == ImportMessageSendingMessageType.Original || orgAmdWithdrawal == ImportMessageSendingMessageType.Replacement);
			if (shouldStoreMPFAndDutyDates)
			{
				forceMPFDateRecalculation = Job.ReCalculateMPFAndDutyDate();
				if (entrySummaryEntryForMPFAndDutyCheck == null)
				{
					lastMPFCalcDate = ZDate.Empty;
					lastDutyCalcDate = ZDate.Empty;
				}
				else
				{
					lastMPFCalcDate = entrySummaryEntryForMPFAndDutyCheck.US_MPFCalcDate.Date;
					lastDutyCalcDate = entrySummaryEntryForMPFAndDutyCheck.US_DutyCalcDate.Date;
				}
			}
			else
			{
				forceMPFDateRecalculation = new DisposableObject();
			}
			using (forceMPFDateRecalculation)
			{
				ZDate? newMPFCalcDate = null;
				if (shouldStoreMPFAndDutyDates)
				{
					newMPFCalcDate = Job.DateForMPFCalculation.Date;
				}
				ZDate? newDutyCalcDate = null;
				if (shouldStoreMPFAndDutyDates && entrySummaryEntryForMPFAndDutyCheck != null)
				{
					newDutyCalcDate = ((IDutyDataLineHeader)entrySummaryEntryForMPFAndDutyCheck).DateForFeeCalculation.Date;
				}
				if (Job.ActiveEntryHeaders.Count == 0 || Job.MergeManager.RequiresMerge || (newMPFCalcDate.HasValue && newMPFCalcDate.Value != lastMPFCalcDate.Value) || (newDutyCalcDate.HasValue && newDutyCalcDate.Value != lastDutyCalcDate.Value))
				{
					if (Job.DoesImportEntryNumberNeedsToBeSpecified && !Job.LockImportEntryNumberAllocationMutex)
					{
						var message = Job.GetImportEntryNumberAllocationMutexLockInfo() + " is in the process of allocating Import Entry Number for this job; system cannot merge and send the data as it will result in a different Import Entry Number being allocated.\r\nPlease retry sending when the other user has finished.";
						Job.MessageInitiator.WarnUserAboutSomething(message, "Send Messages");
						return false;
					}

					isOKToSend = Job.DoMerge();
				}
				if (shouldStoreMPFAndDutyDates)
				{
					if (entrySummaryEntryForMPFAndDutyCheck != null)
					{
						newMPFCalcDate = Job.DateForMPFCalculation.Date;
						newDutyCalcDate = ((IDutyDataLineHeader)entrySummaryEntryForMPFAndDutyCheck).DateForFeeCalculation.Date;
						if ((newMPFCalcDate.HasValue && newMPFCalcDate.Value != lastMPFCalcDate.GetValueOrDefault()) ||
							(newDutyCalcDate.HasValue && newDutyCalcDate.Value != lastDutyCalcDate.GetValueOrDefault()))
						{
							if (newMPFCalcDate.HasValue)
							{
								entrySummaryEntryForMPFAndDutyCheck.US_MPFCalcDate = newMPFCalcDate.Value;
							}
							if (newDutyCalcDate.HasValue)
							{
								entrySummaryEntryForMPFAndDutyCheck.US_DutyCalcDate = newDutyCalcDate.Value;
							}
						}
					}
				}
			}

			if (!isOKToSend)
			{
				return false;
			}

			if (Actions.Count == 0)
			{
				var notificationText = ZString.Empty;
				switch (orgAmdWithdrawal)
				{
					case ImportMessageSendingMessageType.Original:
						notificationText = GetOriginalNotificationText();
						break;
					default:
						notificationText = "There are no entries to send " + orgAmdWithdrawal + " messages for";
						break;
				}

				Job.MessageInitiator.WarnUserAboutSomething(notificationText, "Send Messages");
				return false;
			}

			try
			{
				Job.Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
				return false;
			}

			if (!action.IsWithdrawal && IsCreditCheckRequired)
			{
				isOKToSend = IsCreditCheckOKToSend();
				if (!isOKToSend)
				{
					return false;
				}
			}

			if (OnPrepare != null)
			{
				return OnPrepare(Actions);
			}

			return false;
		}

		protected virtual bool IsCreditCheckRequired
		{
			get
			{
				var result = true;
				if (Job.IsENSFormalImport)
				{
					foreach (CusEntryHeader header in Job.ActiveEntryHeaders)
					{
						if (header.HasBeenLodgedAtCustoms && !Job.HasDiscrepancyInDisbursementAmount)
						{
							result = false;
						}
					}
				}
				return result;
			}
		}

		ZString GetOriginalNotificationText()
		{
			var result = "Use the Send Replacement/Amendment Messages option to send a Replace Message.";
			if (additionalFilter == ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease)
			{
				result = "ACE Cargo Release message has already been accepted. You must use the Send Replacement/Update Messages option to make ACE Cargo Release changes.";
			}
			return result;
		}

		internal static string GetEntrySummaryIsNeededWhenMPFDateIsDifferent(ZDate lastValuationDate, ZDecimal lastAmount, ZDate currentValuationDate, ZDecimal currentAmount)
		{
			return Res.GetString("11AA5E69-BDD0-4AB8-B2CC-AF341813ED6F", "The data that was used to calculate the MPF amount seems to have been changed since last Entry Summary submission; it's recommended that an update Entry Summary should be submitted.\r\nLast Valuation Date: {0}, MPF Amount: {1}\r\nCurrent Valuation Date: {2}, MPF Amount: {3}", lastValuationDate.ToShortDateString(), lastAmount.ToString(2), currentValuationDate.ToShortDateString(), currentAmount.ToString(2));
		}

		protected override bool ValidateJob()
		{
			Actions.CalculatesAndSetValidationModesOnDeclaration();
			return base.ValidateJob();
		}

		protected override bool GenerateMessage()
		{
			var result = false;
			bool shouldSendMessage = true;

			if (Job.IsACE)
			{
				using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(Job))
				{
					foreach (ImportMessageSendingAction action in Actions)
					{
						if (action.IsEntrySummary)
						{
							action.CensusWarningCodes.CopyToEntryLines();

							if (action.IsPaidRelevant && !action.US_Paid.IsEmpty)
							{
								Job.US_Paid = action.US_Paid;
							}

							Job.US_SEMultiCargoDispInd = action.US_SE_MultipleDispositionsIndic;
							break;
						}
					}
					if (HasAtLeastOnePSCReasonAndExplanationToSend())
					{
						Actions.CopyPSCReasonsAndExplanation();
					}
				}

				if ((orgAmdWithdrawal == ImportMessageSendingMessageType.Original || orgAmdWithdrawal == ImportMessageSendingMessageType.Replacement)
					&& eBondLogger.ShouldAddAutoSendLog(Job))
				{
					if (Actions.EntryHeaderActions.Any(x => x.IsEntrySummary && x.US_SendMessage))
					{
						var entrySummaryEntry = Job.ActiveEntryHeaders.EntrySummaryEntry;
						if (entrySummaryEntry != null)
						{
							eBondLogger.AddAutoSendEvent(entrySummaryEntry.Logs, orgAmdWithdrawal, Job.HasMessageErrors);
						}
					}

					if (Actions.EntryHeaderActions.Any(x => x.IsACECargoRelease && x.US_SendMessage))
					{
						var simplifiedEntry = Job.ActiveEntryHeaders.SimplifiedEntry;
						if (simplifiedEntry != null)
						{
							eBondLogger.AddAutoSendEvent(simplifiedEntry.Logs, orgAmdWithdrawal, Job.HasMessageErrors);
						}
					}
					if (AfterATHLogAdded != null)
					{
						AfterATHLogAdded();
					}
					shouldSendMessage = false;
					result = true;
				}
			}

			if (shouldSendMessage)
			{
				result = Actions.SendMessagesWithoutSaving(Job.MessageInitiator);
			}
			return result;
		}

		protected override bool ShouldNotifyUserOfASuccessfulSend
		{
			get { return false; }
		}

		protected override bool ShouldValidateJob
		{
			get { return true; }
		}

		protected override MessageSendingValidation GetMessageSendingValidation(BusinessObject businessObjectForNotifications, IEnumerable<INotification> msgErrors)
		{
			return MessageSendingValidation.New(businessObjectForNotifications, new USCustomsNotificationCollector((IMessageNotificationsProvider)businessObjectForNotifications, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors());
		}

		bool HasAtLeastOnePSCReasonAndExplanationToSend()
		{
			foreach (ImportMessageSendingAction action in Actions)
			{
				if (action.ShouldPSCReasonAndExplanationBeSaved)
				{ return true; }
			}
			return false;
		}

		internal ImportMessageSendingActionCollection Actions
		{
			get
			{
				if (action == null)
				{
					action = new ImportMessageSendingActionCollection(Job, orgAmdWithdrawal, GetEntryFilter());
				}
				return action;
			}
		}
		ImportMessageSendingActionCollection action;

		Func<CusEntryHeader, bool> GetEntryFilter()
		{
			Func<CusEntryHeader, bool> result = null;
			switch (additionalFilter)
			{
				case ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease:
					result = (x) => x.IsACECargoRelease;
					break;
				case ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary:
					result = (x) => x.IsFormalEntry;
					break;
			}
			return result;
		}
	}
}
