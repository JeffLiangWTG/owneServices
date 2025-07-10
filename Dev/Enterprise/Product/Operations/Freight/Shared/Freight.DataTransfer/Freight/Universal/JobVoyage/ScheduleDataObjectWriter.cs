using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalSchedule = Enterprise.UniversalDataBuss.DataObjects.Universal.Schedule;
using UniversalTransport = Enterprise.UniversalDataBuss.DataObjects.Universal.ScheduleTransport;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ScheduleDataObjectWriter : TopLevelDataObjectWriter<JobVoyage, UniversalSchedule>
	{
		public ScheduleDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalSchedule;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.SailingSchedule;
		}

		protected override void PopulateDataObject(JobVoyage voyage, UniversalSchedule dataObject)
		{
			if (voyage.Line != null)
			{
				dataObject.Carrier = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Carrier)).GetDataObject(voyage.Line.MainAddress);
			}
			dataObject.Transport = GetTransport(voyage);
			dataObject.SetLoadingCollection(() => voyage.Origins.Cast<VoyageOrigin>().Select(origin => GetLoadingPort(origin)).ToList());
			dataObject.SetDischargeCollection(() => voyage.Destinations.Cast<VoyageDestination>().Select(destination => GetDischargePort(destination)).ToList());
		}

		UniversalTransport GetTransport(JobVoyage voyage)
		{
			var transport = new UniversalTransport();

			switch (voyage.JV_AirSeaRoad)
			{
				case Core.Constants.TransportModes.Air:
					transport.Air = new Air
					{
						FlightNumber = voyage.JV_VoyageFlight,
						IsCargoOnly = voyage.JV_IsCargoOnly
					};
					break;

				case Core.Constants.TransportModes.Sea:
					transport.Sea = new Sea
					{
						Vessel = new Vessel() { VesselName = voyage.JV_RV_NKVessel },
						VoyageNumber = voyage.JV_VoyageFlight,
						VoyageType = ListHelper.GetWithDescription<CodeDescriptionPair>(voyage.JV_VoyageType, voyage.JV_TransportType_List)
					};
					break;

				case Core.Constants.TransportModes.Rail:
					transport.Rail = new Rail
					{
						Journey = voyage.JV_RV_NKVessel,
						JourneyNumber = voyage.JV_VoyageFlight
					};
					break;

				case Core.Constants.TransportModes.Road:
					transport.Road = new Road
					{
						TruckReference = voyage.JV_VoyageFlight
					};
					break;
			}

			return transport;
		}

		Loading GetLoadingPort(VoyageOrigin origin)
		{
			var loading = new Loading();
			loading.Port = UNLOCO.New(origin.PortOfLoading);

			loading.EstimatedArrival = origin.JA_E_ARV;
			loading.ActualArrival = origin.JA_A_ARV;

			loading.EstimatedDeparture = origin.JA_E_DEP;
			loading.ActualDeparture = origin.JA_A_DEP;

			loading.DepartureBerth = origin.JA_Berth;
			loading.DepartureReference = origin.JA_DepartReference;

			if (origin.DepartureCTOAddress != null)
			{
				loading.DepartureCTO = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.DepartureCTOAddress)).GetDataObject(origin.DepartureCTOAddress);
			}

			loading.DocumentCutOff = origin.JA_DocumentaryCutoff;

			loading.FCLReceivalCommences = origin.JA_ReceivalCommences;
			loading.FCLCutOff = origin.JA_CutOff;

			loading.HazzardReceivalCommences = origin.JA_DGReceivalCommences;
			loading.HazzardCutOffDate = origin.JA_DGCutOff;

			return loading;
		}

		Discharge GetDischargePort(VoyageDestination destination)
		{
			var discharge = new Discharge();
			discharge.Port = UNLOCO.New(destination.PortOfDischarge);

			discharge.EstimatedArrival = destination.JB_E_ARV;
			discharge.ActualArrival = destination.JB_A_ARV;

			discharge.ArrivalBerth = destination.JB_Berth;
			discharge.ArrivalReference = destination.JB_ArrivalReference;

			if (destination.ArrivalCTOAddress != null)
			{
				discharge.ArrivalCTO = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ArrivalCTOAddress)).GetDataObject(destination.ArrivalCTOAddress);
			}

			discharge.FCLAvailability = destination.JB_AvailabilityDate;
			discharge.FCLStorage = destination.JB_StorageDate;

			return discharge;
		}
	}
}
