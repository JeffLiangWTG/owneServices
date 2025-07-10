using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class CO2eTransportLegDataObjectWriter : DataObjectWriter<Transport, TransportLeg>
	{
		public CO2eTransportLegDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override TransportLeg PopulateDataObject(Transport legBO)
		{
			var legData = new TransportLeg(writeManager.WriterStrategy);
			legData.LegOrder = legBO.JW_LegOrder;
			legData.TransportMode = new TransportModeConverter().ToEnumValue(legBO.JW_TransportMode);
			legData.LegType = new LegTypeConverter().ToEnumValue(legBO.JW_TransportType);

			legData.VesselName = legBO.JW_Vessel;
			legData.VesselLloydsIMO = legBO.Vessel.GetLloydsIMO();
			legData.VoyageFlightNo = legBO.JW_VoyageFlight;
			legData.AircraftType = ListHelper.GetWithDescription<CodeDescriptionPair>(legBO.JW_AircraftType, legBO.AircraftType_List);

			legData.PortOfLoading = UNLOCO.New(legBO.LoadPort);
			legData.EstimatedDeparture = legBO.JW_ETD;

			legData.PortOfDischarge = UNLOCO.New(legBO.DiscPort);
			legData.EstimatedArrival = legBO.JW_ETA;

			legData.IsCargoOnly = legBO.JW_IsCargoOnly;

			var carrierAddress = legBO.CarrierAddress;
			if (carrierAddress != null)
			{
				legData.Carrier = new CO2eOrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Carrier)).GetDataObject(carrierAddress);
			}

			return legData;
		}
	}
}
