using System;
using System.Collections.Immutable;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ImportAmendmentMessageWrapper : MetaDataWrapper, IMetaDataWithComparedMetaData
{
	public ImportAmendmentMessageWrapper(JobDeclarationMessageSendingObject provider) : base(provider)
	{
	}

	public string ComparedMessageText => GetComparedMessageInfo().MessageText;

	public ComparedMessageXSDType ComparedMessageXSDType => GetComparedMessageInfo().MessageXSDType;

	(string MessageText, ComparedMessageXSDType MessageXSDType) GetComparedMessageInfo()
	{
		var entryStatus = messageSendingObject.EntryStatus;
		var compareWithCC415A = customsStatusCompareWithCC415A.Contains(entryStatus);
		var compareWithCC429A = !compareWithCC415A && customsStatusCompareWithCC429A.Contains(entryStatus);
		var compareWithCC429AElseCC415A = !compareWithCC415A && !compareWithCC429A && customsStatusCompareWithCC429AElseCC415A.Contains(entryStatus);

		return entryHeader.Factory.GetCachedValue(string.Join("_", "GetComparedMessageInfo", compareWithCC429AElseCC415A, compareWithCC415A, compareWithCC429A), () =>
		{
			var messageXSDTypeResult = ComparedMessageXSDType.Declaration;
			NLEDIMessage comparedMessage = null;
			var messages = entryHeader.Messages;
			if (compareWithCC415A)
			{
				comparedMessage = GetSentMessageForComparison(messages, ImportSendMessageTypes.Codes.DEC);
			}
			else if (compareWithCC429A)
			{
				comparedMessage = GetSentMessageForComparison(messages, ImportSendMessageTypes.Codes.CRI);
				messageXSDTypeResult = ComparedMessageXSDType.AdditionalMessage;
			}
			else if (compareWithCC429AElseCC415A)
			{
				if (HasReleasedMessage(messages, NLIncomingMessageSubTypeList.Codes.CC429A, NLConstants.EntryStatus.ProvisionalRelease))
				{
					comparedMessage = GetSentMessageForComparison(messages, ImportSendMessageTypes.Codes.CRI);
					messageXSDTypeResult = ComparedMessageXSDType.AdditionalMessage;
				}
				else
				{
					comparedMessage = GetSentMessageForComparison(messages, ImportSendMessageTypes.Codes.DEC);
				}
			}

			if (comparedMessage == null)
			{
				throw new ApplicationException("The old message was not captured, so we cannot send an amendment message.");
			}

			return (comparedMessage.EM_MessageText, messageXSDTypeResult);
		}, CacheStalenessPolicy.StaleOnFactorySave);
	}

	static readonly ImmutableArray<string> customsStatusCompareWithCC415A = new[] {
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
		NLConstants.EntryStatus.OtherControl,
		NLConstants.EntryStatus.RequestForInformation,
		NLConstants.EntryStatus.ExportReminder_NoExitInformationReceived,
	}.ToImmutableArray();

	static readonly ImmutableArray<string> customsStatusCompareWithCC429A = new[] {
		NLConstants.EntryStatus.ProvisionalRelease,
		NLConstants.EntryStatus.SupplementReminder,
		NLConstants.EntryStatus.SupplementSent,
		NLConstants.EntryStatus.SupplementReceivedByCustoms,
		NLConstants.EntryStatus.Rejection_415_for_SubStyle_XY,
		NLConstants.EntryStatus.ReleasedAndTaxed,
	}.ToImmutableArray();

	static readonly ImmutableArray<string> customsStatusCompareWithCC429AElseCC415A = new[] {
		NLConstants.EntryStatus._400,
		NLConstants.EntryStatus._410,
		NLConstants.EntryStatus._420
	}.ToImmutableArray();
}
