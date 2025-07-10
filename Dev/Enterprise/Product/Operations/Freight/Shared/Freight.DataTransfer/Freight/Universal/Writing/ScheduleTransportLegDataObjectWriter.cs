using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ScheduleTransportLegDataObjectWriter : DataObjectWriter<JobSailing, TransportLeg>
	{
		public ScheduleTransportLegDataObjectWriter(IDataWritingManager manager) : base(manager) { }

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
				legData.Carrier = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Carrier)).GetDataObject(carrierBO.MainAddress);
			}

			return legData;
		}

		void PopulateOriginFields(JobSailing scheduleBO, TransportLeg legData)
		{
			legData.PortOfLoading = UNLOCO.New(scheduleBO.PortOfLoading);
			legData.EstimatedDeparture = scheduleBO.JX_JA_E_DEP;
			legData.ActualDeparture = scheduleBO.JX_JA_A_DEP;
			legData.FCLReceivalCommences = scheduleBO.JX_JA_CTOReceivalCommences;
			legData.FCLCutOff = scheduleBO.JX_JA_CTOCutOff;
			legData.LCLReceivalCommences = scheduleBO.JX_DepotReceivalCommences;
			legData.LCLCutOff = scheduleBO.JX_DepotCutOff;

			var departureCTOAddress = scheduleBO.DepartureCTOAddress;
			if (departureCTOAddress != null)
			{
				legData.DepartureCTO = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.DepartureCTOAddress)).GetDataObject(departureCTOAddress);
			}

			legData.DepartureBerth = scheduleBO.JX_JA_DepartureBerth;
			legData.DepartureReference = scheduleBO.JX_JA_DepartureReference;
			legData.EstimatedArrivalInPortOfLoading = scheduleBO.JX_JA_E_ARV;
			legData.ActualArrivalInPortOfLoading = scheduleBO.JX_JA_A_ARV;
			legData.DocumentCutOff = scheduleBO.JX_JA_DocumentaryCutoff;
			legData.HazzardReceivalCommences = scheduleBO.JX_JA_DGFCLReceivalCommences;
			legData.HazzardCutOffDate = scheduleBO.JX_JA_DGFCLCutOff;
			legData.VGMCutOff = scheduleBO.JX_JA_VGMCutOff;

			legData.EmptyReceivalCommences = scheduleBO.JX_JA_EmptyReceivalCommences;
			legData.EmptyCutOff = scheduleBO.JX_JA_EmptyCutOff;
			legData.ReeferReceivalCommences = scheduleBO.JX_JA_ReeferReceivalCommences;
			legData.ReeferCutOff = scheduleBO.JX_JA_ReeferCutOff;
			legData.ScheduledDeparture = scheduleBO.JX_JA_S_DEP;
		}

		void PopulateDestinationFields(JobSailing scheduleBO, TransportLeg legData)
		{
			legData.PortOfDischarge = UNLOCO.New(scheduleBO.PortOfDischarge);
			legData.EstimatedArrival = scheduleBO.JX_JB_E_ARV;
			legData.ActualArrival = scheduleBO.JX_JB_A_ARV;
			legData.FCLAvailability = scheduleBO.JX_JB_CTOAvailabilityDate;
			legData.FCLStorage = scheduleBO.JX_JB_CTOStorageDate;
			legData.LCLAvailability = scheduleBO.JX_DepotAvailabilityDate;
			legData.LCLStorageDate = scheduleBO.JX_DepotStorageDate;

			var arrivalCTOAddress = scheduleBO.ArrivalCTOAddress;
			if (arrivalCTOAddress != null)
			{
				legData.ArrivalCTO = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ArrivalCTOAddress)).GetDataObject(arrivalCTOAddress);
			}

			legData.ArrivalBerth = scheduleBO.JX_JB_ArrivalBerth;
			legData.ArrivalReference = scheduleBO.JX_JB_ArrivalReference;

			legData.ScheduledArrivalInPortOfLoading = scheduleBO.JX_JA_S_ARV;
			legData.ScheduledArrival = scheduleBO.JX_JB_S_ARV;
		}
	}
}
