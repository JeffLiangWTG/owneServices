using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Edifact;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.CUSRES;
using Enterprise.Edifact.D96B.Segments;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.FormalEntry
{
	public class UnsolicitedMessageProcessor : Messaging.MessageProcessors.CustomsMessageProcessor
	{
		public UnsolicitedMessageProcessor(LoggingInformation logger)
			: base(logger, "NZC", "Formal Declaration - Unsolicited CUSRES")
		{
		}

		public void ProcessUnsolicitedMessage(EDIMessage message)
		{
			message.EM_Status = DoProcessingReturningStatus(message);
		}

		protected CUSRESMessage cUSRESMessage;

		protected virtual void SetupPropertiesForMessageProcessing(NZCMessage message)
		{
			cUSRESMessage = message.MessageAsCUSRESD96B;
			builder = new ResponseEmailBuilder();
		}
		protected ResponseEmailBuilder builder;

		protected override string DoProcessingReturningStatus(EDIMessage ediMessage)
		{
			Argument.NotNull(ediMessage, "ediMessage");
			NZCMessage message = ediMessage as NZCMessage
				?? throw new InvalidOperationException("You must supply a NZCMessage when using UnsolicitedMessageProcessor.");

			SetupPropertiesForMessageProcessing(message);
			CUSRESMessage cUSRES = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(NzEdifactMessageFactory.NZCMessageFactory, new UNOACharacterSet());
			EmailDef email = new EmailDef();

			try
			{
				if (cUSRESMessage == null)
				{
					throw new MessageProcessingException("Corrupted or Malformed Response Message. Message does not conform to UN-EDIFACT Standard. Cannot Process.");
				}

				ProcessGroup0(cUSRES);
				email.Subject = "Unsolicited Delivery Order";
				email.Body = builder.HeaderString.ToString() + builder.BodyString.ToString();
				SendSuccessfullyProcessedResultEmail(ediMessage, email);

				message.EM_MessageInterpretation = email.Subject + "\r\n\r\n" + email.Body;
				return EDIMessage.Status.Received;
			}
			catch (MessageProcessingException)
			{
				return EDIMessage.Status.Failed;
			}
		}

		protected void SendSuccessfullyProcessedResultEmail(EDIMessage ediMessage, EmailDef email)
		{
			SendReport(email, ediMessage, UnsolicitedDOMode, UnsolicitedDOGroup);
			AcknowledgementEmailSendCount++;
		}

		// Exclude this warning when the type or method is still considered maintainable despite its large number of dependencies on other types.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected void ProcessGroup0(CUSRESMessage cusresMessage)
		{
			builder.OutputHeaderLine("An unsolicited Delivery Order message has been received from New Zealand Customs.");
			builder.OutputHeaderLine("Below are the details contained in the message:");
			builder.OutputHeaderLine();

			var unhSegment = cusresMessage.UNH[0];
			var bgmSegment = cusresMessage.BGM[0];
			var gisSegment = cusresMessage.GIS[0];

			var referenceNumber = unhSegment.CommonAccessReference;
			var responseTypeCode = bgmSegment.DocumentMessageName.DocumentMessageNameCoded.ToString();
			var entryNumber = bgmSegment.DocumentMessageIdentification.DocumentMessageNumber;
			var processingIndicatorCode = gisSegment.ProcessingIndicator.ProcessingIndicatorCoded.ToString();

			builder.OutputHeaderLine(GetResponseTypeFromCode(responseTypeCode));
			builder.OutputHeaderBreakLine();
			builder.OutputHeaderLine("Client Reference Number", referenceNumber);
			builder.OutputHeaderLine("Entry Number", entryNumber);
			builder.OutputHeaderLine("Message No", unhSegment.MessageReferenceNumber);
			builder.OutputHeaderLine();
			builder.OutputMessageStatus(processingIndicatorCode);

			foreach (FTXSegment ftxSegment in cusresMessage.FTX)
			{
				ProcessHeaderFTX(ftxSegment);
			}

			TDTSegment tdtSegment = cusresMessage.TDT[0];
			builder.OutputHeaderLine("Transport Carriage ", tdtSegment.TransportIdentification.IdOfTheMeansOfTransport);
			foreach (LOCSegment locSegment in cusresMessage.LOC)
			{
				if (locSegment.PlaceLocationQualifier == PlaceLocationQualifierList.PlacePortOfLoading)
				{
					builder.OutputHeaderLine("Port of Loading ", locSegment.LocationIdentification.PlaceLocationIdentification);
				}

				if (locSegment.PlaceLocationQualifier == PlaceLocationQualifierList.PortOfDischarge)
				{
					builder.OutputHeaderLine("Port of Discharge ", locSegment.LocationIdentification.PlaceLocationIdentification);
				}
			}

			builder.OutputHeaderLine();
			foreach (SegmentGroup1 group1 in cusresMessage.Group1)
			{
				NADSegment nadSegment = group1.NAD[0];
				if (nadSegment.PartyQualifier == PartyQualifierList.Principal)
				{
					builder.OutputHeaderLine("Client ", nadSegment.NameAndAddress.NameAndAddressLine1);
				}

				if (nadSegment.PartyQualifier == PartyQualifierList.CustomsBroker)
				{
					builder.OutputHeaderLine("Broker ", nadSegment.NameAndAddress.NameAndAddressLine1);
				}
			}

			foreach (SegmentGroup6 group6 in cusresMessage.Group6)
			{
				foreach (RFFSegment rffSegment in group6.RFF)
				{
					ProcessRFF(rffSegment);
				}
			}
		}

		protected void ProcessHeaderFTX(FTXSegment ftxSegment)
		{
			switch (ftxSegment.TextSubjectQualifier)
			{
				case "ACD":
					builder.OutputBodyHeader("Reason for Adjustment");
					break;
				case "DIN":
				case "ICN":
					var outputHeader = ftxSegment.TextSubjectQualifier == "DIN" ? "Delivery Instructions" : "Customs Instructions";
					builder.OutputBodyHeader(outputHeader);
					break;
				default:
					builder.OutputBodyHeader("Unknown Customs Instruction Type Sent: " + ftxSegment.TextSubjectQualifier);
					break;
			}

			OutputTextLiteralElements(ftxSegment.TextLiteral);
		}

		protected void OutputTextLiteralElements(TextLiteralElements textElements)
		{
			if (!string.IsNullOrEmpty(textElements.FreeText1))
			{
				builder.OutputBodyLine(textElements.FreeText1);
			}

			if (!string.IsNullOrEmpty(textElements.FreeText2))
			{
				builder.OutputBodyLine(textElements.FreeText2);
			}

			if (!string.IsNullOrEmpty(textElements.FreeText3))
			{
				builder.OutputBodyLine(textElements.FreeText3);
			}

			if (!string.IsNullOrEmpty(textElements.FreeText4))
			{
				builder.OutputBodyLine(textElements.FreeText4);
			}
		}

		protected void ProcessRFF(RFFSegment rffSegment)
		{
			switch (rffSegment.Reference.ReferenceQualifier)
			{
				case "HWB":
				case "HB":
					builder.OutputHeaderLine("Bill Reference : " + rffSegment.Reference.ReferenceNumber);
					break;
				case "ABU":
					builder.OutputHeaderLine("Parcel Number  : " + rffSegment.Reference.ReferenceNumber);
					break;
				case "BM":
					builder.OutputHeaderLine("Bill of Lading : " + rffSegment.Reference.ReferenceNumber);
					break;
			}
		}

		protected string GetResponseTypeFromCode(string responseTypeCode)
		{
			return new ResponseTypeList().GetDescriptionFromCode(responseTypeCode);
		}

		#region Response Email Group and User Settings

		protected override ZGuid ErrorEmailGroup
		{
			get { return ZGuid.Empty; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return ZString.Empty; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return ZGuid.Empty; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return ZString.Empty; }
		}

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return ZGuid.Empty; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return ZString.Empty; }
		}

		protected ZGuid UnsolicitedDOGroup
		{
			get { return NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.Value; }
		}

		protected ZString UnsolicitedDOMode
		{
			get { return NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.Value; }
		}

		#endregion
	}
}
