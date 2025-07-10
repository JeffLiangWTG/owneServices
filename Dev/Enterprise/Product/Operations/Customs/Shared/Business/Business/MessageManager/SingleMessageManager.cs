using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class SingleMessageManager
	{
		public virtual bool RequiresAmendment()
		{
			return HasActiveMessages && RequiresAmendmentCore();
		}

		public bool PreventSend { get { return GetPreventSendCore; } }
		protected virtual bool GetPreventSendCore { get { return false; } }
		public abstract string MessageFriendlyName { get; }
		public abstract BusinessObject BusinessObject { get; }
		public abstract bool CanSendOriginal { get; }
		public abstract bool CanSendWithdrawal { get; }
		public virtual bool HasActiveMessages { get { return !CanSendOriginal; } }
		public void ResetToOriginal()
		{
			ResetToOriginalCore();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		public void ResetToOriginalWithLog(BusinessObject objectToSaveLogAgainst)
		{
			string resetOriginalCompletedMessage = " " + Res.GetString("61176336-2a80-4af2-96fc-7a129aeee19f", "(Reset Original Completed after user acknowledged responsibility)");
			string reference = new ZString(MessageFriendlyName).Left(StmALogSchema.SL_Reference.MaxLength - resetOriginalCompletedMessage.Length) + resetOriginalCompletedMessage;
			objectToSaveLogAgainst.GetLogs().AddNew(Events.ResetEntryMessageItemFunction, reference);
			ResetToOriginalCore();
		}

		protected virtual void ResetToOriginalCore()
		{
		}

		public
#if DEBUG
 virtual // for mock
#endif
			ForwardingShipmentProcessTask OverdueCargoReportException
		{
			get
			{
				if (exceptionForRequiredReason == null)
				{
					exceptionForRequiredReason = OverdueCargoReportExceptionCore;
				}
				return exceptionForRequiredReason;
			}
		}
		ForwardingShipmentProcessTask exceptionForRequiredReason;

		protected virtual ForwardingShipmentProcessTask OverdueCargoReportExceptionCore
		{
			get { return null; }
		}

		public abstract bool IsWaitingForResponse { get; }

		public virtual bool ShouldSendOriginalOnSave { get { return false; } }
		public virtual bool ShouldSendWithdrawalOnSave { get { return false; } }

		#region Required Messages Detection

		protected virtual internal BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			return newFactory.Load(businessObject.GetType(), businessObject.PK);
		}

		protected virtual bool RequiresAmendmentCore()
		{
			bool result = false;
			var mayRequireAmendment = BusinessObject as IMayRequireAmendment;

			if (mayRequireAmendment != null && !mayRequireAmendment.MayRequireAmendment)
			{
				result = false;
			}
			else
			{
				BusinessObject databaseBusinessObject = GetBusinessObjectInNewFactory(BusinessObject);
				if (databaseBusinessObject != null)
				{
					EDIMessage[] databaseMessages = GenerateMessagesForAmendmentDetection(databaseBusinessObject);
					EDIMessage[] factoryMessages = GenerateMessagesForAmendmentDetection(BusinessObject);
					try
					{
						if (factoryMessages.Length != databaseMessages.Length)
						{
							result = true;
						}
						else
						{
							ZString factoryMessage;
							ZString databaseMessage;

							fFactoryMessages = new ZStringBuilder();
							fDatabaseMessages = new ZStringBuilder();

							for (int i = 0; i < factoryMessages.Length; i++)
							{
								factoryMessage = factoryMessages[i].EM_MessageText;
								databaseMessage = databaseMessages[i].EM_MessageText;

								fFactoryMessages.Append("    ");
								fFactoryMessages.Append(i.ToString());
								fFactoryMessages.Append(" ");
								fFactoryMessages.Append(factoryMessage);
								fFactoryMessages.Append("    ");

								fDatabaseMessages.Append("    ");
								fDatabaseMessages.Append(i.ToString());
								fDatabaseMessages.Append(" ");
								fDatabaseMessages.Append(databaseMessage);
								fDatabaseMessages.Append("    ");

								if (factoryMessage != databaseMessage)
								{
									result = true;
									break;
								}
							}
						}
					}
					finally
					{
						foreach (EDIMessage message in factoryMessages)
						{
							message.Delete();
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region Message Generators

		public EDIMessage[] GenerateOriginalMessages(BusinessObject bizo)
		{
			EDIMessage[] result = GenerateOriginalMessagesCore(bizo);
			GeneratedMessages[MessageType.Original] = result;
			return result;
		}
		protected abstract EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo);

		protected virtual EDIMessage[] GenerateMessagesForAmendmentDetection(BusinessObject bizo)
		{
			return GenerateOriginalMessagesCore(bizo);
		}

		public EDIMessage[] GenerateAmendmentMessages(BusinessObject bizo)
		{
			EDIMessage[] result = GenerateAmendmentMessagesCore(bizo);
			GeneratedMessages[MessageType.Amendment] = result;
			return result;
		}
		protected abstract EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo);

		public EDIMessage[] GenerateWithdrawalMessages(BusinessObject bizo)
		{
			EDIMessage[] result = GenerateWithdrawalMessagesCore(bizo);
			GeneratedMessages[MessageType.Withdrawal] = result;
			return result;
		}
		protected abstract EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo);

		public void OnOriginalSent(bool runValidation = false)
		{
			SetSendWithMessageErrorsIfThereIsMessageErrorForType(MessageType.Original, runValidation);
			OnOriginalSentCore();
		}

		protected virtual void OnOriginalSentCore()
		{
		}

		public void OnAmendmentSent(bool runValidation = false)
		{
			SetSendWithMessageErrorsIfThereIsMessageErrorForType(MessageType.Amendment, runValidation);
			OnAmendmentSentCore();
		}

		protected virtual void OnAmendmentSentCore()
		{
		}

		public void OnWithdrawalSent(bool runValidation = false)
		{
			SetSendWithMessageErrorsIfThereIsMessageErrorForType(MessageType.Withdrawal, runValidation);
			OnWithdrawalSentCore();
		}

		protected virtual void OnWithdrawalSentCore()
		{
		}

		protected virtual BusinessObject BusinessObjectForNotification
		{
			get { return BusinessObject; }
		}

		protected virtual BusinessObject AdditionalBizoForNotifications
		{
			get { return null; }
		}

		Dictionary<MessageType, EDIMessage[]> GeneratedMessages
		{
			get { return generatedMessages ?? (generatedMessages = new Dictionary<MessageType, EDIMessage[]>()); }
		}
		Dictionary<MessageType, EDIMessage[]> generatedMessages;

		enum MessageType { Amendment, Original, Withdrawal }

		#endregion

		#region Developer Exception for false positive amendment detection

		public bool ShouldSendDeveloperExceptionForFalsePositive
		{
			get { return ShouldSendDeveloperExceptionForFalsePositiveCore(FactoryMessages, DatabaseMessages); }
		}

		protected virtual bool ShouldSendDeveloperExceptionForFalsePositiveCore(ZString factoryMessages, ZString databaseMessages)
		{
			return factoryMessages != databaseMessages;
		}

		public
#if DEBUG
 virtual
#endif
 ZString FactoryMessages
		{
			get { return fFactoryMessages == null ? string.Empty : fFactoryMessages.ToString(); }
		}
		ZStringBuilder fFactoryMessages;

		public
#if DEBUG
 virtual
#endif
 ZString DatabaseMessages
		{
			get { return fDatabaseMessages == null ? string.Empty : fDatabaseMessages.ToString(); }
		}
		ZStringBuilder fDatabaseMessages;

		#endregion

		#region Sending Notifications

		protected virtual internal MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			MessageSendingNotificationCollection result = new MessageSendingNotificationCollection();

			if (AdditionalBizoForNotifications != null)
			{
				ZString additionalBizoErrors = AdditionalErrorNotificationCollector.ToUniqueMessageListString();
				if (!additionalBizoErrors.IsEmpty)
				{
					result.AddError(additionalBizoErrors);
				}

				ZString additionalMessageErrors = AdditionalMessageErrorNotificationCollector.ToUniqueMessageListString();
				if (!additionalMessageErrors.IsEmpty)
				{
					if (SendWithMessageErrorsSecurityCheckpoint.IsAllowed)
					{
						result.AddWarning(additionalMessageErrors);
					}
					else
					{
						result.AddError(additionalMessageErrors);
					}
				}
			}

			if (IncludeBusinessLayerNotificationsInMessageSendingNotifications)
			{
				ZString errorsIncludingChildrenString = ErrorNotificationCollector.ToUniqueMessageListString();
				if (!errorsIncludingChildrenString.IsEmpty)
				{
					result.AddError(errorsIncludingChildrenString);
				}

				ZString messageErrorsIncludingChildrenString = MessageErrorNotificationCollector.ToUniqueMessageListString();
				if (!messageErrorsIncludingChildrenString.IsEmpty)
				{
					if (SendWithMessageErrorsSecurityCheckpoint.IsAllowed)
					{
						result.AddWarning(messageErrorsIncludingChildrenString);
					}
					else
					{
						result.AddError(MessageErrorsExistWithNoSecurityRight);
					}
				}
			}

			if (ShouldWaitUntilResponded && IsWaitingForResponse)
			{
				result.AddError(PendingMessagesError);
			}

			if (ShouldSendMessagesInTestMode && !result.ContainsError())
			{
				result.AddWarning(result.ContainsWarning() ? "\r\n" + MessageSendingValidation.WarningWhenInTestModeText : MessageSendingValidation.WarningAndConfirmationWhenInTestModeText);
			}

			return result;
		}

		protected virtual Security.SecurityCheckpoint SendWithMessageErrorsSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationSendWithMessageErrors; }
		}

		public string PendingMessagesError
		{
			get { return fPendingMessagesError ?? (fPendingMessagesError = PendingMessagesErrorMessage); }
			set { fPendingMessagesError = value; }
		}
		string fPendingMessagesError;

		public static string PendingMessagesErrorMessage
		{
			get { return Res.GetString("23dfcdf8-d4ed-4496-b0e4-decaebedcc8b", "There are messages waiting for responses and the system has detected you have made changes that affect Customs messages. The changes cannot be saved. Please wait until the messages are responded."); }
		}
		public static string MessageErrorsExistWithNoSecurityRight
		{
			get { return Res.GetString("3e2a421c-27ac-4071-93de-e2cda3c485f7", "There are message errors on this job and you don't have security rights to send with message errors."); }
		}

		/// <summary>
		/// US formal entries override this and do it manually as they send multiple messages in one place and validate only once
		/// </summary>
		protected virtual internal bool IncludeBusinessLayerNotificationsInMessageSendingNotifications
		{
			get { return true; }
		}

		protected virtual bool ShouldWaitUntilResponded
		{
			get { return true; }
		}

		protected virtual bool ShouldSendMessagesInTestMode
		{
			get { return false; }
		}

		IEnumerable<INotification> ErrorNotificationCollector
		{
			get
			{
				if (fErrorNotificationCollector == null)
				{
					fErrorNotificationCollector = new CustomsNotificationCollector(BusinessObjectForNotification, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
				}
				return fErrorNotificationCollector;
			}
		}
		IEnumerable<INotification> fErrorNotificationCollector;

		IEnumerable<INotification> AdditionalErrorNotificationCollector
		{
			get
			{
				if (fAdditionalErrorNotificationCollector == null)
				{
					fAdditionalErrorNotificationCollector = new CustomsNotificationCollector(AdditionalBizoForNotifications, false, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
				}
				return fAdditionalErrorNotificationCollector;
			}
		}
		IEnumerable<INotification> fAdditionalErrorNotificationCollector;

		protected internal IEnumerable<INotification> MessageErrorNotificationCollector
		{
			get
			{
				if (fMessageErrorNotificationCollector == null)
				{
					fMessageErrorNotificationCollector = GetMessageErrors();
				}
				return fMessageErrorNotificationCollector;
			}
		}
		IEnumerable<INotification> fMessageErrorNotificationCollector;

		protected virtual IEnumerable<INotification> GetMessageErrors()
		{
			return new CustomsNotificationCollector(BusinessObjectForNotification, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
		}

		IEnumerable<INotification> AdditionalMessageErrorNotificationCollector
		{
			get
			{
				if (fAdditionalMessageErrorNotificationCollector == null)
				{
					fAdditionalMessageErrorNotificationCollector = new CustomsNotificationCollector(AdditionalBizoForNotifications, false, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
				}
				return fAdditionalMessageErrorNotificationCollector;
			}
		}
		IEnumerable<INotification> fAdditionalMessageErrorNotificationCollector;

		public virtual MessageSendingNotificationCollection GetNotificationsForSendingAnOriginal()
		{
			return GetCommonNotificationsForSending();
		}

		public virtual MessageSendingNotificationCollection GetNotificationsForSendingAReplacement()
		{
			return GetCommonNotificationsForSending();
		}

		public virtual MessageSendingNotificationCollection GetNotificationsForSendingAWithdrawal()
		{
			return GetCommonNotificationsForSending();
		}

		public MessageSendingQueryCollection GetQueriesForSending()
		{
			return GetQueriesForSendingCore();
		}

		protected virtual MessageSendingQueryCollection GetQueriesForSendingCore()
		{
			return new MessageSendingQueryCollection();
		}

		#endregion

		void SetSendWithMessageErrorsIfThereIsMessageErrorForType(MessageType messageType, bool runValidation)
		{
			if (GeneratedMessages.TryGetValue(messageType, out var messages))
			{
				var hasMessageErrors = false;
				if (runValidation)
				{
					var boForValidationInNewFactory = GetBusinessObjectInNewFactory(BusinessObjectForNotification);
					var validation = MessageSendingValidation.New(boForValidationInNewFactory, new CustomsNotificationCollector(boForValidationInNewFactory, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors(), SendWithMessageErrorsSecurityCheckpoint);
					hasMessageErrors = validation.CheckBusinessObjectLevelValidation().ContainsWarning();
				}
				else
				{
					hasMessageErrors = MessageErrorNotificationCollector.Any();
				}
				if (hasMessageErrors)
				{
					foreach (var message in messages)
					{
						message.EM_SendWithMessageErrors = true;
					}
				}
			}
		}

		#region TestCase

#if DEBUG

		public bool ShouldSendMessagesInTestModeForTesting
		{
			get { return ShouldSendMessagesInTestMode; }
		}

#endif

		#endregion
	}
}
