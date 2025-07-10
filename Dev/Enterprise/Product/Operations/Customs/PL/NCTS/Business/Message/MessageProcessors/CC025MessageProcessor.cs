using System;
using System.Collections.Generic;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS;

public class CC025MessageProcessor : BaseNctsMessageProcessor<IIE025>
{
	public CC025MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => MessageNameList.Descriptions.IE025;

	protected override string EmailSubject(BaseEDIMessage message, IIE025 messageDataProvider) => messageDataProvider.TransitOperation?.ReleaseIndicator switch
	{
		"1" => $"IE025_Full_Released_({messageDataProvider.MRN})",
		"2" or "3" => $"IE025_Partial_Released_({messageDataProvider.MRN})",
		"4" => $"IE025_No_Released_({messageDataProvider.MRN})",
		_ => string.Empty,
	};

	protected override Type MessageInterpreterType => typeof(CC025CMessageInterpreter);

	protected override void PreProcessMessageCore(BaseEDIMessage message, IIE025 messageDataProvider)
	{
		base.PreProcessMessageCore(message, messageDataProvider);

		message.EM_ApplicationReference = messageDataProvider.MRN;
	}

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIE025 dataProvider)
	{
		var arrivalMovementHeader = ((NctsHeader)message.EM_LinkedObject).ArrivalMovementHeader;

		var transitOperation = dataProvider.TransitOperation;

		if (releaseTypeDictionary.TryGetValue(transitOperation.ReleaseIndicator, out var customsStatus))
		{
			arrivalMovementHeader.BM_CustomsStatus = customsStatus;
		}
		else
		{
			return ProcessingResult.Fail;
		}
		arrivalMovementHeader.Header.MovementReferenceEntryNumber.CE_IssueDate = transitOperation.ReleaseDate;

		arrivalMovementHeader.BM_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;

		return ProcessingResult.Succeed;
	}

	readonly IReadOnlyDictionary<string, string> releaseTypeDictionary = new Dictionary<string, string> {
		{ Business.Constants.ReleaseType.ClosedFullRelease, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease },
		{ Business.Constants.ReleaseType.DiscrepancyResolutionPartialRelease, NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease },
		{ Business.Constants.ReleaseType.ClosedPartialRelease, NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease },
		{ Business.Constants.ReleaseType.DiscrepancyResolutionNoRelease, NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease },
	};
}
