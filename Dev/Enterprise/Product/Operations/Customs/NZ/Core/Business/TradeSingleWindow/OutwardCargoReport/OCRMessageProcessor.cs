using System;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	internal class OCRMessageProcessor : TSWMessageProcessor<OCRResponse>
	{
		public OCRMessageProcessor(LoggingInformation logger)
			: base(logger, MessageTypeList.Descriptions.OCR)
		{
		}

		#region Overrides

		protected override void ProcessCore()
		{
			var jobName = Response.IsLinkedToManifestHeader ? "Manifest" : Response.JobName;
			Report.AddLine(jobName + " Number", Response.JobID);
			Report.AddLine("Outward Report No", Response.DeclarationID);
			Report.AddLine("Master Bill", Response.SendingBusinessObject.MasterBillNumber);
			Report.AddLine();
			Report.AddLine("Rsp Message No", Response.MessageNumber);
			Report.AddLine("Message Type", Response.MessageTypeDescription);
			OutputResponseStatus();
			ProcessCustomsInstructions();
			OutputErrors();
			AddOCREvents();
		}

		protected override string[] ExpectedDODocumentNames
		{
			get { return new string[] { "OCR_Delivery_Order" }; }
		}

		protected override ZGuid DocumentParentPK => Response.SendingBusinessObject?.Identifier ?? ZGuid.Empty;

		protected override ZString DocumentParentType => Response.SendingBusinessObject?.DocumentParentType ?? ZString.Empty;

		protected override void OutputCustomsInstruction(string instruction)
		{
			string hwb;
			var itemMatch = Regex.Match(instruction, @"ITEM (?<item>[0-9]+?) -");
			if (itemMatch.Success && FindHWBByItem(itemMatch.Groups["item"].Value, Response.OutgoingMessage, out hwb))
			{
				Report.AddLine("HWB " + hwb, instruction);
			}
			else
			{
				base.OutputCustomsInstruction(instruction);
			}
		}

		protected override void OutputError(TSWResponse.ValueWithPointer error)
		{
			Report.AddLine("--Error Found", error.Value);
		}

		protected override void OutputResponseStatus()
		{
			Report.AddLine("TSW Status Code", TSWProcessingStatus);
			Report.AddLine("Status", Response.EnterpriseStatusDescription);
		}

		string TSWProcessingStatus
		{
			get
			{
				var result = Response.Status;
				switch (Response.Status)
				{
					case StatusList.Codes.EntryHeldInstructionsAsSpecified:
						result = "805 - Instructions as specified";
						break;
					case StatusList.Codes.AdjustmentAccepted:
						result = "830 - Adjustment Accepted";
						break;
					case StatusList.Codes.EciOutwardReportRejectedErrorReportAttached:
						result = "841 - OCR Rejected, error report herewith";
						break;
					case StatusList.Codes.ExportGoodsClearedPortNotification:
						result = "846 - Export Clearance Port Notification **NOTE: users will need to continue providing manual delivery orders to Port Authorities till a fix is made in JBMS.";
						break;
					case StatusList.Codes.OutwardReportAccepted:
						result = "847 - OCR Received OK";
						break;
					case StatusList.Codes.Acknowledgement:
						result = "ACK - Acknowledgement";
						break;
					default:
						break;
				}

				return result;
			}
		}

		protected override void SendEmail(EmailDef email)
		{
			switch (Response.EnterpriseStatus)
			{
				case OutwardReportStatusList.Codes.Cleared:
					SendAcknowledgementReport(Response.SendingBusinessObject, email);
					break;
				case OutwardReportStatusList.Codes.CustomsInstuctionReceived:
					SendImpedimentReport(Response.SendingBusinessObject, email);
					break;
				default:
					SendErrorReport(Response.SendingBusinessObject, email);
					break;
			}
		}

		protected override void WriteEntryStatus()
		{
			var entryNumber = Response.SendingBusinessObject.LoadAndCreateRegistrationNumber();
			if (ResponseReceivedIsNewerThanPreviousResponses(entryNumber.CE_IssueDate) || HasBeenQueuedForReProcessing() || IsClearedResponse)
			{
				entryNumber.CE_EntryNum = Response.DeclarationID;
				entryNumber.CE_EntryStatus = Response.EnterpriseStatus;
				entryNumber.CE_IssueDate = Response.ResponseTime;
			}
		}

		protected override void WriteMessageStatus()
		{
			var manifestHeader = Response.SendingBusinessObject as IAsycudaManifestHeader;
			if (manifestHeader != null)
			{
				switch (Response.EnterpriseStatus)
				{
					case OutwardReportStatusList.Codes.Acknowledgement:
						manifestHeader.AMA_MessageStatus = NZMessageStatusList.Codes.Acknowledged;
						break;
					default:
						manifestHeader.AMA_MessageStatus = NZMessageStatusList.Codes.Accepted;
						break;
				}
			}
		}

		bool ResponseReceivedIsNewerThanPreviousResponses(ZDateTime issueDate)
		{
			return issueDate.IsEmpty || Response.ResponseTime > issueDate;
		}

		bool IsClearedResponse => Response.EnterpriseStatus == OutwardReportStatusList.Codes.Cleared;

		#region Email Addresses

		protected override ZGuid AcknowledgementEmailGroup
		{
			get
			{
				return NZCustomsDataRegistry.Instance.ExportDeclarationsSendAcknowledgementsToGroup.Value;
			}
		}

		protected override ZString AcknowledgementEmailMode
		{
			get
			{
				return NZCustomsDataRegistry.Instance.ExportDeclarationsSendAcknowledgements.Value;
			}
		}

		protected override ZGuid ErrorEmailGroup
		{
			get
			{
				return NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrorsToGroup.Value;
			}
		}

		protected override ZString ErrorEmailMode
		{
			get
			{
				return NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrors.Value;
			}
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get
			{
				return NZCustomsDataRegistry.Instance.ExportDeclarationsSendImpedimentsToGroup.Value;
			}
		}

		protected override ZString ImpedimentEmailMode
		{
			get
			{
				return NZCustomsDataRegistry.Instance.ExportDeclarationsSendImpediments.Value;
			}
		}

		#endregion // Email Addresses

		#endregion // Overrides

		#region Implementation

		static bool FindHWBByItem(string itemNumber, TSWMessage outgoingMessage, out string hwb)
		{
			throw new NotImplementedException();
		}

		#endregion // Implementation

		void AddOCREvents()
		{
			Response.SendingBusinessObject?.Logs.AddNew(Events.MessageReceived, Response.GetEventReference(), ZDateTimeOffset.Now);
		}
	}
}
