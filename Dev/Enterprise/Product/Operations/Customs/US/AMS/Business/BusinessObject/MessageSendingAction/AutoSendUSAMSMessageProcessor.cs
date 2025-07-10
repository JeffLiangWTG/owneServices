using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using IProcessor = CargoWise.EntityFramework.IProcessor;

namespace Enterprise.Customs.US.AMS.Business
{
	public class AutoSendUSAMSMessageProcessor : IProcessor
	{
		public AutoSendUSAMSMessageProcessor(CusInBondHeader cusInBondHeader)
		{
			Header = cusInBondHeader;
		}

		public CusInBondHeader Header { get; private set; }

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			if (Header.HasChanges)
			{
				if (Header.HasErrors)
				{
					LogValidationError(notifications, Header.GetErrors().ToUniqueMessageListString());

					try
					{
						Header.Factory.Save();
					}
					catch (ZSaveException e)
					{
						LogSystemError(notifications, e.Message);
					}
				}
			}

			using (DisposableEnvironment.ForBranch(Header.RegistryBranchPK))
			using (Header.SuspendSettingHasChanges())
			{
				if (Header.SCACInCarrier.IsEmpty)
				{
					LogValidationError(notifications, Enterprise.Customs.US.AMS.Business.MessageSender.Constants.Message.NoOrgProxySCACNotification(Header));
				}
				else
				{
					var hasValidationRunOnHeader = false;
					var hasMessageBeenGenerated = false;
					var messageNum = 0;
					AMSEDIMessage message = null;
					var beginTime = ZDateTime.UtcNow;

					foreach (var bill in Header.Bills)
					{
						bill.RunPreSaveValidation();

						if (!hasValidationRunOnHeader)
						{
							Header.RunPreSaveValidationExcludingChildren();
							hasValidationRunOnHeader = true;
						}

						if (Header.HasErrors || bill.HasErrors)
						{
							LogValidationError(notifications, Header.GetErrors().ToUniqueMessageListString());
						}
						else
						{
							if (bill.MovementDetail is CusInBondMoveDetail moveDetail)
							{
								MessageSendingObject obj;

								var hasActionCodeAndAmendmentCode = !bill.B0_BillActionCode.IsEmpty && !bill.B0_BillAmendmentCode.IsEmpty;
								if (hasActionCodeAndAmendmentCode)
								{
									obj = new MessageSendingObject(moveDetail, ActionCode.AmendingAdd);
								}
								else
								{
									obj = new MessageSendingObject(moveDetail, ActionCode.Creating);
								}

								if (obj.MB_Send)
								{
									if (AMSBillMessageStatusList.IsMessagingInProgressType(obj.MB_MessageStatus))
									{
										notifications.AddWarning($"System cannot send the Manifest message for Bill of Lading {obj.MB_IssuerCode} {obj.MB_BillOfLadingSequenceNumber}, please check whether it's waiting for response from customs.");
									}
									else if (obj.IsCreatingActionAndBillAlreadyOnFile)
									{
										notifications.AddWarning($"System cannot send the Manifest message for Bill of Lading {obj.MB_IssuerCode} {obj.MB_BillOfLadingSequenceNumber}, as it is already on Customs's File.");
									}
									else
									{
										if (hasActionCodeAndAmendmentCode)
										{
											message = new ACEAMSMessageBuilder(new DeleteMessageSendingObject(obj), obj.GetActualActionCode()).PopulateMessage();
											if (obj.MB_BillActionCode == AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd)
											{
												obj.GeneratePendingOriginalAdd();
											}
										}
										else
										{
											message = new ACEAMSMessageBuilder(obj, obj.GetActualActionCode()).PopulateMessage();
											if (message != null)
											{
												message.EM_SendWithMessageErrors = obj.HasMessageErrors;
											}
										}

										if (messageNum >= AMSMessageHelper.BatchSize && message != null)
										{
											message.EM_HeldUntilDate = beginTime.AddMinutes(messageNum / AMSMessageHelper.BatchSize * AMSMessageHelper.Delay);
										}

										messageNum++;
										hasMessageBeenGenerated = true;
										obj.PurgeActionCodeAndAmendmentCode();
									}
								}
							}
						}
					}

					if (hasMessageBeenGenerated)
					{
						try
						{
							Header.Factory.Save();
							LogInformation(notifications, $"Manifest message has been sent to customs for Job:{Header.HumanReadableName}");
						}
						catch (ZSaveException e)
						{
							LogSystemError(notifications, e.Message);
							return;
						}
					}
					else
					{
						notifications.AddWarning($"There is no Manifest message sent to customs for Job:{Header.HumanReadableName}");
					}
				}
			}
		}

		void LogValidationError(INotifications notifications, ZString validationErrorMessage)
		{
			notifications.AddError($"System cannot send Manifest message because of following errors on Job:{Header.HumanReadableName}, please fix all of them and try again.\r\n{validationErrorMessage}");
		}

		void LogInformation(INotifications notifications, ZString information)
		{
			notifications.Add(CargoWise.EntityFramework.NotificationType.Information, information);
		}

		void LogSystemError(INotifications notifications, ZString errorMessage)
		{
			notifications.AddError($"There was a system error while attempting sending Manifest message for Job:{Header.HumanReadableName}. See below for more information.\r\n{errorMessage}\r\n");
		}

		internal AMSMessageHelper AMSMessageHelper
		{
			get
			{
				if (fAMSMessageHelper == null)
				{
					fAMSMessageHelper = new AMSMessageHelper();
				}
				return fAMSMessageHelper;
			}
			set
			{
				fAMSMessageHelper = value;
			}
		}
		AMSMessageHelper fAMSMessageHelper;
	}
}
