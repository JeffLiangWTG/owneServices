using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF
{
	public class CALINFTransportSea : ICALINFTransportInformation
	{
		public CALINFTransportSea(JobVoyage jobVoyage)
		{
			this.jobVoyage = Argument.NotNull(jobVoyage, nameof(jobVoyage));
			var helper = new CALINFCallHelper("139", CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			DepartureDetails = helper.PrepareDepartureData(this.jobVoyage.Origins);
			CallDetails = helper.PrepareDestinationData(this.jobVoyage.Destinations);
		}

		public List<ICALINFCallInformation> DepartureDetails { get; }
		public List<ICALINFCallInformation> DischargeDetails { get; } = new List<ICALINFCallInformation>();
		public List<ICALINFCallInformation> CallDetails { get; }

		#region ITDT_TransportMeansInformation

		ZString ITDT_TransportMeansInformation.ConveyanceNumber => jobVoyage.JV_VoyageFlight;
		ZString ITDT_TransportMeansInformation.TransportMode => "1";
		ZString ITDT_TransportMeansInformation.CarrierCode => jobVoyage.Vessel?.RV_CarrierCode ?? ZString.Empty;
		ZString ITDT_TransportMeansInformation.CarrierName => ZString.Empty;
		CodeListResponsibleAgencyCodeList ITDT_TransportMeansInformation.CarrierCodeListResponsibleAgencyCode => CodeListResponsibleAgencyCodeList.BicBureauInternationalDesContaineurs;

		ZString ITDT_TransportMeansInformation.MeansOfTransportId => jobVoyage.Vessel?.RV_RadioCallSign ?? ZString.Empty;
		ZString ITDT_TransportMeansInformation.MeansOfTransportName => jobVoyage.Vessel?.RV_Code ?? ZString.Empty;
		ZString ITDT_TransportMeansInformation.MeansOfTransportNationality => jobVoyage.Vessel?.RV_RN_NKCountryOfReg ?? ZString.Empty;
		ZString ITDT_TransportMeansInformation.MeansOfTransportCodeListIdentificationCode => "103";

		#endregion

		#region IRFF_PrincipalCarrierConveyanceInformation

		ZString IRFF_PrincipalCarrierConveyanceInformation.PrincipalCarrierConveyanceNumber => jobVoyage.JV_VoyageFlight;

		#endregion

		readonly JobVoyage jobVoyage;
	}
}
