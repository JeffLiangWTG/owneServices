using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US.ISF;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	public class CusISFHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		public static class ReplacementConstants
		{
			public const string CBPResponse = "(*cbpresponse*)";
			public const string CustomsReference = "(*customsreference*)";
			public const string ImporterOfRecord = "(*impname*)";
			public const string ImporterOfRecordCode = "(*impcode*)";
			public const string LastABIStatus = "(*lastabistatus*)";
			public const string LastestMessageStatus = "(*latestmessagestatus*)";
			public const string MessageStatuses = "(*messagestatuses*)";
			public const string BillMatchedDate = "(*billmatcheddate*)";
			public const string ReferenceID = "(*refid*)";
			public const string ReferenceIDs = "(*referenceids*)";
			public const string ValidationMessage = "(*validationmessage*)";
			public const string FirstAcceptedDate = "(*firstaccepteddate*)";
			public const string LastAcceptedDate = "(*lastaccepteddate*)";
		}

		#region ID / Description / ControllerID

		public override string Code
		{
			get { return JobInvoicingConsumerTypes.ImporterSecurityFiling.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.ImporterSecurityFiling.MultilingualDescription; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.ImporterSecurityFiling; }
		}

		public override ZString MilestoneTemplateHintCaption
		{
			get { return ""; }
		}

		#endregion

		#region Criteria Requirements

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return false; }
		}

		public override bool RequiresPort1
		{
			get { return false; }
		}

		public override bool RequiresPort2
		{
			get { return false; }
		}

		#endregion

		#region Workflow Trigger

		/// <summary>
		/// Provides action types that are specific to CusISFHeaderWorkflowDescriptor and available for any ProcessTask
		/// </summary>
		/// <param name="task">The ProcessTask is ignored</param>
		/// <returns>The list of available action types</returns>
		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendXML, "Send XML Document");
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendISFMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendISFMessage);
			return result;
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				JobConsolTransportSchema.JW_RL_NKLoadPort,
				JobConsolTransportSchema.JW_RL_NKDiscPort,
				JobConsolTransportSchema.JW_Vessel,
				JobConsolTransportSchema.JW_VoyageFlight,
				JobConsolTransportSchema.JW_ETD,
				JobConsolTransportSchema.JW_ETA,
				JobConsolTransportSchema.JW_ATD,
				JobConsolTransportSchema.JW_ATA,
				CusISFHeaderSchema.BF_CustomsStatus,
				CusISFBillSchema.BB_CustomsStatus
			};
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.OrgProxy;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var header = (CusISFHeader)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(header.Importer, ZString.Empty));
			}
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog log)
		{
			var action = source.Action;
			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendXML)
			{
				EventsWithSourceType isfTriggeredByEvents = !log.ChangeLogs.Any() ? new EventsWithSourceType(EventsWithSourceType.SourceType.ImporterSecurityFiling, action, source.Job) : EventsWithSourceType.Empty;
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				CusISFHeader header = (CusISFHeader)source.Job;
				XmlMessageDeliver delivery = new XmlMessageDeliver(xmlModes, header, new ImporterSecurityFilingDataAdapter(isfTriggeredByEvents), action);
				delivery.ExtraDataSubstitution = ExtraDataSubstitution;
				return delivery;
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendISFMessage)
			{
				if (source.Job is ICusISFAutoSendingMessageSupporter supporter)
				{
					return supporter.CreateStmProcessQueueProcessor(null, action.PQ_TriggerType);
				}

				return null;
			}
			return base.GetWorkflowTriggerActionCore(source, log);
		}

		protected override WorkflowTriggerNotification GetWorkflowTriggerForNotificationEmail(Lazy<MessageProcessorCommunicationModesResult> modes, ProcessTaskNotification action, BusinessObject parent, Lazy<IStmALog> logProvider)
		{
			WorkflowTriggerNotification workflowTriggerNotification = base.GetWorkflowTriggerForNotificationEmail(modes, action, parent, logProvider);
			workflowTriggerNotification.ExtraDataSubstitution = ExtraDataSubstitution;
			return workflowTriggerNotification;
		}

		ZString ExtraDataSubstitution(ProcessTaskNotification action, BusinessObject parent, ZString data)
		{
			ZString result = data;
			CusISFHeader bizObj = parent as CusISFHeader;
			if (bizObj != null)
			{
				result = result.ReplaceIgnoringCase(ReplacementConstants.FirstAcceptedDate, bizObj.BF_FirstAcceptedDate.ToShortDateString().ToUpper(CultureInfo.CurrentCulture));
				result = result.ReplaceIgnoringCase(ReplacementConstants.LastAcceptedDate, bizObj.BF_LastAcceptedDate.ToShortDateString().ToUpper(CultureInfo.CurrentCulture));
				result = result.ReplaceIgnoringCase(ReplacementConstants.LastestMessageStatus, bizObj.BF_CustomsStatusDescription);
				result = result.ReplaceIgnoringCase(ReplacementConstants.CustomsReference, bizObj.BF_CustomsReference);
				ZString referenceID = ZString.Empty;
				ZDateTime billMatchedDate = ZDateTime.Empty;
				CusISFBill bill = bizObj.BF_OceanBill.IsEmpty ? bizObj.HouseBill : bizObj.OceanBill;
				if (bill != null)
				{
					referenceID = bill.BB_BillNum;
					billMatchedDate = bill.BB_MatchDate;
				}
				result = result.ReplaceIgnoringCase(ReplacementConstants.ReferenceID, referenceID);
				result = result.ReplaceIgnoringCase(ReplacementConstants.BillMatchedDate, billMatchedDate.ToShortDateString().ToUpper(CultureInfo.CurrentCulture));
				ZString importerOfRecord = ZString.Empty;
				ZString importerOfRecordCode = ZString.Empty;
				OrgHeader importer = bizObj.Importer;
				if (importer != null)
				{
					importerOfRecord = importer.OH_FullNameTruncated;
					importerOfRecordCode = importer.OH_Code;
				}
				result = result.ReplaceIgnoringCase(ReplacementConstants.ImporterOfRecord, importerOfRecord);
				result = result.ReplaceIgnoringCase(ReplacementConstants.ImporterOfRecordCode, importerOfRecordCode);
				if (data.Contains(ReplacementConstants.ReferenceIDs, StringComparison.OrdinalIgnoreCase))
				{
					result = GetDataWithReferenceIDs(bizObj, result);
				}

				if (data.Contains(ReplacementConstants.ValidationMessage, StringComparison.OrdinalIgnoreCase))
				{
					result = GetDataWithValidationMessage(bizObj, result);
				}

				if (data.Contains(ReplacementConstants.MessageStatuses, StringComparison.OrdinalIgnoreCase))
				{
					result = GetDataWithMessageStatuses(bizObj, result);
				}

				if (data.Contains(ReplacementConstants.CBPResponse, StringComparison.OrdinalIgnoreCase))
				{
					result = GetDataWithCBPResponse(bizObj, result);
				}

				if (data.Contains(ReplacementConstants.LastABIStatus, StringComparison.OrdinalIgnoreCase))
				{
					result = GetDataWithLastABIStatus(bizObj, result);
				}
			}
			return result;
		}

		ZString GetDataWithLastABIStatus(CusISFHeader bizObj, ZString data)
		{
			ZString result = ZString.Empty;
			LastestCustomsResponseParser parser = new LastestCustomsResponseParser(bizObj);
			if (parser.LastestResponse != null)
			{
				ISFMessageParser messageParser = new ISFMessageParser(bizObj, parser.LastestResponse.GetMessageBlocks<MessageBlock>().ToArray());
				result = messageParser.HtmlMessageDataOnly;
			}
			return data.ReplaceIgnoringCase(ReplacementConstants.LastABIStatus, result);
		}

		ZString GetDataWithCBPResponse(CusISFHeader bizObj, ZString data)
		{
			ZString result = ZString.Empty;
			LastestCustomsResponseParser parser = new LastestCustomsResponseParser(bizObj);
			if (parser.LastestResponse != null)
			{
				HtmlTableCreator cbpResponseHtmlCreator = new HtmlTableCreator(new string[] { "CBP Response" });
				cbpResponseHtmlCreator.EnableHTMLEncoding = false;
				cbpResponseHtmlCreator.WriteRow(parser.LastestResponse.EM_MessageInterpretation.Replace("\r\n", "<BR />").Replace("\n", "<BR />"));
				result = cbpResponseHtmlCreator.ToHtml();
			}
			return data.ReplaceIgnoringCase(ReplacementConstants.CBPResponse, result);
		}

		ZString GetDataWithMessageStatuses(CusISFHeader bizObj, ZString data)
		{
			ZString result = ZString.Empty;
			ZQuery filter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, US.Business.EDIMessage.ApplicationCodes.USCustomsImport);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, US.Business.EDIMessage.Direction.Receive);
			filter.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc;
			BusinessObject[] responseMessages = bizObj.Messages.Find(filter);
			if (responseMessages.Length > 0)
			{
				HtmlTableCreator messageStatusHtmlCreator = new HtmlTableCreator(new string[] { "Message Status", "Received" });
				MessageStatusList list = bizObj.Factory.GetCachedValue<MessageStatusList>();
				foreach (US.Business.MQEDIMessage message in responseMessages)
				{
					CustomsResponseParser parser = new CustomsResponseParser(message);
					messageStatusHtmlCreator.WriteRow(list.GetDescriptionFromCode(MessageStatusList.GetCodeFrom(message.EM_MessageSubType, parser.MessageTypeCode)), message.EM_SystemCreateTimeUtc);
				}
				result = messageStatusHtmlCreator.ToHtml();
			}
			return data.ReplaceIgnoringCase(ReplacementConstants.MessageStatuses, result);
		}

		ZString GetDataWithValidationMessage(CusISFHeader bizObj, ZString data)
		{
			ZString result = ZString.Empty;
			bizObj.LoadChildEditableObjects();
			bizObj.RunPreSaveValidation();
			ZNotificationCollector collector = new ZNotificationCollector(bizObj, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
			List<string> list = new List<string>(collector.GetUniqueMessageList());
			if (list.Count > 0)
			{
				HtmlTableCreator validationHtmlCreator = new HtmlTableCreator(new string[] { "Validation Message(s)" });
				list.Sort();
				list.ForEach((string message) => validationHtmlCreator.WriteRow(message));
				result = validationHtmlCreator.ToHtml();
			}
			return data.ReplaceIgnoringCase(ReplacementConstants.ValidationMessage, result);
		}

		ZString GetDataWithReferenceIDs(CusISFHeader bizObj, ZString data)
		{
			ZString result = ZString.Empty;
			HtmlTableCreator billOfLadingHtmlCreator = new HtmlTableCreator(new string[] { "Bill Of Lading", "Status", "Matched Date" });
			bool hasBill = false;
			foreach (CusISFBill bill in bizObj.ReferenceDatas)
			{
				if (bill.IsHouseBillOfLading || bill.IsOceanBillOfLading)
				{
					hasBill = true;
					billOfLadingHtmlCreator.WriteRow(bill.BB_BillNum, bill.BB_CustomsStatus.IsEmpty ? "" : bill.BB_CustomsStatus + " - " + bill.BB_CustomsStatusDescription, bill.BB_MatchDate.ToShortDateString().ToUpper(CultureInfo.CurrentCulture));
				}
			}
			if (hasBill)
			{
				result = billOfLadingHtmlCreator.ToHtml();
			}
			return data.ReplaceIgnoringCase(ReplacementConstants.ReferenceIDs, result);
		}

		protected override MessageProcessorCommunicationModesResult GetMessageRecipientEdiCommunicationsModesForEmailParty(WorkflowTriggerActionSource source)
		{
			var action = source.Action;
			var parent = source.Job;
			var mode = new NonPersistentEDICommunicationMode()
			{
				EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText,
				EK_Destination = TriggerActionCommunicationModeSubstitutor.Substitute(action, parent, source.Event, CommunicationModeSubstitutorProperty.EmailAddress, action.PQ_EmailAddr),
				EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail,
				EK_ServerAddressSubject = Description + " - " + action.Parent.GetDescriptionWithReference() + " " + EDIMessageDelivery.ReplacementConstants.JobNumber
			};
			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendXML)
			{
				mode.EK_Filename = EDIMessageDelivery.ReplacementConstants.JobNumber + "_Response.xml";
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			}
			return new MessageProcessorCommunicationModesResult(new[] { mode }, null);
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.CusISFHeader }; }
		}

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
		}

		#endregion

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(CusISFHeader); }
		}
	}
}
