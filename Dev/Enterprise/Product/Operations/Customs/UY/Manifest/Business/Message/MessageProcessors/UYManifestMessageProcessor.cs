using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.UY.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class UYManifestMessageProcessor<T> : BranchCustomsApplicationTypeMessageProcessor
		where T : EDIMessage
	{
		public UYManifestMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
			tableCreator = new HtmlTableCreator(new string[] { Res.GetString("D36DADED-BA27-4D0F-A9C3-A6852823B9D3", "Column"), Res.GetString("DCDE064B-36D7-4932-816C-A5E6A7AC115B", "Value") });
		}
		readonly HtmlTableCreator tableCreator;
		ZBool isFailure = ZBool.True;

		protected override string MessageFriendlyNameCore => Res.GetString("C91E6E63-2056-4107-8A26-1D1AD725F948", "UY Message");

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.UYCustoms;

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem() => UYCustomsDataRegistry.Instance.UYMANGroupNotification;

		protected virtual IMessageAttachee FindRelevantBusinessObject(T message) => null;

		protected void SetMessageStatusAsFailed(EDIMessage message) => message.EM_Status = EDIMessageStatusList.Codes.Failed;

		protected ZString GetEDIMessageStatusLogDescription(EDIMessage message) => Res.GetString("33D40049-4C80-4FD9-8F5B-C6FF7D6F96CD", "(Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to {4}.", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference, new EDIMessageStatusList().GetDescriptionFromCode(message.EM_Status));

		protected override void PreProcessMessageCore(EDIMessage baseMessage)
		{
			base.PreProcessMessageCore(baseMessage);
			var message = baseMessage as T;

			bool canLinkMessageToBusinessObject = false;

			var factory = message.Factory;
			var originalInterchange = UYMessageHelper.GetSentInterchangeWithTrackingId(message.Interchange.EI_SessionGUID, factory);
			if (originalInterchange != null)
			{
				var originalMessage = UYMessageHelper.GetOriginalMessage(originalInterchange.PK, factory);
				if (originalMessage != null)
				{
					message.EM_LinkUniqueID = originalMessage.EM_LinkUniqueID;
					message.EM_LinkTable = originalMessage.EM_LinkTable;
				}
			}

			var messageAttachee = message.EM_LinkedObject as IMessageAttachee;
			if (messageAttachee != null)
			{
				message.EM_GB = messageAttachee.GlobalBranchPK;
				canLinkMessageToBusinessObject = true;
			}

			if (!canLinkMessageToBusinessObject)
			{
				SetMessageStatusAsFailed(message);
				Logger.LogWarning(Res.GetString("DE7A41CA-8477-4476-B0E4-578ABA92C6BE", "Unable to find business object for message {0}", GetEDIMessageStatusLogDescription(message)));
			}
			else
			{
				message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
			}
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message.EM_MessageType == MessageTypes.Codes.XER)
			{
				ProcessInvalidCredentialMessage(message);
				message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			}
			else
			{
				ProcessMessageCore(message as T);
				SendNotificationEmailIfNeeded(message, ZBool.False);
			}
		}

		void ProcessMessageCore(T message)
		{
			var bodyText = message.EM_MessageText;
			if (bodyText.IsEmpty || !XmlUtils.IsValidXml(bodyText) || message.EM_LinkedObject == null)
			{
				Logger.LogWarning(Res.GetString("33F561F2-9678-44EE-8DDB-9BB70D2F1B45", "Message {0} failed to parse the message as {1} message.", message.EM_MessageNum, message.EM_MessageType));
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
			}
			else
			{
				message.EM_Status = ProcessManifestMessageCore(message as UYMessage);
			}
		}

		ZString ProcessManifestMessageCore(UYMessage message)
		{
			var status = EDIMessageStatusList.Codes.ProcessedOK;
			var header = message.EM_LinkedObject as AsycudaManifestHeader;

			var responses = UYHelperClass.GetDAEResponseInformation(message.EM_MessageText);
			if (header != null && responses != null)
			{
				var anyBillAccepted = false;
				var anyBillRejected = false;
				var itIsResend = false;
				var itIsCancel = false;

				foreach (var element in responses.Responses)
				{
					var response = element;
					var references = response.References;

					var responseType = references.Any() ? references.Where(x => x.Code == UYMessageConstants.ResponseType)?.FirstOrDefault().Value.Trim() : null;
					if (responseType == UYMessageConstants.IdBill || responseType == UYMessageConstants.IdPack)
					{
						var bill = UYMessageHelper.LookForAsycudaBill(response, header);
						if (bill == null)
						{
							var error = UYMessageConstants.BillNotFound;
							status = EDIMessageStatusList.Codes.Discarded;
							message.Notes.AddNew(true, UYMessageConstants.Processing, error);
							Logger.LogError(error);
							tableCreator.WriteRow(UYMessageConstants.Error, error);
						}
						else
						{
							var column = UYMessageConstants.Error;
							if (response.ResponseCode == UYMessageConstants.AcceptanceCode)
							{
								if (response.Help == UYMessageConstants.CancellationMessage)
								{
									bill.ABL_BillStatus = CustomsStatusList.Codes.CAN;
									bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Cancel;
									itIsCancel = true;
								}
								else if (response.Help.Contains(UYMessageConstants.NewMessage))
								{
									var numberDNA = references.FirstOrDefault(x => x.Code == UYMessageConstants.DNANumber).Value.Trim();

									bill.CustomsEntryNumber = numberDNA;
									bill.CustomsEntryNumberType = CusEntryNumberTypes.Uruguay.DNA;
									bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
									bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Accepted;

									var cusEntryNum = bill.CustomsEntryNumbers.AddNew();
									cusEntryNum.CE_EntryNum = numberDNA;
									cusEntryNum.CE_EntryType = CusEntryNumberTypes.Uruguay.DNA;

									column = UYMessageConstants.Detail;
								}
								else if (response.Help.Contains(UYMessageConstants.AmendMessage))
								{
									bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
									bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Accepted;
								}
								else if (response.Help.Contains(UYMessageConstants.AmendPack) || response.Help.Contains(UYMessageConstants.NewPack))
								{
									bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
									bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Accepted;
								}

								var nodes = UYMessageHelper.GetResponseInformation(response.Description, response.ResponseCode);
								foreach (var item in nodes)
								{
									tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
								}

								isFailure = false;
								message.EM_MessageInterpretation = UYMessageHelper.MessageInterpretation(responses.Responses);
								tableCreator.WriteRow(column, message.EM_MessageInterpretation);

								anyBillAccepted = true;
							}
							else
							{
								bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;

								if (bill.ABL_BillStatus == CustomsStatusList.Codes.SNT)
								{
									bill.ABL_BillStatus = CustomsStatusList.Codes.ERR;
								}
								else if (bill.ABL_BillStatus == CustomsStatusList.Codes.ACP)
								{
									itIsResend = true;
								}

								message.EM_MessageInterpretation = UYMessageHelper.MessageInterpretation(responses.Responses);
								tableCreator.WriteRow(column, message.EM_MessageInterpretation);

								anyBillRejected = true;
							}

							message.Notes.AddNew(true, ((ZString)response.Description).SubstringSafe(0, 50), response.Help);
						}
					}
				}

				var exitAwaitingBillsAfterProcessMessage = header.Bills.OfType<AsycudaBill>().Where(x => x.ABL_MessageStatus == MessageStatusCodeList.Codes.Awaiting);

				if (exitAwaitingBillsAfterProcessMessage.Any())
				{
					UYMessageHelper.UpdateBOWhenBillNotFound(header, exitAwaitingBillsAfterProcessMessage);
					var error = responses.Responses.FirstOrDefault().Description;
					message.Notes.AddNew(true, UYMessageConstants.Processing, error);
					Logger.LogError(error);
					tableCreator.WriteRow(UYMessageConstants.Error, error);
					message.EM_MessageInterpretation = UYMessageHelper.MessageInterpretation(responses.Responses);
				}
				else
				{
					header.AMA_MessageStatus = !itIsResend && !anyBillAccepted
						? (ZString)MessageStatusCodeList.Codes.Error
						: itIsCancel && !anyBillRejected && !header.Bills.Cast<AsycudaBill>().Any(x => x.ABL_BillStatus == CustomsStatusList.Codes.ACP) ? (ZString)MessageStatusCodeList.Codes.Cancel : (ZString)MessageStatusCodeList.Codes.Accepted;

					header.RegistrationStatus = itIsCancel && header.Bills.Cast<AsycudaBill>().All(x => x.ABL_BillStatus == CustomsStatusList.Codes.CAN)
						? (ZString)MessageStatusCodeList.Codes.Cancel
						: header.Bills.Cast<AsycudaBill>().Any(x => x.ABL_BillStatus == CustomsStatusList.Codes.ACP) ? (ZString)MessageStatusCodeList.Codes.Accepted : ZString.Empty;
				}
			}
			else
			{
				status = EDIMessageStatusList.Codes.Discarded;
				message.Notes.AddNew(true, UYMessageConstants.Processing, UYMessageConstants.WrongXML);
			}

			return status;
		}

		void ProcessInvalidCredentialMessage(EDIMessage message)
		{
			var credentialError = ZBool.False;

			var headerAttachee = message.EM_LinkedObject as IMessageAttachee;
			if (headerAttachee != null)
			{
				var headerTextDictionary = MessageHelper.GetHeaderTextDictionary(message.EM_MessageText);
				tableCreator.WriteRow(Res.GetString("E987E23B-88F2-4AC4-B3AD-29DA0ADFCFA6", "Error Type:"), headerTextDictionary.TryGetValue("custom.ErrorType", out var errorTypeValue) ? errorTypeValue : string.Empty);
				tableCreator.WriteRow(Res.GetString("5FE7C301-4DE3-47FC-A4FB-BD28CAF75DEA", "Notification Time:"), headerTextDictionary.TryGetValue("custom.NotificationTime", out var notificationTimeValue) ? notificationTimeValue : string.Empty);
				tableCreator.WriteRow(Res.GetString("38B8567B-6E63-4055-849D-B267447A80EB", "Notification Type:"), headerTextDictionary.TryGetValue("custom.NotificationType", out var notificationType) ? notificationType : string.Empty);
				tableCreator.WriteRow(Res.GetString("5C8FD7A2-D083-40F2-AA23-C09EC110A506", "Error Description:"), headerTextDictionary.TryGetValue("custom.ErrorDescription", out var errorDescription) ? errorDescription : string.Empty);

				message.EM_MessageInterpretation = UYMessageHelper.MessageInterpretationForWrongCredentials(headerTextDictionary);
			}

			if (message.EM_MessageText.Contains((NoResString)"result code 401 not accepted"))
			{
				UpdateCredentialData(message);
				credentialError = ZBool.True;
			}

			var header = message.EM_LinkedObject as AsycudaManifestHeader;
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;

			foreach (AsycudaBill bill in header.Bills)
			{
				if (bill.ABL_MessageStatus == MessageStatusCodeList.Codes.Awaiting)
				{
					bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;

					if (bill.ABL_BillStatus == CustomsStatusList.Codes.SNT)
					{
						bill.ABL_BillStatus = CustomsStatusList.Codes.ERR;
					}
				}
			}

			SendNotificationEmailIfNeeded(message, credentialError);
		}

		void UpdateCredentialData(EDIMessage message)
		{
			var company = message.Branch?.Company;

			if (company != null)
			{
				var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GlbExternalPassword;
				credential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			}
		}

		#region Send Notification Email

		void SendNotificationEmailIfNeeded(EDIMessage message, ZBool credentialsError)
		{
			var messageAttachee = message.EM_LinkedObject as IMessageAttachee;
			if (messageAttachee != null)
			{
				var branchPK = messageAttachee.GlobalBranchPK;
				if (branchPK.IsValid)
				{
					messageBranch = message.Factory.Load<IGlbBranch>(branchPK);
				}
				else
				{
					messageBranch = GlbBranch.CurrentBranch;
				}

				var emailGroupRegistryItem = GetEmailGroupRegistryItem();
				var supportMessageSuppressRegistry = emailGroupRegistryItem as ISupportMessageSuppressRegistry;
				var shouldSendErrorEmailsOnly = supportMessageSuppressRegistry?.ShouldSEndErrorsOnly(MessageBranch.GB_GC, MessageBranch.PK, ZGuid.Empty) ?? false;
				var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest, messageAttachee.PK.ToGuid());

				if (!shouldSendErrorEmailsOnly || (shouldSendErrorEmailsOnly && isFailure))
				{
					GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, url, messageAttachee.JobReference, MailSubject(),
						GetEmailBody(messageAttachee, tableCreator, isFailure, credentialsError),
						isFailure, message.Branch, messageAttachee as BusinessObject,
						() => GetEmailAddressToSendTo(messageAttachee, message));
				}
			}
		}

		string MailSubject() => Res.GetString("FB177D66-08B1-42C1-81C0-7AF1999C17C3", "Manifest Status Message");

		ZString GetEmailBody(IMessageAttachee messageAttachee, HtmlTableCreator tableCreator, bool isFailure, ZBool credentialsError)
		{
			var htmlBody = new StringBuilder();

			if (credentialsError)
			{
				htmlBody.Append(Res.GetString("3B8863CC-630D-4D42-AC65-33D48B7C07D1", "The Message for job {0} has been rejected. Please update credential information of the message sender.", messageAttachee.JobReference) ?? ZString.Empty);
			}
			else if (!isFailure)
			{
				htmlBody.Append(Res.GetString("2B4A9E12-CCB0-41A7-9871-9F8BB8FFA5A3", "Manifest Message for job {0} has been cleared. For details please follow the Link to the Manifest", messageAttachee.JobReference) ?? ZString.Empty);
			}
			else
			{
				htmlBody.Append(Res.GetString("CFBF9EDF-F4D9-4573-8D7D-DB1301D268FC", "Manifest Message for job {0} has been rejected. For details please follow the Link to the Manifest", messageAttachee.JobReference) ?? ZString.Empty);
			}
			htmlBody.Append("<br /><br />");
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}

		ZString GetEmailAddressToSendTo(IMessageAttachee messageAttachee, EDIMessage message)
		{
			var result = ZString.Empty;
			if (messageAttachee != null)
			{
				var originalMessage = GetOriginalMessage(messageAttachee, message);
				var originalSender = (IUser)originalMessage?.UserWhoQueuedThisRecord;

				if (originalSender == null || originalSender.IsBatchProcessor)
				{
					originalMessage = GetLastTransmitMessage(messageAttachee);
					originalSender = originalMessage?.UserWhoQueuedThisRecord;
				}
				result = originalSender?.EmailAddress ?? string.Empty;
			}
			return result;
		}

		EDIMessage GetOriginalMessage(IMessageAttachee messageAttachee, EDIMessage message)
		{
			EDIMessage result = null;
			if (messageAttachee != null)
			{
				if (originalMessageCached == null || originalMessageCached.EM_LinkUniqueID != messageAttachee.PK)
				{
					var messageNumber = message.EM_MessageNum;
					if (!messageNumber.IsEmpty)
					{
						originalMessageCached = messageAttachee.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageNum == messageNumber && x.PK != message.PK && x.EM_MessageType == message.EM_MessageType);
					}
				}
				result = originalMessageCached;
			}
			return result;
		}
		EDIMessage originalMessageCached;

		EDIMessage GetLastTransmitMessage(IMessageAttachee header)
		{
			EDIMessage result = null;
			if (header != null)
			{
				if (lastTransmitMessageCached == null || lastTransmitMessageCached.EM_LinkUniqueID != header.PK)
				{
					lastTransmitMessageCached = header.Messages.OfType<EDIMessage>().Where(x => x.IsTransmitMessage && x.EM_SystemCreateUser != User.ServiceUserCode)
						.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
				}
				result = lastTransmitMessageCached;
			}
			return result;
		}
		EDIMessage lastTransmitMessageCached;

		IGlbBranch MessageBranch
		{
			get { return messageBranch; }
			set { messageBranch = value; }
		}
		IGlbBranch messageBranch;
		#endregion
	}
}
