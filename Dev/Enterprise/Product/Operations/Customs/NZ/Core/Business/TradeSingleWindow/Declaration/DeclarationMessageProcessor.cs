using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using JobComInvoiceLine = Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine;
using JobDeclarationDocumentSupporter = Enterprise.Customs.NZ.Business.Declaration.JobDeclarationDocumentSupporter;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	abstract class DeclarationMessageProcessor<TResponse> : TSWMessageProcessor<TResponse>, ITSWStatus
		where TResponse : DeclarationResponse
	{
		protected DeclarationMessageProcessor(LoggingInformation logger, string declarationType)
			: base(logger, declarationType + " Declaration")
		{
		}

		protected abstract bool IsWarningAboutTotalAmountInconsistencyRequired { get; }

		protected CusEntryHeader EntryHeader
		{
			get { return Response.EntryHeader; }
		}

		protected ForwardingConsol Consol
		{
			get { return Response.Consol; }
		}

		protected ConsolidatedDeclaration ConsolidatedDeclaration
		{
			get { return Response.ConsolidatedDeclaration; }
		}

		#region Overrides

		protected override void ProcessCore()
		{
			Report.AddLine(Response.MessageTypeDescription);
			Report.AddSeparator();
			Report.AddLine("Job Number", Response.JobID);
			if (ConsolidatedDeclaration != null)
			{
				var declarations = ConsolidatedDeclaration.JobDeclarations.Cast<JobDeclaration>();
				Report.AddLine("Master Bill", string.Join(", ", declarations.Select(dec => dec.FormattedMasterBill).ToHashSet()));
			}
			else
			{
				Report.AddLine("Master Bill", EntryHeader.Declaration.FormattedMasterBill);
			}
			Report.AddLine("Entry Type", EntryHeader.Declaration.EntryTypeAndStyle);
			Report.AddLine("Entry Number", Response.DeclarationID);
			Report.AddLine("Message No", Response.MessageNumber);
			Report.AddLine();
			OutputResponseStatus();

			if (Response.IsCustomsResponse)
			{
				EntryHeader.CH_EntryChargeWaived = Response.NullableTotalAmount == 0;

				string amountReturned;
				if (Response.NullableTotalAmount == null)
				{
					amountReturned = "Amount Not Included in Response.";
				}
				else if (Response.NullableTotalAmount == 0)
				{
					amountReturned = "$0.00";
				}
				else
				{
					amountReturned = Response.NullableTotalAmount.Value.ToString("C");
				}

				// Write payment to report
				if (Response.TotalAmount != 0 || !string.IsNullOrEmpty(Response.PaymentMethod))
				{
					Report.AddLine();
					Report.AddLine("Amount Returned", amountReturned);
					if (Response.ExpectedTotalAmount != 0 &&
						Response.ExpectedTotalAmount != Response.TotalAmount &&
						IsWarningAboutTotalAmountInconsistencyRequired)
					{
						var amountExpected = Response.ExpectedTotalAmount.Round(2).ToString(2);
						Report.AddLine(string.Concat("** WARNING - Total Amount Payable returned by Customs does not match the amount calculated by ", Core.Constants.ProductName, ". (", amountExpected, ") **"));
					}

					Report.AddLine("Terms", Response.PaymentMethodDescription);
				}
			}

			ProcessDeliveryInstruction();
			ProcessCustomsInstructions();

			if (!Response.ErrorsWithPointers.IsNullOrEmpty())
			{
				Report.AddHeader("Message Errors");
				foreach (var error in Response.ErrorsWithPointers)
				{
					Report.AddLine("**Error** in {" + error.Pointer + "}:-");
					Report.AddLine("  " + error.Value);
				}
			}
		}

		protected override string[] ExpectedDODocumentNames
		{
			get { return new string[] { "IM1_Delivery_Order", "EX1_Delivery_Order" }; }
		}

		protected override ZGuid DocumentParentPK
		{
			get { return EntryHeader != null ? EntryHeader.Declaration.PK : ZGuid.Empty; }
		}

		protected override ZString DocumentParentType
		{
			get { return Core.Constants.DocManagerCodes.JobDeclaration; }
		}

		protected override void SendEmail(EmailDef email)
		{
			if (ConsolidatedDeclaration != null)
			{
				if (EntryHeader.LastCustomsStatusIsImpediment)
				{
					SendImpedimentReport(ConsolidatedDeclaration, email);
				}
				else
				{
					SendAcknowledgementReport(ConsolidatedDeclaration, email);
				}
			}
			else if (EntryHeader != null)
			{
				if (EntryHeader.LastCustomsStatusIsImpediment)
				{
					SendImpedimentReport(EntryHeader, email);
				}
				else
				{
					SendAcknowledgementReport(EntryHeader, email);
				}
			}
			else if (Consol != null)
			{
				SendAcknowledgementReport(Consol, email);
			}
		}

		protected override void OnMessageProcessed(EDIMessage incomingMessage, TResponse response)
		{
			response?.ConsolidatedDeclaration?.SyncStatusAfterMessageProcessing(incomingMessage);
		}

		#endregion

		#region ITSWStatus

		JobDeclaration ITSWStatus.Declaration => EntryHeader.Declaration;

		ZString ITSWStatus.ResponseStatus => Response.Status;

		ZString ITSWStatus.EnterpriseStatus => Response.EnterpriseStatus;

		ZString ITSWStatus.Agency => Response.ResponsibleGovernmentAgency;

		#endregion

		#region Implementation

		protected override void WriteEntryStatus()
		{
			var declaration = EntryHeader.Declaration;
			using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				if (Response.IsCustomsResponse)
				{
					if (ConsolidatedDeclaration != null)
					{
						ConsolidatedDeclaration.Factory.Load<JobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_ClusterKey, ConsolidatedDeclaration.JobDeclarations.Select(dec => dec.JE_ClusterKey))).ForEach(invoiceLine => invoiceLine.JI_HadErrorInLastResponse = false);
					}
					else
					{
						declaration.InvoiceLines.ResetHadErrorInLastResponse();
					}
				}
				if (Response.IsTSWAcknowledgementResponse)
				{
					ProcessAcknowledgement(declaration);
				}
				else if (ResponseReceivedIsNewerThanPreviousResponses || HasBeenQueuedForReProcessing())
				{
					ProcessResponse(declaration);
					if (string.IsNullOrEmpty(Response.DeclarationID) && (Response.Status == StatusList.Codes.EntryRejected || Response.Status == StatusList.Codes.CustomsProcessingError))
					{
						var outgoingMessage = Response.OutgoingMessage;
						if (outgoingMessage != null && outgoingMessage.IsAnOriginal)
						{
							declaration.JE_EDITransmitDate = EntryHeader.CH_EDITransmitDate = ZDateTime.Empty;  // if this rejection is in response to an original message send reset transmit date
						}
					}
				}

				if (declaration.JE_EntryStatus == FormalEntryStatusList.Codes.EntryCancelled && Response.Status == StatusList.Codes.EntryCancelled)
				{
					EntryHeader.CH_IsActive = false;
					EntryHeader.CH_IsEntryCancelled = true;
				}
			}
		}

		void ProcessAcknowledgement(JobDeclaration declaration)
		{
			declaration.JE_MessageStatus = StatusListBase.Codes.Acknowledgement;
			if (declaration.DeclarationNumber.IsEmpty || declaration.IsPrimaryIndustriesImportDeclaration)
			{
				declaration.DeclarationNumber = Response.DeclarationID;
			}

			if (ResponseReceivedIsNewerThanPreviousResponses)
			{
				EntryHeader.CH_CustomsDeliveryInstructions = ZString.Empty;
			}
		}

		void ProcessResponse(JobDeclaration declaration)
		{
			EntryHeader.EntryNumber = Response.DeclarationID;
			if (!string.IsNullOrEmpty(Response.EnterpriseStatus))
			{
				if (Response.Status == StatusList.Codes.EntryRestored)
				{
					if (ConsolidatedDeclaration != null)
					{
						ConsolidatedDeclaration.JobDeclarations.Cast<JobDeclaration>().ForEach(dec => JobDeclaration.ProcessRestoredEntry(dec, dec.CusEntryHeader));
					}
					else
					{
						JobDeclaration.ProcessRestoredEntry(declaration, EntryHeader);
					}
				}
			}

			if (declaration.IsTSW_IPI_Declaration && (Response.Status == StatusList.Codes.EntryRejected || Response.Status == StatusList.Codes.CustomsProcessingError))
			{
				declaration.JE_MessageStatus = Response.Status;
				declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryRejected;
			}
			else
			{
				if (Response.Status != StatusList.Codes.DepositMayBeUpliftedMethodOfPaymentAsSpecified || declaration.JE_EntryStatus == FormalEntryStatusList.Codes.SentToCustoms)
				{
					Response.SetRelevantEntryStatus();

					if (PreviousResponsesWithSameMessageType != null)
					{
						var deliveryInstruction = EntryHeader.CH_CustomsDeliveryInstructions;
						var previousDeliveryInstructions = string.Join(System.Environment.NewLine, PreviousResponsesWithSameMessageType.DeliveryInstructions);

						if (!deliveryInstruction.IsEmpty && !string.IsNullOrEmpty(previousDeliveryInstructions))
						{
							deliveryInstruction = deliveryInstruction.ReplaceIgnoringCase(previousDeliveryInstructions, "").Replace("\r\n\r\n", "\r\n").TrimStart('\r', '\n');
						}

						var previousCustomsInstructions = string.Join(System.Environment.NewLine, PreviousResponsesWithSameMessageType.CustomsInstructions);
						if (!deliveryInstruction.IsEmpty && !string.IsNullOrEmpty(previousCustomsInstructions))
						{
							deliveryInstruction = deliveryInstruction.ReplaceIgnoringCase(previousCustomsInstructions, "").Replace("\r\n\r\n", "\r\n").TrimStart('\r', '\n');
						}

						EntryHeader.CH_CustomsDeliveryInstructions = deliveryInstruction;
					}

					if (!Response.DeliveryInstructions.IsNullOrEmpty())
					{
						var deliveryInstruction = EntryHeader.CH_CustomsDeliveryInstructions.IsEmpty ? string.Empty : EntryHeader.CH_CustomsDeliveryInstructions + System.Environment.NewLine;
						EntryHeader.CH_CustomsDeliveryInstructions = deliveryInstruction + string.Join(System.Environment.NewLine, Response.DeliveryInstructions);
					}

					if (!Response.CustomsInstructions.IsNullOrEmpty())
					{
						EntryHeader.CH_CustomsDeliveryInstructions = EntryHeader.CH_CustomsDeliveryInstructions + string.Join(System.Environment.NewLine, Response.CustomsInstructions);
					}

					if (Response.NullableTotalAmount.HasValue)
					{
						EntryHeader.CH_TotalAmountReturned = Response.NullableTotalAmount.Value;
					}

					var originalEntryStatus = declaration.JE_EntryStatus;
					if (EntryHeader.CH_IsActive)
					{
						if (!string.IsNullOrEmpty(Response.EnterpriseStatus))
						{
							declaration.AgencyMessageBeingProcessed = Response.ResponsibleGovernmentAgency;
							declaration.JE_TSWCombinedStatus = TSWStatus.GetCombinedStatus;
							declaration.JE_EntryStatus = TSWStatus.CalculateEntryStatus;
							EntryHeader.CH_EntryStatus = declaration.JE_EntryStatus;
						}
					}

					if (!string.IsNullOrEmpty(Response.PaymentMethod))
					{
						declaration.JE_PaymentMethod = PaymentMethodList.GetPaymentPartyCode(Response.PaymentMethod, declaration.JE_PaymentMethod);
					}

					//TODO: Confirm determination of new values for IsDeliveryOrderMessage && InspectionsAuditRequirements
					// Delivery Order in effect means all 3 agencies have cleared responses..
					// Inspection can be any of 3 responding agencies...
					if (Response.IsClearedStatus(declaration.JE_EntryStatus))
					{
						// TODO: to be confirmed if printing should happen to each job declaration. It may reasonably be on the consolidated level only
						if (ConsolidatedDeclaration != null)
						{
							ConsolidatedDeclaration.JobDeclarations.Cast<JobDeclaration>().ForEach(dec => ProcessClearance(dec, originalEntryStatus));
						}
						else
						{
							ProcessClearance(declaration, originalEntryStatus);
						}
					}

					foreach (var entryLine in Response.EntryLinesWithErrors)
					{
						foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
						{
							invoiceLine.JI_HadErrorInLastResponse = true;
						}
					}
				}
			}

			var info = declaration.JE_EntryStatusInfo;

			if (info.HasChanges)
			{
				Report.AddLine();
				Report.AddLine("Declaration Status Updated");
				Report.AddSeparator();
				Report.AddLine($"From {info.OriginalValue} to {info.Value}.");
				Report.AddLine();
			}
		}

		void ProcessClearance(JobDeclaration declaration, ZString originalEntryStatus)
		{
			var documentSupporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var branchPK = declaration == null ? Guid.Empty : declaration.JE_GB.ToGuid();
			var printDO = Response.IsCustomsResponse && StatusList.IsDeliveryOrderHerewithMethodOfPayment(Response.Status);
			using (DisposableEnvironment.ForBranch(branchPK, false))
			{
				documentSupporter.AutoPrintCustomsClearanceDocs(Response.OutgoingMessage, printDO, !Response.IsClearedStatus(originalEntryStatus));
			}
		}

		bool ResponseReceivedIsNewerThanPreviousResponses
		{
			get
			{
				var responseReceivedIsNewerThanPreviousResponses = false;
				var responseTime = Response.ResponseTime;
				if (Response.IsMPIFoodResponse)
				{
					if (EntryHeader.CH_MPIFoodResponseTime.IsEmpty || responseTime >= EntryHeader.CH_MPIFoodResponseTime)
					{
						responseReceivedIsNewerThanPreviousResponses = true;
						EntryHeader.CH_MPIFoodResponseTime = responseTime;
					}
				}
				else if (Response.IsMPIBiosecurityResponse)
				{
					if (EntryHeader.CH_MPIBioResponseTime.IsEmpty || responseTime >= EntryHeader.CH_MPIBioResponseTime)
					{
						responseReceivedIsNewerThanPreviousResponses = true;
						EntryHeader.CH_MPIBioResponseTime = responseTime;
					}
				}
				else if (Response.IsCustomsResponse)
				{
					if (EntryHeader.CH_NZCSResponseTime.IsEmpty || responseTime >= EntryHeader.CH_NZCSResponseTime)
					{
						responseReceivedIsNewerThanPreviousResponses = true;
						EntryHeader.CH_NZCSResponseTime = responseTime;
					}
				}
				else if (Response.IsTSWResponse)
				{
					if (EntryHeader.Declaration.IsPrimaryIndustriesImportDeclaration)
					{
						if (EntryHeader.CH_MPIFoodResponseTime.IsEmpty || responseTime >= EntryHeader.CH_MPIFoodResponseTime)
						{
							responseReceivedIsNewerThanPreviousResponses = true;
							EntryHeader.CH_NZCSResponseTime = responseTime;
						}
					}
					else
					{
						if (EntryHeader.CH_NZCSResponseTime.IsEmpty || responseTime >= EntryHeader.CH_NZCSResponseTime)
						{
							responseReceivedIsNewerThanPreviousResponses = true;
							EntryHeader.CH_NZCSResponseTime = responseTime;
						}
					}
				}

				return responseReceivedIsNewerThanPreviousResponses;
			}
		}

		protected abstract DeclarationResponse PreviousResponsesWithSameMessageType { get; }

		TSWStatus TSWStatus
		{
			get { return new TSWStatus(this); }
		}

		protected void ProcessDeliveryInstruction()
		{
			if (!Response.DeliveryInstructions.IsNullOrEmpty())
			{
				Report.AddHeader("Delivery Instructions");
				foreach (var instruction in Response.DeliveryInstructions)
				{
					Report.AddLine(instruction);
				}
			}
		}

		#endregion
	}
}
