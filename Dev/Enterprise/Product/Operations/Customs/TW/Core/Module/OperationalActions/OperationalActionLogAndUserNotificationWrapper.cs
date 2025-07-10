using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class OperationalActionLogAndUserNotificationWrapper : IMessageNotificationCollector, IUserNotification, IOperationalActionSectionLog
	{
		public OperationalActionLogAndUserNotificationWrapper(IUserNotification notifier, IMessageNotificationCollector notificationCollector, IOperationalActionSectionLog log, ZBool ignoreAllWarnings, ZBool suppressNotificationPopout)
			: base()
		{
			logger = log;
			this.notifier = notifier;
			this.notificationCollector = notificationCollector;
			this.ignoreAllWarnings = ignoreAllWarnings;
			this.suppressNotificationPopout = suppressNotificationPopout;
		}

		readonly IOperationalActionSectionLog logger;
		readonly IUserNotification notifier;
		readonly IMessageNotificationCollector notificationCollector;

		#region User Notification controls

		readonly bool ignoreAllWarnings;
		readonly bool suppressNotificationPopout;

		#endregion

		#region IUserNotification Members

		public bool ShowConfirmation(string message, string caption, bool warning = false)
		{
			logger.Notify(OperationalActionLogErrorLevel.Informational, Constants.AwaitingConfirmation + message);
			var result = suppressNotificationPopout ? ignoreAllWarnings : notifier.ShowConfirmation(message, caption, warning);
			logger.Notify(OperationalActionLogErrorLevel.Informational, (suppressNotificationPopout ? Constants.AutoAnswer : Constants.UsersAnswer) + (result ? YesNoList.Descriptions.Yes : YesNoList.Descriptions.No));
			return result;
		}

		public bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			logger.Notify(OperationalActionLogErrorLevel.Informational, Constants.AwaitingConfirmation + message);
			var result = suppressNotificationPopout ? ignoreAllWarnings : notifier.ShowConfirmation(message, caption, confirmationPrompt, confirmationString);
			logger.Notify(OperationalActionLogErrorLevel.Informational, (suppressNotificationPopout ? Constants.AutoAnswer : Constants.UsersAnswer) + (result ? YesNoList.Descriptions.Yes : YesNoList.Descriptions.No));
			return result;
		}

		public string ShowQuestion(string message, string caption, int answerLength, CodeDescriptionPairList answerList, string defaultAnswer = null)
		{
			logger.Notify(OperationalActionLogErrorLevel.Informational, Constants.AwaitingAnswer + message);
			var result = suppressNotificationPopout ? defaultAnswer : notifier.ShowQuestion(message, caption, answerLength, answerList, defaultAnswer);
			logger.Notify(OperationalActionLogErrorLevel.Informational, Constants.UsersAnswer + result);
			return result;
		}

		public void ShowError(string message, string caption)
		{
			logger.Notify(OperationalActionLogErrorLevel.Error, message);
			if (!suppressNotificationPopout)
			{
				notifier.ShowError(message, caption);
			}
		}

		public void ShowInformation(string message, string caption)
		{
			logger.Notify(OperationalActionLogErrorLevel.Informational, message);
			if (!suppressNotificationPopout)
			{
				notifier.ShowInformation(message, caption);
			}
		}

		public void ShowWarning(string message, string caption)
		{
			if (!ignoreAllWarnings)
			{
				logger.Notify(OperationalActionLogErrorLevel.Warning, message);
			}
			if (!suppressNotificationPopout)
			{
				notifier.ShowWarning(message, caption);
			}
		}

		#endregion

		#region IOperationalActionSectionLog Members

		public void BumpSectionProgress()
		{
			logger.BumpSectionProgress();
		}

		public void Notify(OperationalActionLogErrorLevel errorLevel, string text)
		{
			logger.Notify(errorLevel, text);
		}

		public void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
		{
			logger.NotifyFormat(errorLevel, format, args);
		}

		public void SetSectionProgressMax(int max)
		{
			logger.SetSectionProgressMax(max);
		}
		#endregion

		#region IMessageNotificationCollector
		MessageSendingNotificationCollection IMessageNotificationCollector.Notifications => notificationCollector.Notifications;

		public void Add(INotification notification)
		{
			notificationCollector.Add(notification);
		}
		#endregion

		public static class Constants
		{
			public static string UsersAnswer => Res.GetString("A57C6217-796E-44CD-BBCA-F92C8026F99C", "[User's Answer]:");

			public static string AutoAnswer => Res.GetString("2B19B63E-374B-4DEC-8B64-FFA4A341DCA2", "[Automatic Answer]:");

			public static string AwaitingConfirmation => Res.GetString("1235F7DB-0028-466A-99D0-D5902CA9D98A", "[Awaiting User's Confirmation]:");

			public static string AwaitingAnswer => Res.GetString("6CB47AB9-15D5-40D5-A61D-06D2037D6386", "[Awaiting User's Answer]:");

			public static string SubmitSucceeded => Res.GetString("45B7EADD-9CAD-4577-960E-37645D4BEB3F", "Submit Succeeded.");

			public static string SubmitFailed => Res.GetString("B3896618-59B4-406D-BFCA-D9A5710794FA", "Submit Failed.");

			public static string EntryStatusMustBeEmptyOrREJ => Res.GetString("4EA06B6C-9987-495D-8702-1FB6988CDC79", "Entry status must be empty or REJ.");

			public static string MessageStatusMustBeEmptyOrER => Res.GetString("885420A4-9436-4711-9580-E5D5B62688A7", "Message status must be empty or a code starting with ER.");

			public static string EntryNumberIsMissing => Res.GetString("25192A1D-655A-41E2-B572-F8C2C38AD596", "Entry number is missing.");

			public static string ValidCredentialIsMissing => Res.GetString("F80242C1-5B12-45C2-B119-95A403531A35", "A valid credential is missing.");

			public static string TheJobDoesNotHaveBLT => Res.GetString("8B800FDA-133A-4C82-99DD-A6DD9D60132C", "The job does not have 'Submit Type: BLT - Submit entry using built-in messaging system' selected.");

			public static string SubmitOriginalEntryToTaiwanCustoms => Res.GetString("53EC8AD6-3779-486E-A966-3616058634D7", "Submit Original Entry to Taiwan Customs");

			public static string TaiwanExportCustomsDeclaration_Informal => Res.GetString("AC3C624E-4865-42CE-BE5E-3E174CAFFBE7", "Export Customs Declaration (Informal)");

			public static string TaiwanExportCustomsDeclaration_Formal => Res.GetString("8D390E35-30A8-46A6-B896-57F61B0AAB00", "Export Customs Declaration (Formal)");

			public static string TaiwanExportCustomsDeclaration_Proof => Res.GetString("3A6320A8-670B-4EF3-BBBF-CA50F7C36264", "Export Customs Declaration (Proof)");

			public static string TaiwanExportCustomsDeclaration_English => Res.GetString("6BA362F5-DD35-43E4-8BFB-2D0AC7506C83", "Export Customs Declaration (English)");

			public static string DeclarationDateIsNotToday => Res.GetString("B47D8F10-FEA1-46BB-98B8-CC22AED892A6", "Declaration date is not today.");

			public static string TaiwanImportCustomsDeclaration_Informal => Res.GetString("D4B1F4FE-09F4-4196-B89A-7C1C17522305", "Import Customs Declaration (Informal)");

			public static string TaiwanImportCustomsDeclaration_Formal => Res.GetString("D280A9E2-E021-4C8B-AB83-B10ADF7EF370", "Import Customs Declaration (Formal)");

			public static string TaiwanImportCustomsDeclaration_Proof => Res.GetString("EA3EF68A-3929-48AA-A53C-9D2CEB0E8BB9", "Import Customs Declaration (Proof)");

			public static string TaiwanImportCustomsDeclaration_English => Res.GetString("63808DC0-A5FC-4047-B417-DA5CEF858D9A", "Import Customs Declaration (English)");
		}
	}

	public class OperationalActionMessageNotificationCollector : SendsMessagesToCustomsGUI
	{
		public OperationalActionMessageNotificationCollector(IOperationalActionSectionLog log) : base()
		{
			logger = log;
		}

		readonly IOperationalActionSectionLog logger;

		public override void NotifyUserOfAnInvalidOperation(string text)
		{
			logger.NotifyFormat(OperationalActionLogErrorLevel.Error, text);
		}
	}
}
