using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class CO2eScheduleTransportLegDataObjectWriter : DataObjectWriter<JobSailing, TransportLeg>
	{
		public CO2eScheduleTransportLegDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override TransportLeg PopulateDataObject(JobSailing scheduleBO)
		{
			var legData = new TransportLeg(writeManager.WriterStrategy);

			legData.LegOrder = 1;
			legData.TransportMode = new TransportModeConverter().ToEnumValue(scheduleBO.JX_TransportMode);
			legData.LegType = LegType.Main;

			legData.VesselName = scheduleBO.JX_JV_NKVessel;
			legData.VesselLloydsIMO = scheduleBO.Vessel.GetLloydsIMO();
			legData.VoyageFlightNo = scheduleBO.JX_JV_VoyageFlight;

			if (scheduleBO.JX_TransportMode == TransportModes.Air && scheduleBO.Voyage != null)
			{
				legData.IsCargoOnly = scheduleBO.Voyage.JV_IsCargoOnly;
				legData.AircraftType = ListHelper.GetWithDescription<CodeDescriptionPair>(scheduleBO.Voyage.JV_AircraftType, scheduleBO.Voyage.JV_AircraftType_List);
			}

			PopulateOriginFields(scheduleBO, legData);
			PopulateDestinationFields(scheduleBO, legData);

			var carrierBO = scheduleBO.Line;
			if (carrierBO != null)
			{
				legData.Carrier = new CO2eOrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Carrier)).GetDataObject(carrierBO.MainAddress);
			}

			return legData;
		}

		void PopulateOriginFields(JobSailing scheduleBO, TransportLeg legData)
		{
			legData.PortOfLoading = UNLOCO.New(scheduleBO.PortOfLoading);
			legData.EstimatedDeparture = scheduleBO.JX_JA_E_DEP;
		}

		void PopulateDestinationFields(JobSailing scheduleBO, TransportLeg legData)
		{
			legData.PortOfDischarge = UNLOCO.New(scheduleBO.PortOfDischarge);
			legData.EstimatedArrival = scheduleBO.JX_JB_E_ARV;
		}
	}
}
