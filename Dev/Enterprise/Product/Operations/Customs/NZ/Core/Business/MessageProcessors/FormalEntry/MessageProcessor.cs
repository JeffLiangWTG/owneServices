using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration.FormalEntry;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Edifact;
using Enterprise.Edifact.D98A.Messages.CUSRES;
using Enterprise.Edifact.D98A.Segments;
using Enterprise.Environment;
using CusEntryLine = Enterprise.Customs.NZ.Business.Declaration.CusEntryLine;
using JobComInvoiceLine = Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine;
using JobDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration;
using JobDeclarationDocumentSupporter = Enterprise.Customs.NZ.Business.Declaration.JobDeclarationDocumentSupporter;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.FormalEntry
{
	public class MessageProcessor : MessageProcessorForEntryHeader
	{
		public MessageProcessor(LoggingInformation logger)
			: base(logger, "Formal Declaration CUSRES")
		{
		}

		protected override void ProcessGroup0(CUSRESMessage cusresMessage)
		{
			ResetStateProperties();

			var unhSegment = cusresMessage.UNH[0];
			var bgmSegment = cusresMessage.BGM[0];
			var gisSegment = cusresMessage.GIS[0];

			CheckRequiredSegmentNotNull(unhSegment);
			CheckRequiredSegmentNotNull(bgmSegment);
			CheckRequiredSegmentNotNull(gisSegment);

			parsedJobNumber = unhSegment.CommonAccessReference;
			var responseTypeCode = bgmSegment.DocumentMessageName.DocumentMessageNameCoded.ToString();
			var entryNumber = bgmSegment.DocumentMessageIdentification.DocumentMessageNumber;
			var entryVersion = bgmSegment.DocumentMessageIdentification.Version;
			var processingIndicatorCode = gisSegment.ProcessingIndicator.ProcessingIndicatorCoded.ToString();

			var firstErrorCode = "";
			foreach (SegmentGroup4 group4 in cusresMessage.Group4)
			{
				foreach (ERCSegment eRC in group4.ERC)
				{
					firstErrorCode = eRC.ApplicationErrorDetail.ApplicationErrorIdentification;
					break;
				}
			}

			SetJobStatus(responseTypeCode, processingIndicatorCode, firstErrorCode);

			builder.OutputHeaderLine(GetResponseTypeFromCode(responseTypeCode));
			builder.OutputHeaderBreakLine();
			builder.OutputHeaderLine("Job Number", parsedJobNumber);
			builder.OutputHeaderLine("Master Bill", entryHeader.Declaration.FormattedMasterBill);
			builder.OutputHeaderLine("Entry Type", entryHeader.Declaration.EntryTypeAndStyle);
			builder.OutputHeaderLine("Entry Number", entryNumber);
			builder.OutputHeaderLine("Message No", unhSegment.MessageReferenceNumber);
			builder.OutputHeaderLine();
			builder.OutputMessageStatus(processingIndicatorCode);

			ZInt entryNumberAsZInt;
			if (ZInt.TryParse(entryNumber, out entryNumberAsZInt) && entryNumberAsZInt != 0)
			{
				parsedEntryNumber = entryNumber;
				ZInt.TryParse(entryVersion, out parsedEntryVersion);
			}

			ProcessGroup5(cusresMessage.Group5[0]);

			var dtmSegment = cusresMessage.DTM[0];
			if (dtmSegment != null)
			{
				if (!string.IsNullOrEmpty(dtmSegment.DateTimePeriod.ToString(new UNOACharacterSet())))
				{
					builder.OutputHeaderLine("Payment Due", dtmSegment.DateTimePeriod.ToString());
				}
			}

			foreach (FTXSegment ftxSegment in cusresMessage.FTX)
			{
				ProcessHeaderFTX(ftxSegment);
			}

			// Will need this later for Delivery Orders sent from Other Parties.
			// TDTSegment TDTSegment = cusresMessage.TDT[0]; 
			// LOCSegment LOCSegment = cusresMessage.LOC[0];
			// foreach (SegmentGroup1 Group1 in cusresMessage.Group1)
			// {
			// 	NADSegment NADSegment = Group1.NAD[0];
			// }
			// Will need this later for Delivery Orders sent from Other Parties.

			if (cusresMessage.Group4.Count > 0)
			{
				builder.OutputBodyHeader("Message Errors");

				foreach (SegmentGroup4 group4 in cusresMessage.Group4)
				{
					ProcessGroup4(group4); // Rejection Report
				}
			}

			// Will need this later for Delivery Orders sent from Other Parties.
			// foreach (SegmentGroup6 Group6 in cusresMessage.Group6)
			// {
			// 	SegmentGroup6(Group6);
			// }
			// Will need this later for Delivery Orders sent from Other Parties.

			var untSegment = cusresMessage.UNT[0];
			CheckRequiredSegmentNotNull(untSegment);
		}

		protected void ProcessGroup5(SegmentGroup5 group5)
		{
			if (group5 != null)
			{
				var moaSegment = group5.MOA[0];
				var totalAmount = moaSegment.MonetaryAmount.MonetaryAmount;
				var gisSegment = group5.GIS[0];
				string paymentMethod = gisSegment.ProcessingIndicator.ProcessingIndicatorCoded;

				if (totalAmount != null || paymentMethod != null)
				{
					builder.OutputHeaderLine();
					string expectedTotalAmount = null;
					if (totalAmount == null)
					{
						totalAmount = "Amount Not Included in Response.";
					}
					else
					{
						parsedTotalAmount = Convert.ToDecimal(totalAmount);
						totalAmount = parsedTotalAmount.ToString("C");
						expectedTotalAmount = entryHeader.TotalAmountPayableIncludingEntryFee.ToString("C");
					}

					builder.OutputHeaderLine("Amount Returned", totalAmount);
					if (expectedTotalAmount != null && totalAmount != expectedTotalAmount && DeclarationBeingProcessedIsImport)
					{
						builder.OutputHeaderLine("** WARNING - Total Amount Payable returned by Customs does not **");
						builder.OutputHeaderLine("** match the amount calculated by " + Core.Constants.ProductName + ". (" + expectedTotalAmount + ") **");
					}

					builder.OutputHeaderLine("Terms", GetPaymentTypeDescription(paymentMethod));
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
					parsedCustomsDeliveryInstructions = builder.GetNoteText(ftxSegment.TextLiteral);
					break;
				default:
					builder.OutputBodyHeader("Unknown Customs Instruction Type Sent: " + ftxSegment.TextSubjectQualifier);
					break;
			}

			builder.OutputTextLiteralElements(ftxSegment.TextLiteral);
		}

		public override string JobTypeDescription
		{
			get { return "Customs Declaration"; }
		}

		public override string ResponseTypeForEmailSubject
		{
			get { return new FormalEntryStatusList().GetDescriptionFromCode(newEntryStatus); }
		}

		protected void SetJobStatus(ZString responseType, ZString entryStatus, ZString firstErrorCode)
		{
			if (entryStatus == ResponseStatusList.Codes.EntryRejected
				|| entryStatus == ResponseStatusList.Codes.EntryRejectedReasonsAsSpecified)
			{
				if (firstErrorCode == "605" || firstErrorCode == "606") // Means that "Replace Rejected" was sent when the job wasn't actually rejected. Only the Replace Rejected gets rejected, not the job.
				{
					newEntryStatus = FormalEntryStatusList.Codes.ResponseReceived;
				}
				else
				{
					newEntryStatus = FormalEntryStatusList.Codes.EntryRejected;
				}
			}
			else if (entryStatus == ResponseStatusList.Codes.EntryCancelled)
			{
				newEntryStatus = FormalEntryStatusList.Codes.EntryCancelled;
			}
			else if (entryStatus == ResponseStatusList.Codes.EntryRestored)
			{
				newEntryStatus = FormalEntryStatusList.Codes.EntryRestored;
			}
			else if (entryStatus == ResponseStatusList.Codes.AdjustmentHasPlacedEntryInErrorState)
			{
				newEntryStatus = FormalEntryStatusList.Codes.EntryInError;
			}
			else if (responseType == ResponseTypeList.Codes.DeliveryOrder)
			{
				messageJustProcessedIsADeliveryOrderMessage = true;
				newEntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			}
			else if (responseType == ResponseTypeList.Codes.InspectionsAuditRequirements)
			{
				newEntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			}
			else if (responseType == ResponseTypeList.Codes.DeliveryOnPayment)
			{
				newEntryStatus = FormalEntryStatusList.Codes.DeliveryOnPayment;
			}
			else if (responseType == ResponseTypeList.Codes.ConfirmationOfAdjustment)
			{
				newEntryStatus = FormalEntryStatusList.Codes.AdjustmentAccepted;
			}
			else
			{
				newEntryStatus = FormalEntryStatusList.Codes.ResponseReceived;
			}
		}

		protected override string GetErrorPointFromCode(string errorPointCode)
		{
			var retVal = "Unknown Error Point: " + errorPointCode;
			switch (Convert.ToInt16(errorPointCode))
			{
				case 1:
					retVal = "Header";
					break;
				case 2:
					retVal = "Line";
					break;
				case 3:
					retVal = "Summary";
					break;
			}
			return retVal;
		}

		protected override string GetItemNumberDescription(string errorItemNumber, string errorSection)
		{
			var result = "";
			if (!string.IsNullOrEmpty(errorItemNumber) && Convert.ToInt16(errorSection) == 2)
			{
				result = ", Merged Line " + errorItemNumber;
			}
			return result;
		}

		protected string GetPaymentTypeDescription(string paymentTerms)
		{
			ZString result = new CustomsPaymentTypeList().GetDescriptionFromCode(paymentTerms);
			parsedCustomsPaymentType = paymentTerms;
			if (result.IsEmpty)
			{
				result = "No Payment Terms Specified.";
				parsedCustomsPaymentType = "";
			}
			return result;
		}

		protected override void AddErrorForItem(string errorSection, string errorItemNumber, string fieldCode, string errorCode)
		{
			base.AddErrorForItem(errorSection, errorItemNumber, fieldCode, errorCode);

			if (ZInt.ParseSafe(errorSection, 0) == 2) // Merged Lines
			{
				var mergedLineNumber = ZInt.ParseSafe(errorItemNumber, 0);
				var entryLine = entryHeader.MergedLines.FindByLineNumber(mergedLineNumber);
				if (entryLine != null)
				{
					if (!EntryLinesWithErrors.Contains(entryLine))
					{
						EntryLinesWithErrors.Add(entryLine);
					}
				}
			}
		}

		protected override void WriteEntryStatus()
		{
			var declaration = entryHeader.Declaration;
			using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				var responseReceivedIsNewerThanPreviousResponses = true;
				if (!parsedEntryNumber.IsEmpty)
				{
					entryHeader.EntryNumber = parsedEntryNumber;
					if (!parsedEntryVersion.IsEmpty)
					{
						if (parsedEntryVersion < entryHeader.CH_LastResponseVersionNumber)
						{
							responseReceivedIsNewerThanPreviousResponses = false;
						}
						else
						{
							entryHeader.CH_LastResponseVersionNumber = parsedEntryVersion;
						}
					}
				}

				if (responseReceivedIsNewerThanPreviousResponses)
				{
					UpdateStatusFromLatestResponse(declaration);
				}

				if (parsedEntryNumber.IsEmpty && (newEntryStatus == FormalEntryStatusList.Codes.EntryRejected || newEntryStatus == FormalEntryStatusList.Codes.EntryInError))
				{
					var outgoingMessage = GetLastOutgoingMessage();
					if (outgoingMessage != null && outgoingMessage.IsAnOriginal)
					{
						declaration.JE_EDITransmitDate = entryHeader.CH_EDITransmitDate = ZDateTime.Empty;  // if this rejection is in response to an original message send reset transmit date
					}
				}
			}
		}

		void UpdateStatusFromLatestResponse(JobDeclaration declaration)
		{
			if (!newEntryStatus.IsEmpty)
			{
				entryHeader.CH_EntryStatus = newEntryStatus;
				if (newEntryStatus == FormalEntryStatusList.Codes.EntryRestored)
				{
					JobDeclaration.ProcessRestoredEntry(declaration, entryHeader);
				}
			}

			if (!parsedCustomsDeliveryInstructions.IsEmpty)
			{
				entryHeader.CH_CustomsDeliveryInstructions = parsedCustomsDeliveryInstructions;
			}

			if (!parsedTotalAmount.IsEmpty)
			{
				entryHeader.CH_TotalAmountReturned = parsedTotalAmount;
			}

			if (entryHeader.CH_IsActive)
			{
				if (!newEntryStatus.IsEmpty)
				{
					declaration.JE_EntryStatus = newEntryStatus;
				}

				if (!parsedCustomsPaymentType.IsEmpty)
				{
					declaration.JE_PaymentMethod = PaymentMethodList.GetPaymentPartyCode(parsedCustomsPaymentType, entryHeader.Declaration.JE_PaymentMethod);
				}

				if (messageJustProcessedIsADeliveryOrderMessage)
				{
					declaration.LogCustomsClearedIfNeeded();
					var documentSupporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
					var branch = declaration == null ? null : declaration.Branch;
					var branchPK = branch == null ? Guid.Empty : branch.PK.ToGuid();
					using (DisposableEnvironment.ForBranch(branchPK, false))
					{
						documentSupporter.AutoPrintCustomsClearanceDocs(entryHeader, true);
					}
				}
				else if (newEntryStatus == FormalEntryStatusList.Codes.InspectionsAuditRequirements)
				{
					declaration.LogCustomsImpediment();
				}

				foreach (var entryLine in EntryLinesWithErrors)
				{
					foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
					{
						invoiceLine.JI_HadErrorInLastResponse = true;
					}
				}
			}

			if (newEntryStatus == FormalEntryStatusList.Codes.EntryCancelled)
			{
				entryHeader.CH_IsActive = false;
				entryHeader.CH_IsEntryCancelled = true;
			}
		}

		void ResetStateProperties()
		{
			parsedJobNumber = "";
			newEntryStatus = "";
			parsedEntryNumber = "";
			parsedEntryVersion = 0;
			parsedCustomsDeliveryInstructions = "";
			parsedCustomsPaymentType = "";
			parsedTotalAmount = ZDecimal.Zero;
			messageJustProcessedIsADeliveryOrderMessage = false;
			fEntryLinesWithErrors = null;
		}

		ZString parsedJobNumber;
		ZString newEntryStatus;
		ZString parsedEntryNumber;
		ZInt parsedEntryVersion;
		ZString parsedCustomsDeliveryInstructions;
		ZString parsedCustomsPaymentType;
		ZDecimal parsedTotalAmount;
		bool messageJustProcessedIsADeliveryOrderMessage;

		List<CusEntryLine> EntryLinesWithErrors
		{
			get { return fEntryLinesWithErrors ?? (fEntryLinesWithErrors = new List<CusEntryLine>()); }
		}
		List<CusEntryLine> fEntryLinesWithErrors;

		protected new CusEntryHeader entryHeader
		{
			get { return (CusEntryHeader)base.entryHeader; }
		}

		protected override string GetResponseTypeFromCode(string responseTypeCode)
		{
			return FactorySavedAfterProcessing.GetCachedValue<ResponseTypeList>().GetDescriptionFromCode(responseTypeCode);
		}

		#region Response Email Group and User Settings

		bool DeclarationBeingProcessedIsImport => entryHeader?.IsImport ?? false;

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return DeclarationBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportDeclarationsSendAcknowledgementsToGroup.Value : NZCustomsDataRegistry.Instance.ExportDeclarationsSendAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return DeclarationBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportDeclarationsSendAcknowledgements.Value : NZCustomsDataRegistry.Instance.ExportDeclarationsSendAcknowledgements.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return DeclarationBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportDeclarationsSendImpedimentsToGroup.Value : NZCustomsDataRegistry.Instance.ExportDeclarationsSendImpedimentsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return DeclarationBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportDeclarationsSendImpediments.Value : NZCustomsDataRegistry.Instance.ExportDeclarationsSendImpediments.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return DeclarationBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportDeclarationsSendErrorsToGroup.Value : NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return DeclarationBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportDeclarationsSendErrors.Value : NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrors.Value; }
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
