using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.MessageManagers
{
	public class MessageManager : MultiMessageManager
	{
		#region Constructor

		public MessageManager(JobDeclarationMessageSendingObjectParent declarationWrapper, IMessageNotificationCollector notification, bool shoulSendAll = false) : base()
		{
			this.declarationWrapper = Argument.NotNull(declarationWrapper, "declarationWrapper");
			this.notification = Argument.NotNull(notification, "notification");
			this.shoulSendAll = shoulSendAll;
		}
		readonly IMessageNotificationCollector notification;

		readonly JobDeclarationMessageSendingObjectParent declarationWrapper;

		readonly bool shoulSendAll;

		protected override bool SendWheneverPossibleOnceMessagingActive => true;

		public override IMessageManageableBizObj TopLevelBizObjToManage => declarationWrapper.ParentDeclaration;

		JobDeclaration JobDeclaration => declarationWrapper.ParentDeclaration;
		#endregion

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();
			foreach (var sendingObject in declarationWrapper.SendingObjectsCollection.Cast<MessageSendingObject>().Where(x => shoulSendAll || x.ShouldSend))
			{
				result.Add(GetMessageManager(sendingObject));
			}
			return result.ToArray();
		}

		SingleMessageManager GetMessageManager(MessageSendingObject sendingObject)
		{
			return new DeclarationMessageManager(sendingObject);
		}

		#region Implementation
		public bool SendMessages(ISendsMessagesToCustoms sender)
		{
			if (CanSendMessages())
			{
				var messages = SendMessagesWithoutSaving(sender);
				if (messages.Any())
				{
					using (declarationWrapper.ParentDeclaration.EntryHeader.GetGenerateEntryNumberExceptionSupporter())
					{
						try
						{
							LogDeclarationEvent();
							try
							{
								declarationWrapper.Factory.Save();
							}
							catch (Exception ex) when (!ex.IsCriticalException())
							{
								messages.ForEach(f => f.Delete());
								throw;
							}
						}
						catch (GenerateEntryNumberException ex)
						{
							AllMessageManagers.Select(x => x.BusinessObject as MessageSendingObject).Where(x => x.ShouldSend).ForEach(x => x.Header.ReloadSafe());
							notification.ShowError(ex.Message, Constants.CannotSendMessageCaption);
							return false;
						}
						catch (ZSaveException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
					return true;
				}
			}
			return false;
		}

		protected void LogDeclarationEvent()
		{
			var menuCaption = declarationWrapper.MenuCaption;
			if (!menuCaption.IsEmpty)
			{
				var logs = JobDeclaration.Logs;
				foreach (MessageSendingObject messageSendingObject in declarationWrapper.SelectedSendingObjects)
				{
					var action = messageSendingObject.Action;
					var eventRef = $"{menuCaption}, SendingObjectsCollectionAction='{action}'";
					logs.AddNew(Events.MessageSent, eventRef);
					switch (action)
					{
						case ActionCodeList.Codes.Update:
						case NX101ActionCodeList.Codes._17:
						case NX101ActionCodeList.Codes._18:
							logs.AddNew(Events.DeclarationAmendmentSent, eventRef);
							break;
						case ActionCodeList.Codes.Create:
							if (declarationWrapper.MessageType == MessageTypeList.Codes.ECD)
							{
								logs.AddNew(Events.ExportCustomsCommenced, eventRef);
							}
							else if (declarationWrapper.MessageType == MessageTypeList.Codes.ICD || declarationWrapper.MessageType == MessageTypeList.Codes.CAA)
							{
								logs.AddNew(Events.CustomsCommenced, eventRef);
							}
							break;
					}
				}
			}
		}

		internal IList<EDIMessage> SendMessagesWithoutSaving(ISendsMessagesToCustoms sender)
		{
			Initialise();
			SingleMessageManager[] singleMessageManagers = AllMessageManagers;
			if (singleMessageManagers.Length > 0)
			{
				return SendOriginal(sender, singleMessageManagers);
			}
			return Array.Empty<EDIMessage>();
		}

		bool CanSendMessages()
		{
			var result = true;
			var notifications = notification?.Notifications;
			if (notifications != null)
			{
				CollectNotificationsFromMessageManagers(notifications);
				if (notifications.ContainsWarning())
				{
					result = notification.ShowConfirmation(Res.GetString("F403DB06-D2D9-4E9E-B98C-D24B1A2816E1", "{0}\r\nAre you sure you want to continue with sending?", notifications.WarningNotificationsAsString()),
						Res.GetString("B137F6F2-422E-403F-8CA0-ADFDBA5872AC", "Continue send with warning?"));
				}

				notifications.Clear();
			}

			return result;
		}

		void CollectNotificationsFromMessageManagers(MessageSendingNotificationCollection notifications)
		{
			var allmanagers = GetAllMessageManagers();
			if (allmanagers.Any(x => x.IsWaitingForResponse))
			{
				notifications.AddWarning(Res.GetString("0D79606B-CBC1-435A-BA1D-8E21D5B1386B", "This job is waiting for a Customs response."));
			}
		}

		public bool CheckDaysDelayedDeclaration(string defaultAnswer = DaysOfDelayAnswersList.Codes.Update)
		{
			var result = true;
			if (JobDeclaration?.HasDaysOfDelayedDeclaration ?? false)
			{
				var allmanagers = GetAllMessageManagers().OfType<DeclarationMessageManager>();
				var instructions = allmanagers.Select(x => x.BusinessObject as MessageSendingObject).Where(x => x.Action == ActionCodeList.Codes.Create).Select(x => x.Header.EntryInstruction).Where(instruction => instruction.IsDelayedDeclarationFeeApplicable);

				if (instructions.Any())
				{
					var hasDifferenceFromEnteredData = false;
					var instructionsWithSuggestedDays = new List<Tuple<CusEntryInstruction, int>>();
					var zStringBuilder = new ZStringBuilder(Res.GetString("DF76B9CA-33BC-4CB2-96ED-13F7CE1E219E", "These declarations will be subject to delay fees.\r\nThe calculated and entered days of delay for each one are:")).AppendLine();
					foreach (var instruction in instructions)
					{
						var daysOfDelayCalculated = instruction.DaysOfDelayed;
						if (instruction.CEI_DaysOfDelayedDeclaration != daysOfDelayCalculated)
						{
							hasDifferenceFromEnteredData = true;
							instructionsWithSuggestedDays.Add(new Tuple<CusEntryInstruction, int>(instruction, daysOfDelayCalculated));
							zStringBuilder.Append(Res.GetString("D6B18EFD-28AB-4C34-B8E6-E34055BB68D4", "\t{0} - Calculated {1}, Entered {2}", instruction.EntryHeader.EntryNumber, daysOfDelayCalculated, instruction.CEI_DaysOfDelayedDeclaration)).AppendLine();
						}
					}

					if (hasDifferenceFromEnteredData)
					{
						zStringBuilder.Append(Res.GetString("AC9EE708-B8C2-484A-B9D1-D2224BD726EB", "Please select one of the actions below."));

						var answerList = Factory.GetCachedValue<DaysOfDelayAnswersList>();
						var answer = notification.ShowQuestion(zStringBuilder.ToString(), Res.GetString("ADC20802-6799-427B-8981-9A38AB5C0F86", "Continue send with warning?"), 3, answerList, defaultAnswer);
						switch (answer)
						{
							case "":
								result = false;
								break;
							case DaysOfDelayAnswersList.Codes.Update:
								instructionsWithSuggestedDays.ForEach(item => item.Item1.CEI_DaysOfDelayedDeclaration = item.Item2);
								JobDeclaration.DoMerge();
								Factory.Save();
								break;
						}
					}
				}
			}

			return result;
		}

		public bool CheckDeclarationNumber()
		{
			return CheckDeclarationNumber(JobDeclaration, notification);
		}

		public static bool CheckEntries(JobDeclaration declaration, IMessageNotificationCollector notification)
		{
			var result = true;
			if (declaration.EntryHeader == null)
			{
				var sender = declaration.MessageInitiator;
				if (sender.YesNoQuery(Res.GetString("JobDeclarationForm|Menu|CannotSendMessage_NoEntries", @"There are no entries to send customs declaration for.
Do you want to Generate entries (Merge)?"), Constants.WhetherToGenerateEntry))
				{
					if (declaration.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin)
					{
						if (declaration.DoMerge())
						{
							notification.ShowInformation(Res.GetString("JobDeclarationForm|Menu|GenerateEntrySuccessful", "Generate Entry(Merge) successful, please save and send again."), Constants.GenerateEntrySuccessful);
						}
					}
					else
					{
						notification.ShowError(Res.GetString("JobDeclarationForm|Menu|SubmitTypeIsNotBLT", @"Cannot Generate Entries (Merge) when Submit Type is not 'BLT'. Please change the Submit Type to 'BLT'. 

If you cannot see the Submit Type, configure the Submit Type to 'BLT' or 'BTH' in Registry > Customs > Integration > Local Country Customs Interface."), Constants.CannotSendMessageCaption);
					}
				}
				result = false;
			}
			return result;
		}

		public static bool CheckDeclarationNumber(JobDeclaration declaration, IMessageNotificationCollector notification)
		{
			var result = true;
			if (!declaration.ActiveEntryHeaders.Any())
			{
				notification.ShowError(Res.GetString("B7D543B2-53C2-40E0-BD0D-712742718EFB", "Please generate entries first."), Constants.CannotSendMessageCaption);
				result = false;
			}
			else if (declaration.DeclarationNumber.IsEmpty || declaration.EntryHeader.EntryNumberForSendingObject == MessageConstants.EntryNumberPlaceHolder)
			{
				var allowedGeneratorDescription = new JobDeclarationAllocateNumber(declaration).CheckBeforeAutoRegenerateEntryNumber();
				if (!allowedGeneratorDescription.IsEmpty)
				{
					notification.ShowError(allowedGeneratorDescription, Constants.CannotSendMessageCaption);
					result = false;
				}
			}
			return result;
		}

		public static class Constants
		{
			public static MultilingualString CannotSendMessageCaption { get { return ResString.GetMultilingualString("JobDeclarationForm|Menu|CannotSendMessageCaption", "Cannot Send Message"); } }
			public static MultilingualString SendMessageCaption { get { return ResString.GetMultilingualString("JobDeclarationForm|Menu|SendMessageCaption", "Send Message"); } }
			public static MultilingualString WhetherToGenerateEntry { get { return ResString.GetMultilingualString("JobDeclarationForm|Menu|WhetherToGenerateEntry", "Whether to generate entries?"); } }
			public static MultilingualString GenerateEntrySuccessful { get { return ResString.GetMultilingualString("JobDeclarationForm|Menu|MergeSuccessful", "Generate Entry(Merge) successful"); } }

			public static string ContinueToSend => Res.GetString("C6167985-6FA2-4F8F-A030-225BFE9DB22D", "Continue To Send?");
		}
		#endregion
	}
}
