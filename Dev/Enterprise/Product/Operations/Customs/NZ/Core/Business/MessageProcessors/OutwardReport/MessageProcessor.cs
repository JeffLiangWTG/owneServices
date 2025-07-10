using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Edifact;
using Enterprise.Edifact.D03A.Messages.CUSRES;
using Enterprise.Edifact.D03A.Segments;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using CUSCAR = Enterprise.Edifact.D03A.Messages;
using EntryNumber = Enterprise.Customs.NZ.Business.Declaration.OutwardReport.CusEntryNumber;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.OutwardReport
{
	public class MessageProcessor : Messaging.MessageProcessors.CustomsMessageProcessor
	{
		public MessageProcessor(LoggingInformation logger)
			: base(logger, NZCMessage.MessageTypes.ResponseMsg, "Outward Report Response Message")
		{
		}

		#region Interface

		#endregion

		#region Information in the response message

		protected BusinessObjectFactory factory;
		protected string consolID;
		protected ForwardingConsol consol;
		protected EDIMessage lastOutgoingMessage;
		protected string responseType;
		protected string oRN;
		protected StringBuilder customsInstuctions;
		protected string oRNStatus;
		protected string subject;
		protected string messageNumber;
		protected StringBuilder headerErrors;
		protected StringBuilder lineErrors;

		#endregion

		#region Process

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			bool success = false;

			factory = message.Factory;
			CUSRESMessage cUSRES = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(NzEdifactMessageFactory.NZCMessageFactory, new UNOACharacterSet());

			try
			{
				ProcessUngrouped(cUSRES);
				ProcessGroup4(cUSRES);
				ProcessGroup6(cUSRES);
				success = true;
				SetORNAndStatus();
				SendEmail(message as NZCMessage);
			}
			catch (MessageProcessingException exception)
			{
				success = false;
				Logger.Log("Error happened while processing an ORN response message, " + exception.Message);
			}

			return (success) ? EDIMessage.Status.Received : EDIMessage.Status.Error;
		}

		protected void ProcessUngrouped(CUSRESMessage cUSRES)
		{
			ProcessUNH(cUSRES);
			ProcessBGM(cUSRES);
			ProcessFTX(cUSRES);
			ProcessGEI(cUSRES);
		}

		protected void ProcessUNH(CUSRESMessage cUSRES)
		{
			if (cUSRES == null)
			{
				throw new MessageProcessingException("There is no UNH segment in CUSRES message.");
			}

			UNHSegment uNH = cUSRES.UNH[0];
			messageNumber = uNH.MessageReferenceNumber;
			consolID = uNH.CommonAccessReference;
			consol = (ForwardingConsol)factory.LoadFromNaturalKey(typeof(ForwardingConsol), JobConsolSchema.JK_UniqueConsignRef, consolID);
			if (consol == null)
			{
				throw new MessageProcessingException("Consol cannot be found with this reference number, " + consolID);
			}
			lastOutgoingMessage = consol.Messages.GetLastMessage(EDIInterchange.ApplicationCodes.NewZealandCustoms, OutwardReportMessage.MessageTypes.OutwardReport.MessageType, "TRX");
		}

		protected void ProcessBGM(CUSRESMessage cUSRES)
		{
			if (cUSRES.BGM.Count == 0)
			{
				throw new MessageProcessingException("There is no BGM segment in CUSRES message.");
			}

			BGMSegment bGM = cUSRES.BGM[0];

			switch (bGM.DocumentMessageName.DocumentNameCode)
			{
				case "932":
					responseType = "Acceptance Notice";
					break;
				case "963":
					responseType = "Rejection Notice";
					break;
				case "962":
					responseType = "Inspection/Audit Requirements";
					break;
				case "965":
					responseType = "Confirmation of Adjustment";
					break;
			}

			oRN = bGM.DocumentMessageIdentification.DocumentIdentifier;
		}

		protected void ProcessFTX(CUSRESMessage cUSRES)
		{
			customsInstuctions = new StringBuilder(cUSRES.FTX.Count);
			ZString pattern = "ITEM";

			foreach (FTXSegment fTX in cUSRES.FTX)
			{
				ZString freeText = fTX.TextLiteral.FreeText1;

				string itemNumber = "";
				if (freeText.Occurrences(pattern) > 0)
				{
					itemNumber = freeText.SubstringSafe(freeText.IndexOf(pattern)).KeepCharsUntil("0123456789", new char[] { '-' }).Replace(" ", "");
				}

				if (!string.IsNullOrEmpty(itemNumber))
				{
					customsInstuctions.Append("HWB " + GetHAWBFromItemNumber(itemNumber));
					customsInstuctions.Append("\t: ");
				}
				customsInstuctions.Append(fTX.TextLiteral.FreeText1);
			}
		}

		protected void ProcessGEI(CUSRESMessage cUSRES)
		{
			if (cUSRES.GEI.Count == 0)
			{
				throw new MessageProcessingException("There is no GEI segment in CUSRES message.");
			}

			GEISegment gEI = cUSRES.GEI[0];

			string code = gEI.ProcessingIndicator.ProcessingIndicatorDescriptionCode;
			switch (code)
			{
				case "830":
					if (lastOutgoingMessage != null && lastOutgoingMessage.EM_MessageSubType == OutwardReportMessage.MessageTypes.OutwardReport.MessageSubTypes.Cancellation)
					{
						oRNStatus = OutwardReportStatusList.Codes.Cancelled;
						subject = "Outward Report Cancelled";
					}
					else
					{
						oRNStatus = OutwardReportStatusList.Codes.Cleared;
						subject = "Outward Report Accepted";
					}
					break;
				case "847":
					oRNStatus = OutwardReportStatusList.Codes.Cleared;
					subject = "Outward Report Accepted";
					break;
				case "805":
					oRNStatus = OutwardReportStatusList.Codes.CustomsInstuctionReceived;
					subject = "Customs Instuctions as specified";
					break;
				case "841":
					oRNStatus = OutwardReportStatusList.Codes.Rejected;
					subject = "Outward Report Rejected";
					break;
			}
		}

		protected void ProcessGroup4(CUSRESMessage cUSRES)
		{
			headerErrors = new StringBuilder(cUSRES.Group4.Count);
			foreach (SegmentGroup4 group4 in cUSRES.Group4)
			{
				ProcessGroup4ERP_ERC(group4);
			}
		}

		protected void ProcessGroup4ERP_ERC(SegmentGroup4 group4)
		{
			if (group4.ERC.Count == 0)
			{
				throw new MessageProcessingException("There is no ERC segment in SG4 of CUSRES message.");
			}

			ERPSegment eRP = group4.ERP[0];
			ERCSegment eRC = group4.ERC[0];
			string humanReadableFieldNo = GetFieldDescFromNumber(eRP.ErrorPointDetails.MessageItemIdentifier);
			string humanReadableErrorNo = GetErrorDescFromCode(eRC.ApplicationErrorDetail.ApplicationErrorCode);

			headerErrors.Append("--Error Found\t: " + humanReadableErrorNo + "\r\n");
		}

		protected void ProcessGroup6(CUSRESMessage cUSRES)
		{
			lineErrors = new StringBuilder(cUSRES.Group6.Count);
			foreach (SegmentGroup6 group6 in cUSRES.Group6)
			{
				ProcessGroup6DOC_ERP_ERC(group6);
			}
		}

		protected void ProcessGroup6DOC_ERP_ERC(SegmentGroup6 group6)
		{
			if (group6.Group14.Count == 0)
			{
				throw new MessageProcessingException("There is no Group14 segment in SG6 of CUSRES message.");
			}

			DOCSegment dOC = group6.DOC[0];
			string hAWB = GetHAWBFromItemNumber(dOC.DocumentMessageDetails.DocumentIdentifier);

			SegmentGroup14 group14 = group6.Group14[0];

			if (group14.ERC.Count == 0)
			{
				throw new MessageProcessingException("There is no ERC segment in SG14 of CUSRES message.");
			}

			ERPSegment eRP = group14.ERP[0];
			ERCSegment eRC = group14.ERC[0];

			string humanReadableFieldNo = GetFieldDescFromNumber(eRP.ErrorPointDetails.MessageItemIdentifier);
			string humanReadableErrorNo = GetErrorDescFromCode(eRC.ApplicationErrorDetail.ApplicationErrorCode);

			lineErrors.Append("HWB " + hAWB + "\r\n");
			lineErrors.Append("--Error Found\t: " + humanReadableErrorNo + "\r\n");
		}

		protected string GetHAWBFromItemNumber(string itemNumber)
		{
			if (LastOutgoingCUSCAR != null)
			{
				foreach (CUSCAR.CUSCAR.SegmentGroup7 group7 in LastOutgoingCUSCAR.Group7)
				{
					if (group7.CNI[0].ConsolidationItemNumber == itemNumber)
					{
						return group7.Group8[0].RFF[0].Reference.ReferenceIdentifier;
					}
				}
			}
			return string.Empty;
		}

		CUSCAR.CUSCAR.CUSCARMessage fLastOutgoingCUSCAR;
		protected CUSCAR.CUSCAR.CUSCARMessage LastOutgoingCUSCAR
		{
			get
			{
				if (fLastOutgoingCUSCAR == null)
				{
					EDIMessage lastOutgoingORN = consol.Messages.GetLastMessage(EDIMessage.ApplicationCodes.NewZealandCustoms, OutwardReportMessage.MessageTypes.OutwardReport.MessageType, EDIMessage.Direction.Transmit);
					if (lastOutgoingORN != null)
					{
						fLastOutgoingCUSCAR = (CUSCAR.CUSCAR.CUSCARMessage)lastOutgoingORN.GetAutoEdifactMessageUsingNamedFactory(NzEdifactMessageFactory.NZCMessageFactory, new UNOACharacterSet());
					}
				}
				return fLastOutgoingCUSCAR;
			}
		}

		protected void SetORNAndStatus()
		{
			var entryNumber = consol.Factory.LoadTop1<EntryNumber>(EntryNumber.GetEntryNumberFilter(consol));
			if (entryNumber == null)
			{
				entryNumber = (EntryNumber)consol.Factory.New(typeof(EntryNumber));
				entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
				entryNumber.CE_RN_NKCountryCode = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				entryNumber.CE_ParentTable = ForwardingConsol.Schema.TableName;
				entryNumber.CE_ParentID = consol.PK;
			}

			entryNumber.CE_EntryNum = oRN;
			entryNumber.CE_EntryStatus = oRNStatus;
		}

		protected string GetFieldDescFromNumber(string fieldCode)
		{
			string result = fieldCode;
			switch (fieldCode)
			{
				case "001":
					result = "Transaction Number";
					break;
				case "003":
					result = "Class";
					break;
				case "004":
					result = "Transaction Type";
					break;
				case "016":
					result = "Outward Report Number";
					break;
				case "065":
					result = "Total Items";
					break;
				case "080":
					result = "Craft/Flight No.";
					break;
				case "081":
					result = "Transport Mode";
					break;
				case "085":
					result = "Voyage Number";
					break;
				case "096":
					result = "Country/Region of Destination";
					break;
				case "299":
					result = "Bill type";
					break;
				case "300":
					result = "Bill Number";
					break;
				case "301":
					result = "Container Number";
					break;
				case "302":
					result = "Container Status";
					break;
				case "525":
					result = "Client Code";
					break;
				case "531":
					result = "Vessel Aircraft Operator Name";
					break;
				case "575":
					result = "Item Number";
					break;
				case "730":
					result = "Clearance Number";
					break;
				case "731":
					result = "Senders Reference";
					break;
				case "732":
					result = "Date of Departure";
					break;
				case "733":
					result = "Port of Departure";
					break;
			}
			return result;
		}

		protected string GetErrorDescFromCode(string errorNumber)
		{
			string result = errorNumber;
			switch (errorNumber)
			{
				case "101":
					result = "Flight No. : Not specified";
					break;
				case "102":
					result = "Craft Name : Not specified";
					break;
				case "103":
					result = "Transport Mode : Not specified or invalid";
					break;
				case "112":
					result = "Voyage No : Not specified";
					break;
				case "138":
					result = "Client Code : Not specified or invalid";
					break;
				case "142":
					result = "Client Code : Not Current";
					break;
				case "148":
					result = "Vessel / Aircraft Operator Name : Not specified";
					break;
				case "166":
					result = "Country/Region of Destination : Not specified or invalid";
					break;
				case "186":
					result = "Bill Type : Not specified or Invalid";
					break;
				case "187":
					result = "Bill Number : Not specified or invalid";
					break;
				case "481":
					result = "Container Number : Not specified or invalid";
					break;
				case "482":
					result = "Container Status : Not specified or invalid";
					break;
				case "487":
					result = "Senders Reference Number : Duplicates not allowed";
					break;
				case "491":
					result = "Port of Departure : Not specified or invalid";
					break;
				case "517":
					result = "Total Items : The number of items received does not match total specified";
					break;
				case "522":
					result = "Date of Departure : Not specified or invalid";
					break;
				case "558":
					result = "Senders Reference Number : Not specified";
					break;
				case "601":
					result = "Transaction Number : Not specified or invalid";
					break;
				case "602":
					result = "Class : Not specified or invalid";
					break;
				case "604":
					result = "Transaction Type : Not specified or invalid";
					break;
				case "618":
					result = "Senders Reference Number: Not the same as the quoted on original";
					break;
				case "626":
					result = "Transaction Type : No original Report to adjust";
					break;
				case "627":
					result = "Transaction Type : Report already cancelled";
					break;
				case "675":
					result = "Container Number : Invalid format";
					break;
				case "676":
					result = "Craft Name : Not on file";
					break;
				case "677":
					result = "Flight No : Not of file";
					break;
				case "678":
					result = "Clearance number : Not specified or invalid";
					break;
				case "679":
					result = "Item Number : Not specified or invalid";
					break;
				case "680":
					result = "Report Number : Not specified or invalid";
					break;
				case "997":
					result = "Customs Processing Error - EDI Interface failures";
					break;
				case "998":
					result = "Customs Processing Error - Transaction broken twice in pipe";
					break;
				case "999":
					result = "Customs Processing Error - EDI translation failure";
					break;
			}

			return result;
		}

		#endregion

		#region Overrides for Email Addresses

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendAcknowledgements.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendImpedimentsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendImpediments.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrors.Value; }
		}

		#endregion

		#region Email Report

		protected void SendEmail(NZCMessage incomingMessage)
		{
			var email = new EmailDef();
			email.Subject = "[" + subject + "] Response for Consol: " + consol.JK_UniqueConsignRef;
			email.Body = GetReport();

			incomingMessage.EM_MessageInterpretation = email.Subject + "\r\n\r\n" + email.Body;
			if (oRNStatus == OutwardReportStatusList.Codes.Cleared)
			{
				SendAcknowledgementReport(consol, email);
			}
			else if (oRNStatus == OutwardReportStatusList.Codes.CustomsInstuctionReceived)
			{
				SendImpedimentReport(consol, email);
			}
			else
			{
				SendErrorReport(consol, email);
			}
		}

		public string GetReport()
		{
			var report = new StringBuilder();

			report.Append("Consol Number\t\t: ");
			report.Append(consol.JK_UniqueConsignRef + "\r\n");
			report.Append("Outward Report No\t: ");
			report.Append(oRN + "\r\n");
			report.Append("Master Bill\t\t: ");
			report.Append(consol.JK_MasterBillNum + "\r\n");

			report.Append("\r\n");

			report.Append("Rsp Message No\t\t: ");
			report.Append(messageNumber + "\r\n");

			report.Append("Message Type\t\t: ");
			report.Append(responseType + "\r\n");

			report.Append("Status\t\t\t: ");
			report.Append(subject + "\r\n");

			report.Append("\r\n");
			if (customsInstuctions.Length > 0)
			{
				report.Append("Customs Instructions\r\n");
				report.Append("---------------------------------------------------------------------\r\n");
				report.Append(customsInstuctions.ToString());
				report.Append("\r\n");
			}

			if (headerErrors.Length > 0)
			{
				report.Append("Message Errors\r\n");
				report.Append("---------------------------------------------------------------------\r\n");
				report.Append(headerErrors.ToString());
				report.Append("\r\n");
			}

			if (lineErrors.Length > 0)
			{
				report.Append("Shipment Responses\r\n");
				report.Append("---------------------------------------------------------------------\r\n");
				report.Append(lineErrors.ToString());
			}

			return report.ToString();
		}

		#endregion
	}
}

