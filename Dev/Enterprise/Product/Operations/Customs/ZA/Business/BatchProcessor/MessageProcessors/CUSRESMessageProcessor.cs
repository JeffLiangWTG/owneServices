using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.BatchProcessor.MessageProcessors;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class CUSRESMessageProcessor : ZACApplicationTypeMessageProcessor
	{
		public CUSRESMessageProcessor(LoggingInformation logger) : base(logger) { }

		static class DocumentNames
		{
			public const string CUSNOTIFICATION = "CUSNOTIFICATION";
			public const string VOC = "VOC";
			public const string SAD500 = "SAD500";
			public const string WORKSHEET = "WORKSHEET";
			public const string ZAMANIFESTWITHBARCODE = "ZA Manifest with Barcode";
		}

		static class TemplateNames
		{
			public const string CustomsDeclarationResponse = "Customs Declaration Response";
			public const string VOCDocumentPack = "Voucher Of Correction";
			public const string SADDocumentPack = "SAD Document Pack";
			public const string CustomsWorksheet = "Customs Worksheet";
			public const string RoadManifestWithBarcode = "ZA Manifest with Barcode";
		}

		static class CustomsStatusCodes
		{
			public const string StopDetainReceived = "2";
			public const string AlreadyOnCustomsSystem = "9";
			public const string AmendmentNotificationReceived = "26";
		}

		protected override string MessageFriendlyNameCore => "CUSRES Message";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { SARSEDIMessage.MessageTypes.CUSRES };

		protected override bool RequiresPreProcessingCore => true;

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason, MessageHelper Helper) TryFindLinkedObject(EDIMessage message)
		{
			var branchPK = message.EM_GB;
			BusinessObject linkedObject = null;
			MultilingualString discardReason = (NoResString)string.Empty;

			var cusresMessage = message as CUSRESEDIMessage;
			var helper = cusresMessage?.CUSRESHelper;
			if (helper is null)
			{
				discardReason = GetMessageProcessorCannotProcessMessage("CUSRES", message);
			}
			else
			{
				var factory = message.Factory;
				var outGoingMessage = CUSRESMessageProcessor.LocateOutGoingMessage(factory, helper);
				if (helper.OutgoingMessageNumber.IsEmpty || outGoingMessage is null)
				{
					outGoingMessage = CUSRESMessageProcessor.LoadOutgoingMessageFromCommonAccessReference(cusresMessage, helper);
				}
				if (outGoingMessage != null)
				{
					branchPK = outGoingMessage.EM_GB;
					linkedObject = outGoingMessage.EM_LinkedObject;
				}
				if (linkedObject != null && string.Equals(linkedObject.TableName, JobVoyage.Schema.TableName, StringComparison.InvariantCultureIgnoreCase))
				{
					// do nothing
				}
				else if (linkedObject is IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider linkedEDIFACTMessageAttachee)
				{
					Logger.Log(Res.GetString("61590CEB-B010-4E7A-BF2E-E7F0F308AD5F", "Linking {2} Message: #{0}/{3} to job: {1}", message.EM_MessageNum, linkedEDIFACTMessageAttachee.JobIdentification, "CUSRES", message.Interchange?.EI_InterchangeNum));
				}
				else
				{
					Logger.Log(GetUnableToFindTheLinkedJobMessage("CUSRES", message));
				}
			}
			return (branchPK, linkedObject, discardReason, helper);
		}

		protected override void ProcessMessageMain(EDIMessage message)
		{
			message.Factory.Saved -= BondedWarehouseMessageProcessorCreator.ProcessBondedWarehouseOnFactorySaved;

			delayEmailReport = false;
			messagePK = ZGuid.Empty;
			emailReportThatHasBeenDelayed = null;
			var successful = false;

			var incomingMessage = (CUSRESEDIMessage)message;
			var helper = incomingMessage.CUSRESHelper;
			if (helper is not null)
			{
				if (!helper.ShouldDiscardMessage)
				{
					var processed = false;
					var factory = helper.Factory;
					var linkedObject = incomingMessage.EM_LinkedObject;

					// TODO: Use EM_EM_RequestMessage to load outGoingMessage
					var outGoingMessage = LocateOutGoingMessage(factory, helper);
					if (helper.OutgoingMessageNumber.IsEmpty || outGoingMessage == null)
					{
						outGoingMessage = LoadOutgoingMessageFromCommonAccessReference(incomingMessage, helper);
					}

					if (!processed && linkedObject is CusEntryHeader linkedCusEntryHeader)
					{
						linkedCusEntryHeader.Messages.Add(incomingMessage);
						successful = ProcessForCusEntryHeader(incomingMessage, outGoingMessage, linkedCusEntryHeader, helper);
						processed = true;
					}

					if (!processed && linkedObject is IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider linkedEDIFACTMessageAttachee)
					{
						linkedEDIFACTMessageAttachee.AddMessage(incomingMessage);
						successful = ProcessForEDIFACTMessageAttachee(incomingMessage, linkedEDIFACTMessageAttachee);
						processed = true;
					}

					if (!processed && incomingMessage.EM_LinkTable.EqualsIgnoringCase(JobVoyage.Schema.TableName))
					{
						successful = incomingMessage.EM_LinkUniqueID == outGoingMessage.EM_LinkUniqueID;
						processed = true;
					}

					if (successful && helper.EntryStatus == CustomsStatusCodes.AlreadyOnCustomsSystem)
					{
						outGoingMessage.EM_Status = ZAMessage.Status.Discarded;
					}

					if (!processed && outGoingMessage == null)
					{
						if (GetCustomsEntryHeadersWithLRNFilter(factory, helper).LastOrDefault() is CusEntryHeader entryHeader)
						{
							entryHeader.Messages.Add(incomingMessage);
						}
					}
				}
				else
				{
					Logger.Log(Res.GetString("1ADD4D4C-ECEF-49EA-B023-1F8929B5439F", "Processed CUSRES Message: #{0}/{1} and did not update any job - the first 8 characters of the Interchange Recipient: {2} do not match the Agent Code: {3}.", incomingMessage.EM_MessageNum, helper.Interchange?.EI_InterchangeNum, helper?.Interchange?.UNB?.InterchangeRecipient?.RecipientIdentification, helper?.AgentCode));
					successful = true;
				}
			}
			message.EM_Status = successful ? ZAMessage.Status.ProcessedOK : ZAMessage.Status.Discarded;
		}

		bool ProcessForCusEntryHeader(CUSRESEDIMessage incomingMessage, ZAMessage outGoingMessage, CusEntryHeader linkedCusEntryHeader, CUSRESMessageHelper helper)
		{
			bool successful = true;
			var factory = incomingMessage.Factory;
			var entryStatus = helper.EntryStatus;

			if (helper.IsRejectionOfSubmissionWithinAllocatedTimeframe)
			{
				entryStatus = CustomsStatusCodes.AlreadyOnCustomsSystem;
			}

			var assessmentDate = GetAssessmentDate(helper, linkedCusEntryHeader, outGoingMessage);
			var effectiveAssessmentDate = assessmentDate.IsValid ? assessmentDate : linkedCusEntryHeader.EntryInstructionAssessmentDate;

			var shouldCopyVOCAfterValuesToBefore = !entryStatus.IsEmpty && linkedCusEntryHeader.IsVOCEntry && Universal.CustomsStatusAttributeHelper.IsStatusCleared(factory, entryStatus, Core.Constants.CountryCodes.SouthAfrica, effectiveAssessmentDate);

			UpdateEntryStatus(linkedCusEntryHeader, helper, entryStatus, outGoingMessage, effectiveAssessmentDate);
			UpdateValuationDateOverride(linkedCusEntryHeader, helper);
			UpdateAssessmentDate(incomingMessage, linkedCusEntryHeader, assessmentDate);
			if (UpdateMovementReferenceNumber(linkedCusEntryHeader, helper, entryStatus, assessmentDate))
			{
				shouldCopyVOCAfterValuesToBefore = true;
			}

			var caseNumber = helper.CaseNumber;
			if (!caseNumber.IsEmpty)
			{
				ProcessCaseNumber(linkedCusEntryHeader.EntryInstruction, helper, caseNumber);
			}

			ProcessOtherGovernmentAgencies(linkedCusEntryHeader.EntryInstruction, helper);

			var printInd = helper.CustomsPrintIndicator;
			if (!printInd.IsEmpty)
			{
				ProcessPrintIndicator(linkedCusEntryHeader, printInd);
			}

			var isStatusRejected = Universal.CustomsStatusAttributeHelper.IsStatusRejected(factory, entryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
			var isStatusCleared = Universal.CustomsStatusAttributeHelper.IsStatusCleared(factory, entryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
			if (isStatusCleared)
			{
				UpdateEntryReleaseDate(linkedCusEntryHeader, helper);
			}
			if (isStatusRejected || entryStatus == CustomsStatusCodes.AlreadyOnCustomsSystem)
			{
				DeletePendingEntryPayInfoFromOutGoingCUSDECMessageIfNeeded(linkedCusEntryHeader, outGoingMessage);
			}
			else
			{
				var isStatusFitToMarkPayInfoAsAwaitingResponse = Universal.CustomsStatusAttributeHelper.IsStatusFitToMarkPayInfoAsAwaitingResponse(factory, entryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
				AddOrUpdateEntryPayInfoFromOutGoingCUSDECMessageIfNeeded(linkedCusEntryHeader, helper, outGoingMessage, incomingMessage, isStatusCleared, isStatusRejected, isStatusFitToMarkPayInfoAsAwaitingResponse);
				AddEntryPayInfoOfPPFromIncomingCUSRESMessage(linkedCusEntryHeader, helper);
			}

			var isStatusCancelled = Universal.CustomsStatusAttributeHelper.IsStatusCancelled(factory, entryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
			UpdatePermits(linkedCusEntryHeader, incomingMessage, outGoingMessage, isStatusRejected, isStatusCleared, isStatusCancelled);

			if (shouldCopyVOCAfterValuesToBefore)
			{
				linkedCusEntryHeader.CopyMessageVOCAfterValuesToBefore(outGoingMessage);
			}

			if (isStatusRejected)
			{
				linkedCusEntryHeader.CopyMessageVOCAfterValuesToBefore((IVOCAfterValues)GetPreviousAcceptedSentCUSDECMessage(linkedCusEntryHeader, outGoingMessage.EM_MessageNum) ?? new EmptyVOCAfterValues());

				if (!linkedCusEntryHeader.IsVOCEntry)
				{
					foreach (var baseInvoiceLine in linkedCusEntryHeader.InvoiceLines)
					{
						if (baseInvoiceLine is JobComInvoiceLine invoiceLine &&
							invoiceLine.JI_TargetEntryLineNumber != 0)
						{
							invoiceLine.JI_TargetEntryLineNumber = 0;
						}
					}
				}
			}

			AutoCreateSADDocuments(linkedCusEntryHeader, incomingMessage, outGoingMessage);

			return successful;
		}

		void UpdateEntryReleaseDate(CusEntryHeader linkedCusEntryHeader, CUSRESMessageHelper helper)
		{
			if (!helper.MRNNumber.IsEmpty)
			{
				if (ZDateTime.TryParseExact(helper.MRNNumber.Substring(3, 8), out var mrnDate, MessageBuilders.Constants.DateFormatCCYYMMDD))
				{
					linkedCusEntryHeader.CH_EntryReleaseDate = mrnDate;
				}
			}
		}

		static CUSDECEDIMessage GetPreviousAcceptedSentCUSDECMessage(CusEntryHeader linkedCusEntryHeader, ZString latestCUSDECMessageNumber)
		{
			var messages = linkedCusEntryHeader.Messages;
			var lrn = linkedCusEntryHeader.CH_BGMReference;
			var outgoingMessages = messages.OfType<CUSDECEDIMessage>()
				.Where(outgoingMessage => outgoingMessage.EM_MessageNum != latestCUSDECMessageNumber && outgoingMessage.LocalReferenceNumber == lrn)
				.OrderByDescending(x => x.EM_MessageDateTime)
				.ToArray();
			var	rejectedMessageNumbers = messages.GetIncomingMessages(SARSEDIMessage.MessageTypes.CONTRL, SARSEDIMessage.MessageTypes.CUSRES)
				.Where(m => IsMessageRejected(m, linkedCusEntryHeader))
				.Select(m => m.ParentMessageNumber)
				.Distinct()
				.ToArray();
			return outgoingMessages.FirstOrDefault(m => !rejectedMessageNumbers.Contains(m.EM_MessageNum));
		}

		static bool IsMessageRejected(ZAMessage message, CusEntryHeader entryHeader)
		{
			return message.EntryStatus.HasEntryStatusGotAttribute(entryHeader.Factory, entryHeader.EntryInstructionAssessmentDate, RefCusCodeListAttributeTypes.Codes.CustomsRejected) ||
				((message is CONTRLEDIMessage c) && c.IsRejectionMessage);
		}

		bool ProcessForEDIFACTMessageAttachee(CUSRESEDIMessage incomingMessage, IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider parent)
		{
			var successful = true;
			var helper = incomingMessage.CUSRESHelper;

			var entryStatusInMessage = helper.EntryStatus;
			if (parent is IGateInOutStatusProvider govgioDataProvider && helper.IsGateInOutCUSRESMessage)
			{
				govgioDataProvider.GateInOutCustomsStatus = helper.EntryStatus;
			}
			else if (entryStatusInMessage != CustomsStatusCodeRejected || parent.JobStatus.IsEmpty)
			{
				parent.JobStatus = entryStatusInMessage;
			}
			// No need to change message status - that will already have been done by a positive CONTRL

			var caseNumber = helper.CaseNumber;

			if (parent is AsycudaManifestHeader linkedManifestHeader)
			{
				successful = ProcessForManifestHeader(incomingMessage, linkedManifestHeader);
			}

			if (parent is Integration.Customs.ASYCUDA.IAsycudaManifestHeader asycudaManifestHeader)
			{
				if (incomingMessage.CUSRESHelper.EntryStatus != CustomsStatusCodeRejected)
				{
					asycudaManifestHeader.RegistrationNumber = helper.RegistrationNumber;
				}
				if (!caseNumber.IsEmpty)
				{
					ProcessCaseNumber((ICaseNumberCollectionProvider)asycudaManifestHeader, helper, caseNumber);
				}
			}

			if (parent is Integration.Customs.ASYCUDA.IAsycudaBill asycudaBill)
			{
				if (!asycudaBill.RegistrationDate.IsValid)
				{
					if (helper.DocumentMessageDateTime.IsValid)
					{
						asycudaBill.RegistrationDate = helper.DocumentMessageDateTime;
					}
					else if (helper.MessageDate.IsValid)
					{
						asycudaBill.RegistrationDate = helper.MessageDate;
					}
				}
				if (!caseNumber.IsEmpty)
				{
					ProcessCaseNumber((ICaseNumberCollectionProvider)asycudaBill, helper, caseNumber);
				}
				ReCalculateHeaderStatus(asycudaBill, parent.Factory);
			}
			if (helper.EntryStatus == CustomsStatusCodeAccepted)
			{
				AutoCreateManifestDocuments(parent, incomingMessage, helper.LRNNumber);
			}
			return successful;
		}

		void ReCalculateHeaderStatus(Integration.Customs.ASYCUDA.IAsycudaBill asycudaBill, BusinessObjectFactory factory)
		{
			var header = factory.Load<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>(asycudaBill.ABL_AMA);
			if (header != null)
			{
				var query = new ZQuery(AsycudaBillSchema.ABL_AMA, header.PK)
					.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);

				var billsStatus = factory.Load<Integration.Customs.ASYCUDA.IAsycudaBill>(query).Select(x => x.ABL_BillStatus);

				ZString? status = null;

				if (billsStatus.Any(x => x.IsEmpty))
				{
					status = ZString.Empty;
				}
				else if (billsStatus.All(x => x == CustomsStatusCodeAccepted))
				{
					status = CustomsStatusCodeAccepted;
				}
				else if (billsStatus.Any(x => x == CustomsStatusCodeRejected))
				{
					status = CustomsStatusCodeRejected;
				}

				if (status.HasValue)
				{
					header.RegistrationStatus = status.Value;
				}
			}
		}

		bool ProcessForManifestHeader(CUSRESEDIMessage incomingMessage, AsycudaManifestHeader linkedManifestHeader)
		{
			var successful = true;
			var helper = incomingMessage.CUSRESHelper;
			var entryStatusInMessage = helper.EntryStatus;

			if (!helper.IsGateInOutCUSRESMessage)
			{
				linkedManifestHeader.RegistrationStatus = entryStatusInMessage;
				if (entryStatusInMessage != CustomsStatusCodeRejected)
				{
					linkedManifestHeader.RegistrationNumber = helper.RegistrationNumber;
				}

				Logger.Log(Res.GetString("0893eb1c-5c95-4e9c-9cfc-651a11eb3888", "Customs Status of Manifest:{0} has been updated to '{1}'.", linkedManifestHeader.AMA_MasterBill, entryStatusInMessage));

				if (helper.DocumentMessageDateTime.IsValid)
				{
					linkedManifestHeader.RegistrationDate = helper.DocumentMessageDateTime;
				}
				else if (helper.MessageDate.IsValid)
				{
					linkedManifestHeader.RegistrationDate = helper.MessageDate;
				}
			}

			return successful;
		}

		const string CustomsStatusCodeRejected = "6";
		const string CustomsStatusCodeAccepted = "8";

		internal static ZAMessage LoadOutgoingMessageFromCommonAccessReference(ZAMessage incomingMessage, CUSRESMessageHelper helper)
		{
			var car = helper.CommonAccessReference;
			return !car.IsEmpty && ZGuid.TryParse(car, out var pk)
				? incomingMessage.Factory.Load<ZAMessage>(pk)
				: null;
		}

		static ZDateTime GetAssessmentDate(CUSRESMessageHelper helper, CusEntryHeader linkedCusEntryHeader, ZAMessage outGoingMessage)
		{
			var assessmentDate = helper.AssessmentDate;
			if (assessmentDate.IsValid)
			{
				var acceptedCtrlMessage = linkedCusEntryHeader?.Messages.OfType<CONTRLEDIMessage>()
																		.FirstOrDefault(ctrlMsg => ctrlMsg.ParentMessageNumber == outGoingMessage.EM_MessageNum);

				var messageDateTime = (acceptedCtrlMessage?.Interchange as ZACInterchange)?.DateTimeOfPreparation ?? outGoingMessage.EM_MessageDateTime;

				if (assessmentDate.Date == messageDateTime.Date)
				{
					assessmentDate = messageDateTime;
				}
				else if (assessmentDate.Date == ZDate.Today)
				{
					assessmentDate = ZDateTime.Now;
				}
			}

			return assessmentDate;
		}

		static void ProcessCaseNumber(ICaseNumberCollectionProvider caseNumberCollectionProvider, CUSRESMessageHelper helper, ZString caseNumber)
		{
			AddDistinctCaseNumber(caseNumberCollectionProvider, CaseNumberTypeList.Codes.SupportingDocsRequired, caseNumber);
			if ((helper.EntryStatus == UniversalReferenceConstants.CustomsStatus.CaseClosed) && (caseNumberCollectionProvider is CusEntryInstruction))
			{
				caseNumberCollectionProvider.CaseNumbers.Cast<CaseNumber>().Where(x => x.CY_Data == caseNumber).ForEach(x => x.CY_Date = helper.Message.PreparationDate);
			}
		}

		static void ProcessOtherGovernmentAgencies(ICaseNumberCollectionProvider caseNumberCollectionProvider, CUSRESMessageHelper helper)
		{
			helper.OtherGovernmentAgencies.ForEach(caseNumber => AddDistinctCaseNumber(caseNumberCollectionProvider, CaseNumberTypeList.Codes.OtherGovernmentAgencies, caseNumber));
		}

		static void AddDistinctCaseNumber(ICaseNumberCollectionProvider caseNumberCollectionProvider, ZString caseNumberType, ZString caseNumber)
		{
			var caseNumbers = caseNumberCollectionProvider.CaseNumbers;
			if (!caseNumbers.Cast<CaseNumber>().Any(x => x.CY_Code == caseNumberType && x.CY_Data == caseNumber))
			{
				_ = caseNumbers.AddNew(caseNumberType, caseNumber);
			}
		}

		static void ProcessPrintIndicator(CusEntryHeader linkedCusEntryHeader, ZString printIndicator)
		{
			linkedCusEntryHeader.CH_RelPrintInd = printIndicator.Substring(0, ZACusEntryHeaderSchema.CH_RelPrintInd.MaxLength);
		}

		#region LocateOutGoingMessage

		internal static ZAMessage LocateOutGoingMessage(BusinessObjectFactory factory, CUSRESMessageHelper helper)
		{
			ZAMessage result = null;

			var outGoingMessageNum = helper.OutgoingMessageNumber;
			var outGoingMessageQuery = helper.GetOutgoingMessageQuery(outGoingMessageNum);
			if (outGoingMessageQuery != null)
			{
				var lrn = helper.LRNNumber;
				var outGoingMessages = factory.Load<ZAMessage>(outGoingMessageQuery);
				result = outGoingMessages.FirstOrDefault(x => x.LocalReferenceNumber == lrn);
			}

			if (result == null)
			{
				if (!outGoingMessageNum.IsEmpty)
				{
					foreach (var entryHeader in GetCustomsEntryHeadersWithLRNFilter(factory, helper))
					{
						var entryMessages = entryHeader.Messages.GetMatchingMessages(ZAMessage.ApplicationCodes.SouthAfricanCustoms, new ZString[] { SARSEDIMessage.MessageTypes.CUSDEC }, Messaging.Business.EDIInterchange.Direction.Transmit).ToList();
						var cusDecMessage = entryMessages.Find(x => CUSDECMessageHelper.New((ZAMessage)x).MessageNumber == outGoingMessageNum) as ZAMessage;
						if (cusDecMessage != null)
						{
							return cusDecMessage;
						}
					}
				}
			}
			return result;
		}

		internal static IEnumerable<CusEntryHeader> GetCustomsEntryHeadersWithLRNFilter(BusinessObjectFactory factory, CUSRESMessageHelper helper)
		{
			IEnumerable<CusEntryHeader> result = null;
			var lrn = helper.LRNNumber;
			if (lrn.IsEmpty)
			{
				result = Enumerable.Empty<CusEntryHeader>();
			}
			else
			{
				var lrnQuery = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, lrn);
				lrnQuery.OrderBy = CusEntryHeader.Schema.CH_SystemCreateTimeUtc;
				return factory.Load<CusEntryHeader>(lrnQuery);
			}
			return result;
		}

		#endregion

		#region Update Methods

		void UpdateEntryStatus(CusEntryHeader linkedCusEntryHeader, CUSRESMessageHelper helper, ZString entryStatus, ZAMessage outGoingMessage, ZDateTime assessmentDate)
		{
			var factory = linkedCusEntryHeader.Factory;
			var hasUnknownEntryStatus = !entryStatus.IsEmpty && !ZARefCusCodeListTypes.GetCustomsStatusList(factory).ContainsCode(entryStatus);

			if (hasUnknownEntryStatus || helper.DoesEntryStatusNeedsToBeUpdated(linkedCusEntryHeader))
			{
				linkedCusEntryHeader.CH_EntryStatus = entryStatus;
				_ = linkedCusEntryHeader.Logs.AddNew(Events.CustomsEntryStatus, linkedCusEntryHeader.CH_EntryStatus, helper.MessageDate.ToOffset());
				Logger.Log(Res.GetString("F6AC0014-5674-4AE8-9DAB-8659D1D61599", "Entry Status of Entry:{0} has been updated to '{1}'.", linkedCusEntryHeader.CH_BGMReference, entryStatus));
			}
			if (linkedCusEntryHeader.SupportsBondedWarehousing && ShouldUpdateBondedWhs(linkedCusEntryHeader, assessmentDate))
			{
				linkedCusEntryHeader.Factory.Saved -= BondedWarehouseMessageProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
				linkedCusEntryHeader.Factory.Saved += BondedWarehouseMessageProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
				delayEmailReport = true;
				messagePK = outGoingMessage.PK;
			}

			if (hasUnknownEntryStatus || Universal.CustomsStatusAttributeHelper.ShouldNotify(factory, helper.EntryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
			{
				EmailNotification(linkedCusEntryHeader, helper, outGoingMessage, hasUnknownEntryStatus);
			}
		}
		ZGuid messagePK;
		bool delayEmailReport;
		EmailDef emailReportThatHasBeenDelayed;

		BondedWarehouseMessageProcessorCreator BondedWarehouseMessageProcessorCreator => bondedWarehouseMessageProcessorCreator ??= new BondedWarehouseMessageProcessorCreator(Logger, GetNewBondedWarehouseMessageProcessor, GetFallbackNotificationGroupPK);
		BondedWarehouseMessageProcessorCreator bondedWarehouseMessageProcessorCreator;

		BondedWarehouseMessageProcessor GetNewBondedWarehouseMessageProcessor(Action<EmailDef, EDIMessage> sendEmail)
		{
			return new BondedWarehouseMessageProcessor(messagePK, emailReportThatHasBeenDelayed, sendEmail);
		}

		static bool ShouldUpdateBondedWhs(CusEntryHeader entry, ZDateTime assessmentDate)
		{
			var result = IsBondedWhsStatusPending(entry);
			if (result)
			{
				var entryStatus = entry.CH_EntryStatus;
				result = CustomsStatusAttributeHelper.ShouldUpdateBondedWhs(entry.Factory, entryStatus, Core.Constants.CountryCodes.SouthAfrica, assessmentDate) ||
					CustomsStatusAttributeHelper.ShouldCancelBondedWhs(entry.Factory, entryStatus, Core.Constants.CountryCodes.SouthAfrica, assessmentDate);
			}
			return result;
		}

		static bool IsBondedWhsStatusPending(CusEntryHeader entry)
		{
			var whsStatus = entry.CH_WarehouseTransactionStatus;
			return WarehouseTransactionStatusList.IsPendingInward(whsStatus)
				|| WarehouseTransactionStatusList.IsPendingOutward(whsStatus)
				|| WarehouseTransactionStatusList.IsPendingChangeOfOwnership(whsStatus);
		}

		void UpdateValuationDateOverride(CusEntryHeader linkedCusEntryHeader, CUSRESMessageHelper helper)
		{
			if (linkedCusEntryHeader.Declaration?.IsExport ?? false)
			{
				var entryInstruction = linkedCusEntryHeader.EntryInstruction;
				var entryStatus = linkedCusEntryHeader.CH_EntryStatus;
				var isStatusRejected = CustomsStatusAttributeHelper.IsStatusRejected(linkedCusEntryHeader.Factory, entryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
				if (isStatusRejected && !linkedCusEntryHeader.IsVOCEntry)
				{
					entryInstruction.CEI_ExchangeRateDate = ZDateTime.Empty;
					Logger.Log(Res.GetString("9340B184-4759-4C17-8C73-E962434132C8", "Exchange Rate Date of entry instruction linked to entry:{0} has been cleared because of message rejection.", linkedCusEntryHeader.CH_BGMReference));
				}
				else if (!isStatusRejected && !helper.MRNNumber.IsEmpty)
				{
					if (ZDateTime.TryParseExact(helper.MRNNumber.Substring(3, 8), out var mrnDate, "yyyyMMdd"))
					{
						mrnDate = mrnDate.AddDays(-1);
						if (entryInstruction.CEI_ExchangeRateDate.IsEmpty || !entryInstruction.CEI_ExchangeRateDate.IsValid)
						{
							entryInstruction.CEI_ExchangeRateDate = mrnDate;
							Logger.Log(Res.GetString("AB759836-908B-47E5-A289-A062F97D52D9", "Exchange Rate Date of entry instruction linked to entry:{0} has been updated to '{1}'.", linkedCusEntryHeader.CH_BGMReference, mrnDate));
						}
					}
				}
			}
		}

		static void UpdateAssessmentDate(CUSRESEDIMessage incomingMessage, CusEntryHeader linkedCusEntryHeader, ZDateTime assessmentDate)
		{
			if (!assessmentDate.IsEmpty)
			{
				var entryInstruction = linkedCusEntryHeader.EntryInstruction;
				if (entryInstruction != null && entryInstruction.CEI_DateForDuty != assessmentDate)
				{
					var oldAssessmentDate = entryInstruction.CEI_DateForDuty;
					entryInstruction.CEI_DateForDuty = assessmentDate;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					_ = incomingMessage.Logs.AddNew(Events.EditedARecord, Invariant($"Assessment Date of Entry:{linkedCusEntryHeader.CH_BGMReference} has been updated from '{oldAssessmentDate.ToStandardDateTimeString()}' to '{entryInstruction.CEI_DateForDuty.ToStandardDateTimeString()}'."));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		static bool UpdateMovementReferenceNumber(CusEntryHeader linkedCusEntry, CUSRESMessageHelper helper, ZString entrStatusForLogic, ZDateTime assessmentDate)
		{
			var result = false;
			if (Universal.CustomsStatusAttributeHelper.ShouldUpdateEntryNumber(linkedCusEntry.Factory, entrStatusForLogic, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
			{
				var mrnNumber = helper.MRNNumber;
				if (!mrnNumber.IsEmpty)
				{
					result = linkedCusEntry.MovementReferenceNumber.IsEmpty;
					linkedCusEntry.MovementReferenceNumberSetter(mrnNumber, assessmentDate);
					linkedCusEntry.MarkNeedsAutoRateDSB();
				}
			}
			return result;
		}

		void UpdatePermits(CusEntryHeader entryHeader, EDIMessage incomingMessage, EDIMessage outgoingMessage, bool isStatusRejected, bool isStatusCleared, bool isStatusCancelled)
		{
			var outgoingCUSDECMessage = outgoingMessage as CUSDECEDIMessage;
			if (outgoingCUSDECMessage != null)
			{
				var messageSubType = outgoingMessage.EM_MessageSubType;
				if (isStatusRejected && messageSubType != MessageSubTypeCodes.Codes.Cancellation)
				{
					ZAPermitHelper.UpdatePendingTransactions(entryHeader, incomingMessage, outgoingMessage, Logger, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
				}

				if (isStatusCancelled)
				{
					ZAPermitHelper.RollbackPermitTransactionsForEntry(entryHeader, incomingMessage, outgoingMessage, Logger, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
				}

				if (isStatusCleared)
				{
					ZAPermitHelper.UpdatePendingTransactions(entryHeader, incomingMessage, outgoingMessage, Logger, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
				}
			}
		}
		#endregion

		#region AutoCreateSADDocuments

		class LoggerWrapper : INotifications
		{
			public LoggerWrapper(LoggingInformation logger)
			{
				this.logger = logger;
			}

			readonly LoggingInformation logger;

			public void Add(INotification notification)
			{
				if (notification.Type == CargoWise.ComponentModel.NotificationType.Error)
				{
					logger.LogError(notification.Message);
				}
				else if (notification.Type == CargoWise.ComponentModel.NotificationType.Warning)
				{
					logger.LogWarning(notification.Message);
				}
				else
				{
					logger.Log(notification.Message);
				}
			}
		}

		void AutoCreateSADDocuments(CusEntryHeader linkedCusEntry, ZAMessage incomingMessage, ZAMessage outgoingMessage)
		{
			var factory = linkedCusEntry.Factory;
			if (CustomsStatusAttributeHelper.ShouldAddEntryDocsToEDocs(factory, incomingMessage.EntryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
			{
				var messageType = outgoingMessage.EM_MessageSubType;
				var receivedDate = incomingMessage.EM_SystemCreateTimeUtc.ToString("yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture);
				var localReferenceNumber = linkedCusEntry.CH_BGMReference;
				var fileName = ZString.Format("{0}_{{0}}_{1}_{2}", messageType, localReferenceNumber, receivedDate);

				var incomingCUSRESMessage = incomingMessage as CUSRESEDIMessage;
				if (incomingCUSRESMessage != null)
				{
					DeliverDocument(factory, incomingCUSRESMessage, TemplateNames.CustomsDeclarationResponse, BusinessContext.EDIMessage, ZString.Format(fileName, DocumentNames.CUSNOTIFICATION), localReferenceNumber);
				}
				if (!linkedCusEntry.IsExWarehouse)
				{
					DeliverDocument(factory, linkedCusEntry, TemplateNames.CustomsWorksheet, BusinessContext.CusEntryHeader, ZString.Format(fileName, DocumentNames.WORKSHEET), localReferenceNumber);
				}

				var outgoingCUSDECMessage = outgoingMessage as CUSDECEDIMessage;
				if (outgoingCUSDECMessage != null)
				{
					if (messageType == MessageSubTypeCodes.Codes.Change || messageType == MessageSubTypeCodes.Codes.Cancellation)
					{
						DeliverDocument(factory, outgoingCUSDECMessage, TemplateNames.VOCDocumentPack, BusinessContext.EDIMessage, ZString.Format(fileName, DocumentNames.VOC), localReferenceNumber);
					}
					else if (messageType == MessageSubTypeCodes.Codes.Original || messageType == MessageSubTypeCodes.Codes.Replace)
					{
						DeliverDocument(factory, outgoingCUSDECMessage, TemplateNames.SADDocumentPack, BusinessContext.EDIMessage, ZString.Format(fileName, DocumentNames.SAD500), localReferenceNumber);
					}
				}
			}
		}

		void DeliverDocument(BusinessObjectFactory factory, IDocumentSupportable linkedCusEntryHeader, ZString templateName, BusinessContext businessContext, ZString fileName, ZString jobReferenceNumber)
		{
			try
			{
				var documentFilter = new DocumentZQuery(businessContext, templateName)
					.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
				var documentCommand = factory.LoadTop1<DocumentCommand>(documentFilter);
				var deliveryJob = new CUSRESAutoEDocDeliveryJob(linkedCusEntryHeader, documentCommand.PK, ZGuid.Empty, fileName);
				deliveryJob.Deliver(new LoggerWrapper(Logger));
				Logger.Log(Res.GetString("70A74D9D-CD1F-4F00-9C12-07F89070B62C", "Auto created eDoc:{0} for entry:{1}.", fileName, jobReferenceNumber));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Logger.LogError(Res.GetString("3B4518D3-88B3-4275-9B43-D4D3139D823B", "Unable to create eDoc:{0} for entry:{1}. {2}", fileName, jobReferenceNumber, e.Message));
			}
		}
		#endregion

		#region AutoCreateManifestDocuments
		void AutoCreateManifestDocuments(IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider linkedEDIFACTMessageAttachee, ZAMessage incomingMessage, ZString localReferenceNumber)
		{
			if (incomingMessage is CUSRESEDIMessage && AsycudaUniversalReference.CustomsStatusAttributeHelper.ShouldAddEntryDocsToEDocs(linkedEDIFACTMessageAttachee.Factory, incomingMessage.EntryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
			{
				var header = GetManifestHeaderLinkedToEDIFACTMessage(linkedEDIFACTMessageAttachee.Factory, linkedEDIFACTMessageAttachee);
				if (header?.AMA_ManifestType.ToString() == nameof(ManifestDocumentType.RFM))
				{
					DeliverDocument(linkedEDIFACTMessageAttachee.Factory, header as IDocumentSupportable, TemplateNames.RoadManifestWithBarcode, BusinessContext.AsycudaManifest, DocumentNames.ZAMANIFESTWITHBARCODE, localReferenceNumber);
				}
			}
		}
		Integration.Customs.ASYCUDA.IAsycudaManifestHeader GetManifestHeaderLinkedToEDIFACTMessage(BusinessObjectFactory factory, IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider linkedEDIFACTMessageAttachee)
		{
			if (linkedEDIFACTMessageAttachee is Integration.Customs.ASYCUDA.IAsycudaBill asycudaBill)
			{
				var header = factory.Load<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>(asycudaBill.ABL_AMA);
				return header;
			}
			return linkedEDIFACTMessageAttachee as Integration.Customs.ASYCUDA.IAsycudaManifestHeader;
		}

		#endregion

		#region AddNewEntryPayInfoFromMessages

		void DeletePendingEntryPayInfoFromOutGoingCUSDECMessageIfNeeded(CusEntryHeader linkedCusEntryHeader, ZAMessage cusdecEDIMessage)
		{
			var cusdecMessageNum = cusdecEDIMessage.EM_MessageNum;
			var entryPayInfos = linkedCusEntryHeader.EntryPayInfos;
			var pendingEntryPayInfos = entryPayInfos.Cast<Customs.Business.CusEntryPayInfo>()
				.Where(x => x.C9_IncomingPayResponseNo == cusdecMessageNum
							&& x.C9_PaymentStatus == CusEntryPayInfoStatusList.Codes.Pending)
				.ToArray();
			if (pendingEntryPayInfos.Length > 0)
			{
				foreach (var pendingEntryPayInfo in pendingEntryPayInfos)
				{
					CaseNumberHelper.DeleteCaseNumberFromProvisionalPaymentCusEntryPayInfo(linkedCusEntryHeader, pendingEntryPayInfo as CusEntryPayInfo);
					entryPayInfos.RemoveAndDelete(pendingEntryPayInfo);
				}
				Logger.Log(Res.GetString("339e61a5-4830-4aa2-9253-2308f784aaf1", "Entry Pay Info with the '{0}' payment status deleted on Customs Entry: {1}", CusEntryPayInfoStatusList.Codes.Pending, linkedCusEntryHeader.CH_BGMReference));
			}
		}

		void AddOrUpdateEntryPayInfoFromOutGoingCUSDECMessageIfNeeded(CusEntryHeader linkedCusEntryHeader, CUSRESMessageHelper helper, ZAMessage cusdecEDIMessage, CUSRESEDIMessage incomingMessage, ZBool isStatusCleared, ZBool isStatusRejected, ZBool isStatusFitToMarkPayInfoAsAwaitingResponse)
		{
			var dateTimeOfPayment = helper.PostingDate;
			var cusdecHelper = CUSDECMessageHelper.New(cusdecEDIMessage);
			if (cusdecHelper != null)
			{
				var paymentParty = cusdecHelper.PaymentMethod;
				var cusdecMessageNum = cusdecEDIMessage.EM_MessageNum;

				if (dateTimeOfPayment.IsEmpty && paymentParty == PaymentMethodCodeList.Codes.Cash)
				{
					dateTimeOfPayment = incomingMessage.EM_MessageDateTime.Date;
				}
				var entryPayInfos = linkedCusEntryHeader.EntryPayInfos.Cast<Customs.Business.CusEntryPayInfo>().Where(x => x.C9_IncomingPayResponseNo == cusdecMessageNum).ToArray();

				if (dateTimeOfPayment.IsValid)
				{
					if (entryPayInfos.Length == 0)
					{
						var paymentStatus = ZString.Empty;
						if (isStatusCleared)
						{
							paymentStatus = CusEntryPayInfoStatusList.Codes.Clear;
							EntryPayInfoHelper.AddEntryPayInfoFromOutGoingCUSDECMessage(linkedCusEntryHeader, cusdecEDIMessage, dateTimeOfPayment, paymentParty, paymentStatus);
						}
						else if (paymentParty != PaymentMethodCodeList.Codes.Cash || !RejectedMessageExists(linkedCusEntryHeader, cusdecMessageNum, incomingMessage))
						{
							paymentStatus = CusEntryPayInfoStatusList.Codes.Pending;
							EntryPayInfoHelper.AddEntryPayInfoFromOutGoingCUSDECMessage(linkedCusEntryHeader, cusdecEDIMessage, dateTimeOfPayment, paymentParty, paymentStatus);
						}

						if (!paymentStatus.IsEmpty)
						{
							Logger.Log(Res.GetString("8c8638b0-9740-409f-b06a-d1a1def6286a", "Entry Pay Info with the '{0}' payment status added on Customs Entry: {1}", paymentStatus, linkedCusEntryHeader.CH_BGMReference));
						}
					}
					if (isStatusCleared)
					{
						var oldPayInfosInAWRStatus = linkedCusEntryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Where(x => x.C9_PaymentStatus == CusEntryPayInfoStatusList.Codes.AwaitingResponse).ToArray();
						UpdateEntryPayInfoWhenStatusIsCleared(linkedCusEntryHeader, dateTimeOfPayment, entryPayInfos.Union(oldPayInfosInAWRStatus).ToArray());
					}
					else if (!isStatusCleared && !isStatusRejected)
					{
						UpdateEntryPayInfo_PaymentDate_Only(dateTimeOfPayment, entryPayInfos);
					}

					if (isStatusFitToMarkPayInfoAsAwaitingResponse)
					{
						UpdateEntryPayInfoWhenStatusIsAwaitingResponse(linkedCusEntryHeader, entryPayInfos);
					}
				}
				else if (isStatusCleared)
				{
					UpdateEntryPayInfo_PaymentStatus_Only_WhenIsCleared(linkedCusEntryHeader, entryPayInfos);
				}
			}
		}

		void UpdateEntryPayInfoWhenStatusIsAwaitingResponse(CusEntryHeader linkedCusEntryHeader, Customs.Business.CusEntryPayInfo[] entryPayInfosForCurrentMsg)
		{
			var paymentStatusUpdated = false;

			foreach (var entryPayInfo in entryPayInfosForCurrentMsg)
			{
				if (entryPayInfo.C9_PaymentStatus == CusEntryPayInfoStatusList.Codes.Pending)
				{
					entryPayInfo.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.AwaitingResponse;
					paymentStatusUpdated = true;
				}
			}

			if (paymentStatusUpdated)
			{
				Logger.Log(Res.GetString("CD3E8E57-AE1E-4E78-AB67-6AE606B38FA1", "Entry Pay Info payment status updated from '{0}' to '{1}' on Customs Entry: {2}", CusEntryPayInfoStatusList.Codes.Pending, CusEntryPayInfoStatusList.Codes.AwaitingResponse, linkedCusEntryHeader.CH_BGMReference));
			}
		}

		void UpdateEntryPayInfoWhenStatusIsCleared(CusEntryHeader linkedCusEntryHeader, ZDateTime dateTimeOfPayment, Customs.Business.CusEntryPayInfo[] entryPayInfosForCurrentMsg)
		{
			var paymentStatusUpdated = false;

			foreach (var entryPayInfo in entryPayInfosForCurrentMsg)
			{
				if (entryPayInfo.C9_PaymentStatus == CusEntryPayInfoStatusList.Codes.Pending || entryPayInfo.C9_PaymentStatus == CusEntryPayInfoStatusList.Codes.AwaitingResponse)
				{
					entryPayInfo.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Clear;
					entryPayInfo.C9_PaymentDate = dateTimeOfPayment;
					paymentStatusUpdated = true;
				}
			}

			var pendingEntryPayInfos = linkedCusEntryHeader.EntryPayInfos.Cast<Customs.Business.CusEntryPayInfo>().Where(x => x.C9_PaymentStatus == CusEntryPayInfoStatusList.Codes.Pending).ToArray();
			if (pendingEntryPayInfos.Length > 0)
			{
				var receivedCUSRESMessagesCUSDECMessageNum = linkedCusEntryHeader.Messages.OfType<CUSRESEDIMessage>().Where(x => IsAStatusShouldUpdateEntryPayInfo(x.EntryStatus)).Select(x => x.ParentMessageNumber).Distinct();
				foreach (var item in pendingEntryPayInfos.Where(x => receivedCUSRESMessagesCUSDECMessageNum.Contains(x.C9_IncomingPayResponseNo)))
				{
					item.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Clear;
					item.C9_PaymentDate = dateTimeOfPayment;
					paymentStatusUpdated = true;
				}
			}

			if (paymentStatusUpdated)
			{
				Logger.Log(Res.GetString("0a95572a-d9b5-47fd-bad7-b5ba79dfdff6", "Entry Pay Info payment status updated from '{0}' to '{1}' on Customs Entry: {2}", CusEntryPayInfoStatusList.Codes.Pending, CusEntryPayInfoStatusList.Codes.Clear, linkedCusEntryHeader.CH_BGMReference));
			}
		}

		void UpdateEntryPayInfo_PaymentDate_Only(ZDateTime dateTimeOfPayment, Customs.Business.CusEntryPayInfo[] entryPayInfosForCurrentMsg)
		{
			foreach (var entryPayInfo in entryPayInfosForCurrentMsg)
			{
				entryPayInfo.C9_PaymentDate = dateTimeOfPayment;
			}
		}

		void UpdateEntryPayInfo_PaymentStatus_Only_WhenIsCleared(CusEntryHeader linkedCusEntryHeader, Customs.Business.CusEntryPayInfo[] entryPayInfosForCurrentMsg)
		{
			var paymentStatusUpdated = false;

			foreach (var entryPayInfo in entryPayInfosForCurrentMsg)
			{
				if (entryPayInfo.C9_PaymentStatus == CusEntryPayInfoStatusList.Codes.Pending)
				{
					entryPayInfo.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Clear;
					paymentStatusUpdated = true;
				}
			}

			if (paymentStatusUpdated)
			{
				Logger.Log(Res.GetString("1A9BB067-7833-444D-A2B9-FE9DAD501723", "Entry Pay Info payment status updated from '{0}' to '{1}' on Customs Entry: {2}", CusEntryPayInfoStatusList.Codes.Pending, CusEntryPayInfoStatusList.Codes.Clear, linkedCusEntryHeader.CH_BGMReference));
			}
		}

		static bool IsAStatusShouldUpdateEntryPayInfo(string entryStatus)
		{
			return entryStatus == CustomsStatusCodes.AmendmentNotificationReceived || entryStatus == CustomsStatusCodes.StopDetainReceived;
		}

		static bool RejectedMessageExists(CusEntryHeader linkedCusEntryHeader, ZString cusdecMessageNum, CUSRESEDIMessage incomingMessage)
		{
			return linkedCusEntryHeader.Messages.OfType<CUSRESEDIMessage>().Any(m => m.PK != incomingMessage.PK && m.ParentMessageNumber == cusdecMessageNum && Universal.CustomsStatusAttributeHelper.IsStatusRejected(linkedCusEntryHeader.Factory, m.EntryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today));
		}

		void AddEntryPayInfoOfPPFromIncomingCUSRESMessage(CusEntryHeader linkedCusEntryHeader, CUSRESMessageHelper helper)
		{
			var existingPPPayInfos = linkedCusEntryHeader.EntryPayInfos.OfType<ProvisionalPaymentCusEntryPayInfo>();
			var ppStatus = helper.EntryStatus;
			ppStatus = CustomsStatusAttributeHelper.HasAttribute(linkedCusEntryHeader.Factory, RefCusCodeListAttributeTypes.Codes.IUpdateProvisionalPaymentStatus, ppStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today) ? ppStatus : ZString.Empty;
			foreach (var ppAdditionalInfo in helper.AdditionalProvisionalPaymentInfos.Where(x => x.IsValid))
			{
				UpdateProvisionalPaymentInfoToCusEntryInstruction(linkedCusEntryHeader?.EntryInstruction, ppAdditionalInfo, ppStatus);
				UpdateProvisionalPaymentInfoToCusEntryPayInfo(linkedCusEntryHeader, existingPPPayInfos, helper, ppAdditionalInfo, ppStatus);
			}
		}

		void UpdateProvisionalPaymentInfoToCusEntryInstruction(CusEntryInstruction entryInstruction, ProvisionalPaymentAdditionalInfo ppAdditionalInfo, ZString ppStatus)
		{
			if (ppAdditionalInfo.IsHeaderLevelInfo && entryInstruction != null)
			{
				if (CustomsStatusAttributeHelper.HasAttribute(entryInstruction.Factory, RefCusCodeListAttributeTypes.Codes.IsLiquidatedStatus, ppStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
				{
					entryInstruction.CEI_ProvisionalPaymentType = ZString.Empty;
					entryInstruction.CEI_ProvisionalPaymentAmount = ZDecimal.Zero;
					Logger.Log(Res.GetString("79C71EF8-6B4E-4824-A926-878E398FE165", "Header Level Provisional Payment Pay Info of '{0} - {1}' on Declaration {2} has been liquidated'.", entryInstruction.CEI_Style, entryInstruction.CEI_Description, entryInstruction.JobDeclaration?.JE_DeclarationReference));
				}
				else if (entryInstruction.CEI_ProvisionalPaymentType == ppAdditionalInfo.DutyType)
				{
					entryInstruction.CEI_ProvisionalPaymentAmount = ppAdditionalInfo.PPAmount;
					Logger.Log(Res.GetString("9BA806AC-44EF-4996-AC45-D14DB2B457F9", "Header Level Provisional Payment Pay Info of '{0} - {1}' on Declaration {2} has been updated'.", entryInstruction.CEI_Style, entryInstruction.CEI_Description, entryInstruction.JobDeclaration?.JE_DeclarationReference));
				}
			}
		}

		void UpdateProvisionalPaymentInfoToCusEntryPayInfo(CusEntryHeader linkedCusEntryHeader, IEnumerable<ProvisionalPaymentCusEntryPayInfo> existingPPPayInfos, CUSRESMessageHelper helper, ProvisionalPaymentAdditionalInfo ppAdditionalInfo, ZString ppStatus)
		{
			var ppNo = ppAdditionalInfo.PPNo;
			var ppType = ppAdditionalInfo.DutyType;
			var lineNo = ppAdditionalInfo.EntryLineNumber;
			var targetEntryPayInfo = existingPPPayInfos.FirstOrDefault(x => x.C9_PaymentReference == ppNo && x.C9_TransactionType == ppType && x.C9_IncomingPayResponseNo == lineNo);
			if (targetEntryPayInfo == null)
			{
				targetEntryPayInfo = AddProvisionalPaymentEntryPayInfo(linkedCusEntryHeader, ppAdditionalInfo.ExpiryDate, ppType, ppAdditionalInfo.PPAmount, ppAdditionalInfo.EntryLineNumber.ToString(), ppNo);
				if (targetEntryPayInfo != null)
				{
					Logger.Log(Res.GetString("2D998124-E114-4205-BAB1-B20106D7D93E", "Provisional Payment Pay Info added on Customs Entry: {0} on Line No: {1} of type '{2}' with Reference Number '{3}'.", linkedCusEntryHeader.CH_BGMReference, lineNo, ppType, ppNo));
				}
			}
			else
			{
				targetEntryPayInfo.C9_PaymentAmount = ppAdditionalInfo.PPAmount;
				targetEntryPayInfo.C9_PaymentDate = ppAdditionalInfo.ExpiryDate;
				targetEntryPayInfo.C9_IncomingPayResponseNo = lineNo;
				Logger.Log(Res.GetString("928ABEDD-83B2-4234-B276-5129E942DB1C", "Provisional Payment Pay Info '{2}' of '{1}' on Customs Entry: {0} on Line No: {3} has been updated'.", linkedCusEntryHeader.CH_BGMReference, ppType, ppNo, lineNo));
			}

			if (!ppStatus.IsEmpty && targetEntryPayInfo != null)
			{
				targetEntryPayInfo.C9_PaymentStatus = ppStatus;

				if (CustomsStatusAttributeHelper.HasAttribute(linkedCusEntryHeader.Factory, RefCusCodeListAttributeTypes.Codes.IUpdateProvPayLiquidationDate, ppStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
				{
					if (helper.MessageDate.IsValid)
					{
						targetEntryPayInfo.C9_ReceiptDate = helper.MessageDate.Date;
					}
				}
			}
		}

		static ProvisionalPaymentCusEntryPayInfo AddProvisionalPaymentEntryPayInfo(CusEntryHeader linkedCusEntryHeader, ZDateTime dateTimeOfPayment, ZString transactionType, ZDecimal dutyAmount, ZString messageNum, ZString paymentReference)
		{
			var factory = linkedCusEntryHeader.Factory;
			var newPPPayInfo = factory.New<ProvisionalPaymentCusEntryPayInfo>();
			using (newPPPayInfo.SuspendSettingHasChanges())
			using (newPPPayInfo.GetValidationSuspender())
			{
				newPPPayInfo.C9_PaymentDate = dateTimeOfPayment;
				newPPPayInfo.C9_CusResReceived = true;
				newPPPayInfo.C9_RemAdvReceived = false;
				newPPPayInfo.C9_PaymentParty = PaymentMethodCodeList.Codes.Cash;
				newPPPayInfo.C9_TransactionType = transactionType;
				newPPPayInfo.C9_PaymentAmount = dutyAmount;
				newPPPayInfo.C9_PaymentReference = paymentReference;
				newPPPayInfo.C9_IncomingPayResponseNo = messageNum;
				((ILightValidationInternals)newPPPayInfo).IsValid = true;
			}
			linkedCusEntryHeader.EntryPayInfos.Add(newPPPayInfo);
			CaseNumberHelper.CreateNewCaseNumberFromProvisionalPaymentCusEntryPayInfo(linkedCusEntryHeader, newPPPayInfo);
			return newPPPayInfo;
		}

		#endregion

		#region Email

		void EmailNotification(CusEntryHeader linkedCusEntryHeader, CUSRESMessageHelper helper, ZAMessage outGoingMessage, bool hasUnknownEntryStatus)
		{
			var email = GenerateEmail(linkedCusEntryHeader, helper, outGoingMessage, hasUnknownEntryStatus);
			if (email != null)
			{
				BondedWarehouseMessageProcessorCreator.AddFallBackRecipient(email, linkedCusEntryHeader.Declaration, outGoingMessage);
				if (delayEmailReport && emailReportThatHasBeenDelayed == null)
				{
					emailReportThatHasBeenDelayed = email;
				}
				else
				{
					BondedWarehouseMessageProcessorCreator.SendEmail(email, outGoingMessage);
				}
			}
		}

		static EmailDef GenerateEmail(CusEntryHeader linkedCusEntryHeader, CUSRESMessageHelper helper, ZAMessage outGoingMessage, bool hasUnknownEntryStatus)
		{
			EmailDef result = null;
			var declaration = linkedCusEntryHeader.Declaration;
			var cusdecHelper = CUSDECMessageHelper.New(outGoingMessage);
			if (cusdecHelper != null && declaration != null)
			{
				var statusCode = helper.Message.EntryStatus;
				var subject = ZString.Format("Entry Notification: 'Code {0} - {1}', {2}", statusCode, helper.Message.EntryStatusDescription, declaration.JE_DeclarationReference);
				var email = new EmailDefBuilder(subject, EmailDefBuilder.HtmlTemplates.EmptyWithDynamicHtml5);

				var jobLink = ZString.Format("Job Number: <a href=\"{0}\">{1}</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration), declaration.JE_DeclarationReference);
				email.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtmlHeading, jobLink);

#pragma warning disable CA1861 // Avoid constant arrays as arguments
				var creator = new HtmlTableCreator(new string[] { "Column", "Value" });
#pragma warning restore CA1861 // Avoid constant arrays as arguments
				creator.WriteRow("Shipment Type", declaration.JE_MessageType);
				creator.WriteRow("Customs Office", helper.CustomsOfficeDescription);
				creator.WriteRow("Importer", cusdecHelper.Importer?.Name);
				creator.WriteRow("Main Supplier", cusdecHelper.Exporter == null ? cusdecHelper.Supplier?.Name : cusdecHelper.Exporter?.Name);
				creator.WriteRow("Depot", cusdecHelper.LocationOfGoodsName);
				creator.WriteRow("CPC", cusdecHelper.CustomsProcedureCode);
				email.AddDynamicHtmlReplacement(creator.ToHtml());
				if (hasUnknownEntryStatus)
				{
					email.AddDynamicHtmlReplacement(ZString.Format("Code '{0}' does not exists in the reference file database; please contact support to fix this problem.", statusCode));
				}

				result = email.ToEmail();

				if (hasUnknownEntryStatus || Universal.CustomsStatusAttributeHelper.ShouldSendEntryDocs(linkedCusEntryHeader.Factory, helper.EntryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
				{
					var template = ExcelTemplateRetriever.GetTemplate("Customs Declaration Response", CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair, null);
					var docDataProvider = BODocDataProvider.Get(new CUSDECCUSRESMessagePairDocumentWrapper(linkedCusEntryHeader));
					using var report = new Report(null, template, docDataProvider, template.TemplateName, null, DocumentDirection.ANY, false);
					using var outputStream = new MemoryStream();
					_ = report.Save(outputStream);
					var binaryData = DocumentConverter.ConvertFromExcel(outputStream.ToArray(), OutputFormatType.PDF, ColourDepth.BlackAndWhite);
					_ = result.Attachments.Add(new AttachmentDef("Customs Notification.pdf", binaryData));
				}
			}

			return result;
		}

		static Guid GetFallbackNotificationGroupPK(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return ZACustomsRegistry.Instance.FallbackNotificationGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}

		#endregion
	}
}
