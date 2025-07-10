using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC014CProvider : MessageHeaderProvider, ICC014C
{
	readonly MessageSendingAction sendingAction;
	public CC014CProvider(MessageSendingAction sendingAction) : base(Argument.NotNull(sendingAction, nameof(sendingAction)).Header)
	{
		this.sendingAction = sendingAction;
	}

	public override string MessageType => NLConstants.WCoTypeCodes.DeclarationInvalidationRequest;

	public string MRN => nctsHeader.MovementReferenceNumber;

	public string LRN => MRN.IsEmpty() ? nctsHeader.MovementHeader.BM_PaperlessInbondNum : null;

	public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure => CachedValueHelper.GetValue(ref holderOfTheTransitProcedure, () => HolderOfTheTransitProcedureProvider.New(nctsHeader.Principal));
	CachedValue<IHolderOfTheTransitProcedure> holderOfTheTransitProcedure;

	public string CustomsOfficeOfDeparture => nctsHeader.CommonMovementHeader.DepartureCustomsOfficeCode;

	public DateTime InvalidationRequestDateAndTime => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.Now.ToDateTime(), removeMillisecond: true);

	public DateTime InvalidationDecisionDateAndTime =>
		MessageRecipient.StartsWith(NLNctsConstants.RecepientTypes.NTA) ? default : DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.Now.ToDateTime(), removeMillisecond: true);

	public bool InvalidationDecision => !MessageRecipient.StartsWith(NLNctsConstants.RecepientTypes.NTA);

	public bool InvalidationInitiatedByCustoms => false;

	public string InvalidationJustification => sendingAction.Justification;
}
