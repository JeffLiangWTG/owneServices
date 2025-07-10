using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CountrySpecificDataPLProvider : ChannelAndRepresentativeProvider, IChannelRepresentativeAndLocationOfGoods, IChannelRepresentativeAndTIRInfo
{
	public CountrySpecificDataPLProvider(NctsCommonMovementHeader movementHeader, MessageSendingObject messageSendingObject) : base(movementHeader)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}
	readonly MessageSendingObject messageSendingObject;

	public string LocationOfGoodsCodeFromAuthorisation => locationOfGoodsCodeFromAuthorisation ?? (locationOfGoodsCodeFromAuthorisation = GetLocationOfGoodsCodeFromAuthorisation());
	string locationOfGoodsCodeFromAuthorisation;

	public string TIRPageNumberValue => messageSendingObject.TirPageNumber;

	public string TIRUnloadingNumberValue => messageSendingObject.TirUnloadingNumber;

	string GetLocationOfGoodsCodeFromAuthorisation()
	{
		if (movementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
		{
			var location = arrivalMovementHeader.AuthorizationLocation;
			if (!location.IsEmpty
				&& arrivalMovementHeader.Header.CusAuthorizationUsages.FirstOrDefault() is CusAuthorizationUsage usage
				&& HasMoreThanOneLocationRule(usage))
			{
				return location;
			}
		}
		else if (movementHeader is NctsDepartureMovementHeader departureMovementHeader)
		{
			var cusAuthorizationUsages = departureMovementHeader.IsPhase5 ? (IBusinessObjectCollection<CusAuthorizationUsage>)departureMovementHeader.CusAuthorizationUsages : departureMovementHeader.Header.CusAuthorizationUsages;
			if (cusAuthorizationUsages.FirstOrDefault() is CusAuthorizationUsage usage)
			{
				var location = usage.AGC_Location;
				if (!location.IsEmpty && HasMoreThanOneLocationRule(usage))
				{
					return location;
				}
			}
		}
		return string.Empty;

		bool HasMoreThanOneLocationRule(CusAuthorizationUsage authUsage) => authUsage.RelatedAuthorisationHeader is CusAuthorisationHeader authHeader
																			&& authHeader.CusAuthorisationRules.Count(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Location) > 1;
	}
}
