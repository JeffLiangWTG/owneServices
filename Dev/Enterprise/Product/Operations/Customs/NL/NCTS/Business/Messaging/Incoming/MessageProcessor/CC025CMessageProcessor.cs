using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC025C;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC025CMessageProcessor : NCTSResponseMessageProcessor<ICC025CDataProvider>
{
	public CC025CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override bool SetNewPhase => false;

	protected override ZString NewMessageStatus => LogicalStatusList.Codes.Accepted;

	protected override ICC025CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc025CType, CC025CDataProvider>();

	protected override IMessageInterpreter<ICC025CDataProvider> Interpreter => new CC025CMessageInterpreter();

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByMRN(message, NctsMovementType.Codes.Arrival);

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		var nctsHeader = (NctsHeader)message.EM_LinkedObject;
		var messageDataProvider = GetMessageDataProvider(message);

		var customsStatus = string.Empty;
		switch (messageDataProvider.ReleaseIndicator)
		{
			case NLNctsConstants.ReleaseIndicator.FullRelease:
				customsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				break;
			case NLNctsConstants.ReleaseIndicator.PartialRelease:
				customsStatus = NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease;
				break;
			case NLNctsConstants.ReleaseIndicator.NoRelease:
				customsStatus = NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
				break;
			case NLNctsConstants.ReleaseIndicator.PartialReleaseClosed:
				customsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease;
				break;
		}

		foreach (var houseConsignmentProvider in messageDataProvider.HouseConsignments)
		{
			foreach (var consignmentItemProvider in houseConsignmentProvider.ConsignmentItems)
			{
				foreach (var packageProvider in consignmentItemProvider.Packagings)
				{
					var package = nctsHeader.RetrieveArrivalGoodsItemPackage(houseConsignmentProvider.SequenceNumeric, consignmentItemProvider.DeclarationGoodsItemNumber, packageProvider.SequenceNumeric);

					if (package != null)
					{
						package.B5_UnitsReleased = (ZShort)packageProvider.NumberOfPackages;
					}
				}
			}
		}

		nctsHeader.MovementReferenceEntryNumber.CE_IssueDate = messageDataProvider.ReleaseDate.ConvertToZDateTime();
		if (!customsStatus.IsEmpty())
		{
			nctsHeader.ArrivalMovementHeader.CustomsEntryStatusLogAdded += MovementHeader_CustomsEntryStatusLogAdded;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = customsStatus;
		}
		nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
		message.EM_Status = EDIMessage.Status.ProcessedOK;
	}

	void MovementHeader_CustomsEntryStatusLogAdded(object sender, EventArgs e)
	{
		if (sender is NctsArrivalMovementHeader moveHeader)
		{
			moveHeader.Header.LockFileIfEnabledByConfiguration(Res.GetString("B44A004F-ABD8-43EA-B2D6-335E292E4A58", "The tab 'Unloading Remarks' and its depending tabs were locked when message 'Goods Release Notification' was received."), EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks);
			moveHeader.CustomsEntryStatusLogAdded -= MovementHeader_CustomsEntryStatusLogAdded;
		}
	}
}
