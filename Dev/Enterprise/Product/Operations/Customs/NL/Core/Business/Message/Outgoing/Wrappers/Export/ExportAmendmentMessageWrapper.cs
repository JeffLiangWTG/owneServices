using System;
using System.Collections.Immutable;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ExportAmendmentMessageWrapper : MetaDataWrapper, IMetaDataWithComparedMetaData
{
	public ExportAmendmentMessageWrapper(JobDeclarationMessageSendingObject provider) : base(provider)
	{
	}

	public string ComparedMessageText => GetComparedMessageInfo().MessageText;

	public ComparedMessageXSDType ComparedMessageXSDType => GetComparedMessageInfo().MessageXSDType;

	(string MessageText, ComparedMessageXSDType MessageXSDType) GetComparedMessageInfo()
	{
		var entryStatus = messageSendingObject.EntryStatus;
		var compareWithCC529C_CheckEntryStatus550 = entryStatus == NLConstants.EntryStatus.ReleasedAndTaxed;
		var compareWithCC515C = !compareWithCC529C_CheckEntryStatus550 && customsStatusCompareWithCC515C.Contains(entryStatus);
		var compareWithCC529C_CheckEntryStatus500 = !compareWithCC529C_CheckEntryStatus550 && !compareWithCC515C && customsStatusCompareWithCC529C_CheckEntryStatus500.Contains(entryStatus);
		var compareWithCC529C_CheckEntryStatus550Or500 = !compareWithCC529C_CheckEntryStatus550 && !compareWithCC515C && !compareWithCC529C_CheckEntryStatus500 && customsStatusCompareWithCC529C_CheckEntryStatus500Or550.Contains(entryStatus);

		return entryHeader.Factory.GetCachedValue(string.Join("_", "GetComparedMessageInfo", compareWithCC515C, compareWithCC529C_CheckEntryStatus500, compareWithCC529C_CheckEntryStatus550, compareWithCC529C_CheckEntryStatus550Or500), () =>
		{
			var messageXSDTypeResult = ComparedMessageXSDType.Declaration;
			NLEDIMessage comparedMessage = null;
			var messages = entryHeader.Messages;
			if (compareWithCC515C)
			{
				comparedMessage = GetSentMessageForComparison(messages, ExportSendMessageTypes.Codes.DEC);
			}
			else if ((compareWithCC529C_CheckEntryStatus500 && HasReleasedMessage(messages, NLIncomingMessageSubTypeList.Codes.CC529C, NLConstants.EntryStatus.ProvisionalRelease))
					|| (compareWithCC529C_CheckEntryStatus550 && HasReleasedMessage(messages, NLIncomingMessageSubTypeList.Codes.CC529C, NLConstants.EntryStatus.ReleasedAndTaxed))
					|| (compareWithCC529C_CheckEntryStatus550Or500 && HasReleasedMessage(messages, NLIncomingMessageSubTypeList.Codes.CC529C, NLConstants.EntryStatus.ProvisionalRelease, NLConstants.EntryStatus.ReleasedAndTaxed)))
			{
				comparedMessage = GetSentMessageForComparison(messages, ExportSendMessageTypes.Codes.CRE);
				messageXSDTypeResult = ComparedMessageXSDType.AdditionalMessage;
			}

			if (comparedMessage == null)
			{
				throw new ApplicationException("The old message was not captured, so we cannot send an amendment message.");
			}

			return (comparedMessage.EM_MessageText, messageXSDTypeResult);
		}, CacheStalenessPolicy.StaleOnFactorySave);
	}

	static readonly ImmutableArray<string> customsStatusCompareWithCC515C = new[] {
		NLConstants.EntryStatus.AdvanceDeclarationReceived,
		NLConstants.EntryStatus._220,
		NLConstants.EntryStatus.Accepted,
		NLConstants.EntryStatus.DocumentsControl,
		NLConstants.EntryStatus.NuclearMaterials,
		NLConstants.EntryStatus.NonIntrusiveInspection,
		NLConstants.EntryStatus.PhysicalInspection,
		NLConstants.EntryStatus.IdentificationOfShipment,
		NLConstants.EntryStatus.IntrusiveInspection,
		NLConstants.EntryStatus.QualityControl,
		NLConstants.EntryStatus.CharacteristicsOfGoods,
		NLConstants.EntryStatus.Sampling,
		NLConstants.EntryStatus.RequestForInformation,
		NLConstants.EntryStatus.ExportReminder_NoExitInformationReceived,
	}.ToImmutableArray();

	static readonly ImmutableArray<string> customsStatusCompareWithCC529C_CheckEntryStatus500 = new[] {
		NLConstants.EntryStatus.ProvisionalRelease,
		NLConstants.EntryStatus.SupplementReminder,
		NLConstants.EntryStatus.SupplementSent,
		NLConstants.EntryStatus.SupplementReceivedByCustoms,
		NLConstants.EntryStatus.ExportRejection_515_for_SubStyle_XY,
	}.ToImmutableArray();

	static readonly ImmutableArray<string> customsStatusCompareWithCC529C_CheckEntryStatus500Or550 = new[] {
		NLConstants.EntryStatus._800,
		NLConstants.EntryStatus.NoExitInformationRecievedYet,
		NLConstants.EntryStatus.ExitInformationDetailsSent,
		NLConstants.EntryStatus.ExitInformationDetailsReceived,
		NLConstants.EntryStatus.ExportRejection_583,
		NLConstants.EntryStatus.ExportCancellation,
	}.ToImmutableArray();
}
