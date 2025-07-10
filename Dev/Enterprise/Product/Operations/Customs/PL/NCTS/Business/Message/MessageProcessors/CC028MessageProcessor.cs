using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC028MessageProcessor(LoggingInformation logger) : BaseNctsMessageProcessor<IIE028>(logger)
{
	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE028;

	protected override string EmailSubject(BaseEDIMessage message, IIE028 messageDataProvider) => $"IE028_MRN_Allocated_({messageDataProvider.LRN})";

	protected override Type MessageInterpreterType => typeof(CC028CMessageInterpreter);

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE028 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = messageDataProvider.LRN;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE028 dataProvider)
	{
		var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var header = movementHeader.Header;

		movementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
		movementHeader.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
		movementHeader.BM_EntryDate = dataProvider.DeclarationAcceptanceDate;
		if (movementHeader.BM_AdditionalDeclarationType == EU.NCTS.Business.NctsTypeOfAdditionalDeclarationList.Codes.D)
		{
			movementHeader.BM_AdditionalDeclarationType = EU.NCTS.Business.NctsTypeOfAdditionalDeclarationList.Codes.A;
		}

		header.MovementReferenceEntryNumber.CE_EntryNum = dataProvider.MRN;

		header.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: movementHeader.BM_EntryDate.ToOffset(), reference: movementHeader.BM_CustomsStatus));

		return ProcessingResult.Succeed;
	}
}
