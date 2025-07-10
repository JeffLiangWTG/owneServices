using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF
{
	public class CALINFTransportAir : ICALINFTransportInformation
	{
		public CALINFTransportAir(JobVoyage jobVoyage)
		{
			this.jobVoyage = Argument.NotNull(jobVoyage, nameof(jobVoyage));
			var helper = new CALINFCallHelper("145", CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, mustConvertToIataLocationCode: true, this.jobVoyage.Factory);
			DepartureDetails = helper.PrepareDepartureData(this.jobVoyage.Origins);
			CallDetails = helper.PrepareDestinationData(this.jobVoyage.Destinations);
		}

		public List<ICALINFCallInformation> DepartureDetails { get; }
		public List<ICALINFCallInformation> DischargeDetails { get; } = new List<ICALINFCallInformation>();
		public List<ICALINFCallInformation> CallDetails { get; }

		#region ITDT_TransportMeansInformation

		ZString ITDT_TransportMeansInformation.ConveyanceNumber => jobVoyage.JV_VoyageFlight;
		ZString ITDT_TransportMeansInformation.TransportMode => "4";

		ZString ITDT_TransportMeansInformation.CarrierCode
		{
			get
			{
				var orgHeader = jobVoyage.Factory.Load<OrgHeader>(jobVoyage.JV_OH_Line);
				return orgHeader?.MiscServ?.Airline?.RM_EagleAddedAirlinePrefixOrAccountingCode ?? ZString.Empty;
			}
		}

		ZString ITDT_TransportMeansInformation.CarrierName => ZString.Empty;
		CodeListResponsibleAgencyCodeList ITDT_TransportMeansInformation.CarrierCodeListResponsibleAgencyCode => CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation;

		ZString ITDT_TransportMeansInformation.MeansOfTransportId => ZString.Empty;
		ZString ITDT_TransportMeansInformation.MeansOfTransportName => ZString.Empty;
		ZString ITDT_TransportMeansInformation.MeansOfTransportNationality => ZString.Empty;
		ZString ITDT_TransportMeansInformation.MeansOfTransportCodeListIdentificationCode => "146";

		#endregion

		#region IRFF_PrincipalCarrierConveyanceInformation

		ZString IRFF_PrincipalCarrierConveyanceInformation.PrincipalCarrierConveyanceNumber => jobVoyage.JV_VoyageFlight;

		#endregion

		readonly JobVoyage jobVoyage;
	}
}
