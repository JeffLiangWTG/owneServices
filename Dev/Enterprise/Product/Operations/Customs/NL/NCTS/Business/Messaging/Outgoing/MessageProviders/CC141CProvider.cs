using System;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.NCTS.Business;

sealed class CC141CProvider : MessageHeaderProvider, ICC141C
{
	readonly MessageSendingAction action;
	public CC141CProvider(MessageSendingAction sendingAction) : base(Argument.NotNull(sendingAction, nameof(sendingAction)).Header)
	{
		action = sendingAction;
	}

	public override string MessageType => NLConstants.WCoTypeCodes.InformationAboutNonArrivedMovement;

	public string MRN => nctsHeader.MovementReferenceNumber;

	public string CustomsOfficeOfDestination => action.ActualOfficeOfDestination;

	public string CustomsOfficeOfEnquiryAtDeparture => nctsHeader.MovementHeader.EnquiryCustomsOfficeCode;

	public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure => CachedValueHelper.GetValue(ref holderOfTheTransitProcedure, () => HolderOfTheTransitProcedureProvider.New(nctsHeader.Principal));
	CachedValue<IHolderOfTheTransitProcedure> holderOfTheTransitProcedure;

	public DateTime? EnquiryTC11DeliveryDate => action.TCI11.IsEmpty ? null : action.TCI11.ToDateTime();

	public string EnquiryText => action.QueryInformation;

	public INCTSParty ConsignmentConsignee => new PartyProvider(action.ActualConsignee);
}
