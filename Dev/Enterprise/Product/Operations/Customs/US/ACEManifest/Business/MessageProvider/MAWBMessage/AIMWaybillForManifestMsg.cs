using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMWaybillForManifestMsg : IAIMWaybill
	{
		public AIMWaybillForManifestMsg(AsycudaManifestHeader header, AdditionalMessageInformation additionalMessageInformation)
		{
			this.header = Argument.NotNull(header, "header");
			this.additionalMessageInformation = additionalMessageInformation;
		}

		readonly AsycudaManifestHeader header;
		readonly AdditionalMessageInformation additionalMessageInformation;

		public ZString AirportOfOrigin => header.PortOfLoading?.RL_IATA ?? ZString.Empty;

		public ZDecimal NumberOfPieces => (ZDecimal)(header.TotalPieces);

		public ZDecimal Weight => header.TotalWeightInKilos.Round(1);

		public ZString WeightCode => WeightUnits.Codes.Kilograms;

		public ZString CargoDescription
		{
			get
			{
				if (additionalMessageInformation.AM_IsConsolidation)
				{
					return WaybillConsolidationDescription;
				}
				else
				{
					return header.Bills[0]?.ABL_GoodsDescription ?? ZString.Empty;
				}
			}
		}

		public ZString PermitToProceedDestinationAirport => HasPortOfFirstArrivalIsDifferentToPortOfDischarge ? PortOfDischarge : ZString.Empty;

		public ZDate DateOfArrivalAtThePermitToProceedDestinationAirport => HasPortOfFirstArrivalIsDifferentToPortOfDischarge ? header.AMA_E_ARV.Date : ZDate.Empty;

		bool HasPortOfFirstArrivalIsDifferentToPortOfDischarge => !PortOfFirstArrival.IsEmpty && !PortOfDischarge.IsEmpty && !PortOfDischarge.Equals(PortOfFirstArrival);

		ZString PortOfFirstArrival => portOfFirstArrival ?? (portOfFirstArrival = header.PortOfFirstArrival?.RL_IATA ?? ZString.Empty).Value;
		ZString? portOfFirstArrival;

		ZString PortOfDischarge => portOfDischarge ?? (portOfDischarge = header.PortOfDischarge?.RL_IATA ?? ZString.Empty).Value;
		ZString? portOfDischarge;

		const string WaybillConsolidationDescription = "CONSOLIDATION";
	}
}
