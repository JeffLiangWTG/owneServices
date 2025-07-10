using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.Business
{
	public class MessageSendingValidation
	{
		public static MessageSendingValidation New(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, bool refreshValidation = true)
		{
			return New(topLevelBusinessObjectForValidation, messageErrors, Env.Security.CustomsDeclarationSendWithMessageErrors, refreshValidation);//TODO Remove this option which assumes this security right
		}

		public static MessageSendingValidation New(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true)
		{
			MessageSendingValidation result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(topLevelBusinessObjectForValidation, messageErrors);
			}
			else
			{
				result = new MessageSendingValidation(topLevelBusinessObjectForValidation, messageErrors, sendMessageWithErrorsSecurityCheckpoint, refreshValidation);
			}
			return result;
		}

		public readonly BusinessObject TopLevelBusinessObjectForValidation;
		public readonly IEnumerable<INotification> MessageErrors;
		public readonly SecurityCheckpoint SendMessageWithErrorsSecurityCheckpoint;

		protected internal MessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true)
		{
			TopLevelBusinessObjectForValidation = topLevelBusinessObjectForValidation;
			allBizOForValidation = new List<BusinessObject>();
			if (TopLevelBusinessObjectForValidation != null)
			{
				allBizOForValidation.Add(TopLevelBusinessObjectForValidation);
				if (TopLevelBusinessObjectForValidation is ConsolidatedDeclaration consolidatedDeclaration)
				{
					allBizOForValidation.AddRange(consolidatedDeclaration.JobDeclarations);
				}
			}
			MessageErrors = messageErrors ?? allBizOForValidation.SelectMany(bizO => new CustomsNotificationCollector(bizO, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors());
			this.refreshValidation = refreshValidation;
			SendMessageWithErrorsSecurityCheckpoint = Argument.NotNull(sendMessageWithErrorsSecurityCheckpoint, nameof(sendMessageWithErrorsSecurityCheckpoint));
		}

		protected MessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, bool refreshValidation = true)
			: this(topLevelBusinessObjectForValidation, messageErrors, Env.Security.CustomsDeclarationSendWithMessageErrors, refreshValidation)
		{
		}
		protected bool refreshValidation;

		public MessageSendingNotificationCollection CheckBusinessObjectLevelValidation() => CheckBusinessObjectLevelValidationCore();

		public MessageSendingNotificationCollection CheckBusinessObjectLevelValidation(string errorHeaderText, string messageErrorHeaderText, string confirmationQuestionText)
			=> CheckBusinessObjectLevelValidationCore(errorHeaderText, messageErrorHeaderText, confirmationQuestionText);

		public MessageSendingNotificationCollection CheckBusinessObjectLevelValidation(bool isSendingInTestMode)
		{
			MessageSendingNotificationCollection result;
			result = CheckBusinessObjectLevelValidationCore();
			if (isSendingInTestMode && !result.ContainsError())
			{
				result.AddWarning(result.ContainsWarning() ? "\r\n" + WarningWhenInTestModeText : WarningAndConfirmationWhenInTestModeText);
			}
			return result;
		}

		public bool IsErrorsExistWithNoSecurityRight
		{
			get;
			private set;
		}

		public static string ErrorExistHeaderText => Res.GetString("447c606e-3ad1-4b84-86ae-3b7e8c2e25ea", "Please fix these errors before sending any messages:");

		public static string MessageErrorsExistHeaderText => Res.GetString("833d777b-03a3-4a1c-9c34-03da371fc88d", "It is likely that your message(s) will be rejected by Customs, as they have the following message errors:");

		public static string MessageErrorsExistWithNoSecurityRight => Res.GetString("7aa7914b-8be0-4443-8c2a-93ddefe2585f", "There are message errors on this job and you don't have security rights to send with message errors.");

		public static string InformationForGetSecurityRight => Res.GetString("BA4F29E6-EF1E-4D7C-A187-7160D2751C52", "If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: \r\n{0}");

		public static string MessageErrorConfirmationQuestionText => Res.GetString("39ccc6c6-3832-4fbb-8d8c-5aada6d0ad6f", "Do you want to send the message(s) despite these errors?");

		public static string WarningWhenInTestModeText => Res.GetString("B7FF3524-4631-4CFE-B713-9CDC632E7646", "This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs.");

		public static string WarningAndConfirmationWhenInTestModeText => Res.GetString("EFF5860B-281D-4512-A257-3916844D0B47", "This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs. Do you wish to continue?");

		public bool CheckBusinessObjectLevelValidation(ISendsMessagesToCustoms notifier) => CheckBusinessObjectLevelValidationCore(notifier, false);

		public bool CheckBusinessObjectLevelValidation(ISendsMessagesToCustoms notifier, bool isSendingInTestMode) => CheckBusinessObjectLevelValidationCore(notifier, isSendingInTestMode);

		protected virtual MessageSendingNotificationCollection CheckBusinessObjectLevelValidationCore(string errorHeaderText, string messageErrorHeaderText, string confirmationQuestionText)
		{
			var result = new MessageSendingNotificationCollection();
			IsErrorsExistWithNoSecurityRight = false;
			if (TopLevelBusinessObjectForValidation != null)
			{
				if (refreshValidation)
				{
					foreach (var bizO in allBizOForValidation)
					{
						var declaration = bizO as BaseJobDeclaration;
						using (declaration?.UnRegisterEditableChildObjectsForMessageValidation())
						{
							bizO.LoadChildEditableObjects();
							MarkAsNeedingValidationIncludingChildren();
							bizO.RunPreSaveValidation();
						}
					}
				}
				if (allBizOForValidation.Any(bizO => bizO.HasErrors))
				{
					if (allBizOForValidation.Take(2).Count() > 1)
					{
						result.AddError(errorHeaderText);
						foreach (var bizO in allBizOForValidation.Where(bizO => bizO.HasErrors))
						{
							var errors = new CustomsNotificationCollector(bizO, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().ToUniqueMessageListString();
							result.AddError(System.Environment.NewLine + System.Environment.NewLine + bizO.HumanReadableName + System.Environment.NewLine + errors);
						}
					}
					else
					{
						var errors = new CustomsNotificationCollector(TopLevelBusinessObjectForValidation, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().ToUniqueMessageListString();
						result.AddError(errorHeaderText + System.Environment.NewLine + System.Environment.NewLine + errors);
					}
				}
				else if (allBizOForValidation.Any(bizO => bizO.HasMessageErrors))
				{
					string messageErrorsAsString;

					if (allBizOForValidation.Count > 1)
					{
						messageErrorsAsString = string.Join(System.Environment.NewLine + System.Environment.NewLine, allBizOForValidation.Where(bizO => bizO.HasMessageErrors).Select(bizO => bizO.HumanReadableName + System.Environment.NewLine + new CustomsNotificationCollector(bizO, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors().ToUniqueMessageListString()));
					}
					else
					{
						var messageErrorCollector = MessageErrors;
						messageErrorsAsString = messageErrorCollector.ToUniqueMessageListString();
					}

					if (messageErrorsAsString.Length > 0)
					{
						if (CanSendWithMessageErrors)
						{
							var messageText = messageErrorHeaderText + "\r\n\r\n" + messageErrorsAsString + "\r\n\r\n" + confirmationQuestionText;
							result.AddWarning(messageText);
						}
						else
						{
							result.AddError($"{MessageErrorsExistWithNoSecurityRight} {string.Format(InformationForGetSecurityRight, SendMessageWithErrorsSecurityCheckpoint.DisplayTextPathToSecurityRight)}");
							result.AddError(ZString.Empty);
							result.AddError(ErrorExistHeaderText);
							result.AddError(ZString.Empty);
							result.AddError(messageErrorsAsString);
							IsErrorsExistWithNoSecurityRight = true;
						}
					}
				}
			}
			return result;
		}

		protected virtual void MarkAsNeedingValidationIncludingChildren()
		{
		}

		protected virtual bool CanSendWithMessageErrors => SendMessageWithErrorsSecurityCheckpoint.IsAllowed;

		protected virtual MessageSendingNotificationCollection CheckBusinessObjectLevelValidationCore()
			=> CheckBusinessObjectLevelValidationCore(ErrorExistHeaderText, MessageErrorsExistHeaderText, MessageErrorConfirmationQuestionText);

		protected virtual bool CheckBusinessObjectLevelValidationCore(ISendsMessagesToCustoms notifier, bool isSendingInTestMode)
		{
			MessageSendingNotificationCollection notifications = CheckBusinessObjectLevelValidation(isSendingInTestMode);

			bool result = true;
			if (notifications.ContainsError())
			{
				result = false;
				notifier.NotifyUserOfAnInvalidOperation(notifications.NotificationsAsString());
			}
			else if (notifications.ContainsWarning())
			{
				result = notifier.AskUserToContinueWithAction(notifications.NotificationsAsString(), Res.GetString("fa85ef9a-4636-4ddb-8af9-5a3901d896ae", "Continue to Send"), TopLevelBusinessObjectForValidation);
			}
			return result;
		}

		protected delegate MessageSendingValidation NewDelegate(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		readonly List<BusinessObject> allBizOForValidation;
	}
}
