using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business.MessageManagers
{
	public class MessageManager : MultiMessageManager
	{
		#region Constructor

		public MessageManager(JobDeclarationMessageSendingObjectParent declarationWrapper, IMessageNotificationCollector notification) : base()
		{
			this.declarationWrapper = Argument.NotNull(declarationWrapper, "declarationWrapper");
			this.notification = Argument.NotNull(notification, "notification");
			declaration = declarationWrapper.ParentDeclaration;
			declaration.GetEntryHeadersForPreCreditCheck = () => AllMessageManagers.Select(m => m.EntryHeader);
		}

		readonly JobDeclarationMessageSendingObjectParent declarationWrapper;
		readonly JobDeclaration declaration;

		public override IMessageManageableBizObj TopLevelBizObjToManage => declaration;

		#endregion

		readonly IMessageNotificationCollector notification;

		protected override bool SendWheneverPossibleOnceMessagingActive => true;

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();
			foreach (var sendingObject in declarationWrapper.SendingObjectsCollection.Cast<MessageSendingObject>().Where(x => x.ShouldSend))
			{
				result.Add(new CUSDECMessageManager(sendingObject, notification));
			}
			return result.ToArray();
		}

		protected new CUSDECMessageManager[] AllMessageManagers => base.AllMessageManagers.Cast<CUSDECMessageManager>().ToArray();

		bool CheckRequiredFieldsForBondedWarehousingAreEntered(CusEntryHeader entry)
		{
			var result = true;
			if (!entry.IsBondedWarehousingDisabled)
			{
				var errorMessages = entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(true, true, checkEntryDetails: entry.IsOutOfWarehouseWarehousing);
				if (!errorMessages.IsEmpty)
				{
					result = false;
					declaration.MessageInitiator.NotifyUserOfAnInvalidOperation(errorMessages);
				}
			}
			return result;
		}

		void OnOneOrMoreMessagesSent()
		{
			bool success = true;

			if (declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().All(x => x.MovementReferenceNumber.IsEmpty))
			{
				using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
				{
					declaration.JE_AGTCode = declaration.AgentCode;
					if (!IsAutoSendCustomsMessageProcessor)
					{
						try
						{
							declaration.Factory.Save();
						}
						catch (ZSaveConcurrencyException)
						{
							success = false;
							notification.AddError(Res.GetString("AF0B3567-8D1E-4121-B8D6-01E5F4E8D6B6",
								"Message sending failed.\nAnother user/process has modified this declaration and your changes cannot be saved. Please reload this declaration and try sending the messages again."));
						}
					}
				}
			}

			if (success)
			{
				AddPendingEntryPayInfos();
			}
		}

		void AddPendingEntryPayInfos()
		{
			foreach (var messageManager in AllMessageManagers)
			{
				var entry = messageManager.EntryHeader;
				if (entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.SouthAfricanCustoms, SARSEDIMessage.MessageTypes.CUSDEC, EDIMessage.Direction.Transmit) is CUSDECEDIMessage lastCUSDECMessage)
				{
					var cusdecHelper = CUSDECMessageHelper.New(lastCUSDECMessage);
					if (cusdecHelper != null)
					{
						EntryPayInfoHelper.AddEntryPayInfoFromOutGoingCUSDECMessage(entry, lastCUSDECMessage, GetDateTimeOfPayment(lastCUSDECMessage), cusdecHelper.PaymentMethod, CusEntryPayInfoStatusList.Codes.Pending);
						if (!IsAutoSendCustomsMessageProcessor)
						{
							entry.Factory.Save();
						}
					}
				}
			}
		}

		static ZDateTime GetDateTimeOfPayment(CUSDECEDIMessage lastCUSDECMessage)
		{
			var dateTimeOfPayment = ZDateTime.Empty;

			var heldUntilDateUtc = lastCUSDECMessage.EM_HeldUntilDate;
			var sourceDateTimeUtc = heldUntilDateUtc.IsValid ? heldUntilDateUtc : lastCUSDECMessage.EM_SystemCreateTimeUtc;
			if (sourceDateTimeUtc.IsValid)
			{
				dateTimeOfPayment = EnvProxy.Instance.Time.GetLocalTimeFromUtc(sourceDateTimeUtc.ToDateTime());
			}

			return dateTimeOfPayment;
		}

		#region Implementation

		public void SendMessages()
		{
			if (CanSendMessages())
			{
				var shouldCheckBondedWarehousing = declaration.IsWHSUniversalXMLActive;
				notification.Notifications?.Clear();
				var messagesSent = 0;
				foreach (var messageManager in AllMessageManagers)
				{
					if (!shouldCheckBondedWarehousing || CheckRequiredFieldsForBondedWarehousingAreEntered(messageManager.EntryHeader))
					{
						messagesSent += SendMessage(messageManager) ? 1 : 0;
					}
				}
				if (messagesSent > 0)
				{
					OnOneOrMoreMessagesSent();
				}
			}
			ShowMessageSendingResult();
		}

		bool SendMessage(CUSDECMessageManager manager)
		{
			if (manager is null)
			{
				return false;
			}

			var actionCode = manager.ActionCode;
			var entry = manager.EntryHeader;

			using (IsAutoSendCustomsMessageProcessor ? new MessageManagerFactorySaveSuspender(manager) : null)
			{
				if (actionCode == MessageSubTypes.Create || ((actionCode == MessageSubTypes.Withdraw || actionCode == MessageSubTypes.Change) && entry.HasWHSTransaction()))
				{
					Action factorySaveAction = IsAutoSendCustomsMessageProcessor ? null : entry.Factory.Save;
					return declaration.SendMessageWithBondedWarehouseAutomation(entry, entry.GetInventoryAutomationAction(), manager.SendMessage, manager.CUSDECMessageDataProvider.GetMessageAction(), factorySaveAction);
				}

				return manager.SendMessage();
			}
		}

		protected IEnumerable<CusEntryLine> GetRooCertificateMissingWarnings(CusEntryHeader entryHeader)
		{
			return entryHeader.AllEntryLines.Where(x => x.RandomLine.AreRooDetailsIncomplete);
		}

		bool CanSendMessages()
		{
			var result = true;

			var warningList = AllMessageManagers
				.SelectMany(manager => GetRooCertificateMissingWarnings(manager.EntryHeader))
				.ToList();

			if (warningList.Count != 0)
			{
				var warningListJoiningLRNs = ZString.Join(", ", warningList.Select(x => x.Header.CH_BGMReference).Where(x => !x.IsEmpty).ToArray());
				var warningText = warningListJoiningLRNs.Length > 0 ? Res.GetString("850AED39-7A0A-4F61-AA8C-D07FE1DD4343",
					"If you are certain you wish to send the record(s) with LRN(s) {0} as blank ROO certificates then please type: ",
					warningListJoiningLRNs) : Res.GetString("F2EB312D-C877-4910-BF53-AAB785573857",
					"If you are certain you wish to send some ROO certificates as blank then please type:");

				var isConfirm = declaration.MessageInitiator.ShowUserConfirmation(
					Res.GetString("712C0385-0B7F-483F-ACE5-1632D4C18F51",
						"Warning - Some ROO certificates are empty."),
					Res.GetString("D5186B02-CA1F-41F4-844E-18C854AF63E7",
						"Warning - ROO certificates are empty."),
					warningText,
					Res.GetString("DAD4C4F4-2660-453B-AED3-B448653C5154",
						"I wish to send some ROO certificates as blank."));
				if (isConfirm)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					declaration.Logs.AddNew(AutoEvents.EditedARecord, "User acknowledged send some ROO certificates as blank");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					declaration.Factory.Save();
				}
				else
				{
					result = false;
				}
			}

			if (result)
			{
				var notifications = notification?.Notifications;
				if (notifications != null)
				{
					CollectNotificationsFromMessageManagers(notifications);

					if (notifications.ContainsError())
					{
						result = false;
					}
					if (notifications.ContainsWarning())
					{
						result = notification.ShowConfirmation(notifications.WarningNotificationsAsString(), Res.GetString("2D25643B-B0BC-47A8-A225-697A237ED71D", "Continue sending with warning?"));
					}

					notifications.Clear();
					result = result && CheckCreditForAllSendingMessages(notifications);
				}
			}

			return result;
		}

		bool CheckCreditForAllSendingMessages(MessageSendingNotificationCollection notificationCollection)
		{
			var helper = new MessageManagerCreditCheckWithSecurityHelper(declaration, JobDeclarationMessageSendingObjectParent.DocumentApprovalReasonDescription);

			var shouldContinue = helper.IsCreditCheckOKToSend;
			if (!shouldContinue)
			{
				if (!helper.IsCreditCheckDoneOutsideCW1)
				{
					notificationCollection.AddError(helper.ReasonForNotAllowed);
				}
			}
			return shouldContinue;
		}

		void CollectNotificationsFromMessageManagers(MessageSendingNotificationCollection notifications)
		{
			var allManagers = AllMessageManagers;
			if (allManagers.Any(x => x.IsWaitingForResponse))
			{
				notifications.AddWarning(Res.GetString("2054A8DB-BE37-47AC-BE7D-DAB804D426C4", "This job is waiting for a Customs response.\r\nAre you sure that you want to resend to Customs?"));
			}
			if (allManagers.Any(x => x.IsTestMessage))
			{
				notifications.AddWarning(MessageSendingValidation.WarningWhenInTestModeText);
			}
		}

		void ShowMessageSendingResult()
		{
			var notifications = notification?.Notifications;
			if (notifications != null)
			{
				if (notifications.ContainsError())
				{
					notification.ShowError(notifications.ErrorNotificationsAsString(), Res.GetString("7E374F2E-6A64-4066-9DA2-778C7574F0F9", "Error in Message Sending"));
				}
				if (notifications.ContainsInformation())
				{
					notification.ShowInformation(notifications.InformationNotificationsAsString(), Res.GetString("F0655D94-E567-479A-B301-DC40F51D0E66", "Message Sending Result"));
				}
			}
		}

		#endregion

		public bool IsAutoSendCustomsMessageProcessor { get; set; }
	}
}
