using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF
{
	public class CALINFCall : ICALINFCallInformation
	{
		public CALINFCall(ZString location, ZString locationCodeListId, CodeListResponsibleAgencyCodeList locationCodeListAgency, ZDateTime departureOrArrivalDateTime)
		{
			this.location = Argument.NotNull(location, nameof(location));
			this.locationCodeListId = Argument.NotNull(locationCodeListId, nameof(locationCodeListId));
			this.locationCodeListAgency = Argument.NotNull(locationCodeListAgency, nameof(locationCodeListAgency));
			this.departureOrArrivalDateTime = Argument.NotNull(departureOrArrivalDateTime, nameof(departureOrArrivalDateTime));
		}

		ZString ILOC_LocationInformation.CallLocation => location;

		ZString ILOC_LocationInformation.LocationCodeListIdentificationCode => locationCodeListId;

		CodeListResponsibleAgencyCodeList ILOC_LocationInformation.LocationCodeListResponsibleAgencyCode => locationCodeListAgency;

		ZDateTime IDTM_DateTimeInformation.CallDateTime => departureOrArrivalDateTime;

		readonly ZString location;
		readonly ZString locationCodeListId;
		readonly CodeListResponsibleAgencyCodeList locationCodeListAgency;
		readonly ZDateTime departureOrArrivalDateTime;
	}
}
