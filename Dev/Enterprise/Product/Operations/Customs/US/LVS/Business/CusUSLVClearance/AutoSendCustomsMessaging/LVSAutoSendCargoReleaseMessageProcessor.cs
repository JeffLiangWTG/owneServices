using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.US.LVS.Business.MessageManager;
using NotificationType = CargoWise.ComponentModel.NotificationType;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class LVSAutoSendCargoReleaseMessageProcessor : CargoWise.EntityFramework.IProcessor
	{
		public LVSAutoSendCargoReleaseMessageProcessor(CusUSLVClearance clearance, string eventCode)
		{
			Clearance = Argument.NotNull(clearance, nameof(clearance));
			this.eventCode = eventCode;
		}

		readonly string eventCode;
		public CusUSLVClearance Clearance { get; private set; }

		void CargoWise.EntityFramework.IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			if (Clearance.HasChanges)
			{
				if (Clearance.HasErrors)
				{
					notifications.AddError(LogErrorsOnLowValue);

					try
					{
						Clearance.Factory.Save();
					}
					catch (ZSaveException e)
					{
						LogSystemError(notifications, e.Message);
					}
				}
			}

			using (DisposableEnvironment.ForBranch(Clearance.RegistryBranchPK))
			using (Clearance.SuspendSettingHasChanges())
			{
				var actionCode = UpdateActionCode.Add;
				Clearance.PrepareCusUSLVConsignmentsForUpdateAction(actionCode);

				var hasMessageBeenGenerated = false;

				var cusUSLVConsignmentsToSend = GetConsignmentForMessaging();
				var consignmentCount = cusUSLVConsignmentsToSend.Count();

				if (consignmentCount > 0)
				{
					if (Clearance.ULH_EntryFilerCode.IsEmpty)
					{
						notifications.AddError(Res.GetString("53c26b2c-6af6-483d-b689-a9f041b70aa0", "An entry filer code has not been set up for this branch or company. Please set up one in Admin->System->Registry {0}", ((IRegistryItemInternals)USCustomsDataRegistry.Instance.EntryFiler).Location));
					}
					else
					{
						try
						{
							if (Clearance.LockSendCustomsMessageMutex())
							{
								var messageNumberStrategy = new BulkAllocateMessageNumberStrategy(Clearance.Factory, consignmentCount);

								foreach (CusUSLVConsignmentForMessaging consignmentForMessaging in cusUSLVConsignmentsToSend)
								{
									var consignment = consignmentForMessaging.Consignment;

									if (!MessageSendingHelper.IsConsignmentNotWaitingForResponse(consignment))
									{
										notifications.AddWarning($"System cannot send the release message for house bill:{consignment.ULB_HouseBill}, please check whether it's waiting for response from customs.");
									}
									else
									{
										if (consignment.CE_EntryNum.IsEmpty)
										{
											var consignmentToAllocateEntryNumberList = new List<CusUSLVConsignmentForMessaging>();
											consignmentToAllocateEntryNumberList.Add(consignmentForMessaging);
											var reasonForUnableToAllocateEntryNumber = consignmentToAllocateEntryNumberList.AllocateEntryNumbersForConsignments(Clearance);
											if (!reasonForUnableToAllocateEntryNumber.IsEmpty)
											{
												notifications.AddError($"System cannot allocate entry number for house bill:{consignment.ULB_HouseBill}, becase {reasonForUnableToAllocateEntryNumber}");
											}
										}

										if (!consignment.CE_EntryNum.IsEmpty)
										{
											hasMessageBeenGenerated = MessageManager.SubmitConsignmentToCustoms(consignmentForMessaging, actionCode, messageNumberStrategy, true);
										}
									}
								}
							}
							else
							{
								var mutexLockByInfo = Clearance.GetSendCustomsMessageMutexLockByInfo();
								if (!string.IsNullOrEmpty(mutexLockByInfo))
								{
									notifications.AddWarning($"{mutexLockByInfo} is sending messages for this Low Value Entries job, please try again later.");
								}
							}
						}
						finally
						{
							Clearance.UnlockSendCustomsMessageMutex();
						}
					}
				}

				if (hasMessageBeenGenerated)
				{
					try
					{
						Clearance.Factory.Save();
						notifications.Add(NotificationType.Information, $"Release message has been sent to customs for Job:{Clearance.ULH_JobNumber}");
					}
					catch (ZSaveException e)
					{
						LogSystemError(notifications, e.Message);
						return;
					}
				}
				else
				{
					notifications.AddWarning($"There is no release message sent to customs for Job:{Clearance.ULH_JobNumber}");
				}
			}
		}
		ZString LogErrorsOnLowValue => $"System cannot send release message because of following errors on Job:{Clearance.ULH_JobNumber}, please fix all of them and try again.\r\n{Clearance.GetErrors().ToUniqueMessageListString()}";

		void LogSystemError(INotifications notifications, ZString errorMessage)
		{
			notifications.AddError($"There was a system error while attempting sending release message for Job:{Clearance.ULH_JobNumber}. See below for more information.\r\n{errorMessage}\r\n");
		}

		IEnumerable<CusUSLVConsignmentForMessaging> GetConsignmentForMessaging()
		{
			if (eventCode == AutoEvents.MessagePendingProcessingCode)
			{
				return Clearance.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Where(c => c.Consignment.ULB_MessageStatus == Common.US.ImportMessageStatusList.Codes.OriginalRequestPending);
			}

			return Clearance.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>();
		}
	}
}
