using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.Business.CustomsMessaging;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class MessageManager : IMessageManager
	{
		protected MessageManager(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public bool SendOriginalMessages(Customs.Business.ISendsMessagesToCustoms sender)
		{
			var result = CheckDeniedParty(declaration);
			if (result)
			{
				var valuationDate = ZDateTime.Today;
				foreach (JobComInvoiceHeader invoiceHeader in declaration.Invoices)
				{
					if (invoiceHeader.JZ_ValuationDateOverride.IsEmpty)
					{
						invoiceHeader.JZ_ValuationDateOverride = valuationDate;
					}
				}

				result = declaration.DoMerge();
			}

			if (result)
			{
				result = SendMessageCore(sender, CUSDECEDIMessage.Declaration, Core.SGConstants.DeclarationStatus.DeclarationPending, (f, e) => f.GetOriginalMessage(GetCustomsDec(e)));
			}

			return result;
		}

		public bool SendAmendmentMessages(Customs.Business.ISendsMessagesToCustoms sender)
		{
			return declaration.DoMerge() && SendMessageCore(sender, CUSDECEDIMessage.Amendment, Core.SGConstants.DeclarationStatus.AmendmentPending, (f, e) => f.GetAmendmentMessage(GetCustomsDec(e)));
		}

		public bool SendRefundMessage(Customs.Business.ISendsMessagesToCustoms sender)
		{
			return SendMessageCore(sender, CUSDECEDIMessage.Refund, Core.SGConstants.DeclarationStatus.RefundPending, (f, e) => f.GetRefundMessage(GetCustomsDec(e)));
		}

		public bool SendCancellationMessages(Customs.Business.ISendsMessagesToCustoms sender)
		{
			return SendMessageCore(sender, CUSDECEDIMessage.Amendment, Core.SGConstants.DeclarationStatus.CancellationPending, (f, e) => f.GetCancellationMessage(GetCustomsDec(e)));
		}

		protected bool CheckDeniedParty(BusinessObject master)
		{
			return MessageManagerCreditCheckWithSecurityHelper.CheckDeniedParty(master as BaseJobDeclaration);
		}

		bool SaveDeclaration()
		{
			try
			{
				declaration.Factory.Save();
				return true;
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
				return false;
			}
		}

		#region Send Message Core

		bool SendMessageCore(Customs.Business.ISendsMessagesToCustoms sender, string messageSubType, string messageStatusCode, Func<IMessageFactory, CusEntryHeader, ICusMessage> getMessageFunc)
		{
			var result = true;

			var entryAndMessageList = new List<(CusEntryHeader EntryHeader, ICusMessage CusMessage)>();
			var messageFactory = GetMessageFactory();

			result = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().All(c =>
			{
				declaration.JE_GS_NKCusAgent = c.AdditionalMessageInformation.Broker;

				var cusMessage = getMessageFunc?.Invoke(messageFactory, c);
				entryAndMessageList.Add((c, cusMessage));

				return CheckTotalSize(c, cusMessage, sender);
			});

			if (result)
			{
				foreach (var entryAndMessage in entryAndMessageList)
				{
					CreateMessageAndUpdateStatus(entryAndMessage.CusMessage, entryAndMessage.EntryHeader, messageStatusCode, messageFactory.MessageType);
				}

				result = SaveDeclaration();
			}

			return result;
		}

		bool CheckTotalSize(CusEntryHeader entryHeader, ICusMessage message, Customs.Business.ISendsMessagesToCustoms sender)
		{
			var result = true;

			var totalSize = 0m;

			if (entryHeader.AdditionalMessageInformation is AdditionalMessageInformation additionalMessageInformation)
			{
				foreach (ICusAttachment cusAttachment in additionalMessageInformation.SupportingDocuments)
				{
					totalSize += additionalMessageInformation.GetDocSizeInMB(cusAttachment.UniqueIdentifier);
				}
			}

			totalSize += (string.IsNullOrWhiteSpace(message?.MessageText) ? 0 : System.Text.Encoding.Default.GetBytes(message.MessageText).Length / 1024 / 1024);

			var maximumSize = SGCustomsDataRegistry.Instance.MaximumMessageSize.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

			if (totalSize > maximumSize)
			{
				sender.NotifyUserOfAnInvalidOperation
				(
					Res.GetString("A6D93552-9128-40B2-98BC-A802DD0DFA3C",
					"The size of the message to Customs is greater than the Maximum Message Size - {0}(MB). Please review these attachments and reduce the size of the files being submitted. You can also change the Maximum Message Size at {1}",
					maximumSize,
					SGCustomsDataRegistry.Instance.MaximumMessageSize.Location())
				);

				result = false;
			}

			return result;
		}

		void CreateMessageAndUpdateStatus(ICusMessage cusMessage, CusEntryHeader cusEntryHeader, string messageStatusCode, MessageType messageType)
		{
			if (cusMessage != null)
			{
				if (messageType == MessageType.XML)
				{
					CreateXmlMessage(cusEntryHeader, cusMessage);
				}
				else
				{
					CreateMessage(cusEntryHeader, cusMessage);
				}

				cusEntryHeader.CH_Status = messageStatusCode;
				declaration.JE_EntrySubmittedDate = ZDateTime.Now;
			}
		}

		IMessageFactory GetMessageFactory()
		{
			if (declaration.IsXMLTradeNetMessageEnabled)
			{
				return tradenetDeclarationMessageFactory;
			}
			else
			{
				return declaration.IsTradeNet4Point1 ? edifactD09BMessageFactory : edifactD05BMessageFactory;
			}
		}

		readonly IMessageFactory edifactD05BMessageFactory = new Sg05bEdifactMessageFactory();
		readonly IMessageFactory edifactD09BMessageFactory = new Sg09bEdifactMessageFactory();
		readonly IMessageFactory tradenetDeclarationMessageFactory = new TradenetDeclarationMessageFactory();

		#endregion

		void CreateMessage(CusEntryHeader cusEntryHeader, ICusMessage cusMessage)
		{
			var isXmlVersion = cusMessage is ITradeNetMessage;

			var ediMessage = isXmlVersion
				? (EDIMessage)cusEntryHeader.Factory.New<SGXmlEDIMessage>()
				: cusEntryHeader.Factory.New<CUSDECEDIMessage>();

			ediMessage.EM_Status = CUSDECEDIMessage.Status.Queued;
			ediMessage.EM_MessageType = ((ZString)cusMessage.MessageType).Left(3);
			ediMessage.EM_MessageSubType = cusMessage.MessageSubType;
			ediMessage.EM_MessageText = cusMessage.MessageText;
			ediMessage.EM_IsTestMessage = cusEntryHeader.IsTestMessage;
			ediMessage.EM_SendWithMessageErrors = cusEntryHeader.Declaration.HasMessageErrors;

			if (!isXmlVersion)
			{
				ediMessage.EM_ApplicationCode = MessageApplicationCode;
			}

			cusEntryHeader.Messages.Add(ediMessage);

			if (cusEntryHeader.AdditionalMessageInformation is IAdditionalMessageInformation additionalMessageInformation)
			{
				foreach (var cusAttachment in cusEntryHeader.AdditionalMessageInformation.SupportingDocuments)
				{
					var ediMessageAttach = ediMessage.MessageAttachments.AddNew();
					ediMessageAttach.EG_FileName = cusAttachment.FileName.ToASCII();
					ediMessageAttach.EG_StorageDocsGuid = cusAttachment.UniqueIdentifier;
				}

				ediMessage.EM_MessageText = additionalMessageInformation.MessageCreated(ediMessage.EM_MessageText);
			}
		}

		void CreateXmlMessage(CusEntryHeader cusEntryHeader, ICusMessage cusMessage)
		{
			var ediMessage = cusEntryHeader.Factory.New<SGXmlEDIMessage>();
			ediMessage.EM_Status = CUSDECEDIMessage.Status.Queued;
			ediMessage.EM_MessageType = ((ZString)cusMessage.MessageType).Left(3);
			ediMessage.EM_MessageSubType = cusMessage.MessageSubType;
			ediMessage.EM_MessageText = cusMessage.MessageText;
			ediMessage.EM_IsTestMessage = cusEntryHeader.IsTestMessage;
			ediMessage.EM_SendWithMessageErrors = cusEntryHeader.Declaration.HasMessageErrors;
			cusEntryHeader.Messages.Add(ediMessage);

			if (cusEntryHeader.AdditionalMessageInformation is IAdditionalMessageInformation additionalMessageInformation)
			{
				foreach (ICusAttachment cusAttachment in additionalMessageInformation.SupportingDocuments)
				{
					var ediMessageAttach = ediMessage.MessageAttachments.AddNew();
					ediMessageAttach.EG_FileName = cusAttachment.FileName.ToASCII();
					ediMessageAttach.EG_StorageDocsGuid = cusAttachment.UniqueIdentifier;
				}

				ediMessage.EM_MessageText = additionalMessageInformation.MessageCreated(ediMessage.EM_MessageText);
			}
		}

		ICustomsDec GetCustomsDec(CusEntryHeader cusEntryHeader)
		{
			return cusEntryHeader;
		}

		protected abstract ZString MessageApplicationCode { get; }
	}
}
