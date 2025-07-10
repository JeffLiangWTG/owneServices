using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.NCTS.Business;

sealed class CC170CProvider : DepartureHeaderProvider, ICC170C
{
	public CC170CProvider(MessageSendingAction sendingAction) : base(Argument.NotNull(sendingAction, nameof(sendingAction)).Header)
	{
	}

	public override string MessageType => NLConstants.WCoTypeCodes.PresentationNotification;
}
