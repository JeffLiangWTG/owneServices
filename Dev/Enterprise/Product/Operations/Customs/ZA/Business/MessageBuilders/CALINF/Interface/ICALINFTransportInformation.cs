using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF
{
	public interface ICALINFTransportInformation : ITDT_TransportMeansInformation, IRFF_PrincipalCarrierConveyanceInformation
	{
		List<ICALINFCallInformation> DepartureDetails { get; }
		List<ICALINFCallInformation> DischargeDetails { get; }
		List<ICALINFCallInformation> CallDetails { get; }
	}

	public interface ITDT_TransportMeansInformation
	{
		ZString ConveyanceNumber { get; }
		ZString TransportMode { get; }
		ZString CarrierCode { get; }
		ZString CarrierName { get; }
		CodeListResponsibleAgencyCodeList CarrierCodeListResponsibleAgencyCode { get; }

		ZString MeansOfTransportId { get; }
		ZString MeansOfTransportName { get; }
		ZString MeansOfTransportNationality { get; }
		ZString MeansOfTransportCodeListIdentificationCode { get; }
	}

	public interface IRFF_PrincipalCarrierConveyanceInformation
	{
		ZString PrincipalCarrierConveyanceNumber { get; }
	}
}
