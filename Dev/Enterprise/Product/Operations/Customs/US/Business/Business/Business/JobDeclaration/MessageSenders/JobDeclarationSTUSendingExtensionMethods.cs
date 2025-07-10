using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.STU
{
	public static class JobDeclarationSTUSendingExtensionMethods
	{
		public static bool HasPSDChangedAndLiveEntry(this JobDeclaration declaration)
		{
			var fromPSD = declaration.GetOldPSDForLiveEntry();
			var toPSD = declaration.NewPSDForLiveEntry();

			return (declaration.US_LiveEntryIndicator == YesNoDefaultList.Codes.Yes)
					&& fromPSD != toPSD
					&& new AutomaticSTUConditionChecker().ShouldSend(declaration, declaration.US_PreliminaryStatementPrintDate.Date)
					&& !declaration.IsSTUPending();
		}

		public static string GetPSDChangedNotification(this JobDeclaration declaration)
		{
			string result;
			var fromPSD = declaration.GetOldPSDForLiveEntry();

			if (fromPSD.IsValid)
			{
				var toPSD = declaration.NewPSDForLiveEntry();

				var fromPSDString = fromPSD.IsValid ? fromPSD.Date.ToString(DateFormat) : string.Empty;
				var toPSDString = toPSD.IsValid ? toPSD.ToString(DateFormat) : string.Empty;

				result = string.Format(NotificationWhenOldPSDIsValid, fromPSDString, toPSDString);
			}
			else
			{
				result = NotificationWhenOldPSDIsEmpty;
			}

			return result;
		}

		internal const string NotificationWhenOldPSDIsValid = "The Preliminary Statement Date on file at Customs is '{0}'. Would you like to send a Statement Delete/Add Message to change the date to '{1}'?";
		internal const string NotificationWhenOldPSDIsEmpty = "The entry was filed with Payment Type 1. Would you like to send a Statement Delete/Add Message to change the payment details as entered?";
		internal const string DateFormat = "MM-dd-yyyy";// This is a US format.
		internal const string LogReference = "PSD changed on Live entry, STU not sent";

		public static void LogNoSTUSent(this JobDeclaration declaration)
		{
			declaration.Logs.AddNew(Events.PeriodDateChanged, LogReference);
		}

		static ZDateTime GetOldPSDForLiveEntry(this JobDeclaration declaration)
		{
			return declaration.US_PSDAccepted;
		}

		static ZDateTime NewPSDForLiveEntry(this JobDeclaration declaration)
		{
			return declaration.US_PreliminaryStatementPrintDate;
		}

		public static bool IsSTUPending(this JobDeclaration declaration)
		{
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var result = false;

			if (entry != null)
			{
				var messages = declaration.TransmittedStatementMessagesInDescOrder;

				foreach (MQEDIMessage message in messages)
				{
					if (message.RelatedMessage == null)
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		public static bool IsHPSTUPending(this JobDeclaration declaration)
		{
			var latestHPMessage = declaration.Messages.Cast<MQEDIMessage>().Where(x => x.EM_MessageType == ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction && x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			return latestHPMessage != null && latestHPMessage.RelatedMessage == null;
		}
	}
}
