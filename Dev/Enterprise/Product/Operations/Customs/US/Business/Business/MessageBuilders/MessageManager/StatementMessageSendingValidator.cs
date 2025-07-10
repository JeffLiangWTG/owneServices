using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.Business
{
	public class StatementMessageSendingValidator
	{
		public MessageSendingNotificationCollection GetNotificationsForPayment(CusStatementHeader statementHeader)
		{
			var result = GetCommonNotifications(statementHeader);

			if (statementHeader.HasLinesWithDeletionPending)
			{
				result.AddWarning(PaymentWarning);
			}

			if (statementHeader.GetACHPayMethodToDefault() == ACHPaymentTypeList.Codes.ImporterCheck)
			{
				result.AddWarning(ImporterCheckIsADefaultOption);
			}

			if (!statementHeader.B2_CheckNo.IsEmpty)
			{
				result.AddError(CheckNoEntered);
			}

			return result;
		}

		public MessageSendingNotificationCollection GetNotificationsForResettingPaymentStatus(CusStatementHeader statementHeader)
		{
			var result = new MessageSendingNotificationCollection();

			string warningMessage = null;

			if (statementHeader != null)
			{
				if (statementHeader.IsFinal)
				{
					warningMessage = Finalised;
				}
				else if (statementHeader.IsPaid)
				{
					warningMessage = AlreadyPaid;
				}
				else if (statementHeader.IsPaymentInProgress)
				{
					warningMessage = PaymentInProgress;
				}
			}

			if (warningMessage != null)
			{
				warningMessage = warningMessage.Substring(0, 1).ToUpper() + warningMessage.Substring(1);

				result.Add(new MessageSendingWarning(warningMessage));
			}

			return result;
		}

		internal const string PaymentWarning = "There are entries that are being deleted from this statement. If you make a payment now, the total amount will include duty/tax/fees for such entries.";
		internal const string ImporterCheckIsADefaultOption = "The payment method configuration on the importer organization is 'Importer Check'. ACH Authorization is not required.";
		internal const string CheckNoEntered = "Importer's check number has been entered indicating it will be paid by check. ACH Authorization is not required.";

		public MessageSendingNotificationCollection GetNotificationsForStatementDeleteAdd(JobDeclaration declaration)
		{
			var result = new MessageSendingNotificationCollection();

			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			if (entrySummaryEntry == null || !entrySummaryEntry.HasBeenLodgedAtCustoms)
			{
				if (declaration.DecEntryNumber.IsEmpty)
				{
					result.AddError(DecEntryNumberIsRequired);
				}
				else
				{
					result.AddWarning(string.Format(Entry7501NotLodged, "7501"));
				}
			}

			if (declaration.US_PaymentType == PaymentTypeList.Codes.IndividualBasis)
			{
				result.AddError(EntriesNotMeetConditions);
			}

			if (declaration.StatementIsPaid)
			{
				if (Env.Security.USCustomsImportStatementSendUpdateMsgWithErrors.IsAllowed && declaration.StatementIsPMSType)
				{
					result.AddWarning(AlreadyPaidAndFinalised);
				}
				else
				{
					result.AddError(ThisMessageWillFail + AlreadyPaid);
				}
			}

			return result;
		}

		public MessageSendingNotificationCollection GetNotificationsForStatementDeleteAdd(ReconDeclaration reconDeclaration)
		{
			var result = new MessageSendingNotificationCollection();

			var entry = reconDeclaration.ReconEntry.GetEntry();
			if (entry != null && !entry.HasBeenLodgedAtCustoms)
			{
				result.AddError(string.Format(Entry7501NotLodged, "Reconciliation"));
			}

			if (reconDeclaration.US_PaymentType == PaymentTypeList.Codes.IndividualBasis)
			{
				result.AddError(EntriesNotMeetConditions);
			}

			return result;
		}
		public const string DecEntryNumberIsRequired = "Declaration entry number is required.";
		public const string Entry7501NotLodged = "{0} entry has not been lodged yet. If Customs do not have this entry number in their system, this message will be rejected.";
		public const string EntriesNotMeetConditions = "There are no entries on this job that require Statement Delete/Add transactions.\r\nThis transaction is only relevant for Entry Summary with Payment Type not equal to 1 that have been accepted by Customs.";

		public MessageSendingNotificationCollection GetNotificationsForStatementDeleteAdd(CusStatementHeader statementHeader)
		{
			return GetCommonNotifications(statementHeader);
		}

		MessageSendingNotificationCollection GetCommonNotifications(CusStatementHeader statementHeader)
		{
			var result = new MessageSendingNotificationCollection();

			MessageSendingNotification notificationOnStatus = null;

			if (statementHeader != null)
			{
				if (statementHeader.IsPaid)
				{
					if (Env.Security.USCustomsImportStatementSendUpdateMsgWithErrors.IsAllowed && statementHeader.IsPMSType)
					{
						notificationOnStatus = new MessageSendingWarning(AlreadyPaidAndFinalised);
					}
					else
					{
						notificationOnStatus = new MessageSendingError(ThisMessageWillFail + AlreadyPaid);
					}
				}
				else if (statementHeader.B2_Status == StatementHeaderStatusList.Codes.Deleted)
				{
					notificationOnStatus = new MessageSendingError(ThisMessageWillFail + Deleted);
				}
				else if (statementHeader.IsPaymentInProgress)
				{
					notificationOnStatus = new MessageSendingWarning(ThisMessageWillFail + PaymentInProgress);
				}
			}

			if (notificationOnStatus != null)
			{
				result.Add(notificationOnStatus);
			}

			return result;
		}

		public MessageSendingNotificationCollection CheckBusinessObjectLevelValidation(StatementPaymentAction action)
		{
			var result = new MessageSendingNotificationCollection();
			var actionMessageErrorAsString = GetMessageAsString(action);
			if (!string.IsNullOrEmpty(actionMessageErrorAsString))
			{
				result.AddWarning(actionMessageErrorAsString);
			}
			var header = action.StatementHeader;
			var headerMessageErrorAsString = GetMessageAsString(header);
			if (!string.IsNullOrEmpty(headerMessageErrorAsString))
			{
				result.AddWarning(headerMessageErrorAsString);
			}
			return result;
		}

		public string GetMessageAsString(BusinessObject topLevelBusinessObjectForValidation)
		{
			string result = string.Empty;
			if (topLevelBusinessObjectForValidation != null)
			{
				topLevelBusinessObjectForValidation.RunPreSaveValidation();
				if (topLevelBusinessObjectForValidation.HasMessageErrors)
				{
					var actionMessageErrors = new CustomsNotificationCollector(topLevelBusinessObjectForValidation, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
					string actionMessageErrorAsString = actionMessageErrors.ToUniqueMessageListString();
					if (actionMessageErrorAsString.Length > 0)
					{
						result = actionMessageErrorAsString;
					}
				}
			}
			return result;
		}

		internal const string ThisMessageWillFail = "This message will fail as ";
		internal const string AlreadyPaid = "this statement is already paid.";
		internal const string PaymentInProgress = "payment is in progress for this statement.";
		internal const string Deleted = "this statement has been deleted.";
		internal const string Finalised = "this statement is already finalized.";
		internal const string AlreadyPaidAndFinalised = "This statement has already been paid and its final statement has been received. Entries should not be removed at this point. If you remove an entry from a Final Statement, the system will not post any accounting adjustments. If these are needed, these will need to be created manually.";

		public MessageSendingNotificationCollection GetNotificationsForDeletePaymentAuthorization(CusStatementHeader statementHeader)
		{
			var result = new MessageSendingNotificationCollection();

			string warningMessage = null;

			if (statementHeader != null)
			{
				if (!statementHeader.IsPaid)
				{
					warningMessage = NotPaidYet;
				}
				else if (statementHeader.B2_PaymentAuthorizationDate.IsEmpty || statementHeader.B2_PaymentAuthorizationDate.Date != CargoWise.Types.ZDate.Today)
				{
					warningMessage = NegationDate;
				}
			}

			if (warningMessage != null)
			{
				warningMessage = warningMessage.Substring(0, 1).ToUpper(System.Globalization.CultureInfo.InvariantCulture) + warningMessage.Substring(1);

				result.Add(new MessageSendingWarning(warningMessage));
			}

			return result;
		}
		internal const string NotPaidYet = "Payment authorization has never been accepted.";
		internal const string NegationDate = "The negation message must be transmitted on the same day when an ACH payment authorization is accepted.";
	}
}
