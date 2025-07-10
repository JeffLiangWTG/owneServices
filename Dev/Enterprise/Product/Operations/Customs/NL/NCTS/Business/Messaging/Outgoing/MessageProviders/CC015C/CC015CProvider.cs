using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.NCTS.Business;

sealed class CC015CProvider : DepartureHeaderProvider, ICC015C
{
	public CC015CProvider(MessageSendingAction sendingAction) : base(Argument.NotNull(sendingAction, nameof(sendingAction)).Header)
	{
	}

	public override ITransitOperation TransitOperation => CachedValueHelper.GetValue(ref transitOperation, () => new TransitOperationWithBindingItineraryZeroProvider(nctsHeader));
	CachedValue<ITransitOperation> transitOperation;

	public override string MessageType => NLConstants.WCoTypeCodes.DeclarationData;

	protected override INCTSConsignment ConsignmentCore => new CC015CConsignmentProvider(nctsHeader);
}
