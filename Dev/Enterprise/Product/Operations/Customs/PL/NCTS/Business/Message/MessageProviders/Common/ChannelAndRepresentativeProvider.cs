using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class ChannelAndRepresentativeProvider : IChannelAndRepresentative
{
	public ChannelAndRepresentativeProvider(NctsCommonMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		nctsHeader = (NctsHeader)Argument.NotNull(movementHeader.Header, $"{nameof(NctsCommonMovementHeader)}.{nameof(movementHeader.Header)}");
	}

	protected readonly NctsCommonMovementHeader movementHeader;
	protected readonly NctsHeader nctsHeader;
	public ICommunicationChannelType CommunicationChannel => communicationChannel ?? (communicationChannel = new CommunicationChannelProvider(GlbStaff.CurrentUser));
	ICommunicationChannelType communicationChannel;

	public string RepresentativeIdentificationNumber => CachedValueHelper.GetValue(ref representativeIdentificationNumber, GetRepresentativeIdentificationNumber);
	CachedValue<string> representativeIdentificationNumber;

	string GetRepresentativeIdentificationNumber()
	{
		var identificationNumberForDestination = ZString.Empty;
		var identificationNumberForRepresentative = movementHeader.Representative.Organisation?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori)
													?? ZString.Empty;

		if (movementHeader is NctsArrivalMovementHeader arrivalMovement)
		{
			identificationNumberForDestination = arrivalMovement.Header.DestinationTrader.Organisation?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori)
												?? ZString.Empty;
		}

		if (identificationNumberForRepresentative != identificationNumberForDestination)
		{
			return MessageProviderHelper.ReturnNullIfEmpty(identificationNumberForRepresentative);
		}

		return null;
	}
}
