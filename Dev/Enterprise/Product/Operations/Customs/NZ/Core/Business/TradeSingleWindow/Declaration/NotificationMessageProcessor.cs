using System;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	class NotificationMessageProcessor : TSWMessageProcessor<TSWResponse>
	{
		public NotificationMessageProcessor(LoggingInformation logger)
			: base(logger, "")
		{
		}

		#region Overrides

		protected override ZGuid DocumentParentPK => ZGuid.Empty;   // These are unsolicited messages - jobs will not be in clients CW1 system

		protected override ZString DocumentParentType => ZString.Empty;

		protected override string[] ExpectedDODocumentNames => new string[] { "" };

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return ZGuid.Empty; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return ZString.Empty; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return ZGuid.Empty; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return ZString.Empty; }
		}

		protected string ResponseIsFor
		{
			get { return Response.Recipient; }
		}

		string LodgementType
		{
			get
			{
				var result = "TSW entry";
				if (Response.IsIM1)
				{
					result = "Import Declaration";
				}
				else if (Response.IsEX1)
				{
					result = "Export Declaration";
				}
				else if (Response.IsCRE)
				{
					result = "Cargo Report Export";
				}
				else if (Response.IsICR)
				{
					result = "Inward Cargo Report";
				}
				else if (Response.IsOCR)
				{
					result = "Outward Cargo Report";
				}

				return result;
			}
		}

		protected override void ProcessCore()
		{
			CompileNotificationResponseEmail();
			AddAttachments();
			SendAcknowledgementReport(null, responseEmail);
			Response.IncomingTSWMessage.EM_MessageInterpretation = responseEmail.Subject + System.Environment.NewLine + System.Environment.NewLine + responseEmail.Body;
		}

		protected override void SendEmail(EmailDef email)
		{
			SendAcknowledgementReport(null, responseEmail);
		}

		protected override void WriteEntryStatus()
		{
			// This is an unsolicited notification message.
		}

		protected override void OutputResponseStatus()
		{
			var statusDescriptionParts = Response.StatusDescription.Split(new[] { ',' }, 2);
			DeclarationDetails.Append("Message Status: (" + Response.Status + ") " + statusDescriptionParts[0]);
			if (statusDescriptionParts.Length == 2)
			{
				statusDescriptionParts = statusDescriptionParts[1].Split(new[] { '-' }, 2);
				DeclarationDetails.Append(statusDescriptionParts[0].Trim() + ".");
				if (statusDescriptionParts.Length == 2)
				{
					DeclarationDetails.Append(statusDescriptionParts[1].Trim() + ".");
				}
			}
		}

		#endregion // Overrides

		#region Implementation

		void CompileNotificationResponseEmail()
		{
			HtmlTableCreator creator = null;

			string emailTemplateHtml;
			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.NZ.Business.TradeSingleWindow.HTMLTemplates.NotificationEmail.htm"))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}

			emailTemplateHtml = emailTemplateHtml.Replace("{0}", AgencyReceivedFrom);
			emailTemplateHtml = emailTemplateHtml.Replace("{1}", ResponseIsFor);
			emailTemplateHtml = emailTemplateHtml.Replace("{2}", LodgementType);
			emailTemplateHtml = emailTemplateHtml.Replace("{3}", Response.MessageTypeDescription);
			emailTemplateHtml = emailTemplateHtml.Replace("{4}", Response.DeclarationID);
			emailTemplateHtml = emailTemplateHtml.Replace("{5}", Response.AcceptanceTime.ToLongTimeString());
			emailTemplateHtml = emailTemplateHtml.Replace("{6}", Response.SendersReference);
			emailTemplateHtml = emailTemplateHtml.Replace("{7}", Response.MessageNumber);

			if (Response.IsIM1 || Response.IsEX1)
			{
				OutputDeclarationInfo();
				emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml1-->", DeclarationDetails.ToStringWithDelimiterBetweenAppends("<br />"));
			}
			else if (Response.IsCRE || Response.IsICR || Response.IsOCR)
			{
				OutputConsignmentInfo();
				emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml1-->", ConsignmentDetails.ToStringWithDelimiterBetweenAppends("<br />"));
			}

			emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml2-->", creator != null ? creator.ToHtml() : string.Empty);

			var emailSender = new HtmlNotificationEmailSender();
			string subject = string.Format(CultureInfo.CurrentCulture, AgencyReceivedFrom.Replace("the ", "") + " - " + LodgementType + " - TSW Notification Message");
			responseEmail = emailSender.CreateEmail(subject, emailTemplateHtml);
		}

		ZStringBuilder DeclarationDetails
		{
			get { return declarationDetails ?? (declarationDetails = new ZStringBuilder()); }
		}
		ZStringBuilder declarationDetails;

		ZStringBuilder ConsignmentDetails
		{
			get { return consignmentDetails ?? (consignmentDetails = new ZStringBuilder()); }
		}
		ZStringBuilder consignmentDetails;

		void ProcessDeliveryInstruction()
		{
			if (!Response.DeliverInstructions.IsNullOrEmpty())
			{
				DeclarationDetails.Append("Delivery Instructions");
				foreach (var instruction in Response.DeliverInstructions)
				{
					DeclarationDetails.Append(instruction);
				}
			}
		}

		protected override void ProcessCustomsInstructions()
		{
			if (!Response.CustomsInstructions.IsNullOrEmpty())
			{
				DeclarationDetails.Append("Customs Instructions");
				Response.CustomsInstructions.ForEach(OutputCustomsInstruction);
			}
		}

		protected override void OutputCustomsInstruction(string instruction)
		{
			ZString customsInstruction = instruction;
			DeclarationDetails.Append(customsInstruction.Replace("seq:1,instructions:", ""));
		}

		void OutputDeclarationInfo()
		{
			var clearanceStatus = string.IsNullOrEmpty(Response.GoodsClearanceStatus) ? Response.GoodsStatus : Response.GoodsClearanceStatus;
			if (!string.IsNullOrEmpty(clearanceStatus))
			{
				var statusList = Response.IncomingTSWMessage.Factory.GetCachedValue<FormalEntryStatusList>();
				var clearanceStatusDescription = statusList.GetDescriptionFromCode(clearanceStatus);
				DeclarationDetails.Append("Clearance Status: " + clearanceStatus + " / " + clearanceStatusDescription);
			}

			var movementStatus = Response.GoodsMovementStatus;
			if (!string.IsNullOrEmpty(movementStatus))
			{
				var movementStatusList = Response.IncomingTSWMessage.Factory.GetCachedValue<MovementStatus>();
				var movementStatusDescription = movementStatusList.GetDescriptionFromCode(movementStatus);
				DeclarationDetails.Append("Goods Movement Status: " + Response.GoodsMovementStatus + " / " + movementStatusDescription);
				DeclarationDetails.AppendLine();
			}

			DeclarationDetails.Append("Master Bill: " + Response.MasterBillNumber);
			DeclarationDetails.Append("House Bill: " + Response.HouseBill);
			DeclarationDetails.Append("");
			DeclarationDetails.Append("Submitter: " + Response.SubmitterName);
			DeclarationDetails.Append("Agent: " + Response.AgentName);
			DeclarationDetails.Append("Carrier: " + Response.Carrier);
			if (Response.TransportType == TransportModeTypeList.Codes.T4)
			{
				DeclarationDetails.Append("Flight No.: " + Response.FlightNo);
			}
			else
			{
				DeclarationDetails.Append("Vessel: " + Response.Vessel);
				DeclarationDetails.Append("IMO Number: " + Response.IMONo);
				DeclarationDetails.Append("Voyage: " + Response.Voyage);
			}

			if (Response.IsIM1)
			{
				DeclarationDetails.Append("Port of Discharge: " + Response.PortOfDischarge);
				DeclarationDetails.Append("");
				DeclarationDetails.Append("Importer: " + Response.ImporterName);
			}
			else
			{
				DeclarationDetails.Append("Port of Loading: " + Response.PortOfLoading);
				DeclarationDetails.Append("");
				DeclarationDetails.AppendLine("Exporter: " + Response.ExporterName);
			}

			DeclarationDetails.AppendLine();

			hasContainers = false;
			containerTable = new HtmlTableCreator(new string[] { "Container", "Container Status", "Seal Numbers", "Number of Packages", "Type of Package" });
			foreach (string container in Response.Containers)
			{
				WriteContainerLinesForInterpretation(container);
			}

			if (hasContainers)
			{
				DeclarationDetails.Append(containerTable.ToHtml());
			}
			else
			{
				var packagingHeading = "Packaging: ";
				foreach (var packagingDetails in Response.PackagingDetails)
				{
					if (!packagingDetails.IsEmpty)
					{
						var packagingValues = packagingDetails.Split(ValueDelimiter);
						var packagingQty = packagingValues[1];
						var packagingType = packagingValues[2];
						DeclarationDetails.AppendLine(packagingHeading + packagingQty + " " + packagingType);
						packagingHeading = "";
					}
				}
			}

			OutputResponseStatus();
			ProcessDeliveryInstruction();
			ProcessCustomsInstructions();
		}

		void OutputConsignmentInfo()
		{
			ConsignmentDetails.Append("Submitter: " + Response.SubmitterName);
			if (!string.IsNullOrEmpty(Response.Carrier))
			{
				ConsignmentDetails.Append("Carrier: " + Response.Carrier);
			}

			bool isAir = Response.TransportType == TransportModeTypeList.Codes.T4;
			if (Response.IsOCR)
			{
				consignmentTable = new HtmlTableCreator(new string[] { "Master Bill", "House Bill", "Export Delivery Order" });
			}
			else if (isAir)
			{
				ConsignmentDetails.Append("Flight No.: " + Response.FlightNo);
				consignmentTable = new HtmlTableCreator(new string[] { "Master Bill", "House Bill", "Consignee", "Consignor", "Clearance Status", "Movement Status" });
			}
			else
			{
				ConsignmentDetails.Append("Vessel: " + Response.Vessel);
				ConsignmentDetails.Append("IMO Number: " + Response.IMONo);
				ConsignmentDetails.Append("Voyage: " + Response.Voyage);
				consignmentTable = new HtmlTableCreator(new string[] { "Master Bill", "House Bill", "Consignee", "Consignor", "Containers", "Clearance Status", "Movement Status" });
			}

			if (Response.IsICR)
			{
				ConsignmentDetails.Append("Arrival Date: " + Response.ArrivalDate.ToShortDateString());
				ConsignmentDetails.Append("Port of Arrival: " + Response.PortOfArrival);
			}
			else // CRE & OCR
			{
				ConsignmentDetails.Append("Departure Date: " + Response.DepartureDate.ToShortDateString());
				var portOfDeparture = Response.IsOCR ? Response.ExitOffice : Response.PortOfDeparture;
				ConsignmentDetails.Append("Port of Departure: " + portOfDeparture);
			}

			ConsignmentDetails.AppendLine();
			foreach (var consignment in Response.ConsignmentsToNotify)
			{
				WriteConsignmentLinesForInterpretation(consignment, isAir);
			}

			ConsignmentDetails.Append(consignmentTable.ToHtml());

			if (Response.IsOCR)
			{
				hasContainers = false;
				consignmentContainerTable = new HtmlTableCreator(new string[] { "Container", "Container Status" });
				WriteConsignmentContainerLinesForInterpretation();
				if (hasContainers)
				{
					ConsignmentDetails.AppendLine();
					ConsignmentDetails.Append(consignmentContainerTable.ToHtml());
				}
			}

			ConsignmentDetails.AppendLine();
			OutputConsignmentResponseStatus();
			ConsignmentDetails.AppendLine();
		}

		bool hasContainers;

		internal void WriteConsignmentLinesForInterpretation(string consignment, bool isAir)
		{
			var consignmentValues = consignment.Split(ValueDelimiter);
			var masterBill = consignmentValues[0];
			var houseBill = consignmentValues[1];
			var consignee = consignmentValues[2];
			var consignor = consignmentValues[3];
			var clearanceStatus = consignmentValues[4];
			var statusList = Response.IncomingTSWMessage.Factory.GetCachedValue<LowValueConsignmentStatusList>();
			var clearanceStatusDescription = statusList.GetDescriptionFromCode(clearanceStatus);
			var movementStatus = consignmentValues[5];
			var movementStatusDescription = statusList.GetDescriptionFromCode(movementStatus);
			var consignmentContainerDetails = consignmentValues[6];
			var exportDeliveryOrder = consignmentValues[7];
			if (Response.IsOCR)
			{
				consignmentTable.WriteRow(masterBill, houseBill, exportDeliveryOrder);
			}
			else if (isAir)
			{
				consignmentTable.WriteRow(masterBill, houseBill, consignee, consignor, clearanceStatusDescription, movementStatusDescription);
			}
			else
			{
				consignmentTable.WriteRow(masterBill, houseBill, consignee, consignor, consignmentContainerDetails, clearanceStatusDescription, movementStatusDescription);
			}
		}

		internal void WriteContainerLinesForInterpretation(string container)
		{
			var containerTypeList = new ContainerStatusList();
			foreach (var containerDetails in Response.ContainerDetails)
			{
				if (!containerDetails.IsEmpty)
				{
					hasContainers = true;
					var containerValues = containerDetails.Split(ValueDelimiter);
					var containerNo = containerValues[0];
					if (containerNo == container)
					{
						var fullnessCode = containerValues[1];
						var packingSequence = containerValues[2];
						var sealNumbers = containerValues[3];
						var mode = containerTypeList.GetDescriptionFromCode(fullnessCode);
						var packingDetails = GetContainerPacking(packingSequence).Split(ValueDelimiter);
						var packageQty = packingDetails[0];
						var packageType = packingDetails[1];

						containerTable.WriteRow(containerNo, mode, sealNumbers, packageQty, packageType);
					}
				}
			}
		}

		HtmlTableCreator consignmentTable;
		HtmlTableCreator containerTable;
		HtmlTableCreator consignmentContainerTable;

		internal void WriteConsignmentContainerLinesForInterpretation()
		{
			var containerTypeList = new ContainerStatusList();
			foreach (var containerDetails in Response.ConsignmentContainerDetails)
			{
				if (!containerDetails.IsEmpty)
				{
					hasContainers = true;
					var containerValues = containerDetails.Split(ValueDelimiter);
					var containerNo = containerValues[0];
					var fullnessCode = containerValues[1];
					var mode = containerTypeList.GetDescriptionFromCode(fullnessCode);

					consignmentContainerTable.WriteRow(containerNo, mode);
				}
			}
		}

		string GetContainerPacking(string packingSequence)
		{
			foreach (var packagingDetails in Response.PackagingDetails)
			{
				if (!packagingDetails.IsEmpty)
				{
					var packagingValues = packagingDetails.Split(ValueDelimiter);
					var sequence = packagingValues[0];
					if (sequence == packingSequence)
					{
						var packagingQty = packagingValues[1];
						var packagingType = packagingValues[2];
						return packagingQty + ValueDelimiter + packagingType;
					}
				}
			}

			return ValueDelimiter.ToString();
		}

		void OutputConsignmentResponseStatus()
		{
			var statusDescriptionParts = Response.StatusDescription.Split(new[] { ',' }, 2);
			ConsignmentDetails.Append("Message Status: (" + Response.Status + ") " + statusDescriptionParts[0]);
			if (statusDescriptionParts.Length == 2)
			{
				statusDescriptionParts = statusDescriptionParts[1].Split(new[] { '-' }, 2);
				ConsignmentDetails.Append(statusDescriptionParts[0].Trim() + ".");
				if (statusDescriptionParts.Length == 2)
				{
					ConsignmentDetails.Append(statusDescriptionParts[1].Trim() + ".");
				}
			}
		}

		string AgencyReceivedFrom
		{
			get
			{
				var agencyReceivedFrom = ResponsibleGovernmentAgencyList.Descriptions.NZCS;
				switch (Response.ResponsibleGovernmentAgency)
				{
					case ResponsibleGovernmentAgencyList.Codes.MPIFOOD:
						agencyReceivedFrom = "the " + ResponsibleGovernmentAgencyList.Descriptions.MPIFOOD;
						break;
					case ResponsibleGovernmentAgencyList.Codes.MPIBIO:
						agencyReceivedFrom = "the " + ResponsibleGovernmentAgencyList.Descriptions.MPIBIO;
						break;
					case ResponsibleGovernmentAgencyList.Codes.TSW:
						agencyReceivedFrom = ResponsibleGovernmentAgencyList.Descriptions.TSW;
						break;
					case ResponsibleGovernmentAgencyList.Codes.MOH:
						agencyReceivedFrom = "the " + ResponsibleGovernmentAgencyList.Descriptions.MOH;
						break;
					case ResponsibleGovernmentAgencyList.Codes.MNZ:
						agencyReceivedFrom = ResponsibleGovernmentAgencyList.Descriptions.MNZ;
						break;
				}

				return agencyReceivedFrom;
			}
		}

		void AddAttachments()
		{
			if (Response.IncomingTSWMessage.Interchange != null)
			{
				IStorageDocsBaseCollection interchangeDocs = ((IDocManagerSupport)Response.IncomingTSWMessage.Interchange).DocManagerInfo.AllEDocs;
				if (interchangeDocs != null)
				{
					foreach (IeDoc document in interchangeDocs)
					{
						if (document.FileName.Contains("_Delivery_Order", StringComparison.OrdinalIgnoreCase))
						{
							var characterToStopOn = new char[] { '-' };
							var docName = document.FileName.KeepCharsUntil(DocNameValues, characterToStopOn) + "_" + Response.JobID + ".pdf";
							responseEmail.Attachments.Add(new AttachmentDef(docName, document.ImageData));
						}
						else if (document.FileName.StartsWith("PDF", StringComparison.OrdinalIgnoreCase))
						{
							var docName = document.FileName.SubstringSafe(0, 13) + "_" + Response.JobID + ".pdf";
							responseEmail.Attachments.Add(new AttachmentDef(docName, document.ImageData));
						}
						else
						{
							var docName = document.FileName.SubstringSafe(0, 50);
							responseEmail.Attachments.Add(new AttachmentDef(docName, document.ImageData));
						}
					}
				}
			}
		}

		const string DocNameValues = "DeliveryOrder_IM1EXOCR";
		const char ValueDelimiter = '|';

		#endregion
	}
}
