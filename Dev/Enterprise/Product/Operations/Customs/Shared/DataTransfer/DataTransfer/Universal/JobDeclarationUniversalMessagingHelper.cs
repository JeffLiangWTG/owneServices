using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class JobDeclarationUniversalMessagingHelper
	{
		public JobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent messageSendingObject)
		{
			MessageSendingObjectParent = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		}

		protected readonly IJobDeclarationMessageSendingObjectParent MessageSendingObjectParent;

		protected virtual ZString UniversalCustomsMessagingRecipientID { get => ZString.Empty; }

		protected virtual ZBool ShouldPopulateAttachedDocumentCollection => ZBool.False;

		protected virtual IEnumerable<BusinessObject> GetSelectedMessageSendingObjects() => MessageSendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().Where(obj => obj.ShouldSend).Select(obj => obj.Header);

		protected virtual DeclarationDataObjectWriterConfiguration GetDeclarationDataObjectWriterConfiguraion(BusinessObject header) => new DeclarationDataObjectWriterConfiguration
		{
			EntryHeaderPKsToPopulate = new List<ZGuid> { header.PK },
			ShouldPopulateAttachedDocumentCollection = ShouldPopulateAttachedDocumentCollection
		};

		public int SendUniversalMessage()
		{
			var messagesCount = 0;
			var declaration = MessageSendingObjectParent.ParentDeclaration;
			var eventType = new CodeDescriptionPair
			{
				Code = Events.MessageRequestedToBeSentCode,
				Description = Events.MessageRequestedToBeSent.Description
			};
			var userCode = StaticCurrentFetcher.Instance.CurrentUserCode;
			var userName = StaticCurrentFetcher.Instance.CurrentUser?.GS_FullName;

			var selectedEntryHeaders = GetSelectedMessageSendingObjects();
			foreach (var header in selectedEntryHeaders)
			{
				var declarationContent = declaration.GetUniversalShipment(
					GetDeclarationDataObjectWriterConfiguraion(header),
					filteredDataContextType: DataContextType.CustomsDeclaration);

				declarationContent.DataContext.SetWorkflowInfo(
					new WorkflowInfo
					{
						EventType = eventType,
						EventReference = GetEventReference(header),
						EventUser = new Staff { Code = userCode, Name = userName }
					}
				);

				if (CreateEDIEnterchage(header, declarationContent, UniversalCustomsMessagingRecipientID))
				{
					UpdateSendingObjectHeaderAfterCreatingEDIEnterchage(header, GetMessageAwaitingStatus(header));
					messagesCount++;
				}
			}
			try
			{
				declaration.Factory.Save();
			}
			catch (ZSaveException ex)
			{
				messagesCount = 0;

				foreach (EnterpriseBusinessObject header in selectedEntryHeaders)
				{
					header.Logs.LogsNotInDB.DeleteAll();
				}
				declaration.Logs.LogsNotInDB.DeleteAll();

				ZExceptionReporting.HandleSaveException(ex);
			}

			return messagesCount;
		}

		protected virtual void UpdateSendingObjectHeaderAfterCreatingEDIEnterchage(BusinessObject businessObject, ZString status)
		{
			if (businessObject is CusEntryHeader entryHeader)
			{
				entryHeader.CH_Status = status;
			}
		}

		public ZString ValidateCanSubmit()
		{
			var result = ZString.Empty;
			if (UniversalCustomsMessagingRecipientID.IsEmpty)
			{
				result = Res.GetString("47E5496D-9E03-4711-91F3-C1F862ED32A5", "A Recipient ID should be entered. It can be entered in the Registry: {0}", InstructionForRecipientIDSetup);
			}
			if (result.IsEmpty)
			{
				result = ValidateCanSubmitCore();
			}
			if (result.IsEmpty)
			{
				result = CheckDeniedParty(MessageSendingObjectParent.ParentDeclaration);
			}

			return result;
		}

		ZString CheckDeniedParty(BaseJobDeclaration dec)
		{
			var creditCheckManager = new MessageManagerCreditCheckWithSecurityHelper(dec);
			return creditCheckManager.IsDeniedPartyOKToSend ? string.Empty : creditCheckManager.ReasonForNotAllowed;
		}

		protected virtual ZString ValidateCanSubmitCore() => ZString.Empty;

		protected virtual ZString InstructionForRecipientIDSetup { get; }

		#region Message Type & Num for Event Reference

		ZString GetEventReference(BusinessObject header)
		{
			return ZString.Format("{0}={1}|{2}={3}",
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
				GetMessageTypeForEventReference(header),
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber,
				GetReferenceNumberForEventReference()
			);
		}

		protected virtual ZString GetMessageTypeForEventReference(BusinessObject header) => ZString.Empty;

		protected virtual ZBool UseMessageNumberAsRerenceNumber => false;

		ZString GetReferenceNumberForEventReference()
		{
			return UseMessageNumberAsRerenceNumber
				? (ZString)Constants.JobDeclarationUniversalMessaging.JobDeclarationUniversalMessageNumberPlaceHolder
				: GetReferenceNumberForEventReferenceCore();
		}

		protected virtual ZString GetReferenceNumberForEventReferenceCore() => ZString.Empty;

		#endregion

		protected bool CreateEDIEnterchage(BusinessObject header, Shipment shipment, ZString destination)
		{
			var result = false;

			var context = new DeliveryContext(header.Factory)
			{
				ParentInfo = EntityInfo.New(header),
				ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
				MessageTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				Notifications = new NullLogger()
			};

			var delivery = new EHubDelivery();

			var mode = GetEDICommunicationMode(header, destination);

			if (delivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(context.ParentInfo, shipment, new XmlWriter(), null)).Succeeded)
			{
				var interchange = (XmlEDIInterchange)delivery.InterchangeCreated;
				if (interchange != null)
				{
					interchange.ContainedMessages.Cast<XmlEDIMessage>().ForEach(
						message =>
						{
							message.Saving += EDIMessage_OnSaving;
							message.Saved += EDIMessage_OnSaved;
						}
					);
					result = true;
				}
			}

			return result;
		}

		protected virtual NonPersistentEDICommunicationMode GetEDICommunicationMode(BusinessObject header, ZString destination)
		{
			return new NonPersistentEDICommunicationMode
			{
				EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
				EK_Destination = destination
			};
		}

		void EDIMessage_OnSaving(EDIMessage message)
		{
			if (!message.EM_MessageNum.IsEmpty && UseMessageNumberAsRerenceNumber)
			{
				message.EM_MessageText = message.EM_MessageText.Replace(Constants.JobDeclarationUniversalMessaging.JobDeclarationUniversalMessageNumberPlaceHolder, message.EM_MessageNum);

				var interchange = message.Interchange;
				if (interchange != null)
				{
					interchange.EI_BodyText = interchange.EI_BodyText.Replace(Constants.JobDeclarationUniversalMessaging.JobDeclarationUniversalMessageNumberPlaceHolder, message.EM_MessageNum);
				}
			}

			SetMessageInterpretion(message);
		}

		protected virtual void SetMessageInterpretion(EDIMessage message)
		{
			_ = message.EM_MessageInterpretation; // trigger setting Message Interpretation Note, avoid HasChanges changed on loading
		}

		void EDIMessage_OnSaved(EDIMessage message, bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				message.Saving -= EDIMessage_OnSaving;
				message.Saved -= EDIMessage_OnSaved;
			}
			else if (!message.IsDeleted && !message.IsInDatabase)
			{
				var interchange = message.Interchange;
				if (interchange != null && !interchange.IsDeleted)
				{
					interchange.Delete();
				}
				message.Delete();
			}
		}

		protected virtual ZString GetMessageAwaitingStatus(BusinessObject header) => MessageStatusList.Codes.AwaitingOriginal;

		class NullLogger : INotifications
		{
			public void Add(INotification notification)
			{
				// Do nothing, because probably the person who write this didn't care about these logs.
			}
		}
	}
}
