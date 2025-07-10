using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using EDIMessage = Enterprise.Customs.US.Business.EDIMessage;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public abstract class BaseBondEventProcessor
	{
		protected readonly BusinessObjectFactory factory;
		protected readonly Event eventDataObject;

		protected BaseBondEventProcessor(Event eventDataObject, BusinessObjectFactory factory)
		{
			this.eventDataObject = Argument.NotNull(eventDataObject, "eventDataObject");
			this.factory = Argument.NotNull(factory, "factory");
		}

		public abstract BusinessObject[] Process();

		protected void SendNotification(string linkMsg, string jobNumber, ZString additionalNote, GlbBranch branch, string[] emailAddressToSendTo)
		{
			var body = GetEmailBody(eventDataObject) + additionalNote;
			var updateCollection = eventDataObject.AdditionalFieldsToUpdateCollection;
			var additionalSubject = updateCollection != null ? GetFielsInUpdateCollection(updateCollection, BondStatusNotificationMessageProcessor.DispositionCode) : "";
			additionalSubject = GetAdditionalEmailSubject(additionalSubject);
			GenerateHtmlEmailAndSendToBrokerOrGroup(linkMsg, jobNumber, additionalSubject, MessageTypeDesc, body, branch, emailAddressToSendTo);
		}

		protected ZString GetBondNumber()
		{
			if (!bondNumber.HasValue)
			{
				var updateCollection = eventDataObject.AdditionalFieldsToUpdateCollection;
				bondNumber = updateCollection != null ? GetFielsInUpdateCollection(updateCollection, BondStatusNotificationMessageProcessor.CBPBondNumber) : "";
			}
			return bondNumber.Value;
		}
		ZString? bondNumber;

		protected string GetFielsInUpdateCollection(IEnumerable<AdditionalFieldToUpdate> list, string typeName)
		{
			var result = string.Empty;
			var additionalField = list.FirstOrDefault(x => x.Type.ToString() == typeName);
			if (additionalField != null)
			{
				result = additionalField.Value;
			}
			return result;
		}

		protected virtual string GetAdditionalEmailSubject(string additionalSubject)
		{
			return additionalSubject;
		}

		protected void LinkMessage(ZString applicationReference, BusinessObject linkObj)
		{
			var messageNumber = GetMessageNumber();
			if (!messageNumber.IsEmpty)
			{
				var messageQuery = new ZQuery(EDIMessageSchema.EM_MessageNum, messageNumber);
				messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
				messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification);
				messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationReference, applicationReference);
				messageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, null);
				messageQuery.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc;

				var message = factory.LoadTop1<EDIMessage>(messageQuery);
				if (message != null)
				{
					message.EM_LinkedObject = linkObj;
				}
			}
		}

		ZString GetMessageNumber() => ((IXmlEventValueObject)eventDataObject).Context.InternalTransactionNumber;

		const string MessageTypeDesc = "Customs eBond Status Notification";

		string GetEmailBody(IXmlEventValueObject xmleventDataObject)
		{
			var result = string.Empty;
			if (xmleventDataObject.Context.NotificationDetails.HasValue)
			{
				result = xmleventDataObject.Context.NotificationDetails.Value;
			}
			return result;
		}

		void GenerateHtmlEmailAndSendToBrokerOrGroup(string uri, string jobNumber, string additionalSubject, string messageTypeInSubject, string body, GlbBranch branch, string[] emailAddressToSendTo)
		{
			EmailDef email;
			new HtmlResponseEmailGenerator().TryGenerateEmail(uri, jobNumber, messageTypeInSubject, body, false, out email, branch);
			email.Subject = email.Subject + " " + additionalSubject;
			SendEmailToBrokerOrGroupIfSenderInvalid(email, branch, emailAddressToSendTo);
		}

		void SendEmailToBrokerOrGroupIfSenderInvalid(EmailDef email, GlbBranch branch, string[] emailAddressToSendTo)
		{
			if (email != null)
			{
				using (branch.SetAsTemporaryContext())
				{
					if (emailAddressToSendTo.Length > 0)
					{
						email.AddRecipientForSystemCommunication(emailAddressToSendTo);
					}
					if (email.Recipients.Count > 0)
					{
						Env.OutgoingCustomsMailManager.Create(factory, email);
					}
					else
					{
						var bondStatusNotificationGroupPk = USCustomsDataRegistry.Instance.BondStatusNotificationGroup.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
						if (bondStatusNotificationGroupPk != ZGuid.Empty)
						{
							Env.OutgoingCustomsMailManager.Create(factory, email, bondStatusNotificationGroupPk, GroupSourceLocator.GetFromRegistryItem(USCustomsDataRegistry.Instance.BondStatusNotificationGroup));
						}
					}
				}
			}
		}
	}
}
