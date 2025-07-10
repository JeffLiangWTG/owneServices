using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.NCTS.Business;

public abstract class NCTSResponseMessageProcessor<TDataProvider> : NLBranchCustomsApplicationTypeMessageProcessor<TDataProvider>
	where TDataProvider : INCTSIncomingDataProvider
{
	protected NCTSResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override string MessageFriendlyNameCore => Res.GetString("EE1BCFB8-8264-4D4B-8AC7-5184D0CC6630", "NCTS Response");

	protected sealed override ZBool LinkMessageToParentJob(EDIMessage message)
	{
		var result = false;
		var header = FindParentOfMessage(message);
		if (header != null)
		{
			message.EM_LinkedObject = header;
			result = true;
			var branchPk = GetBranchPkFromJobBO(header);
			if (branchPk.IsValid)
			{
				message.EM_GB = branchPk;
			}

			Logger.Log(Res.GetString("D4F7D226-D3E2-423D-B7F6-D8DAC6AAEDE3", "Message linked to movement header"));
		}
		else
		{
			var logMessage = GetLogMessageForFailingToLinkMessageToParentJob(message);
			Logger.Log(logMessage);
			message.Notes.AddNew(true, NLConstants.Notes.Descriptions.DataImportLogText, logMessage);
		}
		return result;
	}

	protected virtual string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_MRN(message);

	protected string GetLogMessageForFailingToLinkMessageToParentJob_MRN(EDIMessage message)
	{
		return Res.GetString("C8C26609-5A6C-42C2-8127-AC13194CBA4E", $"{NLConstants.Notes.Texts.NctsHeaderNotFoundWithMRN}'{GetMessageDataProvider(message).MRN}'. (Interchange Number: {message.EM_InterchangeNumber}, Number: {message.EM_MessageNum}, Type: {message.EM_MessageType})");
	}

	protected string GetLogMessageForFailingToLinkMessageToParentJob_LRN(EDIMessage message)
	{
		return Res.GetString("0CECDA18-D69C-40F1-9D8F-92079883CFDA", $"{NLConstants.Notes.Texts.NctsHeaderNotFoundWithLRN}'{GetMessageDataProvider(message).LRN}'. (Interchange Number: {message.EM_InterchangeNumber}, Number: {message.EM_MessageNum}, Type: {message.EM_MessageType})");
	}

	protected string GetLogMessageForFailingToLinkMessageToParentJob_LRNFallbackOnMRN(EDIMessage message)
	{
		return Res.GetString("3F014E09-CCA9-4B94-BB6B-CCF209155877", $"{NLConstants.Notes.Texts.NctsHeaderNotFoundWithLRN}'{GetMessageDataProvider(message).LRN}'. {NLConstants.Notes.Texts.NctsHeaderNotFoundWithMRN}'{GetMessageDataProvider(message).MRN}'. (Interchange Number: {message.EM_InterchangeNumber}, Number: {message.EM_MessageNum}, Type: {message.EM_MessageType})");
	}

	protected string GetLogMessageForFailingToLinkMessageToParentJob_MRNFallbackOnLRN(EDIMessage message)
	{
		return Res.GetString("513BB389-7B50-4A13-BFA6-1EF4FDE5FC7B", $"{NLConstants.Notes.Texts.NctsHeaderNotFoundWithMRN}'{GetMessageDataProvider(message).MRN}'. {NLConstants.Notes.Texts.NctsHeaderNotFoundWithLRN}'{GetMessageDataProvider(message).LRN}'.");
	}

	protected string GetLogMessageForFailingToLinkMessageToParentJob_LRNFallbackOnMRNFallbackOnCorrelationID(EDIMessage message)
	{
		return Res.GetString("8DF29E58-B635-4E53-9CBF-71662743C9F1", $"{NLConstants.Notes.Texts.NctsHeaderNotFoundWithLRN}'{GetMessageDataProvider(message).LRN}'. {NLConstants.Notes.Texts.NctsHeaderNotFoundWithMRN}'{GetMessageDataProvider(message).MRN}'. {NLConstants.Notes.Texts.NctsHeaderNotFoundWithCorrelationId}'{GetMessageDataProvider(message).CorrelationIdentifier}.");
	}

	protected sealed override void ProcessMessage(NLEDIMessage message)
	{
		if (!IsMessageOkForProcessing(message))
		{
			Logger.LogError(Res.GetString("B9CE90D7-0300-4C24-AA2C-C3E5D1A49316", "{0}. (Interchange Number:{1}, Number:{2}, Type:{3}); message status set to DISCARDED.", LogMessageWhenDiscarded, message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
			message.Notes.AddNew(true, NLConstants.Notes.Descriptions.ProcessingLog, NoteMessageWhenDiscarded);
			message.EM_Status = EDIMessage.Status.Discarded;
		}
		else
		{
			var nctsHeader = GetNctsHeaderFromLinkedObject(message);
			var moveHeader = (NctsCommonMovementHeader)nctsHeader.MovementHeader ?? nctsHeader.ArrivalMovementHeader;
			var previousCustomsStatus = moveHeader.BM_CustomsStatus;
			if (SetNewCustomsStatus)
			{
				var newCustomsStatus = GetNewCustomsStatus(moveHeader);
				if (!newCustomsStatus.IsEmpty)
				{
					moveHeader.BM_CustomsStatus = newCustomsStatus;
				}
			}
			if (SetNewPhase)
			{
				moveHeader.BM_Phase = NewPhase;
			}
			if (SetNewMessageStatus)
			{
				nctsHeader.EffectiveMessageStatus = NewMessageStatus;
			}
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			if (nctsHeader.IsDepartureMovement && previousCustomsStatus.IsEmpty)
			{
				nctsHeader.MovementHeader.ActivateForceRegenerateLocalReferenceNumber = true;
			}

			ProcessMessageCore(message);
			UpdateGuaranteeTransactionsIfNeeded(message, GetMessageDataProvider(message));
		}
	}

	protected virtual void ProcessMessageCore(NLEDIMessage message)
	{
	}

	protected sealed override ZString InterpretMessage(EDIMessage message) => Interpreter.Interpret(GetMessageDataProvider(message));

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByMRN(message, NctsMovementType.Codes.Departure);

	protected BusinessObject FindParentOfMessageByMRN(EDIMessage message, string subApplicationCode = "", string[] messageStatusArray = null, bool returnNullIfMultipleFound = false)
	{
		var dataProvider = GetMessageDataProvider(message);
		var mrn = dataProvider.MRN;
		NctsHeader result = null;

		if (!string.IsNullOrEmpty(mrn))
		{
			var cusInBondHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, mrn);
			cusInBondHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);

			if (!string.IsNullOrEmpty(subApplicationCode))
			{
				var moveHeaderWithCorrectSubApplicationCodeQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
				moveHeaderWithCorrectSubApplicationCodeQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, subApplicationCode);
				cusInBondHeaderQuery.AddSubQuery(moveHeaderWithCorrectSubApplicationCodeQuery, JoinCondition.And);
			}

			var matchingCusInBondHeaders = message.Factory.Load<NctsHeader>(cusInBondHeaderQuery);
			if (!matchingCusInBondHeaders.IsNullOrEmpty() && matchingCusInBondHeaders.Length > 1 && messageStatusArray != null && messageStatusArray.Length > 0)
			{
				matchingCusInBondHeaders = matchingCusInBondHeaders.Where(c => messageStatusArray.Any(s => s == c.CommonMovementHeader.BM_MessageStatus)).ToArray();
			}

			result = returnNullIfMultipleFound && matchingCusInBondHeaders.Length > 1 ? null : matchingCusInBondHeaders.FirstOrDefault();
		}

		return result?.MovementHeader is NctsDepartureMovementHeader departureMovementHeader ? departureMovementHeader : result;
	}

	protected BusinessObject FindParentOfMessageByLRN(EDIMessage message)
	{
		var dataProvider = GetMessageDataProvider(message);
		var lrn = dataProvider.LRN;
		BusinessObject result = null;

		if (!string.IsNullOrEmpty(lrn))
		{
			var entryQuery = new ZDBOnlyQuery(typeof(CusInBondMoveHeader));
			entryQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_PaperlessInbondNum, lrn);
			var moveHeader = message.Factory.LoadTop1<CusInBondMoveHeader>(entryQuery);

			if (moveHeader != null)
			{
				if (moveHeader.BM_SubApplicationCode == NctsMoveHeaderType.Codes.Departure)
				{
					result = moveHeader;
				}
				else
				{
					result = moveHeader.Header as NctsHeader;
				}
			}
		}

		return result;
	}

	protected BusinessObject FindParentOfMessageByCorrelationID(EDIMessage message)
	{
		var dataProvider = GetMessageDataProvider(message);
		var correlationId = dataProvider.CorrelationIdentifier;
		BusinessObject result = null;

		if (!string.IsNullOrEmpty(correlationId))
		{
			var interchangeBySessionSubQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
			interchangeBySessionSubQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.NLCustoms)
										.AddToFilter(EDIInterchangeSchema.EI_IsActive, true)
										.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
										.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Sent)
										.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + OrderByClause.Descending;

			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			messageQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeBySessionSubQuery, JoinCondition.And);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.NLCustoms)
						.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
						.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent)
						.AddToFilter(EDIMessageSchema.EM_MessageType, NLEDIMessageTypes.Codes.NCT)
						.AddToFilter(EDIMessageSchema.EM_MessageNum, correlationId)
						.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending;

			var originalOutgoingEdiMessage = message.Factory.LoadTop1<EDIMessage>(messageQuery);
			result = originalOutgoingEdiMessage?.EM_LinkedObject;
		}

		return result;
	}

	protected BusinessObject FindParentOfMessageByLRNFallbackOnMRN(EDIMessage message, string subApplicationCode = "", string[] messageStatusArray = null, bool returnNullIfMultipleFoundForMRN = false) => FindParentOfMessageByLRN(message) ?? FindParentOfMessageByMRN(message, subApplicationCode, messageStatusArray, returnNullIfMultipleFoundForMRN);

	protected abstract IMessageInterpreter<TDataProvider> Interpreter { get; }

	protected NctsHeader GetNctsHeaderFromLinkedObject(EDIMessage message)
	{
		var linkedObject = message.EM_LinkedObject;
		NctsHeader returnValue = null;

		if (linkedObject is NctsHeader header)
		{
			returnValue = header;
		}
		else if (linkedObject is NctsDepartureMovementHeader movementHeader)
		{
			returnValue = movementHeader.Header;
		}

		return returnValue;
	}

	protected BusinessObject FindParentOfMessageByMRNFallbackOnLRN(EDIMessage message, string subApplicationCode = "") => FindParentOfMessageByMRN(message, subApplicationCode) ?? FindParentOfMessageByLRN(message);

	protected BusinessObject FindParentOfMessageByLRNFallbackOnMRNFallbackOnCorrelationID(EDIMessage message, string subapplicationCode = "", string[] messageStatusArray = null) => FindParentOfMessageByLRNFallbackOnMRN(message, subapplicationCode, messageStatusArray, returnNullIfMultipleFoundForMRN: true) ?? FindParentOfMessageByCorrelationID(message);

	protected virtual bool SetNewCustomsStatus => true;
	protected virtual ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => ZString.Empty;
	protected virtual bool SetNewPhase => true;
	protected virtual bool SetNewMessageStatus => true;
	protected virtual ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	protected virtual ZString NewMessageStatus => LogicalStatusList.Codes.Accepted;
	protected virtual ZBool IsMessageOkForProcessing(EDIMessage message) => true;
	protected virtual ZString LogMessageWhenDiscarded { get; }
	protected virtual ZString NoteMessageWhenDiscarded { get; }
	protected virtual void UpdateGuaranteeTransactionsIfNeeded(NLEDIMessage message, TDataProvider messageDataProvider) { }
	protected override ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => NCTSResponseMessageHelper.GetBranchPkFromJobBO(linkedObject);
}
