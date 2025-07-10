using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.US.LVS.Business
{
	public class MessageManager
	{
		public MessageManager(CusUSLVClearance clearance)
		{
			this.clearance = clearance;
		}
		readonly CusUSLVClearance clearance;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event Customs.Business.MessageSender.SaveEventHandler OnSave;

		public bool SaveJob()
		{
			bool result = true;
			if (OnSave != null)
			{
				OnSave();

				if (clearance.HasChanges || clearance.HasErrors)
				{
					result = false;
				}
			}
			return result;
		}

		int SubmitToCustomsViaASCServiceTask(IEnumerable<CusUSLVConsignmentForMessaging> consignments)
		{
			var billsActuallySent = consignments.Count();

			foreach (var consignment in consignments)
			{
				consignment.Consignment.IgnoreWhenOnlyMessageStatusJustChanged = true;
				consignment.Consignment.ULB_MessageStatus = Common.US.ImportMessageStatusList.Codes.OriginalRequestPending;
				consignment.Consignment.Logs.AddNew(AutoEvents.OriginalMessageRequestPending, new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, consignment.Consignment.ULB_HouseBill));
			}

			var stmProcessQueue = CustomsStmProcessQueueLoader.LoadOrCreate(clearance, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage);
			stmProcessQueue.SW_EventCode = AutoEvents.MessagePendingProcessingCode;

			try
			{
				clearance.Factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				billsActuallySent = 0;
			}
			finally
			{
				foreach (var consignment in consignments)
				{
					consignment.Consignment.IgnoreWhenOnlyMessageStatusJustChanged = false;
				}
			}

			return billsActuallySent;
		}

		public int SubmitToCustoms(IEnumerable<CusUSLVConsignmentForMessaging> consignments, UpdateActionCode updateAction)
		{
			var billsActuallySent = 0;

			if (updateAction == UpdateActionCode.Add)
			{
				billsActuallySent = SubmitToCustomsViaASCServiceTask(consignments);
			}
			else
			{
				var messageNumberStrategy = new BulkAllocateMessageNumberStrategy(consignments.First().Factory, consignments.Count());
				foreach (CusUSLVConsignmentForMessaging consignmentForMessaging in consignments)
				{
					if (SubmitConsignmentToCustoms(consignmentForMessaging, updateAction, messageNumberStrategy))
					{
						billsActuallySent++;
					}
				}
			}

			return billsActuallySent;
		}

		public static ZBool SubmitConsignmentToCustoms(CusUSLVConsignmentForMessaging consignmentForMessaging, UpdateActionCode updateAction, IMessageNumberStrategy messageNumberStrategy, bool needValidattion = false)
		{
			var consignment = consignmentForMessaging.Consignment;
			var sendingOption = new MessageSendingOption(consignmentForMessaging);
			var message = new SimplifiedEntryMessageBuilder(consignment, updateAction, sendingOption).PopulateMessage();
			message.MessageNumberStrategy = messageNumberStrategy;
			if (needValidattion)
			{
				consignment.Validation.ValidateAll();
			}
			message.EM_SendWithMessageErrors = consignment.HasMessageErrors;

			if (updateAction == UpdateActionCode.Add)
			{
				var recentLog = consignment.Logs.MostRecentLogByEventTime(AutoEvents.OriginalMessageRequestPending);
				if (recentLog != null)
				{
					var user = recentLog.SL_GS_NKUser;
					if (!user.IsEmpty)
					{
						message.EM_SystemCreateUser = user;
					}
				}
			}

			new SimplifiedEntryMessageStatusCalculator(consignment).CalculateStatus(message, ABIResponseStatus.Undefined);
			return message != null;
		}

		public class BulkAllocateMessageNumberStrategy : IMessageNumberStrategy
		{
			public BulkAllocateMessageNumberStrategy(BusinessObjectFactory factory, int numberCount)
			{
				this.factory = Argument.NotNull(factory, nameof(factory));
				this.numberCount = Argument.NotNull(numberCount, nameof(numberCount));
			}
			readonly BusinessObjectFactory factory;
			readonly int numberCount;
			Stack<string> numbers;
			public string GetMessageReferenceNumber()
			{
				if (numbers == null)
				{
					numbers = new Stack<string>(Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EDIInterchange.ApplicationCodes.USCustomsImport).GetNextsFormatted(factory, numberCount));
				}
				return GlbCompany.CurrentCompany.LicenceKeyIdentifier + "_" + numbers.Pop();
			}
		}
	}
}
