using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
namespace Enterprise.Customs.NL.Business;

public abstract class NLBranchCustomsApplicationTypeMessageProcessor<TProvider> : BranchCustomsApplicationTypeMessageProcessor where TProvider : IIncomingDataProvider
{
	public NLBranchCustomsApplicationTypeMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.NLCustoms;

	protected abstract ZString InterpretMessage(EDIMessage message);

	protected override void PreProcessMessageCore(EDIMessage message)
	{
		var successfull = false;
		var responseMessage = message;
		var dataProvider = GetMessageDataProvider(responseMessage);

		if (dataProvider != null)
		{
			successfull = LinkMessageToParentJob(responseMessage);
		}
		else
		{
			Logger.Log(Res.GetString("D33100AD-8DB9-4CE7-9307-97548CC24B82", "Failed to convert message content"));
			message.Notes.AddNew(false, NLConstants.Notes.Descriptions.DataImportLogText, NLConstants.Notes.Texts.FailedToDeserialize);
		}

		message.EM_Status = successfull ? EDIMessage.Status.PreProcessedOK : EDIMessage.Status.Failed;
	}

	protected abstract TProvider GetMessageDataProvider(EDIMessage message);

	protected virtual ZString StatusForUnableToFindALinkedBusinessObject => EDIMessage.Status.Failed;

	protected virtual ZString NoteForUnableToFindALinkedBusinessObject(IIncomingDataProvider messageDataProvider) => ZString.Empty;

	protected sealed override void ProcessMessageCore(EDIMessage baseMessage)
	{
		if (baseMessage is NLEDIMessage message && message.EM_Status == EDIMessage.Status.PreProcessedOK)
		{
			ProcessMessage(message);
			if (message.EM_LinkedObject != null)
			{
				if (message.EM_Status != EDIMessage.Status.Discarded)
				{
					message.EM_MessageInterpretation = InterpretMessage(message);
					UpdateGuaranteeTransactionsIfNeeded(message, GetMessageDataProvider(message));
				}
				else
				{
					message.EM_MessageInterpretation = InterpretDiscardedMessage(message);
				}
			}
		}
	}

	protected virtual void UpdateGuaranteeTransactionsIfNeeded(EDIMessage message, TProvider messageDataProvider)
	{
	}

	string InterpretDiscardedMessage(NLEDIMessage message)
	{
		var noteText = message.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).LastOrDefault()?.ST_NoteDataAsText;
		return noteText.HasValue ? ((string)noteText.GetValueOrDefault()) : null;
	}

	protected abstract ZBool LinkMessageToParentJob(EDIMessage message);

	protected abstract void ProcessMessage(NLEDIMessage message);

	protected abstract BusinessObject FindParentOfMessage(EDIMessage message);

	protected abstract ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject);
}
