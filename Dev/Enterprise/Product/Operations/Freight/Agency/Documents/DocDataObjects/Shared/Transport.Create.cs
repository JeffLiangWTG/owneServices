using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class Transport
	{
		public static Transport Create(IContext context, Freight.Business.Transport transport)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));

			return new Transport(transport.PK)
			{
				LegOrder = transport.JW_LegOrder,
				Mode = new CodeDescription(transport.JW_TransportMode_List)
				{
					Code = transport.JW_TransportMode
				},
				AdditionalTransportMode = new CodeDescription(transport.JW_AdditionalTransportMode_List)
				{
					Code = transport.JW_AdditionalTransportMode
				},
				Type = new CodeDescription(context.TransportTypes)
				{
					Code = transport.JW_TransportType
				},
				ETD = transport.JW_ETD,
				ETA = transport.JW_ETA,
				ATD = transport.JW_ATD,
				ATA = transport.JW_ATA,
				LCLCutOff = transport.Sailing?.JX_DepotCutOff ?? ZDateTime.Empty,
				LCLReceivalCommences = transport.Sailing?.JX_DepotReceivalCommences ?? ZDateTime.Empty,
				VoyageFlightNumber = transport.JW_VoyageFlight,
				Vessel = Vessel.Create(context, transport),
				PortOfLoading = Unloco.Create(context, transport.LoadPort),
				PortOfDischarge = Unloco.Create(context, transport.DiscPort),
				Carrier = AddressBuilder.Create(context, transport.CarrierAddress),
			};
		}

		public static Transport Create(IContext context, TransportLeg transportLeg)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));

			return new Transport()
			{
				LegOrder = transportLeg.LegOrder.GetValueOrDefault(),
				Mode = new CodeDescription(context.TransportModes)
				{
					Code = new TransportModeConverter().FromEnumValue(transportLeg.TransportMode)
				},
				AdditionalTransportMode = new CodeDescription(context.TransportModes)
				{
					Code = transportLeg.AdditionalTransportModeCollection != null && transportLeg.AdditionalTransportModeCollection.Any() ?
						new TransportModeConverter().FromEnumValue(transportLeg.AdditionalTransportModeCollection.FirstOrDefault().TransportMode) : ZString.Empty
				},
				Type = new CodeDescription(context.TransportTypes)
				{
					Code = ConvertUniversalLegTypeToTransportTypeCode(transportLeg.LegType.GetValueOrDefault())
				},
				ETD = transportLeg.EstimatedDeparture.GetValueOrDefault(),
				ETA = transportLeg.EstimatedArrival.GetValueOrDefault(),
				ATD = transportLeg.ActualDeparture.GetValueOrDefault(),
				ATA = transportLeg.ActualArrival.GetValueOrDefault(),
				LCLCutOff = transportLeg.LCLCutOff.GetValueOrDefault(),
				LCLReceivalCommences = transportLeg.LCLReceivalCommences.GetValueOrDefault(),
				VoyageFlightNumber = transportLeg.VoyageFlightNo.GetValueOrDefault(),
				Vessel = Vessel.Create(context, transportLeg),
				PortOfLoading = UnlocoExtensions.CreateFromUNLOCO(context, transportLeg.PortOfLoading),
				PortOfDischarge = UnlocoExtensions.CreateFromUNLOCO(context, transportLeg.PortOfDischarge),
				Carrier = AddressBuilder.Create(context, transportLeg.Carrier),
			};
		}

		static ZString ConvertUniversalLegTypeToTransportTypeCode(LegType legType)
		{
			switch (legType)
			{
				case LegType.Flight1:
					return TransportTypes.Codes.Flight1;

				case LegType.Flight2:
					return TransportTypes.Codes.Flight2;

				case LegType.Flight3:
					return TransportTypes.Codes.Flight3;

				case LegType.Main:
					return TransportTypes.Codes.Main;

				case LegType.PreCarriage:
					return TransportTypes.Codes.PreCarriage;

				case LegType.OnForwarding:
					return TransportTypes.Codes.OnForwarding;

				case LegType.Other:
					return TransportTypes.Codes.Other;

				case LegType.LocalTransport:
					return TransportTypes.Codes.LocalTransport;

				default:
					return ZString.Empty;
			}
		}
	}
}
