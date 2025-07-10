using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.Common;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC141CProvider : CCProviderBase, ICC141C
{
	public CC141CProvider(NctsDepartureMovementHeader movementHeader, string messageType, MessageSendingObject messageSendingObject)
		: base(Argument.NotNull(movementHeader, nameof(movementHeader)), Argument.NotNull(messageType, nameof(messageType)))
	{
		this.movementHeader = movementHeader;
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;

	public string TransitOperationMRN => messageSendingObject.MRN;

	public string CustomsOfficeOfDestinationReferenceNumber => CachedValueHelper.GetValue(ref customsOfficeOfDestinationReferenceNumber, ()
		=> CheckRuleC0215() ? null : messageSendingObject.ActualOfficeOfDestination);
	CachedValue<string> customsOfficeOfDestinationReferenceNumber;

	public string CustomsOfficeOfEnquiryReferenceNumber => CachedValueHelper.GetValue(ref customsOfficeOfEnquiryReferenceNumber, ()
		=> movementHeader.DepartureCustomsOffice?.OfficeCode);
	CachedValue<string> customsOfficeOfEnquiryReferenceNumber;

	public IHolderOfTheTransitProcedureWithMaxLength HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure
		= new HolderOfTheTransitProcedureProvider(nctsHeader.Principal, movementHeader));
	IHolderOfTheTransitProcedureWithMaxLength holderOfTheTransitProcedure;

	public IEnquiry Enquiry => enquiry ?? (enquiry = new EnquiryProvider(messageSendingObject));
	IEnquiry enquiry;

	public IConsignee Consignee => CachedValueHelper.GetValue(ref consignee, ()
		=> CheckRuleC0215() ? null : new ConsigneeProvider(messageSendingObject.ActualConsignee, IsInPhase5TransitionPeriod));
	CachedValue<IConsignee> consignee;

	public PhaseID? PhaseID => CachedValueHelper.GetValue(ref phaseId, () => IsInPhase5TransitionPeriod
		? CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_0
		: CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_1);
	CachedValue<PhaseID?> phaseId;

	bool CheckRuleC0215() => messageSendingObject.AdditionalText.IsEmpty;

	bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => movementHeader.IsInPhase5TransitionPeriod);
	CachedValue<bool> isInPhase5TransitionPeriod;
}
