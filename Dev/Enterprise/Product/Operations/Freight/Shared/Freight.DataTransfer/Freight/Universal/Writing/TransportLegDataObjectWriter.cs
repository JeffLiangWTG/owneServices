using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class TransportLegDataObjectWriter : DataObjectWriter<Transport, TransportLeg>
	{
		readonly BusinessObject parent;

		public TransportLegDataObjectWriter(IDataWritingManager manager, BusinessObject parent = default) : base(manager)
		{
			this.parent = parent;
		}

		internal TransportLeg PopulateDataObjectForLocalTransport(BusinessObject bo)
		{
			return PopulateDataObject((Transport)bo);
		}

		protected override TransportLeg PopulateDataObject(Transport legBO)
		{
			var legData = new TransportLeg(writeManager.WriterStrategy);
			legData.LegOrder = legBO.JW_LegOrder;
			legData.TransportMode = new TransportModeConverter().ToEnumValue(legBO.JW_TransportMode);
			legData.LegType = new LegTypeConverter().ToEnumValue(legBO.JW_TransportType);
			legData.LegNotes = legBO.JW_LegNotes;

			legData.VesselName = legBO.JW_Vessel;
			legData.VesselLloydsIMO = legBO.Vessel.GetLloydsIMO();
			legData.VoyageFlightNo = legBO.JW_VoyageFlight;
			legData.AircraftType = ListHelper.GetWithDescription<CodeDescriptionPair>(legBO.JW_AircraftType, legBO.AircraftType_List);

			legData.PortOfLoading = UNLOCO.New(legBO.LoadPort);
			legData.EstimatedDeparture = legBO.JW_ETD;
			legData.ActualDeparture = legBO.JW_ATD;

			legData.PortOfDischarge = UNLOCO.New(legBO.DiscPort);
			legData.EstimatedArrival = legBO.JW_ETA;
			legData.ActualArrival = legBO.JW_ATA;

			legData.CarrierBookingReference = legBO.JW_CarrierBookingReference;

			legData.IsCargoOnly = legBO.JW_IsCargoOnly;

			var departureLocation = legBO.DepartureLocation;
			if (departureLocation != null)
			{
				legData.DepartureFrom = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.PickUpAddress)).GetDataObject(departureLocation);
			}

			var arrivalLocation = legBO.ArrivalLocation;
			if (arrivalLocation != null)
			{
				legData.ArrivalAt = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.DropOffAddress)).GetDataObject(arrivalLocation);
			}

			AddSailingDetails(legBO.Sailing, legData);
			PopulateTransportDateDetails(legBO, legData);

			var carrierAddress = legBO.CarrierAddress;
			if (carrierAddress != null)
			{
				legData.Carrier = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Carrier)).GetDataObject(carrierAddress);
			}

			legData.CarrierServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(legBO.JW_PL_NKCarrierServiceLevel, legBO.CarrierServiceLevel_List);

			var creditorAddress = legBO.CreditorAddress;
			if (creditorAddress != null)
			{
				legData.Creditor = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Creditor)).GetDataObject(creditorAddress);
			}

			legData.BookingStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(legBO.JW_Status, legBO.JW_Status_List);

			PopulateAddtionalTransportModes(legBO, legData);

			PopulateCO2eFields(legBO, legData);

			return legData;
		}

		void PopulateCO2eFields(Transport legBO, TransportLeg legData)
		{
			if (parent is ICO2eLegBasedSupporter supporter && legBO is ICO2eLegProvider provider)
			{
				using (provider.WithTempCurrentCO2eCalcSupporter(supporter))
				{
					legData.GreenhouseGasEmission = new GreenhouseGasEmission
					{
						CO2e = provider.DynamicTotalCO2e,
						CO2eUnit = ListHelper.GetWithDescription<UnitOfWeight>(Constants.Weight.Kilograms, legBO.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight)),
						CO2eDescriptiveStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(legBO.GetCO2eStatus(), new CO2eStatusList())
							.AdditionalSetup(cdp => cdp.Description = CO2eHelper.GetCO2eStatusShortDescription(cdp.Code)),
					};
				}
			}
		}

		void PopulateAddtionalTransportModes(Transport legBO, TransportLeg legData)
		{
			var additionalTransportModes = new List<AdditionalTransportMode>();
			var additionalTransportMode = new TransportModeConverter().ToEnumValue(legBO.JW_AdditionalTransportMode);
			if (additionalTransportMode.HasValue)
			{
				additionalTransportModes.Add(new AdditionalTransportMode
				{
					TransportMode = additionalTransportMode.Value
				});
			}
			legData.SetAdditionalTransportModeCollection(() => additionalTransportModes.Any() ? additionalTransportModes : null);
		}

		void AddSailingDetails(JobSailing jobSailing, TransportLeg legData)
		{
			if (jobSailing != null)
			{
				AddOriginDetails(jobSailing.Origin, legData);
				AddDestinationDetails(jobSailing.Destination, legData);
			}
		}

		void PopulateTransportDateDetails(Transport legBO, TransportLeg legData)
		{
			legData.LCLReceivalCommences = legBO.JW_DepotReceivalCommences;
			legData.LCLCutOff = legBO.JW_DepotCutOff;
			legData.LCLAvailability = legBO.JW_DepotAvailabilityDate;
			legData.LCLStorageDate = legBO.JW_DepotStorageDate;

			legData.DocumentCutOff = legBO.JW_DocumentaryCutOff;
			legData.VGMCutOff = legBO.JW_VGMCutOff;

			legData.FCLReceivalCommences = legBO.JW_TerminalReceivalCommences;
			legData.FCLCutOff = legBO.JW_TerminalCutOff;
			legData.FCLAvailability = legBO.JW_TerminalAvailabilityDate;
			legData.FCLStorage = legBO.JW_TerminalStorageDate;

			legData.EmptyReceivalCommences = legBO.JW_EmptyReceivalCommences;
			legData.EmptyCutOff = legBO.JW_EmptyCutOff;
			legData.ReeferReceivalCommences = legBO.JW_ReeferReceivalCommences;
			legData.ReeferCutOff = legBO.JW_ReeferCutOff;
			legData.HazzardReceivalCommences = legBO.JW_DGReceivalCommences;
			legData.HazzardCutOffDate = legBO.JW_DGCutOff;

			legData.ScheduledDeparture = legBO.JW_STD;
			legData.ScheduledArrival = legBO.JW_STA;
		}

		void AddOriginDetails(VoyageOrigin voyageOrigin, TransportLeg legData)
		{
			if (voyageOrigin != null)
			{
				var departureCTOAddress = voyageOrigin.DepartureCTOAddress;
				if (departureCTOAddress != null)
				{
					legData.DepartureCTO = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.DepartureCTOAddress)).GetDataObject(departureCTOAddress);
				}
				legData.DepartureBerth = voyageOrigin.JA_Berth;
				legData.DepartureReference = voyageOrigin.JA_DepartReference;

				legData.EstimatedArrivalInPortOfLoading = voyageOrigin.JA_E_ARV;
				legData.ActualArrivalInPortOfLoading = voyageOrigin.JA_A_ARV;

				legData.ScheduledArrivalInPortOfLoading = voyageOrigin.JA_S_ARV;
			}
		}

		void AddDestinationDetails(VoyageDestination voyageDestination, TransportLeg legData)
		{
			if (voyageDestination != null)
			{
				var arrivalCTOAddress = voyageDestination.ArrivalCTOAddress;
				if (arrivalCTOAddress != null)
				{
					legData.ArrivalCTO = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ArrivalCTOAddress)).GetDataObject(arrivalCTOAddress);
				}

				legData.ArrivalBerth = voyageDestination.JB_Berth;
				legData.ArrivalReference = voyageDestination.JB_ArrivalReference;
			}
		}
	}

	class TransportLegDataObjectWriterForLocalTransport : DataObjectWriter<BusinessObject, TransportLeg>, ITransportLegDataObjectWriter
	{
		public TransportLegDataObjectWriterForLocalTransport(IDataWritingManager manager) : base(manager) { }

		protected override TransportLeg PopulateDataObject(BusinessObject sourceBO)
		{
			var writer = new TransportLegDataObjectWriter(writeManager);
			return writer.PopulateDataObjectForLocalTransport(sourceBO);
		}
	}
}
